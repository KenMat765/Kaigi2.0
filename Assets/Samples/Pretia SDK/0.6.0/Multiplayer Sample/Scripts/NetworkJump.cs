using PretiaArCloud.Networking;
using UnityEngine;

namespace PretiaArCloud.Samples.ShooterSample
{
    public class NetworkJump : NetworkBehaviour
    {
        protected override bool NetSyncV2 => true;
        protected override bool ClientAuthority => true;
        protected override SynchronizationTarget SyncTarget => SynchronizationTarget.HostOnly;

        private Jump _jump;
        private Rigidbody _rigidbody;
        private NetworkVariable<bool> _jumpTrigger = new NetworkVariable<bool>(forceSend: true);

        private void Awake()
        {
            _jump = GetComponent<Jump>();
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.maxAngularVelocity = 0f;
        }

        protected override void NetworkStart()
        {
            _gameSession.OnHostAppointment += DisableKinematic;

            if(!_gameSession.LocalPlayer.IsHost)
            {
                _rigidbody.isKinematic = true;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (_gameSession != null)
            {
                _gameSession.OnHostAppointment -= DisableKinematic;
            }
        }

        private void DisableKinematic()
        {
            _rigidbody.isKinematic = false;
        }

        public void TriggerJump()
        {
            if (_gameSession.LocalPlayer.IsHost)
            {
                _jump.JumpIfGrounded();
            }
            else
            {
                _jumpTrigger.Value = true;
            }
        }

        protected override void SerializeNetworkVars(ref NetworkVariableWriter writer)
        {
            writer.Write(_jumpTrigger);
            _jumpTrigger.Value = false;
        }

        protected override void DeserializeNetworkVars(ref NetworkVariableReader reader)
        {
            reader.Read(_jumpTrigger);
        }

        protected override void ApplySyncUpdate(int tick)
        {
            if (_jumpTrigger.Value)
            {
                _jumpTrigger.Value = false;
                _jump.JumpIfGrounded();
            }
        }
    }
}
