using UnityEngine;

namespace DodgeGame.Player
{
    public class PlayerAnimator : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The Animator component on the Dodger character model.")]
        [SerializeField] private Animator animator;

        [Tooltip("Root PlayerController component.")]
        [SerializeField] private PlayerController playerController;

        [Tooltip("DimensionShifter ability controller.")]
        [SerializeField] private DimensionShifter dimensionShifter;

        [Tooltip("DashController ability controller.")]
        [SerializeField] private DashController dashController;

        [Header("Smoothing")]
        [Tooltip("Time window in seconds to smooth speed transitions.")]
        [SerializeField] private float speedSmoothTime = 0.1f;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int IsPhasedHash = Animator.StringToHash("IsPhased");
        private static readonly int IsDashingHash = Animator.StringToHash("IsDashing");

        private float currentSpeed;
        private float speedVelocity;

        public float CurrentSpeed => currentSpeed;
        public Animator Animator => animator;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (playerController == null)
            {
                playerController = GetComponentInParent<PlayerController>();
                if (playerController == null)
                {
                    playerController = GetComponent<PlayerController>();
                }
            }

            if (dimensionShifter == null)
            {
                dimensionShifter = GetComponentInParent<DimensionShifter>();
                if (dimensionShifter == null)
                {
                    dimensionShifter = GetComponent<DimensionShifter>();
                }
            }

            if (dashController == null)
            {
                dashController = GetComponentInParent<DashController>();
                if (dashController == null)
                {
                    dashController = GetComponent<DashController>();
                }
            }

            if (animator != null)
            {
                animator.applyRootMotion = false;
                animator.updateMode = AnimatorUpdateMode.Normal;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }
        }

        private void Update()
        {
            if (animator == null) return;

            float targetSpeed = 0f;
            bool isGrounded = true;

            if (playerController != null)
            {
                Vector3 vel = playerController.Velocity;
                targetSpeed = Mathf.Sqrt(vel.x * vel.x + vel.z * vel.z);
                isGrounded = playerController.IsGrounded;
            }

            float dt = Time.unscaledDeltaTime;
            if (dt > 0f)
            {
                currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedVelocity, speedSmoothTime, Mathf.Infinity, dt);
            }
            else
            {
                currentSpeed = targetSpeed;
            }

            animator.SetFloat(SpeedHash, currentSpeed);
            animator.SetBool(IsGroundedHash, isGrounded);

            bool isPhased = false;
            if (dimensionShifter != null)
            {
                isPhased = dimensionShifter.IsShifted;
            }

            bool isDashing = false;
            if (dashController != null)
            {
                isDashing = dashController.IsDashing;
            }

            animator.SetBool(IsPhasedHash, isPhased);
            animator.SetBool(IsDashingHash, isDashing);
        }
    }
}
