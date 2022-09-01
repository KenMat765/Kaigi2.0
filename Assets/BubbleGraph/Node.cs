using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(LineRenderer))]
public class Node : MonoBehaviour
{
    public LineRenderer line { get; private set; }
    public float lineWidth { get; set; }
    public Bubble startBubble { get; set; }
    public Bubble endBubble { get; set; }
    public bool visible { get; private set; }

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.enabled = true;
        this.visible = true;
    }

    public void SetBubbles(Bubble start_bubble, Bubble end_bubble)
    {
        startBubble = start_bubble;
        endBubble = end_bubble;
    }

    public void DrawLineBetweenBubbles(Bubble start_bubble = null, Bubble end_bubble = null)
    {
        if (start_bubble) startBubble = start_bubble;
        if (end_bubble) endBubble = end_bubble;

        if (!startBubble) { Debug.LogWarning("<color=green>startBubble is null. Line was not drawn.</color>"); return; }
        if (!endBubble) { Debug.LogWarning("<color=green>endBubble is null. Line was not drawn.</color>"); return; }

        Vector3 start_position = startBubble.transform.position;
        Vector3 end_position = endBubble.transform.position;
        Vector3[] positions = new Vector3[] { start_position, end_position };
        line.SetPositions(positions);
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.enabled = true;
        this.visible = true;
    }

    public void IsVisible(bool visible)
    {
        line.enabled = visible;
        this.visible = visible;
    }

    public void UpdatePosition()
    {
        Vector3 start_position = startBubble.transform.position;
        Vector3 end_position = endBubble.transform.position;
        Vector3[] new_positions = new Vector3[] { start_position, end_position };
        line.SetPositions(new_positions);
    }

    public void Destroy(Bubble destroyer_bubble = null)
    {
        if (destroyer_bubble)
        {
            // Delete this Node from destroyer bubble.
            if (startBubble == destroyer_bubble) endBubble.connectedNodes.Remove(this);
            else startBubble.connectedNodes.Remove(this);
        }
        else
        {
            startBubble.connectedNodes.Remove(this);
            endBubble.connectedNodes.Remove(this);
        }
        gameObject.SetActive(false);
    }
}
