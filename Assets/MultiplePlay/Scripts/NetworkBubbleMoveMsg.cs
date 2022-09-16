using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MessagePack;
using PretiaArCloud.Networking;

[System.Serializable]
[NetworkMessage]
[MessagePackObject]
public class NetworkBubbleMoveStartMsg
{
    [Key(0)] public int id;
    [Key(1)] public float distance;
}

[System.Serializable]
[NetworkMessage]
[MessagePackObject]
public class NetworkBubbleMoveStopMsg
{
    [Key(0)] public int id;
}

[System.Serializable]
[NetworkMessage]
[MessagePackObject]
public class NetworkBubbleSpawnMsg
{
    [Key(0)] public int id;
    [Key(1)] public string inputText;
}

[System.Serializable]
[NetworkMessage]
[MessagePackObject]
public class NetworkBubbleColorMsg
{
    [Key(0)] public int id;
    [Key(1)] public int colorNumber;
}

[System.Serializable]
[NetworkMessage]
[MessagePackObject]
public class NetworkBubbleDiscardMsg
{
    [Key(0)] public int id;
}

[System.Serializable]
[NetworkMessage]
[MessagePackObject]
public class NetworkRecordMsg
{
}

[System.Serializable]
[NetworkMessage]
[MessagePackObject]
public class NetworkPlaybackMsg
{
    [Key(0)] public int editingHistory;
}

[System.Serializable]
[NetworkMessage]
[MessagePackObject]
public class NetworkHistoryEditMsg
{
    [Key(0)] public bool enableEdit;
}

[System.Serializable]
[NetworkMessage]
[MessagePackObject]
public class NetworkSuspendHistoryEditMsg
{
    [Key(0)] public bool suspend;
}

[System.Serializable]
[NetworkMessage]
[MessagePackObject]
public class NetworkNodeSpawnMsg
{
    [Key(0)] public int startId;
    [Key(1)] public int endId;
}

[System.Serializable]
[NetworkMessage]
[MessagePackObject]
public class NetworkHostNumberMsg
{
    [Key(0)] public uint hostNumber;
}

// ホストがMove依頼を受けるために必要な情報群。
// ホストがMove依頼を受け取ったタイミングで生成される。
public class BubbleMoveInfoPack
{
    public NetworkBubble bubble;
    public Transform proxy;
    public float distance;
}
