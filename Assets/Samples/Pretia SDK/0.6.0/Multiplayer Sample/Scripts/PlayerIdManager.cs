using System;
using MessagePack;
using PretiaArCloud.Networking;
using UnityEngine;

namespace PretiaArCloud.Samples.ShooterSample
{
    [NetworkMessage]
    [MessagePackObject]
    public class PlayerIdSnapshotMsg
    {
        [Key(0)]
        public uint?[] UserNumbers;
    }

    public class PlayerIdManager : MonoBehaviour, ISnapshot
    {
        private uint?[] _userNumbers;
        private int _nextUsedId = 0;

        public int Rent(uint userNumber)
        {
            int id = _nextUsedId;

            if (_userNumbers == null)
            {
                _userNumbers = new uint?[8];
            }

            _userNumbers[id] = userNumber;
            _nextUsedId = FindNextUsedId();

            return id;
        }

        public void Return(uint userNumber)
        {
            for (int i = 0; i < _userNumbers.Length; i++)
            {
                if (_userNumbers[i] == userNumber)
                {
                    _userNumbers[i] = null;

                    if (i < _nextUsedId)
                    {
                        _nextUsedId = i;
                    }
                    break;
                }
            }
        }

        public void EnqueueSnapshot(HostToPlayerMessageHandler hostToPlayerMsg)
        {
            hostToPlayerMsg.Enqueue(new PlayerIdSnapshotMsg { UserNumbers = _userNumbers });
        }

        public void RegisterSnapshotCallback(HostToPlayerMessageHandler hostToPlayerMsg)
        {
            hostToPlayerMsg.Register<PlayerIdSnapshotMsg>(SetupUserNumbers);
        }

        private void SetupUserNumbers(PlayerIdSnapshotMsg msg)
        {
            _userNumbers = msg.UserNumbers;
            _nextUsedId = FindNextUsedId();
        }

        private int FindNextUsedId()
        {
            int nextUsedId = _userNumbers.Length;
            for (int i = 0; i < _userNumbers.Length; i++)
            {
                if (_userNumbers[i] == null && nextUsedId > i)
                {
                    nextUsedId = i;
                    break;
                }
            }

            return nextUsedId;
        }

        public int GetIdByUserNumber(uint userNumber)
        {
            int id = Array.IndexOf(_userNumbers, userNumber);
            if (id == -1)
            {
                throw new Exception($"PlayerId for userNumber {userNumber} not found");
            }

            return id;
        }
    }
}