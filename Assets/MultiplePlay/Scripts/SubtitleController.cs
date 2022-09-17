using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PretiaArCloud.Networking;

public class SubtitleController : Singleton<SubtitleController>
{
    protected override bool dont_destroy_on_load { get; set; } = false;

    [SerializeField] GameObject subtitlePrefab;
    [SerializeField] Transform subtitleParent;

    // UserNumber と Subtitle の対応関係。
    public Dictionary<uint, Subtitle> subtitleMap { get; private set; } = new Dictionary<uint, Subtitle>();

    IGameSession gameSession;

    async void OnEnable()
    {
        gameSession = await NetworkManager.Instance.GetLatestSessionAsync();
        gameSession.OnDisconnected += DeleteSubtitle;
    }

    void OnDisable()
    {
        if (gameSession != null)
        {
            if (!gameSession.Disposed) gameSession.Dispose();
            gameSession.OnDisconnected -= DeleteSubtitle;
        }
    }

    async void Start()
    {
        // NetworkCameraManagerのOnEnableで、OnInstantiatedにProxyを追加する処理が入るが、それよりも後にOnInstantiatedに処理を追加したい。
        var gameSession = await NetworkManager.Instance.GetLatestSessionAsync();
        gameSession.NetworkSpawner.OnInstantiated += GenerateSubtitle;
    }

    void OnDestroy()
    {
        gameSession.NetworkSpawner.OnInstantiated -= GenerateSubtitle;
    }

    void GenerateSubtitle(NetworkIdentity networkIdentity)
    {
        Player player = networkIdentity.Owner;

        // スポーンされたProxyが自分のものだったら何もしない（自分に字幕は付けない）
        if (player == gameSession.LocalPlayer) return;

        // 入室者のProxyが得られなかった場合は何もしない。(基本的には得られるはず)
        Transform player_proxy;
        if (!NetworkCameraManager.Instance.TryGetProxyByOwner(player, out player_proxy)) return;

        // スポーンされたのがCameraProxyだった場合。(= Joinしたタイミング)
        NetworkCameraProxy cameraProxy;
        if (networkIdentity.TryGetComponent<NetworkCameraProxy>(out cameraProxy))
        {
            // 字幕はスポーンする(皆んなで共有する)必要はない。
            // 参加者が各々Localで他の参加者の位置に字幕を表示させれば良い。
            GameObject subtitle_object = Instantiate(subtitlePrefab, player_proxy.position, Quaternion.identity, subtitleParent);
            subtitle_object.GetComponent<FollowProxy>().targetProxy = player_proxy;
            Subtitle subtitle = subtitle_object.GetComponent<Subtitle>();

            // 
            // 
            // 
            subtitle.tmp.text = player.UserNumber.ToString();

            subtitleMap.Add(player.UserNumber, subtitle);
        }
    }

    void DeleteSubtitle(Player player)
    {
        uint user_number = player.UserNumber;
        if (subtitleMap.ContainsKey(user_number))
        {
            Subtitle subtitle = subtitleMap[user_number];
            subtitleMap.Remove(user_number);
            Destroy(subtitle.gameObject);
        }
    }
}
