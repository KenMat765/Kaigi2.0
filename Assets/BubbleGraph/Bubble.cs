using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Bubble : MonoBehaviour
{
    public int bubbleId { get; set; }

    public bool focused { get; set; }
    public bool selected { get; set; }

    public List<Bubble> connectedBubbles { get; set; }
    public List<Node> connectedNodes { get; set; }

    TextMeshProUGUI tmp;
    public void InputText(string input_text) => tmp.text = input_text;


    // 
    // 
    // 
    [SerializeField] GameObject red;
    [SerializeField] GameObject green;


    void Awake()
    {
        connectedBubbles = new List<Bubble>();
        connectedNodes = new List<Node>();
        tmp = GetComponentInChildren<TextMeshProUGUI>();
    }

    void Update()
    {
        // 
        // 
        // 
        red.SetActive(focused);
        green.SetActive(selected);
        tmp.text = selected.ToString();
    }

    public void Destroy()
    {
        if (connectedBubbles.Count > 0)
        {
            foreach (Bubble bubble in connectedBubbles)
            {
                bubble.connectedBubbles.Remove(this);
            }
            connectedBubbles.Clear();
        }
        if (connectedNodes.Count > 0)
        {
            foreach (Node node in connectedNodes)
            {
                node.Destroy(destroyer_bubble: this);
            }
            connectedNodes.Clear();
        }
        selected = false;
        gameObject.SetActive(false);
    }
}
