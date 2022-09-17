using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using PretiaArCloud.Networking;

public class NetworkBubble : MonoBehaviour
{
    public int id;

    public bool focused { get; set; }
    public bool selected { get; set; }

    public RectTransform canvasRect { get; private set; }
    TextMeshProUGUI tmp;
    public void InputText(string input_text) => tmp.text = input_text;

    // These parameters are saved in history.
    Existence existence = Existence.BEFORE_GENERATION;
    public List<NetworkBubble> connectedBubbles { get; set; } = new List<NetworkBubble>();
    public List<NetworkNode> connectedNetworkNodes { get; set; } = new List<NetworkNode>();
    public Color bubbleColor { get; private set; }

    Dictionary<int, NetworkBubbleState> bubbleHistory = new Dictionary<int, NetworkBubbleState>();

    Material material;



    /// <summary> バブルのセットアップをする。Instantiateの直後に必ず呼ぶこと！！</summary>
    /// <param name="id"> バブルの固有Id </param>
    /// <param name="generated_history"> バブルが生成されたヒストリ番号 </param>
    /// <param name="input_text"> バブル内に表示する文字列 </param>
    public void Generate(int id, string input_text, Color default_color)
    {
        this.id = id;
        canvasRect = transform.Find("Canvas").GetComponent<RectTransform>();
        tmp = GetComponentInChildren<TextMeshProUGUI>();
        tmp.text = input_text;
        existence = Existence.EXISTS;
        material = GetComponentInChildren<Renderer>().material;
        Color(default_color);

        // 
        // 
        // 
        identity = GetComponent<NetworkIdentity>();
    }



    /// <summary> バブルを捨てる（復活可能） </summary>
    public void Discard()
    {
        selected = false;
        existence = Existence.DISCARDED;

        if (connectedBubbles.Count > 0)
        {
            foreach (NetworkBubble connected_bubble in connectedBubbles)
            {
                // Contained check is necessary, because this bubble might be already removed at connected bubbles.
                bool contained = connected_bubble.connectedBubbles.Contains(this);
                if (contained) connected_bubble.connectedBubbles.Remove(this);
            }
            connectedBubbles.Clear();
        }

        if (connectedNetworkNodes.Count > 0)
        {
            // Copy is necessary, because original connectedNetworkNodes is modified during foreach process, which causes error.
            List<NetworkNode> connectedNetworkNodes_copy = new List<NetworkNode>(connectedNetworkNodes);
            foreach (NetworkNode connected_node in connectedNetworkNodes_copy)
            {
                // Contained check is not necessary, because nodes are always deleted after bubbles.
                connected_node.Delete(false);
            }
            connectedNetworkNodes.Clear();
        }

        gameObject.SetActive(false);
    }



    /// <summary> バブルを削除する。ヒストリを戻す以外の方法でバブルを復活させることができなくなる。</summary>
    /// <param name="completely"> 完全にバブルを削除する。（＝ヒストリを戻しても復活できなくなる）</param>
    public void Delete(bool completely)
    {
        if (connectedBubbles.Count > 0)
        {
            foreach (NetworkBubble connected_bubble in connectedBubbles)
            {
                // Contained check is necessary, because this bubble might be already removed at connected bubbles.
                bool contained = connected_bubble.connectedBubbles.Contains(this);
                if (contained) connected_bubble.connectedBubbles.Remove(this);
            }
            connectedBubbles.Clear();
        }

        if (connectedNetworkNodes.Count > 0)
        {
            foreach (NetworkNode connected_node in connectedNetworkNodes)
            {
                connected_node.Delete(completely);
            }
            connectedNetworkNodes.Clear();
        }

        if (completely)
        {
            // Add this to cache to delete AFTER all bubbles recording is done.
            NetworkBubbleController.I.deletedBubblesCache.Add(this);
            Destroy(gameObject);
        }
        else
        {
            existence = Existence.DELETED;
            selected = false;
            gameObject.SetActive(false);
        }
    }



    /// <summary> バブルを復活させる。</summary>
    public void Revive()
    {
        existence = Existence.EXISTS;
        gameObject.SetActive(true);
    }



    /// <summary> バブルの状態をヒストリに記録する。</summary>
    public void Record(int current_editing_hitory)
    {
        // List is a class, which means it is reference type. Thus, we need to copy connected bubbles[nodes] list before saving it to bubble state list.
        List<NetworkBubble> connected_bubbles = new List<NetworkBubble>(connectedBubbles);
        List<NetworkNode> connected_nodes = new List<NetworkNode>(connectedNetworkNodes);
        NetworkBubbleState new_state = new NetworkBubbleState(transform.position, connected_bubbles, connected_nodes, existence, bubbleColor);

        // Branched from past history.
        if (bubbleHistory.ContainsKey(current_editing_hitory))
        {
            // Replace past history.
            bubbleHistory[current_editing_hitory] = new_state;

            // Delete histories which are later than new head history.
            bubbleHistory.TakeWhile(s => s.Key <= current_editing_hitory);
        }

        // Recorded new history or Branched from history before generation.
        else
        {
            // Delete this bubble completely when branched from history before generation.
            if (existence == Existence.BEFORE_GENERATION)
            {
                Delete(true);
                return;
            }

            // Add new history.
            bubbleHistory.Add(current_editing_hitory, new_state);
        }
    }



    /// <summary> バブルの状態を戻す(進める)。</summary>
    public void PlayBack(int editing_history)
    {
        // Bubble state should be at -1 from editing history.
        int state_history = editing_history - 1;

        // When played back to history before generation.
        if (!bubbleHistory.ContainsKey(state_history))
        {
            // If already BEFORE_GENERATION, do nothing.
            if (existence == Existence.BEFORE_GENERATION) return;

            Delete(false);
            // Change existence AFTER Delete, because existence is also changed (to DELETE) in Delete.
            existence = Existence.BEFORE_GENERATION;
            return;
        }

        NetworkBubbleState new_state = bubbleHistory[state_history];

        // Update parameters.
        transform.position = new_state.position;
        connectedBubbles = new_state.connectedBubbles;
        connectedNetworkNodes = new_state.connectedNetworkNodes;
        Color(new_state.color);

        if (new_state.existence == existence) return;

        existence = new_state.existence;

        switch (new_state.existence)
        {
            // BEFORE_EXISTENCE does not reach here.
            case Existence.EXISTS: Revive(); break;
            case Existence.DISCARDED: Discard(); break;
            case Existence.DELETED: Delete(false); break;
        }
    }



    ///<summary> バブルの色を変える。</summary>
    public void Color(Color color)
    {
        material.SetColor("_Color", color);
        bubbleColor = color;
        if (connectedNetworkNodes.Count > 0)
        {
            foreach (NetworkNode node in connectedNetworkNodes)
            {
                if (this == node.startNetworkBubble) node.UpdateColor(color, null);
                if (this == node.endNetworkBubble) node.UpdateColor(null, color);
            }
        }
    }



    // 
    // 
    // 
    // Network.
    public NetworkIdentity identity { get; private set; }
}





public struct NetworkBubbleState
{
    public Vector3 position;
    public List<NetworkBubble> connectedBubbles;
    public List<NetworkNode> connectedNetworkNodes;
    public Existence existence;
    public Color color;
    public NetworkBubbleState(Vector3 position, List<NetworkBubble> connectedBubbles, List<NetworkNode> connectedNetworkNodes, Existence existence, Color color)
    {
        this.position = position;
        this.connectedBubbles = connectedBubbles;
        this.connectedNetworkNodes = connectedNetworkNodes;
        this.existence = existence;
        this.color = color;
    }
}