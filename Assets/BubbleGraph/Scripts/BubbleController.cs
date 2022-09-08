using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using DG.Tweening;

public class BubbleController : Singleton<BubbleController>
{
    protected override bool dont_destroy_on_load { get; set; } = false;



    // Prefabs.
    [Header("Prefabs")]
    [SerializeField] GameObject bubblePrefab;
    [SerializeField] GameObject nodePrefab;
    List<Bubble> allBubbles = new List<Bubble>();
    List<Node> allNodes = new List<Node>();
    public List<Bubble> deletedBubblesCache { get; set; } = new List<Bubble>();
    public List<Node> deletedNodesCache { get; set; } = new List<Node>();
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
            case GraphAction.HISTORY: break;
        }
        switch (new_action)
        {
            case GraphAction.NONE:
                IconController.I.ShowTriggerIcons(-1);
                IconController.I.MoveSelectRing(-1);
                break;
            case GraphAction.GENERATE:
                IconController.I.ShowTriggerIcons(0);
                IconController.I.MoveSelectRing(0);
                break;
            case GraphAction.SELECT:
                IconController.I.ShowTriggerIcons(1);
                IconController.I.MoveSelectRing(1);
                TargetController.I.ActivateTarget();
                break;
            case GraphAction.HISTORY:
                IconController.I.ShowTriggerIcons(2);
                IconController.I.MoveSelectRing(2);
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
    public void OnReleasedVoiceInputButton() => GenerateBubble("Debug");

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
                GenerateNode(current_selected_bubble, current_focused_bubble);
                TargetController.I.DeactivateTarget(TargetController.I.current_focusing_target);
                TargetController.I.ActivateTarget();
                ZoomBubbleText(current_focused_bubble, false);
            }
            else
            {
                // When Selected Bubble to Edit.
                current_focused_bubble.selected = true;
                current_selected_bubble = current_focused_bubble;

                // 
                // 
                // 複数選択モードでは処理を変える必要がある。
                IconController.I.ColorColoringButton(current_selected_bubble.bubbleColor);

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
        IconController.I.ShowGraphActionIcons(false);
        IconController.I.ShowEditMenuIcons(true);
        ChangeEditMode(0);
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
        IconController.I.InteractRaycastButton(false);
        switch (old_mode)
        {
            case BubbleEditMode.NONE: break;
            case BubbleEditMode.MOVE: break;
            case BubbleEditMode.CONNECT:
                TargetController.I.DeactivateTarget(TargetController.I.current_ready_target);
                TargetController.I.DeactivateTarget(TargetController.I.current_focusing_target);
                break;
            case BubbleEditMode.DELETE: break;
            case BubbleEditMode.COLOR: IconController.I.ShowColorPallete(false); break;
        }
        switch (new_mode)
        {
            case BubbleEditMode.NONE:
                IconController.I.MoveSelectRing(-1);
                break;
            case BubbleEditMode.MOVE:
                ignoreRecord = false;
                distanceFromDevice = Vector3.Distance(DeviceInfo.I.transform.localPosition, current_selected_bubble.transform.position);
                IconController.I.MoveSelectRing(3);
                break;
            case BubbleEditMode.CONNECT:
                IconController.I.InteractRaycastButton(true);
                IconController.I.MoveSelectRing(4);
                TargetController.I.ActivateTarget();
                break;
            case BubbleEditMode.DELETE:
                ignoreRecord = false;
                IconController.I.MoveSelectRing(6);
                DiscardBubble(current_selected_bubble);
                break;
            case BubbleEditMode.COLOR:
                ignoreRecord = false;
                IconController.I.ShowColorPallete(true);
                IconController.I.MoveSelectRing(5);
                break;
        }
    }
    void WhileEditing()
    {
        switch (editMode)
        {
            case BubbleEditMode.MOVE: MoveBubble(current_selected_bubble); break;
            case BubbleEditMode.CONNECT: break;
            case BubbleEditMode.DELETE: break;
        }
    }
    public void ExitEditMode(bool save)
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

        IconController.I.ShowTriggerIcons(-1);
        IconController.I.InteractRaycastButton(true);
        IconController.I.ShowEditMenuIcons(false);
        IconController.I.ShowColorPallete(false);
        IconController.I.ShowGraphActionIcons(true);
        IconController.I.MoveSelectRing(-1);

        TargetController.I.DeactivateAllTargets();

        // Record 2.
        RecordHistory();
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
    Bubble current_focused_bubble;
    Bubble current_selected_bubble;
    float distanceFromDevice;

    void GenerateBubble(string input_text)
    {
        Transform device_trans = DeviceInfo.I.transform;
        Bubble new_bubble = Instantiate(bubblePrefab, device_trans.localPosition + device_trans.forward * offset, Quaternion.identity, bubblesParent).GetComponent<Bubble>();
        new_bubble.Generate(idCounter, input_text, colors[0]);
        idCounter++;
        allBubbles.Add(new_bubble);

        // Record 1.
        ignoreRecord = false;
        RecordHistory();
    }

    void MoveBubble(Bubble bubble)
    {
        Transform device_trans = DeviceInfo.I.transform;
        Vector3 current = bubble.transform.position;
        Vector3 destination = device_trans.localPosition + device_trans.forward * distanceFromDevice;
        Vector3 next = current + (destination - current) * smoothing;
        bubble.transform.position = next;

        if (bubble.connectedNodes.Count > 0)
        {
            foreach (Node node in bubble.connectedNodes)
            {
                node.UpdatePosition();
            }
        }
    }

    void DiscardBubble(Bubble bubble)
    {
        string caution = "Discard this bubble?/n(You can revive this bubble later.)";

        Action on_yes = () =>
        {
            bubble.Discard();
            ExitEditMode(true);
        };

        Action on_no = () =>
        {
            ChangeEditMode(0);
        };

        ConfirmBox.OpenConfirmBox(caution, on_yes, on_no);
    }

    void DeleteBubble(Bubble bubble)
    {
        string caution = "Delete this bubble?/n(You can not revive this bubble.)";

        Action on_yes = () =>
        {
            bubble.Delete(false);
            ExitEditMode(true);
        };

        Action on_no = () =>
        {
            ChangeEditMode(0);
        };

        ConfirmBox.OpenConfirmBox(caution, on_yes, on_no);
    }

    void ReviveBubble(Bubble bubble)
    {
        string caution = "Revive this bubble?";

        Action on_yes = () =>
        {
            bubble.Revive();
            ExitEditMode(true);
        };

        Action on_no = () =>
        {
            ChangeEditMode(0);
        };

        ConfirmBox.OpenConfirmBox(caution, on_yes, on_no);
    }

    void FocusOnBubble()
    {
        Transform device_trans = DeviceInfo.I.transform;
        RaycastHit hit_result;
        bool detected = Physics.Raycast(device_trans.localPosition, device_trans.forward, out hit_result, maxDistance, graphLayermask);

        // Bubble Detected.
        if (detected)
        {
            Bubble bubble = hit_result.transform.GetComponent<Bubble>();

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
        IconController.I.ColorColoringButton(color);
    }

    public void ZoomBubbleText(Bubble bubble, bool zoom)
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
    void GenerateNode(Bubble start_bubble, Bubble end_bubble)
    {
        Node new_node = Instantiate(nodePrefab, nodesParent).GetComponent<Node>();
        new_node.Generate(start_bubble, end_bubble, line_width: 0.005f, draw_immediate: true);
        allNodes.Add(new_node);

        start_bubble.connectedNodes.Add(new_node);
        end_bubble.connectedNodes.Add(new_node);
        start_bubble.connectedBubbles.Add(end_bubble);
        end_bubble.connectedBubbles.Add(start_bubble);
    }

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

    void RecordHistory()
    {
        // This is necessary in order to prevent from recording unnecessary history.
        if (ignoreRecord) return;
        ignoreRecord = true;

        foreach (Bubble bubble in allBubbles) bubble.Record(currentEditingHistory);
        foreach (Node node in allNodes) node.Record(currentEditingHistory);

        // Delete bubbles & nodes which are deleted for good from "all" list.
        allBubbles.RemoveAll(b => deletedBubblesCache.Contains(b));
        allNodes.RemoveAll(n => deletedNodesCache.Contains(n));
        deletedBubblesCache.Clear();
        deletedNodesCache.Clear();

        currentEditingHistory++;
        headEditingHistory = currentEditingHistory;

        // Enable history editing when history is recorded.
        IconController.I.InteractHistoryButton(true);
    }

    void PlayBackHistory(int editing_history)
    {
        foreach (Bubble bubble in allBubbles) bubble.PlayBack(editing_history);
        foreach (Node node in allNodes) node.PlayBack(editing_history);
        currentEditingHistory = editing_history;
    }

    public void OnHistorySliderChanged()
    {
        int editing_history = tailEditingHistory + IconController.I.historySliderValue;
        editing_history = Mathf.Clamp(editing_history, Mathf.Max(tailEditingHistory, 0), headEditingHistory);

        if (editing_history == currentEditingHistory) return;

        PlayBackHistory(editing_history);
    }



    // 
    // 
    // 
    [Header("For Debug")]
    [SerializeField] TextMeshProUGUI tmp;



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
    }
}
