using UnityEngine;

namespace PretiaArCloud.Samples.ShooterSample
{
    public class GroundCheck : MonoBehaviour
    {
        [SerializeField]
        private BoxCollider _trigger;

        public bool IsGrounded { get; private set; }

        private void OnTriggerStay(Collider other)
        {
            IsGrounded = true;
        }

        private void OnTriggerExit(Collider other)
        {
            IsGrounded = false;
        }
    }
}