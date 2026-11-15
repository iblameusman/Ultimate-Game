using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using DodgeGame.Arena;
using DodgeGame.Core;
using DodgeGame.Data;
using DodgeGame.Player;

namespace DodgeGame.Editor
{
    public static class ArenaMapIntegrator
    {
        private const string FbxPath = "Assets/Art/Arena/DodgeBallArena.fbx";
        private const string ScenePath = "Assets/Scenes/MainGame.unity";
        private const string MatDir = "Assets/Art/Materials/Arena";
        private const string TexDir = "Assets/Art/Textures/Arena";

        [MenuItem("Tools/Integrate Dodge Ball Arena Map")]
        public static void IntegrateMap()
        {
            Debug.Log("[ArenaMapIntegrator] Starting Dodge Ball Arena integration...");

            if (!AssetDatabase.IsValidFolder("Assets/Art/Materials/Arena"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Art/Materials"))
                    AssetDatabase.CreateFolder("Assets/Art", "Materials");
                AssetDatabase.CreateFolder("Assets/Art/Materials", "Arena");
            }

            var materials = SetupMaterials();

            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.path != ScenePath)
            {
                activeScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            var arenaRoot = GameObject.Find("Arena");
            if (arenaRoot == null)
            {
                arenaRoot = new GameObject("Arena");
                Undo.RegisterCreatedObjectUndo(arenaRoot, "Create Arena Root");
            }

            DeactivateOldArenaParts(arenaRoot);

            var existingEnv = arenaRoot.transform.Find("Arena_Environment");
            if (existingEnv != null)
            {
                Undo.DestroyObjectImmediate(existingEnv.gameObject);
            }

            var fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
            if (fbxAsset == null)
            {
                Debug.LogError($"[ArenaMapIntegrator] Could not load FBX at {FbxPath}!");
                return;
            }

            var envInstance = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset, arenaRoot.transform);
            envInstance.name = "Arena_Environment";
            envInstance.transform.localPosition = Vector3.zero;
            envInstance.transform.localRotation = Quaternion.identity;
            envInstance.transform.localScale = Vector3.one;
            Undo.RegisterCreatedObjectUndo(envInstance, "Instantiate Arena Environment");

            int arenaLayer = LayerMask.NameToLayer("Arena");
            if (arenaLayer < 0) arenaLayer = 11;

            ApplyMaterialsAndColliders(envInstance, materials, arenaLayer);

            SetupGroundColliders(envInstance, arenaLayer);

            SetupPerimeterColliders(arenaRoot, arenaLayer);

            SetupStreetlights(envInstance);

            UpdateBallSpawner(arenaLayer);

            UpdateDimensionVisuals(arenaRoot);

            UpdateCameraFraming();

            UpdatePlayerStart();

            EditorUtility.SetDirty(arenaRoot);
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[ArenaMapIntegrator] Dodge Ball Arena integration completed successfully!");
        }

        private static Dictionary<string, Material> SetupMaterials()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            var dict = new Dictionary<string, Material>();

            Material GetOrCreate(string name, Color color, float smoothness, float metallic, Color? emission = null, string texName = null)
            {
                string path = $"{MatDir}/{name}.mat";
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null)
                {
                    mat = new Material(shader);
                    AssetDatabase.CreateAsset(mat, path);
                }
                else
                {
                    mat.shader = shader;
                }

                mat.SetColor("_BaseColor", color);
                mat.SetFloat("_Smoothness", smoothness);
                mat.SetFloat("_Metallic", metallic);

                if (emission.HasValue && emission.Value != Color.black)
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", emission.Value);
                }
                else
                {
                    mat.DisableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", Color.black);
                }

                if (!string.IsNullOrEmpty(texName))
                {
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexDir}/{texName}");
                    if (tex != null)
                    {
                        mat.SetTexture("_BaseMap", tex);
                    }
                }

                EditorUtility.SetDirty(mat);
                dict[name] = mat;
                return mat;
            }

            GetOrCreate("M_Asphalt", new Color(0.12f, 0.13f, 0.15f, 1f), 0.18f, 0.0f);
            GetOrCreate("M_Road_White", new Color(0.88f, 0.90f, 0.92f, 1f), 0.35f, 0.0f);
            GetOrCreate("M_Road_Yellow", new Color(0.96f, 0.64f, 0.08f, 1f), 0.35f, 0.0f);
            GetOrCreate("M_Sidewalk", new Color(0.48f, 0.50f, 0.53f, 1f), 0.22f, 0.0f);
            GetOrCreate("M_Curb", new Color(0.36f, 0.38f, 0.40f, 1f), 0.30f, 0.0f);

            GetOrCreate("M_WallConcrete", new Color(0.42f, 0.44f, 0.47f, 1f), 0.20f, 0.0f);
            GetOrCreate("M_WallMetal", new Color(0.20f, 0.22f, 0.25f, 1f), 0.65f, 0.85f);
            GetOrCreate("M_WarningYellow", new Color(1.0f, 0.60f, 0.02f, 1f), 0.45f, 0.1f);

            GetOrCreate("M_Container_Navy", new Color(0.12f, 0.28f, 0.48f, 1f), 0.45f, 0.65f);
            GetOrCreate("M_Container_Orange", new Color(0.78f, 0.32f, 0.10f, 1f), 0.45f, 0.65f);
            GetOrCreate("M_Container_Green", new Color(0.18f, 0.38f, 0.25f, 1f), 0.45f, 0.65f);
            GetOrCreate("M_RustyCar", new Color(0.52f, 0.22f, 0.18f, 1f), 0.32f, 0.45f);
            GetOrCreate("M_CardboardBox", new Color(0.68f, 0.52f, 0.36f, 1f), 0.12f, 0.0f);

            GetOrCreate("M_Streetlight_Pole", new Color(0.18f, 0.20f, 0.22f, 1f), 0.70f, 0.85f);
            GetOrCreate("M_Streetlight_Lamp", new Color(1f, 0.95f, 0.85f, 1f), 0.90f, 0.1f, new Color(2.5f, 2.1f, 1.4f, 1f));

            GetOrCreate("M_House_01", Color.white, 0.35f, 0.0f, null, "file27.tga");
            GetOrCreate("M_House_02", Color.white, 0.35f, 0.0f, null, "file41.tga");
            GetOrCreate("M_Plants", Color.white, 0.25f, 0.0f, null, "file39.tga");
            GetOrCreate("M_Building_Facade", new Color(0.40f, 0.45f, 0.52f, 1f), 0.35f, 0.15f);
            GetOrCreate("M_Building_Windows", new Color(0.15f, 0.22f, 0.32f, 1f), 0.85f, 0.80f, new Color(0.2f, 0.35f, 0.55f, 1f));

            return dict;
        }

        private static void DeactivateOldArenaParts(GameObject arenaRoot)
        {
            string[] oldNames = { "Arena_Floor", "Wall_North", "Wall_South", "Wall_East", "Wall_West" };
            foreach (var n in oldNames)
            {
                var tr = arenaRoot.transform.Find(n);
                if (tr != null)
                {
                    tr.gameObject.SetActive(false);
                    tr.gameObject.name = "[Archived]_" + n;
                }
            }
        }

        private static void ApplyMaterialsAndColliders(GameObject envInstance, Dictionary<string, Material> materials, int arenaLayer)
        {
            var allRenderers = envInstance.GetComponentsInChildren<MeshRenderer>(true);

            foreach (var mr in allRenderers)
            {
                var go = mr.gameObject;
                string n = go.name;

                if (n.StartsWith("[COL_"))
                {
                    go.layer = arenaLayer;
                    mr.enabled = false;

                    var mc = go.GetComponent<MeshCollider>();
                    if (mc == null) mc = go.AddComponent<MeshCollider>();
                    var mf = go.GetComponent<MeshFilter>();
                    if (mf != null) mc.sharedMesh = mf.sharedMesh;
                    mc.convex = true;

                    continue;
                }

                go.layer = arenaLayer;

                if (n.Contains("Asphalt") || n.Contains("Road_Main") || n.Contains("Road_Cross"))
                {
                    mr.sharedMaterial = materials["M_Asphalt"];
                }
                else if (n.Contains("CenterLine_W"))
                {
                    mr.sharedMaterial = materials["M_Road_White"];
                }
                else if (n.Contains("CenterLine_Y"))
                {
                    mr.sharedMaterial = materials["M_Road_Yellow"];
                }
                else if (n.Contains("Sidewalk"))
                {
                    mr.sharedMaterial = materials["M_Sidewalk"];
                }
                else if (n.Contains("Curb"))
                {
                    mr.sharedMaterial = materials["M_Curb"];
                }
                else if (n.Contains("ConcreteBase"))
                {
                    mr.sharedMaterial = materials["M_WallConcrete"];
                }
                else if (n.Contains("UpperFencing"))
                {
                    mr.sharedMaterial = materials["M_WallMetal"];
                }
                else if (n.Contains("Stripe"))
                {
                    mr.sharedMaterial = materials["M_WarningYellow"];
                }
                else if (n.Contains("Pole"))
                {
                    mr.sharedMaterial = materials["M_Streetlight_Pole"];
                }
                else if (n.Contains("Head"))
                {
                    mr.sharedMaterial = materials["M_Streetlight_Lamp"];
                }
                else if (n.Contains("Box"))
                {
                    mr.sharedMaterial = materials["M_CardboardBox"];
                }
                else if (n.Contains("Car") || n.Contains("Object006") || n.Contains("Object007"))
                {
                    mr.sharedMaterial = materials["M_RustyCar"];
                }
                else if (n.Contains("House"))
                {
                    mr.sharedMaterial = materials["M_House_01"];
                }
                else if (n.Contains("Plane") && go.transform.parent != null && go.transform.parent.name.Contains("Building"))
                {
                    mr.sharedMaterial = materials["M_Building_Facade"];
                }
                else if (n.Contains("Circle") || n.Contains("Cube"))
                {
                    mr.sharedMaterial = materials["M_Building_Windows"];
                }

                var containerParent = go.transform.parent;
                while (containerParent != null && !containerParent.name.StartsWith("[OBS_Container"))
                {
                    containerParent = containerParent.parent;
                }
                if (containerParent != null)
                {
                    if (containerParent.name.EndsWith("01")) mr.sharedMaterial = materials["M_Container_Navy"];
                    else if (containerParent.name.EndsWith("02")) mr.sharedMaterial = materials["M_Container_Orange"];
                    else if (containerParent.name.EndsWith("03")) mr.sharedMaterial = materials["M_Container_Green"];
                }
            }

            string[] refNames = { "[REF_Player_Scale]", "[SPAWN_Player_Start]", "[REF_HazardBall_Fire]", "[REF_HazardBall_Ice]" };
            foreach (var rn in refNames)
            {
                var tr = envInstance.transform.Find(rn);
                if (tr != null) tr.gameObject.SetActive(false);
            }
        }

        private static void SetupGroundColliders(GameObject envInstance, int arenaLayer)
        {
            var groundTr = envInstance.transform.Find("[ENV_Ground_Asphalt]");
            if (groundTr != null)
            {
                var bc = groundTr.GetComponent<BoxCollider>();
                if (bc == null) bc = groundTr.gameObject.AddComponent<BoxCollider>();
                bc.center = new Vector3(0f, -0.25f, 0f);
                bc.size = new Vector3(60f, 0.5f, 60f);
                groundTr.gameObject.layer = arenaLayer;
            }

            string[] sidewalks = { "[ENV_Sidewalk_NE]", "[ENV_Sidewalk_NW]", "[ENV_Sidewalk_SE]", "[ENV_Sidewalk_SW]" };
            foreach (var sw in sidewalks)
            {
                var swTr = envInstance.transform.Find(sw);
                if (swTr != null)
                {
                    var bc = swTr.GetComponent<BoxCollider>();
                    if (bc == null) bc = swTr.gameObject.AddComponent<BoxCollider>();
                    var mr = swTr.GetComponent<MeshRenderer>();
                    if (mr != null)
                    {
                        bc.center = swTr.InverseTransformPoint(mr.bounds.center);
                        bc.size = mr.bounds.size;
                    }
                    swTr.gameObject.layer = arenaLayer;
                }
            }
        }

        private static void SetupPerimeterColliders(GameObject arenaRoot, int arenaLayer)
        {
            var boundsHolder = arenaRoot.transform.Find("Boundary_Colliders");
            if (boundsHolder == null)
            {
                var go = new GameObject("Boundary_Colliders");
                go.transform.SetParent(arenaRoot.transform, false);
                boundsHolder = go.transform;
            }

            void EnsureWall(string name, Vector3 center, Vector3 size)
            {
                var wallTr = boundsHolder.Find(name);
                if (wallTr == null)
                {
                    var go = new GameObject(name);
                    go.transform.SetParent(boundsHolder, false);
                    wallTr = go.transform;
                }

                wallTr.gameObject.layer = arenaLayer;
                wallTr.localPosition = center;

                var bc = wallTr.GetComponent<BoxCollider>();
                if (bc == null) bc = wallTr.gameObject.AddComponent<BoxCollider>();
                bc.center = Vector3.zero;
                bc.size = size;
            }

            EnsureWall("Wall_North", new Vector3(0f, 3f, -30.4f), new Vector3(61.6f, 6f, 0.8f));
            EnsureWall("Wall_South", new Vector3(0f, 3f, 30.4f), new Vector3(61.6f, 6f, 0.8f));
            EnsureWall("Wall_East", new Vector3(-30.4f, 3f, 0f), new Vector3(0.8f, 6f, 61.6f));
            EnsureWall("Wall_West", new Vector3(30.4f, 3f, 0f), new Vector3(0.8f, 6f, 61.6f));
        }

        private static void SetupStreetlights(GameObject envInstance)
        {
            var lightsHolder = envInstance.transform.Find("Streetlights_Active");
            if (lightsHolder == null)
            {
                var go = new GameObject("Streetlights_Active");
                go.transform.SetParent(envInstance.transform, false);
                lightsHolder = go.transform;
            }

            Vector3[] streetlightPositions = new Vector3[]
            {
                new Vector3(-26f, 5.8f, -26f),
                new Vector3(26f, 5.8f, -26f),
                new Vector3(-26f, 5.8f, 26f),
                new Vector3(26f, 5.8f, 26f)
            };

            string[] names = { "Light_NW", "Light_NE", "Light_SW", "Light_SE" };

            Color warmSodium = new Color(1.0f, 0.86f, 0.65f, 1f);

            for (int i = 0; i < names.Length; i++)
            {
                var lightTr = lightsHolder.Find(names[i]);
                if (lightTr == null)
                {
                    var lGo = new GameObject(names[i]);
                    lGo.transform.SetParent(lightsHolder, false);
                    lightTr = lGo.transform;
                }

                lightTr.localPosition = streetlightPositions[i];

                var l = lightTr.GetComponent<Light>();
                if (l == null) l = lightTr.gameObject.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = warmSodium;
                l.intensity = 18f;
                l.range = 22f;
                l.shadows = LightShadows.Soft;
            }
        }

        private static void UpdateBallSpawner(int arenaLayer)
        {
            var spawner = UnityEngine.Object.FindAnyObjectByType<BallSpawner>();
            if (spawner != null)
            {
                var so = new SerializedObject(spawner);
                var prop = so.FindProperty("perimeterOffset");
                if (prop != null)
                {
                    prop.floatValue = 28.0f;
                    so.ApplyModifiedProperties();
                }
                spawner.PerimeterOffset = 28.0f;
                EditorUtility.SetDirty(spawner);
                Debug.Log("[ArenaMapIntegrator] BallSpawner perimeterOffset updated to 28.0f");
            }
        }

        private static void UpdateDimensionVisuals(GameObject arenaRoot)
        {
            var dv = arenaRoot.GetComponent<DimensionVisuals>();
            if (dv == null) dv = arenaRoot.AddComponent<DimensionVisuals>();

            var floorRenderers = new List<Renderer>();
            var wallRenderers = new List<Renderer>();

            var allRenderers = arenaRoot.GetComponentsInChildren<Renderer>(true);
            foreach (var r in allRenderers)
            {
                string n = r.gameObject.name;
                if (!r.enabled) continue;

                if (n.Contains("Ground") || n.Contains("Road") || n.Contains("Sidewalk") || n.Contains("Curb"))
                {
                    floorRenderers.Add(r);
                }
                else if (n.Contains("ConcreteBase") || n.Contains("UpperFencing") || n.Contains("Stripe") || n.Contains("Wall"))
                {
                    wallRenderers.Add(r);
                }
            }

            var so = new SerializedObject(dv);
            if (floorRenderers.Count > 0)
            {
                so.FindProperty("arenaFloorRenderer").objectReferenceValue = floorRenderers[0];
            }

            var wallArrayProp = so.FindProperty("arenaWallRenderers");
            if (wallArrayProp != null)
            {
                wallArrayProp.arraySize = wallRenderers.Count;
                for (int i = 0; i < wallRenderers.Count; i++)
                {
                    wallArrayProp.GetArrayElementAtIndex(i).objectReferenceValue = wallRenderers[i];
                }
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(dv);
            Debug.Log($"[ArenaMapIntegrator] DimensionVisuals linked with {floorRenderers.Count} floor parts and {wallRenderers.Count} wall parts.");
        }

        private static void UpdateCameraFraming()
        {
            var cmCam = GameObject.Find("CinemachineCamera");
            if (cmCam != null)
            {
                var follow = cmCam.GetComponent<Unity.Cinemachine.CinemachineFollow>();
                if (follow != null)
                {
                    follow.FollowOffset = new Vector3(0f, 22f, -19f);
                    EditorUtility.SetDirty(cmCam);
                }
            }
        }

        private static void UpdatePlayerStart()
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                player.transform.position = new Vector3(0f, 0.1f, -5f);
                player.transform.rotation = Quaternion.identity;
                EditorUtility.SetDirty(player);
            }
        }
    }
}
