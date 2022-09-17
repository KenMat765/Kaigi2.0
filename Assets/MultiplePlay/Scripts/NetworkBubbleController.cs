using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using PretiaArCloud.Networking;

public class NetworkBubbleController : Singleton<NetworkBubbleController>
{
    protected override bool dont_destroy_on_load { get; set; } = false;

    // Prefabs.
    List<NetworkBubble> allBubbles = new List<NetworkBubble>();
    List<NetworkNode> allNodes = new List<NetworkNode>();
    public List<NetworkBubble> deletedBubblesCache { get; set; } = new List<NetworkBubble>();
    public List<NetworkNode> deletedNodesCache { get; set; } = new List<NetworkNode>();
    Transform bubblesParent, nodesParent;
    public List<Color> colors = new List<Color>();



    // Graph Actions.
    public enum GraphAction { NONE, GENERATE, SELECT, HISTORY }
    GraphAction graphAction;

    public void ChangeGraphAction(int action_number)
    {
        GraphAction old_action = graphAction;
        GraphAction new_action = (GraphAction)action_number;
        if (old_action == new_action) new_action = GraphAction.NONE;
        graphAction = new_action;
        OnGraphActionValueChanged(old_action, new_action);
    }

    void OnGraphActionValueChanged(GraphAction old_action, GraphAction new_action)
    {
        switch (old_action)
        {
            case GraphAction.NONE: break;
            case GraphAction.GENERATE: break;
            case GraphAction.SELECT: TargetController.I.DeactivateAllTargets(); break;
            case GraphAction.HISTORY:

                // 
                // 
                // 
                // ヒストリー編集を終えた時、他の参加者にバブルの編集を許可する。
                gameSession.PlayerMsg.Send(new NetworkHistoryEditMsg { enableEdit = true });

                break;
        }
        switch (new_action)
        {
            case GraphAction.NONE:
                NetworkIconController.I.ShowTriggerIcons(-1);
                NetworkIconController.I.MoveSelectRing(-1);
                break;
            case GraphAction.GENERATE:
                NetworkIconController.I.ShowTriggerIcons(0);
                NetworkIconController.I.MoveSelectRing(0);
                break;
            case GraphAction.SELECT:
                NetworkIconController.I.ShowTriggerIcons(1);
                NetworkIconController.I.MoveSelectRing(1);
                TargetController.I.ActivateTarget();
                break;
            case GraphAction.HISTORY:
                NetworkIconController.I.ShowTriggerIcons(2);
                NetworkIconController.I.MoveSelectRing(2);

                // 
                // 
                // 
                // ヒストリー編集モードに入ったとき、他の参加者がバブルの編集を行うのを禁止するメッセージを送る。
                gameSession.PlayerMsg.Send(new NetworkHistoryEditMsg { enableEdit = false });

                break;
        }
    }

    // public void WhilePressingVoiceInputButton()
    // {
    //     // Detect user's voice, and input as an argument of GenerateBubble() ...
    // }

    // 
    // 
    // 
    public void OnReleasedVoiceInputButton()
    {
        // GenerateBubble("Debug");
        NetworkSpawnBubble(gameSession.LocalPlayer.IsHost.ToString());
    }

    bool raycasting;
    public void StartRaycasting()
    {
        raycasting = true;
    }
    void WhileRaycasting() => FocusOnBubble();
    public void EndRaycasting()
    {
        raycasting = false;
        if (current_focused_bubble)
        {
            current_focused_bubble.focused = false;
            if (editing)
            {
                // When Selected Bubble to Connect Node.
                ignoreRecord = false;
                TargetController.I.DeactivateTarget(TargetController.I.current_focusing_target);
                TargetController.I.ActivateTarget();
                ZoomBubbleText(current_focused_bubble, false);

                // 
                // 
                // 
                // ノードを生成。
                NetworkSpawnNode(current_selected_bubble, current_focused_bubble);
            }
            else
            {
                // When Selected Bubble to Edit.
                current_focused_bubble.selected = true;
                current_selected_bubble = current_focused_bubble;

                // 
                // 
                // 複数選択モードでは処理を変える必要がある。
                NetworkIconController.I.ColorColoringButton(current_selected_bubble.bubbleColor);

                TargetController.I.SelectTarget(current_selected_bubble.gameObject);

                // Enter Edit Mode.
                EnterEditMode();
            }
            current_focused_bubble = null;
        }
    }



    // Edit Mode.
    bool editing = false;
    public enum BubbleEditMode { NONE, MOVE, CONNECT, DELETE, COLOR }
    BubbleEditMode editMode;
    void EnterEditMode()
    {
        editing = true;
        NetworkIconController.I.ShowGraphActionIcons(false);
        NetworkIconController.I.ShowEditMenuIcons(true);
        ChangeEditMode(0);

        // 
        // 
        // 
        // 他の参加者のヒストリー編集権限を奪う。
        gameSession.PlayerMsg.Send(new NetworkSuspendHistoryEditMsg { suspend = true });
    }
    public void ChangeEditMode(int mode_number)
    {
        BubbleEditMode old_mode = editMode;
        BubbleEditMode new_mode = (BubbleEditMode)mode_number;
        if (old_mode == new_mode) new_mode = BubbleEditMode.NONE;
        editMode = new_mode;
        OnEditModeValueChanged(old_mode, new_mode);
    }
    void OnEditModeValueChanged(BubbleEditMode old_mode, BubbleEditMode new_mode)
    {
        NetworkIconController.I.InteractRaycastButton(false);
        switch (old_mode)
        {
            case BubbleEditMode.NONE: break;
            case BubbleEditMode.MOVE:

                // 
                // 
                // 
                // 他の参加者にバブルの移動を終了するようにメッセージを送る。
                gameSession.PlayerMsg.Send(new NetworkBubbleMoveStopMsg { id = current_selected_bubble.id });

                break;
            case BubbleEditMode.CONNECT:
                TargetController.I.DeactivateTarget(TargetController.I.current_ready_target);
                TargetController.I.DeactivateTarget(TargetController.I.current_focusing_target);
                break;
            case BubbleEditMode.DELETE: break;
            case BubbleEditMode.COLOR: NetworkIconController.I.ShowColorPallete(false); break;
        }
        switch (new_mode)
        {
            case BubbleEditMode.NONE:
                NetworkIconController.I.MoveSelectRing(-1);
                break;
            case BubbleEditMode.MOVE:
                ignoreRecord = false;
                distanceFromDevice = Vector3.Distance(NetworkCameraManager.Instance.LocalProxy.position, current_selected_bubble.transform.position);
                NetworkIconController.I.MoveSelectRing(3);

                // 
                // 
                // 
                // 他の参加者にバブルを動かすようメッセージを送る。
                gameSession.PlayerMsg.Send(new NetworkBubbleMoveStartMsg { id = current_selected_bubble.id, distance = distanceFromDevice });

                break;
            case BubbleEditMode.CONNECT:
                NetworkIconController.I.InteractRaycastButton(true);
                NetworkIconController.I.MoveSelectRing(4);
                TargetController.I.ActivateTarget();
                break;
            case BubbleEditMode.DELETE:
                NetworkIconController.I.MoveSelectRing(6);
                DiscardBubble(current_selected_bubble);
                break;
            case BubbleEditMode.COLOR:
                ignoreRecord = false;
                NetworkIconController.I.ShowColorPallete(true);
                NetworkIconController.I.MoveSelectRing(5);
                break;
        }
    }
    void WhileEditing()
    {
        switch (editMode)
        {
            case BubbleEditMode.MOVE:

                // 
                // 
                // 
                // ローカルで自分の選択したバブルを自分で動かす。
                Transform device_trans = NetworkCameraManager.Instance.LocalProxy;
                Vector3 destination = device_trans.position + device_trans.forward * distanceFromDevice;
                MoveBubble(current_selected_bubble, destination);

                break;
            case BubbleEditMode.CONNECT: break;
            case BubbleEditMode.DELETE: break;
        }
    }
    public void ExitEditMode()
    {
        editing = false;

        if (current_selected_bubble)
        {
            ZoomBubbleText(current_selected_bubble, false);
            current_selected_bubble.selected = false;
            current_selected_bubble = null;
        }
        distanceFromDevice = 0;

        graphAction = GraphAction.NONE;
        editMode = BubbleEditMode.NONE;

        NetworkIconController.I.ShowTriggerIcons(-1);
        NetworkIconController.I.InteractRaycastButton(true);
        NetworkIconController.I.ShowEditMenuIcons(false);
        NetworkIconController.I.ShowColorPallete(false);
        NetworkIconController.I.ShowGraphActionIcons(true);
        NetworkIconController.I.MoveSelectRing(-1);

        TargetController.I.DeactivateAllTargets();

        // 
        // 
        // 
        // ignoreRecordでないなら他の参加者に対してもレコードするようメッセージを送る。
        // ignoreRecordはRecordHistory内でtrueになってしまうので、その前に送信する。
        if (!ignoreRecord) gameSession.PlayerMsg.Send(new NetworkRecordMsg { });

        // Record 2.
        RecordHistory();

        // 
        // 
        // 
        // 他の参加者のヒストリー編集権限を与える。
        gameSession.PlayerMsg.Send(new NetworkSuspendHistoryEditMsg { suspend = false });
    }



    // Raycast.
    [Header("Raycast")]
    public float maxDistance;
    [SerializeField] LayerMask graphLayermask;



    // Bubble.
    [Header("Bubble")]
    public float offset = 0.5f;
    public float smoothing = 0.9f;
    public float zoomScale = 3;
    public float zoomOffset = 0.25f;
    public float zoomDuration = 0.2f;
    int idCounter = 0;
    NetworkBubble current_focused_bubble;
    NetworkBubble current_selected_bubble;
    float distanceFromDevice;



    // 
    // 
    // 
    void MoveBubble(NetworkBubble bubble, Vector3 destination)
    {
        Transform device_trans = NetworkCameraManager.Instance.LocalProxy;
        Vector3 current = bubble.transform.position;
        Vector3 next = current + (destination - current) * smoothing;
        bubble.transform.position = next;

        if (bubble.connectedNetworkNodes.Count > 0)
        {
            foreach (NetworkNode node in bubble.connectedNetworkNodes)
            {
                node.UpdatePosition(node.startNetworkBubble.transform.position, node.endNetworkBubble.transform.position);
            }
        }
    }

    void DiscardBubble(NetworkBubble bubble)
    {
        string caution = "Discard this bubble?";

        Action on_yes = () =>
        {
            bubble.Discard();
            ignoreRecord = false;
            ExitEditMode();

            // 
            // 
            // 
            // 他の参加者に同じバブルを消すようメッセージを送る。
            gameSession.PlayerMsg.Send(new NetworkBubbleDiscardMsg { id = bubble.id });
        };

        Action on_no = () =>
        {
            ChangeEditMode(0);
        };

        ConfirmBox.OpenConfirmBox(caution, on_yes, on_no);
    }



    void DeleteBubble(NetworkBubble bubble)
    {
        string caution = "Delete this bubble?/n(You can not revive this bubble.)";

        Action on_yes = () =>
        {
            bubble.Delete(false);
            ExitEditMode();
        };

        Action on_no = () =>
        {
            ChangeEditMode(0);
        };

        ConfirmBox.OpenConfirmBox(caution, on_yes, on_no);
    }

    void ReviveBubble(NetworkBubble bubble)
    {
        string caution = "Revive this bubble?";

        Action on_yes = () =>
        {
            bubble.Revive();
            ExitEditMode();
        };

        Action on_no = () =>
        {
            ChangeEditMode(0);
        };

        ConfirmBox.OpenConfirmBox(caution, on_yes, on_no);
    }

    void FocusOnBubble()
    {
        Transform device_trans = NetworkCameraManager.Instance.LocalProxy;
        RaycastHit hit_result;
        bool detected = Physics.Raycast(device_trans.position, device_trans.forward, out hit_result, maxDistance, graphLayermask);

        // Bubble Detected.
        if (detected)
        {
            NetworkBubble bubble = hit_result.transform.GetComponent<NetworkBubble>();

            // Focused on Same Bubble.
            if (current_focused_bubble == bubble) return;

            // While Editing Mode. (= Connect Mode.)
            if (editing)
            {
                // Your Self.
                if (current_selected_bubble == bubble) return;
                // Already Connected.
                if (current_selected_bubble.connectedBubbles.Contains(bubble)) return;
            }

            // Focused on New Bubble.
            if (current_focused_bubble)
            {
                ZoomBubbleText(current_focused_bubble, false);
                current_focused_bubble.focused = false;
            }
            bubble.focused = true;
            current_focused_bubble = bubble;
            TargetController.I.FocusTarget(bubble.gameObject);
            ZoomBubbleText(bubble, true);
        }

        // No Bubble Detected.
        else
        {
            if (current_focused_bubble)
            {
                ZoomBubbleText(current_focused_bubble, false);
                current_focused_bubble.focused = false;
                current_focused_bubble = null;
                TargetController.I.ReadyTarget();
            }
        }
    }

    // Called from color pallete buttons.
    public void ColorCurrentSelectedBubble(int color_number)
    {
        if (!current_selected_bubble)
        {
            Debug.LogError("<color=green>バブルが選択されていません!!</color>");
            return;
        }
        if (color_number >= colors.Count)
        {
            Debug.LogError("<color=green>colors.Countを超える数値が入力されました!!</color>");
            return;
        }
        Color color = colors[color_number];
        current_selected_bubble.Color(color);
        NetworkIconController.I.ColorColoringButton(color);

        // 
        // 
        // 
        gameSession.PlayerMsg.Send(new NetworkBubbleColorMsg { id = current_selected_bubble.id, colorNumber = color_number });
    }

    public void ZoomBubbleText(NetworkBubble bubble, bool zoom)
    {
        if (zoom)
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(bubble.canvasRect.DOScale(zoomScale, zoomDuration));
            seq.Join(bubble.canvasRect.DOLocalMoveY(zoomOffset, zoomDuration));
            seq.Play();
        }
        else
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(bubble.canvasRect.DOScale(1, zoomDuration));
            seq.Join(bubble.canvasRect.DOLocalMoveY(0, zoomDuration));
            seq.Play();
        }
    }



    // Node.
    void DiscardNode(Node node)
    {

    }



    // History.
    [Header("History")]
    public int maxHistoryCount = 10;

    // Editing History is a history which user is editing now, and not recorded yet.
    public int currentEditingHistory { get; private set; }
    public int headEditingHistory { get; private set; }
    public int tailEditingHistory { get { return headEditingHistory - maxHistoryCount + 1 < 0 ? 0 : headEditingHistory - maxHistoryCount + 1; } }

    // Turn this off before recording. Otherwise, recording will be ignored.
    bool ignoreRecord = true;

    // !! そのまま呼んでもレコードされないので注意(ignoreRecordを事前にtrueにしておく必要がある) !!
    void RecordHistory()
    {
        // This is necessary in order to prevent from recording unnecessary history.
        if (ignoreRecord) return;
        ignoreRecord = true;

        foreach (NetworkBubble bubble in allBubbles) bubble.Record(currentEditingHistory);
        foreach (NetworkNode node in allNodes) node.Record(currentEditingHistory);

        // Delete bubbles & nodes which are deleted for good from "all" list.
        allBubbles.RemoveAll(b => deletedBubblesCache.Contains(b));
        allNodes.RemoveAll(n => deletedNodesCache.Contains(n));
        deletedBubblesCache.Clear();
        deletedNodesCache.Clear();

        currentEditingHistory++;
        headEditingHistory = currentEditingHistory;

        // Enable history editing when history is recorded.
        NetworkIconController.I.InteractHistoryButton(true);
    }

    void PlayBackHistory(int editing_history)
    {
        foreach (NetworkBubble bubble in allBubbles) bubble.PlayBack(editing_history);
        foreach (NetworkNode node in allNodes) node.PlayBack(editing_history);
        currentEditingHistory = editing_history;
    }

    public void OnHistorySliderChanged()
    {
        int editing_history = tailEditingHistory + NetworkIconController.I.historySliderValue;
        editing_history = Mathf.Clamp(editing_history, Mathf.Max(tailEditingHistory, 0), headEditingHistory);

        if (editing_history == currentEditingHistory) return;

        PlayBackHistory(editing_history);

        // 
        // 
        // 
        // 他の参加者に対し歴史を戻すようメッセージを送る。
        gameSession.PlayerMsg.Send(new NetworkPlaybackMsg { editingHistory = editing_history });
    }



    protected override void Awake()
    {
        base.Awake();

        bubblesParent = transform.Find("Bubbles");
        nodesParent = transform.Find("Nodes");
    }

    void Update()
    {
        if (raycasting) WhileRaycasting();
        if (editing) WhileEditing();

        // Move Requests.
        // 各バブルの移動は各参加者がそれぞれのローカルで行う（カクカク動くのを防ぐ）
        if (moveRequests.Count > 0)
        {
            foreach (BubbleMoveInfoPack info in moveRequests)
            {
                NetworkBubble bubble = info.bubble;
                Vector3 destination = info.proxy.position + info.proxy.forward * info.distance;
                MoveBubble(bubble, destination);
            }
        }

        // ここから先はホストのみが実行。
        // gameSessionが生成される前にUpdateが少し呼ばれてしまう（gameSessionの生成には少し時間がかかる）ので、Nullチェックを入れる。
        if (gameSession == null || !gameSession.LocalPlayer.IsHost) return;
    }



    // 
    // 
    // 
    // Networking.
    IGameSession gameSession;

    [Header("Network")]
    [SerializeField] NetworkIdentity bubbleIdentity;
    [SerializeField] NetworkIdentity nodeIdentity;

    // バブルのスポーン時に共有されるバブルの情報。
    int id_cache;
    string input_text_cache;

    // ノードのスポーン時に共有されるノードの情報。
    int start_id_cache;
    int end_id_cache;

    // 編集するバブル ＋ 編集モードのセット。
    List<BubbleMoveInfoPack> moveRequests = new List<BubbleMoveInfoPack>();

    async void OnEnable()
    {
        gameSession = await NetworkManager.Instance.GetLatestSessionAsync();

        // Callbacks.
        gameSession.NetworkSpawner.OnInstantiated += OnInstantiated;

        // Bubble Messages.
        gameSession.PlayerMsg.Register<NetworkBubbleMoveStartMsg>(OnRecievedMoveStartMsg);
        gameSession.PlayerMsg.Register<NetworkBubbleMoveStopMsg>(OnRecievedMoveStopMsg);
        gameSession.PlayerMsg.Register<NetworkBubbleSpawnMsg>(OnRecievedBubbleSpawnMsg);
        gameSession.PlayerMsg.Register<NetworkBubbleColorMsg>(OnRecievedColorMsg);
        gameSession.PlayerMsg.Register<NetworkBubbleDiscardMsg>(OnRecievedDiscardMsg);

        // Node Messages.
        gameSession.PlayerMsg.Register<NetworkNodeSpawnMsg>(OnRecievedNodeSpawnMsg);

        // History Message.
        gameSession.PlayerMsg.Register<NetworkRecordMsg>(OnRecievedRecordMsg);
        gameSession.PlayerMsg.Register<NetworkPlaybackMsg>(OnRecievedPlaybackMsg);
        gameSession.PlayerMsg.Register<NetworkHistoryEditMsg>(OnRecievedHistoryEditMsg);
        gameSession.PlayerMsg.Register<NetworkSuspendHistoryEditMsg>(OnRecievedSuspendHistoryEditMsg);
    }

    void OnDisable()
    {
        if (gameSession != null)
        {
            if (!gameSession.Disposed) gameSession.Dispose();

            // Callbacks.
            gameSession.NetworkSpawner.OnInstantiated -= OnInstantiated;

            // Bubble Messages.
            gameSession.PlayerMsg.Unregister<NetworkBubbleMoveStartMsg>(OnRecievedMoveStartMsg);
            gameSession.PlayerMsg.Unregister<NetworkBubbleMoveStopMsg>(OnRecievedMoveStopMsg);
            gameSession.PlayerMsg.Unregister<NetworkBubbleSpawnMsg>(OnRecievedBubbleSpawnMsg);
            gameSession.PlayerMsg.Unregister<NetworkBubbleColorMsg>(OnRecievedColorMsg);
            gameSession.PlayerMsg.Unregister<NetworkBubbleDiscardMsg>(OnRecievedDiscardMsg);

            // Node Messages.
            gameSession.PlayerMsg.Unregister<NetworkNodeSpawnMsg>(OnRecievedNodeSpawnMsg);

            // History Messages.
            gameSession.PlayerMsg.Unregister<NetworkRecordMsg>(OnRecievedRecordMsg);
            gameSession.PlayerMsg.Unregister<NetworkPlaybackMsg>(OnRecievedPlaybackMsg);
            gameSession.PlayerMsg.Unregister<NetworkHistoryEditMsg>(OnRecievedHistoryEditMsg);
            gameSession.PlayerMsg.Unregister<NetworkSuspendHistoryEditMsg>(OnRecievedSuspendHistoryEditMsg);
        }
    }

    void NetworkSpawnBubble(string input_text)
    {
        // まずバブルの情報を全員に送信する。
        gameSession.PlayerMsg.Send(new NetworkBubbleSpawnMsg { id = idCounter, inputText = input_text });

        // ホストを親としてバブル生成。
        Transform device_trans = NetworkCameraManager.Instance.LocalProxy;
        gameSession.NetworkSpawner.Instantiate(bubbleIdentity, device_trans.position + device_trans.forward * offset, Quaternion.identity, ConnectionController.Host);
    }

    void NetworkSpawnNode(NetworkBubble start_bubble, NetworkBubble end_bubble)
    {
        // まずノードの情報を全員に送信する。
        gameSession.PlayerMsg.Send(new NetworkNodeSpawnMsg { startId = start_bubble.id, endId = end_bubble.id });

        // ノードのTransformとオーナーはどうでも良い。
        gameSession.NetworkSpawner.Instantiate(nodeIdentity, Vector3.zero, Quaternion.identity, ConnectionController.Host);
    }

    void OnInstantiated(NetworkIdentity networkIdentity)
    {
        NetworkBubble new_bubble;
        if (networkIdentity.TryGetComponent<NetworkBubble>(out new_bubble))
        {
            new_bubble.Generate(id_cache, input_text_cache, colors[0]);
            idCounter = id_cache + 1;
            allBubbles.Add(new_bubble);

            // Record 1.
            // OnInstantiatedは全参加者で呼ばれるので、レコードメッセージを送る必要はない。
            ignoreRecord = false;
            RecordHistory();
            return;
        }

        NetworkNode new_node;
        if (networkIdentity.TryGetComponent<NetworkNode>(out new_node))
        {
            NetworkBubble start_bubble = GetBubbleById(start_id_cache);
            NetworkBubble end_bubble = GetBubbleById(end_id_cache);
            new_node.Generate(start_bubble, end_bubble, line_width: 0.005f, draw_immediate: true);
            allNodes.Add(new_node);

            start_bubble.connectedNetworkNodes.Add(new_node);
            end_bubble.connectedNetworkNodes.Add(new_node);
            start_bubble.connectedBubbles.Add(end_bubble);
            end_bubble.connectedBubbles.Add(start_bubble);

            return;
        }
    }

    NetworkBubble GetBubbleById(int id)
    {
        foreach (NetworkBubble bubble in allBubbles)
        {
            if (bubble.id == id) return bubble;
        }
        Debug.Log($"Id = {id} のバブルは見つかりませんでした。");
        return null;
    }



    // メッセージ。
    // Player to Everybody.
    void OnRecievedMoveStartMsg(NetworkBubbleMoveStartMsg move_start_msg, Player sender)
    {
        // 自分からのメッセージの場合は何もしない。
        if (sender == gameSession.LocalPlayer) return;

        NetworkBubble bubble = GetBubbleById(move_start_msg.id);
        Transform proxy;
        if (!NetworkCameraManager.Instance.TryGetProxyByOwner(sender, out proxy))
        {
            Debug.Log($"UserNumber = {sender.UserNumber} に対応するプロキシが見つかりませんでした。");
            return;
        }
        float distance = move_start_msg.distance;
        moveRequests.Add(new BubbleMoveInfoPack { bubble = bubble, proxy = proxy, distance = distance, });
    }

    // Player to Everybody.
    void OnRecievedMoveStopMsg(NetworkBubbleMoveStopMsg move_stop_msg, Player sender)
    {
        // 自分からのメッセージの場合は何もしない。
        if (sender == gameSession.LocalPlayer) return;

        NetworkBubble bubble_to_remove = GetBubbleById(move_stop_msg.id);
        BubbleMoveInfoPack request_to_remove = null;
        foreach (BubbleMoveInfoPack request in moveRequests)
        {
            if (request.bubble == bubble_to_remove)
            {
                request_to_remove = request;
                break;
            }
        }
        if (request_to_remove != null) moveRequests.Remove(request_to_remove);
        else Debug.Log($"Id = {move_stop_msg.id} に対応するMoveRequestはありませんでした。");
    }

    // Player to Everybody.
    // !! PlayerMsgは "送信した人も含めて" 全員に対して送られる。!!
    void OnRecievedBubbleSpawnMsg(NetworkBubbleSpawnMsg spawn_msg, Player sender)
    {
        // 全員が受け取る。
        id_cache = spawn_msg.id;
        input_text_cache = spawn_msg.inputText;
    }

    // Player to Everybody.
    void OnRecievedColorMsg(NetworkBubbleColorMsg color_msg, Player sender)
    {
        // 送信者以外が実行する。（送信者は自分で色を変える。）
        if (sender == gameSession.LocalPlayer) return;

        NetworkBubble bubble = GetBubbleById(color_msg.id);
        Color color = colors[color_msg.colorNumber];
        bubble.Color(color);
    }

    // Player to Everybody.
    void OnRecievedDiscardMsg(NetworkBubbleDiscardMsg discard_msg, Player sender)
    {
        // 送信者以外が実行する。（送信者は自分で消す。）
        if (sender == gameSession.LocalPlayer) return;

        NetworkBubble bubble = GetBubbleById(discard_msg.id);
        bubble.Discard();
    }

    // Player to Everybody.
    void OnRecievedNodeSpawnMsg(NetworkNodeSpawnMsg spawn_msg, Player sender)
    {
        // 全員が受け取る。
        start_id_cache = spawn_msg.startId;
        end_id_cache = spawn_msg.endId;
    }

    // Player to Everybody.
    void OnRecievedRecordMsg(NetworkRecordMsg record_msg, Player sender)
    {
        // 送信者以外が実行する。（送信者は自分で記録する。）
        if (sender == gameSession.LocalPlayer) return;

        ignoreRecord = false;
        RecordHistory();
    }

    // Player to Everybody.
    void OnRecievedPlaybackMsg(NetworkPlaybackMsg playback_msg, Player sender)
    {
        // 送信者以外が実行する。（送信者は自分で戻す。）
        if (sender == gameSession.LocalPlayer) return;

        PlayBackHistory(playback_msg.editingHistory);
    }

    // Player to Everybody.
    void OnRecievedHistoryEditMsg(NetworkHistoryEditMsg msg, Player sender)
    {
        // 送信者以外が実行する。
        if (sender == gameSession.LocalPlayer) return;

        if (!msg.enableEdit && graphAction != GraphAction.NONE) ChangeGraphAction(0);
        NetworkIconController.I.ShowGraphActionIcons(msg.enableEdit);
    }

    // Player to Everybody.
    void OnRecievedSuspendHistoryEditMsg(NetworkSuspendHistoryEditMsg msg, Player sender)
    {
        // 送信者以外が実行する。
        if (sender == gameSession.LocalPlayer) return;

        // 先にヒストリー編集モードに入っていた人がいた場合。（同時押しなど、イレギュラーな場合。）
        if (msg.suspend && graphAction == GraphAction.HISTORY) ChangeEditMode(0);
        NetworkIconController.I.InteractHistoryButton(!msg.suspend);
    }
}