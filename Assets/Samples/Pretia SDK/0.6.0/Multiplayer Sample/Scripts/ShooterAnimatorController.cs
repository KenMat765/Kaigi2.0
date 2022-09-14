using UnityEngine;

namespace PretiaArCloud.Samples.ShooterSample
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(GroundCheck))]
    public class ShooterAnimatorController : MonoBehaviour
    {
        private const string IS_GROUNDED_PARAMETER = "IsGrounded";
        private const string SHOOT_TRIGGER = "Shoot";

        private Animator _animator;
        private GroundCheck _groundCheck;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _groundCheck = GetComponent<GroundCheck>();
        }

        private void FixedUpdate()
        {
            _animator.SetBool(IS_GROUNDED_PARAMETER, _groundCheck.IsGrounded);
        }

        public void ShootAnimation()
        {
            _animator.SetTrigger(SHOOT_TRIGGER);
        }
    }
}