using UnityEngine;

namespace DodgeGame.Arena
{
    public class AutoRotate : MonoBehaviour
    {
        [Tooltip("Rotation speed in degrees per second.")]
        [SerializeField] public float rotationSpeed = 8f;

        [Tooltip("Rotation axis for spinning in place.")]
        [SerializeField] public Vector3 rotationAxis = Vector3.up;

        [Tooltip("Optional pivot center to orbit around.")]
        [SerializeField] public Vector3 pivotPoint = Vector3.zero;

        [Tooltip("Whether to orbit around the pivotPoint instead of rotating in place.")]
        [SerializeField] public bool orbitAroundPivot = true;

        private void Update()
        {
            if (orbitAroundPivot)
            {
                transform.RotateAround(pivotPoint, Vector3.up, rotationSpeed * Time.deltaTime);
            }
            else
            {
                Vector3 axis = rotationAxis.sqrMagnitude > 0.001f ? rotationAxis.normalized : Vector3.up;
                transform.Rotate(axis, rotationSpeed * Time.deltaTime, Space.World);
            }
        }
    }
}
