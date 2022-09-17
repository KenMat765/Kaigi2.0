using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PretiaArCloud.Networking;

public class ConnectionController : MonoBehaviour
{
    IGameSession gameSession;
    public static Player Host { get; private set; }

    async void OnEnable()
    {
        gameSession = await NetworkManager.Instance.GetLatestSessionAsync();

        // コールバック。
        gameSession.OnPlayerJoined += OnPlayerJoined;
        gameSession.OnHostAppointment += OnHostAppointment;
        gameSession.OnDisconnected += OnDisconnected;

        // メッセージ。
        gameSession.HostMsg.Register<NetworkHostNumberMsg>(OnReceivedHostNumberMsg);
    }

    void OnDisable()
    {
        if (gameSession != null)
        {
            if (!gameSession.Disposed) gameSession.Dispose();

            // コールバック。
            gameSession.OnPlayerJoined -= OnPlayerJoined;
            gameSession.OnHostAppointment -= OnHostAppointment;
            gameSession.OnDisconnected -= OnDisconnected;

            // メッセージ。
            gameSession.HostMsg.Unregister<NetworkHostNumberMsg>(OnReceivedHostNumberMsg);
        }
    }

    void Start()
    {
        // リローカライゼーションが完了したら接続。
        RelocManager.I.onRelocalized += Connect;
    }

    async void Connect()
    {
        // 接続。
        var gameSession = await NetworkManager.Instance.GetLatestSessionAsync();
        await gameSession.ConnectSessionAsync();
    }

    // 自分も含めた誰かが入室した時。
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

    // ホストの準備が完了したとき。
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
