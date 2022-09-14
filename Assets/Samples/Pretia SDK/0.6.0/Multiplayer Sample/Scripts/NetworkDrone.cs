using PretiaArCloud.Networking;
using UnityEngine;

namespace PretiaArCloud.Samples.ShooterSample
{
    public class NetworkDrone : NetworkBehaviour
    {
        protected override bool NetSyncV2 => true;

        [SerializeField] private Rigidbody _rigidbody;
        private NetworkVariable<Vector3> Pos;
        private NetworkVariable<float> RotY;

        [SerializeField] private Transform _barrel;
        private NetworkVariable<Quaternion> BarrelRot;

        private void Awake()
        {
            Pos = new NetworkVariable<Vector3>(_rigidbody.position);
            RotY = new NetworkVariable<float>(_rigidbody.rotation.eulerAngles.y);
            BarrelRot = new NetworkVariable<Quaternion>(_barrel.localRotation);
        }

        protected override void SyncUpdate(int tick)
        {
            Pos.Value = _rigidbody.position;
            RotY.Value = _rigidbody.rotation.eulerAngles.y;
            BarrelRot.Value = _barrel.localRotation;
        }

        protected override void SerializeNetworkVars(ref NetworkVariableWriter writer)
        {
            writer.Write(Pos);
            writer.Write(RotY);
            writer.Write(BarrelRot);
        }

        protected override void DeserializeNetworkVars(ref NetworkVariableReader reader)
        {
            reader.Read(Pos);
            reader.Read(RotY);
            reader.Read(BarrelRot);
        }

        protected override void ApplySyncUpdate(int tick)
        {
            _rigidbody.position = Pos.Value;
            _rigidbody.rotation = Quaternion.Euler(_rigidbody.rotation.eulerAngles.x, RotY.Value, _rigidbody.rotation.eulerAngles.z);
            _barrel.localRotation = BarrelRot.Value;
        }
    }
}