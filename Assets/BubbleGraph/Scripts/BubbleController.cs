using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

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
            case GraphAction.GENERATE: voiceInputButton.gameObject.SetActive(false); break;
            case GraphAction.SELECT:
                raycastButton.gameObject.SetActive(false);
                TargetController.I.DeactivateAllTargets();
                break;
            case GraphAction.HISTORY: historySlider.gameObject.SetActive(false); break;
        }
        switch (new_action)
        {
            case GraphAction.NONE: break;
            case GraphAction.GENERATE: voiceInputButton.gameObject.SetActive(true); break;
            case GraphAction.SELECT:
                raycastButton.gameObject.SetActive(true);
                TargetController.I.ActivateTarget();
                break;
            case GraphAction.HISTORY:
                historySlider.gameObject.SetActive(true);
                historySlider.maxValue = headEditingHistory < maxHistoryCount ? headEditingHistory : maxHistoryCount - 1;
                historySlider.value = currentEditingHistory - tailEditingHistory;
                break;
        }
    }

    // public void WhilePressingVoiceInputButton()
    // {
    //     // Detect user's voice, and input as an argument of GenerateBubble() ...
    // }
    public void OnReleasedVoiceInputButton() => GenerateBubble("");
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
                GenerateNode(current_selected_bubble, current_focused_bubble);
                TargetController.I.DeactivateTarget(TargetController.I.current_focusing_target);
                TargetController.I.ActivateTarget();
            }
            else
            {
                // When Selected Bubble to Edit.
                current_focused_bubble.selected = true;
                current_selected_bubble = current_focused_bubble;
                TargetController.I.SelectTarget(current_selected_bubble.gameObject);

                // Enter Edit Mode.
                EnterEditMode();
            }
            current_focused_bubble = null;
        }
    }



    // Edit Mode.
    bool editing = false;
    public enum BubbleEditMode { NONE, MOVE, CONNECT, DELETE }
    BubbleEditMode editMode;
    void EnterEditMode()
    {
        editing = true;
        generateButton.gameObject.SetActive(false);
        selectButton.gameObject.SetActive(false);
        historyButton.gameObject.SetActive(false);
        editMenu.SetActive(true);
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
        switch (old_mode)
        {
            case BubbleEditMode.NONE: raycastButton.interactable = false; break;
            case BubbleEditMode.MOVE: raycastButton.interactable = false; break;
            case BubbleEditMode.CONNECT:
                raycastButton.interactable = true;
                TargetController.I.DeactivateTarget(TargetController.I.current_ready_target);
                TargetController.I.DeactivateTarget(TargetController.I.current_focusing_target);
                break;
            case BubbleEditMode.DELETE: raycastButton.interactable = false; break;
        }
        switch (new_mode)
        {
            case BubbleEditMode.NONE: raycastButton.interactable = false; break;
            case BubbleEditMode.MOVE:
                raycastButton.interactable = false;
                distanceFromDevice = Vector3.Distance(DeviceInfo.I.transform.localPosition, current_selected_bubble.transform.position);
                break;
            case BubbleEditMode.CONNECT:
                raycastButton.interactable = true;
                TargetController.I.ActivateTarget();
                break;
            case BubbleEditMode.DELETE:
                raycastButton.interactable = false;
                DiscardBubble(current_selected_bubble);
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
            current_selected_bubble.selected = false;
            current_selected_bubble = null;
        }
        distanceFromDevice = 0;

        graphAction = GraphAction.NONE;
        editMode = BubbleEditMode.NONE;

        raycastButton.gameObject.SetActive(false);
        if (!raycastButton.interactable) raycastButton.interactable = true;
        editMenu.SetActive(false);
        generateButton.gameObject.SetActive(true);
        selectButton.gameObject.SetActive(true);
        historyButton.gameObject.SetActive(true);

        TargetController.I.DeactivateAllTargets();

        // Record 2.
        RecordHistory();
    }



    // Raycast.
    [Header("Raycast")]
    public float maxDistance;
    [SerializeField] LayerMask graphLayermask;



    // UI.
    [Header("UI")]
    [SerializeField] Button generateButton;
    [SerializeField] Button selectButton;
    [SerializeField] Button historyButton;
    [SerializeField] Button voiceInputButton;
    [SerializeField] Button raycastButton;
    [SerializeField] GameObject editMenu;
    [SerializeField] Slider historySlider;



    // Bubble.
    [Header("Bubble")]
    public float offset = 0.5f;
    public float smoothing = 0.9f;
    int idCounter = 0;
    Bubble current_focused_bubble;
    Bubble current_selected_bubble;
    float distanceFromDevice;

    void GenerateBubble(string input_text)
    {
        Transform device_trans = DeviceInfo.I.transform;
        Bubble new_bubble = Instantiate(bubblePrefab, device_trans.localPosition + device_trans.forward * offset, Quaternion.identity, bubblesParent).GetComponent<Bubble>();
        new_bubble.Generate(idCounter, input_text);
        idCounter++;
        allBubbles.Add(new_bubble);

        // Record 1.
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
            if (current_focused_bubble) current_focused_bubble.focused = false;
            bubble.focused = true;
            current_focused_bubble = bubble;
            TargetController.I.FocusTarget(bubble.gameObject);
        }

        // No Bubble Detected.
        else
        {
            if (current_focused_bubble)
            {
                current_focused_bubble.focused = false;
                current_focused_bubble = null;
                TargetController.I.ReadyTarget();
            }
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
    [SerializeField] int maxHistoryCount = 10;

    // Editing History is a history which user is editing now, and not recorded yet.
    int currentEditingHistory;
    int headEditingHistory;

    int tailEditingHistory { get { return headEditingHistory - maxHistoryCount + 1 < 0 ? 0 : headEditingHistory - maxHistoryCount + 1; } }

    void RecordHistory()
    {
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
        if (!historyButton.interactable) historyButton.interactable = true;
    }

    void PlayBackHistory(int editing_history)
    {
        foreach (Bubble bubble in allBubbles) bubble.PlayBack(editing_history);
        foreach (Node node in allNodes) node.PlayBack(editing_history);
        currentEditingHistory = editing_history;
    }

    public void OnHistorySliderChanged()
    {
        int editing_history = tailEditingHistory + (int)historySlider.value;
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

        historyButton.interactable = false;
        voiceInputButton.gameObject.SetActive(false);
        raycastButton.gameObject.SetActive(false);
        historySlider.gameObject.SetActive(false);
        editMenu.SetActive(false);
    }

    void Update()
    {
        if (raycasting) WhileRaycasting();
        if (editing) WhileEditing();
    }
}
