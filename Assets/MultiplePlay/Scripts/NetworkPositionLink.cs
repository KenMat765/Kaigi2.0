using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PretiaArCloud.Networking;

// Ownerが動かした時のみシンクロされる。
// Ownerでない人が動かす場合は、Ownerに対してMessageを送り、Ownerに動かしてもらう必要がある。
public class NetworkPositionLink : NetworkBehaviour
{
    protected override bool NetSyncV2 => true;
    protected override bool ClientAuthority => true;

    NetworkVariable<Vector3> pos;

    void Awake()
    {
        pos = new NetworkVariable<Vector3>(transform.position);
    }

    protected override void SyncUpdate(int tick)
    {
        pos.Value = transform.position;
    }

    protected override void SerializeNetworkVars(ref NetworkVariableWriter writer)
    {
        writer.Write(pos);
    }

    protected override void DeserializeNetworkVars(ref NetworkVariableReader reader)
    {
        reader.Read(pos);
    }

    protected override void ApplySyncUpdate(int tick)
    {
        transform.position = pos.Value;
    }
}
