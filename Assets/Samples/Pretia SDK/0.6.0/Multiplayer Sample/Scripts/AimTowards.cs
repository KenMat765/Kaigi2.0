using UnityEngine;

namespace PretiaArCloud.Samples.ShooterSample
{
    public class AimTowards : MonoBehaviour
    {
        [SerializeField]
        private Transform _barrel;

        [SerializeField]
        private float _maxAimDistance = 10f;

        private Vector3 _lookAtTarget;

        public Transform Target { get; set; }

        int layerMask;

        private void Awake()
        {
            layerMask = ~LayerMask.GetMask("Plane");
        }

        private void FixedUpdate()
        {
            if (Target == null) return;

            _lookAtTarget = Target.position + (Target.forward * _maxAimDistance);
            var hits = Physics.RaycastAll(Target.position, Target.forward, _maxAimDistance, layerMask);
            if (hits.Length > 0)
            {
                for (int i = 0; i < hits.Length; i++)
                {
                    var hit = hits[i];
                    if (hit.transform == transform)
                    {
                        continue;
                    }

                    _lookAtTarget = hit.point;
                    break;
                }
            }

            _barrel.LookAt(_lookAtTarget);
        }
    }
}