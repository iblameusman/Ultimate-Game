using UnityEngine;

namespace DodgeGame.Player
{
    public class MenuDodgerAnimator : MonoBehaviour
    {
        [Tooltip("Cached Animator reference.")]
        [SerializeField] private Animator animator;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (animator != null)
            {
                animator.applyRootMotion = false;
                animator.updateMode = AnimatorUpdateMode.Normal;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.SetFloat(SpeedHash, 0f);
                animator.SetBool(IsGroundedHash, true);
            }
        }
    }
}
