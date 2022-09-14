using UnityEngine;

namespace PretiaArCloud.Samples.ShooterSample
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(GroundCheck))]
    public class Jump : MonoBehaviour
    {
        [SerializeField]
        private Transform _forwardTransform;

        private const float JUMP_FORCE = 6f;

        private Rigidbody _rigidbody;
        private GroundCheck _groundCheck;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _groundCheck = GetComponent<GroundCheck>();
        }

        public void JumpIfGrounded()
        {
            if (_groundCheck.IsGrounded)
            {
                _rigidbody.AddForce((Vector3.up + _forwardTransform.forward).normalized * JUMP_FORCE, ForceMode.Impulse);
            }
        }
    }
}