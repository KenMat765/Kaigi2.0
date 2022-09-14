using UnityEngine;

namespace PretiaArCloud.Samples.ShooterSample
{
    [RequireComponent(typeof(GroundCheck))]
    public class FollowObject : MonoBehaviour
    {
        public Transform Target;
        private GroundCheck _groudCheck;

        [SerializeField]
        private float _speed = .5f;

        [SerializeField]
        private float _distanceThreshold = .5f;

        public bool IsMoving { get; private set; }

        private void Awake()
        {
            _groudCheck = GetComponent<GroundCheck>();
        }

        private void FixedUpdate()
        {
            if (Target != null && _groudCheck.IsGrounded)
            {
                var targetPos = (Target.position + Target.forward * 2f);
                targetPos.y = transform.position.y;
                IsMoving = Vector3.Distance(targetPos, transform.position) > _distanceThreshold;
                if (IsMoving)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, _speed * Time.fixedDeltaTime);
                }
            }
        }
    }
}