using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using PretiaArCloud.Networking;
using TextSpeech;

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



    // Top Menu.
    public enum TopMenu { NONE, SUBTITLE, GRAPH }
    TopMenu topMenu;
    public void ChangeTopMenu(int menu_number)
    {
        TopMenu old_menu = topMenu;
        TopMenu new_menu = (TopMenu)menu_number;
        topMenu = new_menu;
        OnMenuChanged(old_menu, new_menu);
    }
    void OnMenuChanged(TopMenu old_menu, TopMenu new_menu)
    {
        switch (old_menu)
        {
            case TopMenu.NONE:
                NetworkIconController.I.ShowTopMenu(false);
                break;
            case TopMenu.SUBTITLE:
                NetworkIconController.I.ShowSubtitleVoiceButton(false);
                NetworkIconController.I.ShowBackButton(false);
                break;
            case TopMenu.GRAPH:
                NetworkIconController.I.ShowGraphActionIcons(false);
                NetworkIconController.I.ShowBackButton(false);
                if (graphAction != GraphAction.NONE) ChangeGraphAction(0);
                break;
        }
        switch (new_menu)
        {
            case TopMenu.NONE:
                NetworkIconController.I.ShowTopMenu(true);
                break;
            case TopMenu.SUBTITLE:
                NetworkIconController.I.ShowSubtitleVoiceButton(true);
                NetworkIconController.I.ShowBackButton(true);
                break;
            case TopMenu.GRAPH:
                NetworkIconController.I.ShowGraphActionIcons(true);
                NetworkIconController.I.ShowBackButton(true);
                break;
        }
    }



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

                // ?????????????????????????????????????????????????????????????????????????????????????????????
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

                // ?????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????
                gameSession.PlayerMsg.Send(new NetworkHistoryEditMsg { enableEdit = false });

                break;
        }
    }

    public void OnPressedVoiceInputButton()
    {
        VoiceInputSwitch.I.SwitchAction(actionNumber);
        SpeechToText.Instance.StartRecording();
    }

    public void OnReleasedVoiceInputButton()
    {
        SpeechToText.Instance.StopRecording();
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

                // ?????????????????????
                NetworkSpawnNode(current_selected_bubble, current_focused_bubble);
            }
            else
            {
                // When Selected Bubble to Edit.
                current_focused_bubble.selected = true;
                current_selected_bubble = current_focused_bubble;

                // 
                // 
                // ???????????????????????????????????????????????????????????????
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

        // ?????????????????????????????????????????????????????????
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

                // ???????????????????????????????????????????????????????????????????????????????????????
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

                // ????????????????????????????????????????????????????????????????????????
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

                // ?????????????????????????????????????????????????????????????????????
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

        // ignoreRecord????????????????????????????????????????????????????????????????????????????????????????????????
        // ignoreRecord???RecordHistory??????true?????????????????????????????????????????????????????????
        if (!ignoreRecord) gameSession.PlayerMsg.Send(new NetworkRecordMsg { });

        // Record 2.
        RecordHistory();

        // ????????????????????????????????????????????????????????????
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

            // ???????????????????????????????????????????????????????????????????????????
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
            Debug.LogError("<color=green>???????????????????????????????????????!!</color>");
            return;
        }
        if (color_number >= colors.Count)
        {
            Debug.LogError("<color=green>colors.Count??????????????????????????????????????????!!</color>");
            return;
        }
        Color color = colors[color_number];
        current_selected_bubble.Color(color);
        NetworkIconController.I.ColorColoringButton(color);

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

    // !! ????????????????????????????????????????????????????????????(ignoreRecord????????????true??????????????????????????????) !!
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

        // ????????????????????????????????????????????????????????????????????????
        gameSession.PlayerMsg.Send(new NetworkPlaybackMsg { editingHistory = editing_history });
    }



    protected override void Awake()
    {
        base.Awake();

        bubblesParent = transform.Find("Bubbles");
        nodesParent = transform.Find("Nodes");
    }

    uint actionNumber;
    void Start()
    {
        actionNumber = VoiceInputSwitch.I.RegisterAction(NetworkSpawnBubble);
    }

    void Update()
    {
        if (raycasting) WhileRaycasting();
        if (editing) WhileEditing();

        // Move Requests.
        // ???????????????????????????????????????????????????????????????????????????????????????????????????????????????
        if (moveRequests.Count > 0)
        {
            foreach (BubbleMoveInfoPack info in moveRequests)
            {
                NetworkBubble bubble = info.bubble;
                Vector3 destination = info.proxy.position + info.proxy.forward * info.distance;
                MoveBubble(bubble, destination);
            }
        }

        // ?????????????????????????????????????????????
        // gameSession????????????????????????Update?????????????????????????????????gameSession???????????????????????????????????????????????????Null???????????????????????????
        if (gameSession == null || !gameSession.LocalPlayer.IsHost) return;
    }



    // Networking.
    IGameSession gameSession;

    [Header("Network")]
    [SerializeField] NetworkIdentity bubbleIdentity;
    [SerializeField] NetworkIdentity nodeIdentity;

    // ??????????????????????????????????????????????????????????????????
    int id_cache;
    string input_text_cache;

    // ??????????????????????????????????????????????????????????????????
    int start_id_cache;
    int end_id_cache;

    // ????????????????????? ??? ??????????????????????????????
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
        // ???????????????????????????????????????????????????
        gameSession.PlayerMsg.Send(new NetworkBubbleSpawnMsg { id = idCounter, inputText = input_text });

        // ??????????????????????????????????????????
        Transform device_trans = NetworkCameraManager.Instance.LocalProxy;
        gameSession.NetworkSpawner.Instantiate(bubbleIdentity, device_trans.position + device_trans.forward * offset, Quaternion.identity, ConnectionController.Host);
    }

    void NetworkSpawnNode(NetworkBubble start_bubble, NetworkBubble end_bubble)
    {
        // ???????????????????????????????????????????????????
        gameSession.PlayerMsg.Send(new NetworkNodeSpawnMsg { startId = start_bubble.id, endId = end_bubble.id });

        // ????????????Transform???????????????????????????????????????
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
            // OnInstantiated?????????????????????????????????????????????????????????????????????????????????????????????
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
        Debug.Log($"Id = {id} ????????????????????????????????????????????????");
        return null;
    }



    // ??????????????????
    // Player to Everybody.
    void OnRecievedMoveStartMsg(NetworkBubbleMoveStartMsg move_start_msg, Player sender)
    {
        // ????????????????????????????????????????????????????????????
        if (sender == gameSession.LocalPlayer) return;

        NetworkBubble bubble = GetBubbleById(move_start_msg.id);
        Transform proxy;
        if (!NetworkCameraManager.Instance.TryGetProxyByOwner(sender, out proxy))
        {
            Debug.Log($"UserNumber = {sender.UserNumber} ???????????????????????????????????????????????????????????????");
            return;
        }
        float distance = move_start_msg.distance;
        moveRequests.Add(new BubbleMoveInfoPack { bubble = bubble, proxy = proxy, distance = distance, });
    }

    // Player to Everybody.
    void OnRecievedMoveStopMsg(NetworkBubbleMoveStopMsg move_stop_msg, Player sender)
    {
        // ????????????????????????????????????????????????????????????
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
        else Debug.Log($"Id = {move_stop_msg.id} ???????????????MoveRequest??????????????????????????????");
    }

    // Player to Everybody.
    // !! PlayerMsg??? "???????????????????????????" ?????????????????????????????????!!
    void OnRecievedBubbleSpawnMsg(NetworkBubbleSpawnMsg spawn_msg, Player sender)
    {
        // ????????????????????????
        id_cache = spawn_msg.id;
        input_text_cache = spawn_msg.inputText;
    }

    // Player to Everybody.
    void OnRecievedColorMsg(NetworkBubbleColorMsg color_msg, Player sender)
    {
        // ??????????????????????????????????????????????????????????????????????????????
        if (sender == gameSession.LocalPlayer) return;

        NetworkBubble bubble = GetBubbleById(color_msg.id);
        Color color = colors[color_msg.colorNumber];
        bubble.Color(color);
    }

    // Player to Everybody.
    void OnRecievedDiscardMsg(NetworkBubbleDiscardMsg discard_msg, Player sender)
    {
        // ?????????????????????????????????????????????????????????????????????
        if (sender == gameSession.LocalPlayer) return;

        NetworkBubble bubble = GetBubbleById(discard_msg.id);
        bubble.Discard();
    }

    // Player to Everybody.
    void OnRecievedNodeSpawnMsg(NetworkNodeSpawnMsg spawn_msg, Player sender)
    {
        // ????????????????????????
        start_id_cache = spawn_msg.startId;
        end_id_cache = spawn_msg.endId;
    }

    // Player to Everybody.
    void OnRecievedRecordMsg(NetworkRecordMsg record_msg, Player sender)
    {
        // ???????????????????????????????????????????????????????????????????????????
        if (sender == gameSession.LocalPlayer) return;

        ignoreRecord = false;
        RecordHistory();
    }

    // Player to Everybody.
    void OnRecievedPlaybackMsg(NetworkPlaybackMsg playback_msg, Player sender)
    {
        // ?????????????????????????????????????????????????????????????????????
        if (sender == gameSession.LocalPlayer) return;

        PlayBackHistory(playback_msg.editingHistory);
    }

    // Player to Everybody.
    void OnRecievedHistoryEditMsg(NetworkHistoryEditMsg msg, Player sender)
    {
        // ?????????????????????????????????
        if (sender == gameSession.LocalPlayer) return;

        if (topMenu == TopMenu.GRAPH)
        {
            if (!msg.enableEdit && graphAction != GraphAction.NONE) ChangeGraphAction(0);
            NetworkIconController.I.ShowGraphActionIcons(msg.enableEdit);
        }
        else
        {
            NetworkIconController.I.EnableGraphActions(msg.enableEdit);
        }

    }

    // Player to Everybody.
    void OnRecievedSuspendHistoryEditMsg(NetworkSuspendHistoryEditMsg msg, Player sender)
    {
        // ?????????????????????????????????
        if (sender == gameSession.LocalPlayer) return;

        // ????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????
        if (msg.suspend && graphAction == GraphAction.HISTORY) ChangeEditMode(0);
        NetworkIconController.I.InteractHistoryButton(!msg.suspend);
    }
}