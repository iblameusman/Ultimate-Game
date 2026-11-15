using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using DodgeGame.Player;

namespace DodgeGame.Tests.EditMode
{
    [TestFixture]
    public class DodgerAnimationEditModeTests
    {
        private const string DodgerModelPath = "Assets/Art/Characters/Dodger/Dodger.fbx";
        private const string IdleAnimPath = "Assets/Art/Characters/Dodger/Animations/Idle.fbx";
        private const string RunAnimPath = "Assets/Art/Characters/Dodger/Animations/Running.fbx";
        private const string ControllerPath = "Assets/Art/Characters/Dodger/Animation/Dodger.controller";
        private const string PlayerPrefabPath = "Assets/Prefabs/Prefab_Player.prefab";

        [Test]
        public void DodgerAvatar_IsValid_AndIsHumanoid()
        {
            var avatar = AssetDatabase.LoadAssetAtPath<Avatar>(DodgerModelPath);
            Assert.IsNotNull(avatar, "DodgerAvatar must exist on Dodger.fbx");
            Assert.IsTrue(avatar.isValid, "DodgerAvatar must be valid");
            Assert.IsTrue(avatar.isHuman, "DodgerAvatar must be humanoid");
        }

        [Test]
        public void AnimationFiles_ReferenceDodgerAvatar_WithCopyFromOtherAvatar()
        {
            var idleImporter = AssetImporter.GetAtPath(IdleAnimPath) as ModelImporter;
            var runImporter = AssetImporter.GetAtPath(RunAnimPath) as ModelImporter;

            Assert.IsNotNull(idleImporter, "Idle.fbx ModelImporter must exist");
            Assert.IsNotNull(runImporter, "Running.fbx ModelImporter must exist");

            Assert.AreEqual(ModelImporterAnimationType.Human, idleImporter.animationType, "Idle.fbx must be Humanoid");
            Assert.AreEqual(ModelImporterAnimationType.Human, runImporter.animationType, "Running.fbx must be Humanoid");

            Assert.AreEqual(ModelImporterAvatarSetup.CopyFromOther, idleImporter.avatarSetup, "Idle.fbx must Copy From Other Avatar");
            Assert.AreEqual(ModelImporterAvatarSetup.CopyFromOther, runImporter.avatarSetup, "Running.fbx must Copy From Other Avatar");

            var expectedAvatar = AssetDatabase.LoadAssetAtPath<Avatar>(DodgerModelPath);
            Assert.AreEqual(expectedAvatar, idleImporter.sourceAvatar, "Idle.fbx must point to DodgerAvatar");
            Assert.AreEqual(expectedAvatar, runImporter.sourceAvatar, "Running.fbx must point to DodgerAvatar");
        }

        [Test]
        public void AnimationFiles_HaveBlendShapesDisabled_AndBakeEnabled()
        {
            var idleImporter = AssetImporter.GetAtPath(IdleAnimPath) as ModelImporter;
            var runImporter = AssetImporter.GetAtPath(RunAnimPath) as ModelImporter;

            Assert.IsFalse(idleImporter.importBlendShapes, "Idle.fbx must have importBlendShapes disabled");
            Assert.IsFalse(runImporter.importBlendShapes, "Running.fbx must have importBlendShapes disabled");

            Assert.AreEqual(ModelImporterAnimationCompression.Optimal, idleImporter.animationCompression, "Idle.fbx must use Optimal compression");
            Assert.AreEqual(ModelImporterAnimationCompression.Optimal, runImporter.animationCompression, "Running.fbx must use Optimal compression");
        }

        [Test]
        public void Dodger_Idle_ClipExists_AndHasLoopTimeEnabled()
        {
            var clips = AssetDatabase.LoadAllAssetsAtPath(IdleAnimPath).OfType<AnimationClip>().ToArray();
            var idleClip = clips.FirstOrDefault(c => c.name == "Dodger_Idle");
            Assert.IsNotNull(idleClip, "Dodger_Idle clip must exist in Idle.fbx");
            Assert.IsTrue(idleClip.isLooping, "Dodger_Idle must have Loop Time enabled");

            var idleImporter = AssetImporter.GetAtPath(IdleAnimPath) as ModelImporter;
            var clipAnim = idleImporter.clipAnimations.FirstOrDefault(c => c.name == "Dodger_Idle");
            Assert.IsNotNull(clipAnim, "Clip animation configuration for Dodger_Idle must exist");
            Assert.IsTrue(clipAnim.loopTime, "Dodger_Idle must have loopTime true");
            Assert.IsTrue(clipAnim.loopPose, "Dodger_Idle must have loopPose true");
            Assert.IsTrue(clipAnim.lockRootRotation, "Dodger_Idle root rotation must be baked into pose");
            Assert.IsTrue(clipAnim.lockRootHeightY, "Dodger_Idle root Y must be baked into pose");
            Assert.IsTrue(clipAnim.lockRootPositionXZ, "Dodger_Idle root XZ must be baked into pose");
        }

        [Test]
        public void Dodger_Run_ClipExists_AndHasLoopTimeEnabled()
        {
            var clips = AssetDatabase.LoadAllAssetsAtPath(RunAnimPath).OfType<AnimationClip>().ToArray();
            var runClip = clips.FirstOrDefault(c => c.name == "Dodger_Run");
            Assert.IsNotNull(runClip, "Dodger_Run clip must exist in Running.fbx");
            Assert.IsTrue(runClip.isLooping, "Dodger_Run must have Loop Time enabled");

            var runImporter = AssetImporter.GetAtPath(RunAnimPath) as ModelImporter;
            var clipAnim = runImporter.clipAnimations.FirstOrDefault(c => c.name == "Dodger_Run");
            Assert.IsNotNull(clipAnim, "Clip animation configuration for Dodger_Run must exist");
            Assert.IsTrue(clipAnim.loopTime, "Dodger_Run must have loopTime true");
            Assert.IsTrue(clipAnim.loopPose, "Dodger_Run must have loopPose true");
            Assert.IsTrue(clipAnim.lockRootRotation, "Dodger_Run root rotation must be baked into pose");
            Assert.IsTrue(clipAnim.lockRootHeightY, "Dodger_Run root Y must be baked into pose");
            Assert.IsTrue(clipAnim.lockRootPositionXZ, "Dodger_Run root XZ must be baked into pose");
        }

        [Test]
        public void AnimatorController_HasRequiredParameters_WithCorrectTypes()
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            Assert.IsNotNull(controller, "Dodger.controller must exist at " + ControllerPath);

            var speedParam = controller.parameters.FirstOrDefault(p => p.name == "Speed");
            Assert.IsNotNull(speedParam, "Speed parameter must exist");
            Assert.AreEqual(AnimatorControllerParameterType.Float, speedParam.type, "Speed must be Float");

            var isGroundedParam = controller.parameters.FirstOrDefault(p => p.name == "IsGrounded");
            Assert.IsNotNull(isGroundedParam, "IsGrounded parameter must exist");
            Assert.AreEqual(AnimatorControllerParameterType.Bool, isGroundedParam.type, "IsGrounded must be Bool");

            var isPhasedParam = controller.parameters.FirstOrDefault(p => p.name == "IsPhased");
            Assert.IsNotNull(isPhasedParam, "IsPhased parameter must exist");
            Assert.AreEqual(AnimatorControllerParameterType.Bool, isPhasedParam.type, "IsPhased must be Bool");

            var isDashingParam = controller.parameters.FirstOrDefault(p => p.name == "IsDashing");
            Assert.IsNotNull(isDashingParam, "IsDashing parameter must exist");
            Assert.AreEqual(AnimatorControllerParameterType.Bool, isDashingParam.type, "IsDashing must be Bool");
        }

        [Test]
        public void AnimatorController_LocomotionBlendTree_AndTransitionsConfigured()
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            Assert.IsNotNull(controller, "Dodger.controller must exist");

            var sm = controller.layers[0].stateMachine;
            var idleState = sm.states.FirstOrDefault(s => s.state.name == "Idle").state;
            var locoState = sm.states.FirstOrDefault(s => s.state.name == "Locomotion").state;

            Assert.IsNotNull(idleState, "Idle state must exist");
            Assert.IsNotNull(locoState, "Locomotion state must exist");

            var bt = locoState.motion as BlendTree;
            Assert.IsNotNull(bt, "Locomotion motion must be a Blend Tree");
            Assert.AreEqual("Speed", bt.blendParameter, "Blend Tree parameter must be Speed");
            Assert.AreEqual(BlendTreeType.Simple1D, bt.blendType, "Blend Tree must be 1D");
            Assert.IsTrue(bt.children.Length >= 2, "Blend tree must contain at least Idle and Run motions");

            Assert.IsTrue(locoState.speedParameterActive, "Locomotion speedParameterActive must be true");
            Assert.AreEqual("Speed", locoState.speedParameter, "Locomotion speedParameter must be Speed");

            var toLoco = idleState.transitions.FirstOrDefault(t => t.destinationState == locoState);
            Assert.IsNotNull(toLoco, "Transition Idle -> Locomotion must exist");
            Assert.AreEqual(0.15f, toLoco.duration, 0.001f, "Transition duration must be 0.15s");
            Assert.IsFalse(toLoco.hasExitTime, "Has Exit Time must be off");
            var toLocoCond = toLoco.conditions.FirstOrDefault(c => c.parameter == "Speed");
            Assert.IsNotNull(toLocoCond, "Condition on Speed must exist");
            Assert.AreEqual(AnimatorConditionMode.Greater, toLocoCond.mode);
            Assert.AreEqual(0.1f, toLocoCond.threshold, 0.001f);

            var toIdle = locoState.transitions.FirstOrDefault(t => t.destinationState == idleState);
            Assert.IsNotNull(toIdle, "Transition Locomotion -> Idle must exist");
            Assert.AreEqual(0.15f, toIdle.duration, 0.001f, "Transition duration must be 0.15s");
            Assert.IsFalse(toIdle.hasExitTime, "Has Exit Time must be off");
            var toIdleCond = toIdle.conditions.FirstOrDefault(c => c.parameter == "Speed");
            Assert.IsNotNull(toIdleCond, "Condition on Speed must exist");
            Assert.AreEqual(AnimatorConditionMode.Less, toIdleCond.mode);
            Assert.AreEqual(0.1f, toIdleCond.threshold, 0.001f);
        }

        [Test]
        public void PlayerPrefab_HasCorrectComponentsAndCollider()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            Assert.IsNotNull(prefab, "Player prefab must exist");

            var col = prefab.GetComponent<CapsuleCollider>();
            Assert.IsNotNull(col, "CapsuleCollider must exist on Player");
            Assert.AreEqual(0.5f, col.radius, 0.05f, "Collider radius must match Dodger (~0.5)");
            Assert.AreEqual(1.8f, col.height, 0.05f, "Collider height must match Dodger (1.8m)");
            Assert.AreEqual(LayerMask.NameToLayer("Player"), prefab.layer, "Player layer must be 'Player'");

            var anim = prefab.GetComponentInChildren<Animator>();
            Assert.IsNotNull(anim, "Animator must exist in Dodger model hierarchy");
            Assert.IsFalse(anim.applyRootMotion, "Apply Root Motion must be off");
            Assert.AreEqual(AnimatorUpdateMode.Normal, anim.updateMode, "Update Mode must be Normal");
            Assert.AreEqual(AnimatorCullingMode.AlwaysAnimate, anim.cullingMode, "Culling Mode must be Always Animate");

            var playerAnim = prefab.GetComponent<PlayerAnimator>();
            Assert.IsNotNull(playerAnim, "PlayerAnimator component must be present on Player");
        }
    }
}
