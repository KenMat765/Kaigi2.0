using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class BubbleController : MonoBehaviour
{
    // Prefabs.
    [Header("Prefabs")]
    [SerializeField] GameObject bubblePrefab;
    [SerializeField] GameObject nodePrefab;
    List<Bubble> allBubbles = new List<Bubble>();
    List<Node> allNodes = new List<Node>();
    Transform bubblesParent, nodesParent;



    // Device.
    GameObject arCamera;
    Vector3 devicePosition { get { return arCamera.transform.localPosition; } }
    Vector3 deviceDirection { get { return arCamera.transform.forward; } }



    // Graph Actions (Generate or Edit).
    public enum GraphAction { NONE, GENERATE, SELECT }
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
            case GraphAction.NONE:
                break;

            case GraphAction.GENERATE:
                voiceInputButton.gameObject.SetActive(false);
                break;

            case GraphAction.SELECT:
                raycastButton.gameObject.SetActive(false);
                break;
        }
        switch (new_action)
        {
            case GraphAction.NONE:
                break;

            case GraphAction.GENERATE:
                voiceInputButton.gameObject.SetActive(true);
                break;

            case GraphAction.SELECT:
                raycastButton.gameObject.SetActive(true);
                break;
        }
    }
    // public void WhilePressingVoiceInputButton()
    // {
    //     // Detect user's voice, and input to Bubble.tmp ...
    // }
    public void OnReleasedVoiceInputButton() => GenerateBubble();
    public bool raycasting { get; set; }
    void WhilePressingRaycastButton() => FocusOnBubble();
    public void OnReleasedRaycastButton()
    {
        if (current_focused_bubble)
        {
            current_focused_bubble.focused = false;
            if (editing)
            {
                // When Selected Bubble to Connect.
                GenerateNode(current_selected_bubble, current_focused_bubble);
            }
            else
            {
                // When Selected Bubble to Edit.
                current_focused_bubble.selected = true;
                current_selected_bubble = current_focused_bubble;

                // Enter Edit Mode.
                generateButton.gameObject.SetActive(false);
                selectButton.gameObject.SetActive(false);
                editMenu.SetActive(true);
                editing = true;
                ChangeEditMode(0);
            }
            current_focused_bubble = null;
        }
    }



    // Bubble Edit Modes.
    bool editing = false;
    public enum BubbleEditMode { NONE, MOVE, CONNECT, DELETE }
    BubbleEditMode editMode;
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
            case BubbleEditMode.NONE:
                raycastButton.interactable = false;
                break;

            case BubbleEditMode.MOVE:
                raycastButton.interactable = false;
                break;

            case BubbleEditMode.CONNECT:
                raycastButton.interactable = true;
                break;

            case BubbleEditMode.DELETE:
                raycastButton.interactable = false;
                break;
        }
        switch (new_mode)
        {
            case BubbleEditMode.NONE:
                raycastButton.interactable = false;
                break;

            case BubbleEditMode.MOVE:
                raycastButton.interactable = false;
                distanceFromDevice = Vector3.Distance(devicePosition, current_selected_bubble.transform.position);
                break;

            case BubbleEditMode.CONNECT:
                raycastButton.interactable = true;
                break;

            case BubbleEditMode.DELETE:
                raycastButton.interactable = false;
                DeleteBubble(current_selected_bubble);
                break;
        }
    }
    void WhileEditing()
    {
        switch (editMode)
        {
            case BubbleEditMode.MOVE:
                MoveBubble(current_selected_bubble);
                break;

            case BubbleEditMode.CONNECT:
                break;

            case BubbleEditMode.DELETE:
                break;
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
    }



    // Focus & Select.
    int idCounter = 0;
    Bubble current_focused_bubble;
    Bubble current_selected_bubble;
    float distanceFromDevice;



    // Raycast.
    [Header("Raycast")]
    [SerializeField] float max_distance;
    [SerializeField] LayerMask bubble_layermask;



    // UI.
    [Header("UI")]
    [SerializeField] Button generateButton;
    [SerializeField] Button selectButton;
    [SerializeField] Button voiceInputButton;
    [SerializeField] Button raycastButton;
    [SerializeField] GameObject editMenu;



    // Bubble.
    [Header("Bubble")]
    [SerializeField] float offset = 0.5f;
    [SerializeField] float smoothing = 0.9f;


    // 
    // 
    // 
    [Header("For Debug")]
    [SerializeField] TextMeshProUGUI tmp;



    void Awake()
    {
        bubblesParent = transform.Find("Bubbles");
        nodesParent = transform.Find("Nodes");
        arCamera = GameObject.FindGameObjectWithTag("MainCamera");

        voiceInputButton.gameObject.SetActive(false);
        raycastButton.gameObject.SetActive(false);
        editMenu.SetActive(false);
    }

    void Update()
    {
        if (raycasting) WhilePressingRaycastButton();
        if (editing) WhileEditing();
    }



    // Bubble Functions.
    void GenerateBubble(string input_text = "")
    {
        Quaternion device_rotation = arCamera.transform.localRotation;
        Bubble new_bubble = Instantiate(bubblePrefab, devicePosition + deviceDirection * offset, device_rotation, bubblesParent).GetComponent<Bubble>();
        allBubbles.Add(new_bubble);
        new_bubble.bubbleId = idCounter;
        idCounter++;
        new_bubble.InputText(input_text);
    }

    void MoveBubble(Bubble bubble)
    {
        Vector3 current = bubble.transform.position;
        Vector3 destination = devicePosition + deviceDirection * distanceFromDevice;
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

    void DeleteBubble(Bubble bubble)
    {
        string explanation = "Delete this Bubble?";
        Action on_yes = () =>
        {
            current_selected_bubble.Destroy();
            ExitEditMode(true);
        };
        Action on_no = () =>
        {
            ChangeEditMode(0);
        };
        ConfirmBox.OpenConfirmBox(explanation, on_yes, on_no);
    }

    void FocusOnBubble()
    {
        RaycastHit hit_result;
        bool detected = Physics.Raycast(arCamera.transform.localPosition, arCamera.transform.forward, out hit_result, max_distance, bubble_layermask);

        // Bubble Detected.
        if (detected)
        {
            Bubble bubble = hit_result.transform.GetComponent<Bubble>();

            // Focused on Same Bubble.
            if (current_focused_bubble == bubble) return;

            // While Editing Mode.
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
        }

        // No Bubble Detected.
        else
        {
            if (current_focused_bubble)
            {
                current_focused_bubble.focused = false;
                current_focused_bubble = null;
            }
        }
    }



    // Node Functions.
    void GenerateNode(Bubble start_bubble, Bubble end_bubble)
    {
        Vector3 start_position = start_bubble.transform.position;
        Vector3 end_position = end_bubble.transform.position;
        Vector3 middle_position = (start_position + end_position) / 2.0f;

        Node new_node = Instantiate(nodePrefab, middle_position, Quaternion.identity, nodesParent).GetComponent<Node>();
        allNodes.Add(new_node);
        new_node.lineWidth = 0.005f;
        new_node.DrawLineBetweenBubbles(start_bubble, end_bubble);

        start_bubble.connectedNodes.Add(new_node);
        end_bubble.connectedNodes.Add(new_node);
        start_bubble.connectedBubbles.Add(end_bubble);
        end_bubble.connectedBubbles.Add(start_bubble);
    }

    void DeleteNode(Node node)
    {

    }
}
