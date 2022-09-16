using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PretiaArCloud;
using PretiaArCloud.Networking;
using System;

public class BubbleManager : MonoBehaviour
{
    IGameSession gameSession;
    public static Player Host { get; private set; }
    [SerializeField] NetworkCameraManager cameraManager;
    [SerializeField] ARSharedAnchorManager relocManager;

    [SerializeField] GameObject subtitlePrefab;
    [SerializeField] Transform subtitleParent;

    async void OnEnable()
    {
        gameSession = await NetworkManager.Instance.GetLatestSessionAsync();
        gameSession.OnLocalPlayerJoined += OnSelfJoined;
        gameSession.OnPlayerJoined += OnPlayerJoined;
        gameSession.OnHostAppointment += OnHostAppointment;
        gameSession.OnDisconnected += OnDisconnected;

        // メッセージ。
        gameSession.HostMsg.Register<NetworkHostNumberMsg>(OnReceivedHostNumberMsg);
    }

    void Start()
    {
        // リローカライゼーションが完了したら接続。
        RelocManager.I.onRelocalized += Connect;
    }

    async void Connect()
    {
        // Join Session.
        var gameSession = await NetworkManager.Instance.GetLatestSessionAsync();
        // NetworkCameraManagerのOnEnableで、OnInstantiatedにProxyを追加する処理が入るが、それよりも後にOnInstantiatedに処理を追加したい。
        gameSession.NetworkSpawner.OnInstantiated += OnInstantiated;
        await gameSession.ConnectSessionAsync();
    }

    void OnDisable()
    {
        if (gameSession != null)
        {
            if (!gameSession.Disposed) gameSession.Dispose();
            gameSession.OnLocalPlayerJoined -= OnSelfJoined;
            gameSession.OnPlayerJoined -= OnPlayerJoined;
            gameSession.OnHostAppointment -= OnHostAppointment;
            gameSession.OnDisconnected -= OnDisconnected;

            // メッセージ。
            gameSession.HostMsg.Unregister<NetworkHostNumberMsg>(OnReceivedHostNumberMsg);
        }
    }

    void OnDestroy()
    {
        if (gameSession != null)
        {
            // Connect内で登録されたメソッドも忘れずに消去。
            gameSession.NetworkSpawner.OnInstantiated -= OnInstantiated;
        }
        RelocManager.I.onRelocalized -= Connect;
    }

    // 自分が入室した時。
    void OnSelfJoined()
    {
        MultiDebugger.Log($"MyNumber : {gameSession.LocalPlayer.UserNumber.ToString()}");
    }

    // 自分も含めた誰かが入室した時。
    // 後に入った人には呼ばれない。
    void OnPlayerJoined(Player player)
    {
        // 入室した当人は実行しない。
        if (player == gameSession.LocalPlayer) return;

        MultiDebugger.Log($"UserNumber : {player.UserNumber.ToString()}");

        // ここから先はホストのみが実行。
        if (!gameSession.LocalPlayer.IsHost) return;

        // ホストから全プレイヤーに向けて、自分のUserNumberを送る。
        // これにより、クライアントは誰がホストなのかすぐにわかるようになる。
        uint host_number = gameSession.LocalPlayer.UserNumber;
        gameSession.HostMsg.Send(new NetworkHostNumberMsg { hostNumber = host_number });
    }

    void OnInstantiated(NetworkIdentity networkIdentity)
    {
        Player player = networkIdentity.Owner;

        // スポーンされたProxyが自分のものだったら何もしない（自分に字幕は付けない）
        if (player == gameSession.LocalPlayer) return;

        // 入室者のProxyが得られなかった場合は何もしない。(基本的には得られるはず)
        Transform player_proxy;
        if (!cameraManager.TryGetProxyByOwner(player, out player_proxy)) return;

        // スポーンされたのがCameraProxyだった場合。(= Joinしたタイミング)
        NetworkCameraProxy cameraProxy;
        if (networkIdentity.TryGetComponent<NetworkCameraProxy>(out cameraProxy))
        {
            // 字幕はスポーンする(皆んなで共有する)必要はない。
            // 参加者が各々Localで他の参加者の位置に字幕を表示させれば良い。
            GameObject subtitle_object = Instantiate(subtitlePrefab, player_proxy.position, Quaternion.identity, subtitleParent);
            subtitle_object.GetComponent<FollowProxy>().targetProxy = player_proxy;
            subtitle_object.GetComponent<Subtitle>().tmp.text = player.UserNumber.ToString();
        }
    }

    void OnHostAppointment()
    {
        Host = gameSession.LocalPlayer;
    }

    // 
    //
    // 
    // !! 問題あり : ホストが抜けた時、新しいホストが更新されない !! 
    void OnDisconnected(Player player)
    {
        // // ホストが抜けたときに以下を実行。
        // if (!player.IsHost) return;

        MultiDebugger.Log($"{player.UserNumber} Disconnected.");

        // 自分が新しいホストなら以下を実行。
        if (!gameSession.LocalPlayer.IsHost) return;

        // 新しいホストが他の全プレイヤーに向けて自分のUserNumberを送る。
        uint host_number = gameSession.LocalPlayer.UserNumber;
        gameSession.HostMsg.Send(new NetworkHostNumberMsg { hostNumber = host_number });

        MultiDebugger.Log($"New host is UserNumber = {host_number}");
    }



    // メッセージ。
    void OnReceivedHostNumberMsg(NetworkHostNumberMsg msg)
    {
        uint host_number = msg.hostNumber;
        Player host = gameSession.Players[host_number];

        if (Host != null && Host == host) return;

        Host = host;
    }
}
