using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class Bubble : MonoBehaviour
{
    public int bubbleId { get; set; }

    public bool focused { get; set; }
    public bool selected { get; set; }

    TextMeshProUGUI tmp;
    public void InputText(string input_text) => tmp.text = input_text;

    // These parameters are saved in history.
    Existence existence = Existence.BEFORE_GENERATION;
    public List<Bubble> connectedBubbles { get; set; }
    public List<Node> connectedNodes { get; set; }

    Dictionary<int, BubbleState> bubbleHistory = new Dictionary<int, BubbleState>();





    // 
    // 
    // 
    // For Debug.
    [SerializeField] GameObject red;
    [SerializeField] GameObject green;
    [SerializeField] int historyCount;





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
        // For Debug.
        red.SetActive(focused);
        green.SetActive(selected);
        historyCount = bubbleHistory.Count;
    }



    /// <summary> バブルのセットアップをする。Instantiateの直後に必ず呼ぶこと！！</summary>
    /// <param name="id"> バブルの固有Id </param>
    /// <param name="generated_history"> バブルが生成されたヒストリ番号 </param>
    /// <param name="input_text"> バブル内に表示する文字列 </param>
    public void Generate(int id, string input_text)
    {
        bubbleId = id;
        tmp.text = input_text;
        existence = Existence.EXISTS;
    }



    /// <summary> バブルを捨てる（復活可能） </summary>
    public void Discard()
    {
        selected = false;
        existence = Existence.DISCARDED;

        if (connectedBubbles.Count > 0)
        {
            foreach (Bubble connected_bubble in connectedBubbles)
            {
                // Contained check is necessary, because this bubble might be already removed at connected bubbles.
                bool contained = connected_bubble.connectedBubbles.Contains(this);
                if (contained) connected_bubble.connectedBubbles.Remove(this);
            }
            connectedBubbles.Clear();
        }

        if (connectedNodes.Count > 0)
        {
            foreach (Node connected_node in connectedNodes)
            {
                // Contained check is not necessary, because nodes are always deleted after bubbles.
                connected_node.Delete(false);
            }
            connectedNodes.Clear();
        }

        gameObject.SetActive(false);
    }



    /// <summary> バブルを削除する。ヒストリを戻す以外の方法でバブルを復活させることができなくなる。</summary>
    /// <param name="completely"> 完全にバブルを削除する。（＝ヒストリを戻しても復活できなくなる）</param>
    public void Delete(bool completely)
    {
        if (connectedBubbles.Count > 0)
        {
            foreach (Bubble connected_bubble in connectedBubbles)
            {
                // Contained check is necessary, because this bubble might be already removed at connected bubbles.
                bool contained = connected_bubble.connectedBubbles.Contains(this);
                if (contained) connected_bubble.connectedBubbles.Remove(this);
            }
            connectedBubbles.Clear();
        }

        if (connectedNodes.Count > 0)
        {
            foreach (Node connected_node in connectedNodes)
            {
                // Contained check is not necessary, because nodes are always deleted after bubbles.
                connected_node.Delete(completely);
            }
            connectedNodes.Clear();
        }

        if (completely)
        {
            // Add this to cache to delete AFTER all bubbles recording is done.
            BubbleController.I.deletedBubblesCache.Add(this);
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
        List<Bubble> connected_bubbles = new List<Bubble>(connectedBubbles);
        List<Node> connected_nodes = new List<Node>(connectedNodes);
        BubbleState new_state = new BubbleState(transform.position, connected_bubbles, connected_nodes, existence);

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

        BubbleState new_state = bubbleHistory[state_history];

        // Update parameters.
        transform.position = new_state.position;
        connectedBubbles = new_state.connectedBubbles;
        connectedNodes = new_state.connectedNodes;

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
}





public enum Existence
{
    BEFORE_GENERATION,
    EXISTS,
    DISCARDED,
    DELETED
}





public struct BubbleState
{
    public Vector3 position;
    public List<Bubble> connectedBubbles;
    public List<Node> connectedNodes;
    public Existence existence;
    public BubbleState(Vector3 position, List<Bubble> connectedBubbles, List<Node> connectedNodes, Existence existence)
    {
        this.position = position;
        this.connectedBubbles = connectedBubbles;
        this.connectedNodes = connectedNodes;
        this.existence = existence;
    }
}
