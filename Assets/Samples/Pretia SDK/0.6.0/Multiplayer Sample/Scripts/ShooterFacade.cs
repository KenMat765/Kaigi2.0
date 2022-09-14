using UnityEngine;

namespace PretiaArCloud.Samples.ShooterSample
{
    [RequireComponent(typeof(FollowObject))]
    [RequireComponent(typeof(AimTowards))]
    [RequireComponent(typeof(NetworkJump))]
    [RequireComponent(typeof(ShooterColor))]
    [RequireComponent(typeof(ShooterAnimatorController))]
    public class ShooterFacade : MonoBehaviour
    {
        public FollowObject FollowObject { get; private set; }
        public AimTowards AimTowards { get; private set; }
        public NetworkJump NetworkJump { get; private set; }
        public ShooterColor ShooterColor { get; private set; }
        public ShooterAnimatorController AnimController { get; private set; }

        private void Awake()
        {
            FollowObject = GetComponent<FollowObject>();
            AimTowards = GetComponent<AimTowards>();
            NetworkJump = GetComponent<NetworkJump>();
            ShooterColor = GetComponent<ShooterColor>();
            AnimController = GetComponent<ShooterAnimatorController>();
        }
    }
}