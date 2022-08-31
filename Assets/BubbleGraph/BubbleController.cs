using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BubbleController : MonoBehaviour
{
    [SerializeField] GameObject bubblePrefab;
    List<Bubble> allBubbles = new List<Bubble>();
    GameObject arCamera;

    public enum BubbleEditMode { MOVE, CONNECT, DELETE }
    BubbleEditMode editMode;
    public void ChangeEditMode(BubbleEditMode newMode) => editMode = newMode;

    int idCounter = 0;
    Bubble current_focused_bubble;
    Bubble current_selected_bubble;

    [Header("Raycast Infos")]
    [SerializeField] float max_distance;
    [SerializeField] LayerMask bubble_layermask;


    // 
    // 
    // 
    [SerializeField] TextMeshProUGUI tmp;
    [SerializeField] LineRenderer lineRenderer;


    void Awake()
    {
        arCamera = GameObject.FindGameObjectWithTag("MainCamera");
    }

    void Update()
    {
        // Edit Mode.
        if (current_selected_bubble) OnEditMode(current_selected_bubble);

        // Selection Mode.
        else OnSelectionMode();
    }

    public void GenerateBubble()
    {
        Vector3 device_position = arCamera.transform.localPosition;
        Vector3 device_direction = arCamera.transform.forward;
        float offset = 0.5f + bubblePrefab.transform.localScale.x;
        Quaternion device_rotation = arCamera.transform.localRotation;
        var new_bubble = Instantiate(bubblePrefab, device_position + device_direction * offset, device_rotation, transform).GetComponent<Bubble>();
        allBubbles.Add(new_bubble);
        new_bubble.bubbleId = idCounter;
        idCounter++;
    }

    void MoveBubble(Bubble bubble)
    {

    }

    void ConnectBubble(Bubble bubble)
    {

    }

    void DeleteBubble(Bubble bubble)
    {

    }

    void OnEditMode(Bubble bubble)
    {
        switch (editMode)
        {
            case BubbleEditMode.MOVE:
                MoveBubble(bubble);
                break;

            case BubbleEditMode.CONNECT:
                ConnectBubble(bubble);
                break;

            case BubbleEditMode.DELETE:
                DeleteBubble(bubble);
                break;
        }
    }

    void OnSelectionMode()
    {
        if (Input.touchCount > 0)
        {
            Touch first_touch = Input.GetTouch(0);
            TouchPhase touch_phase = first_touch.phase;
            switch (touch_phase)
            {
                case TouchPhase.Stationary:
                case TouchPhase.Moved:
                    RaycastHit hit_result;
                    bool hit_bubble = Physics.Raycast(arCamera.transform.localPosition, arCamera.transform.forward, out hit_result, max_distance, bubble_layermask);
                    if (hit_bubble)
                    {
                        Bubble bubble = hit_result.transform.GetComponent<Bubble>();
                        if (current_focused_bubble == bubble) break;
                        if (current_focused_bubble) current_focused_bubble.focused = false;
                        bubble.focused = true;
                        current_focused_bubble = bubble;
                    }
                    else
                    {
                        if (current_focused_bubble)
                        {
                            current_focused_bubble.focused = false;
                            current_focused_bubble = null;
                        }
                    }
                    break;

                case TouchPhase.Ended:


                    // 
                    // 
                    // 
                    var positions = new Vector3[] { arCamera.transform.localPosition, arCamera.transform.forward * 1000 };
                    lineRenderer.SetPositions(positions);
                    lineRenderer.startWidth = 0.03f;
                    lineRenderer.endWidth = 0.03f;


                    if (current_focused_bubble)
                    {
                        current_focused_bubble.selected = true;
                        current_focused_bubble.focused = false;
                        current_selected_bubble = current_focused_bubble;
                        current_focused_bubble = null;
                    }
                    break;
            }
        }
    }
}
