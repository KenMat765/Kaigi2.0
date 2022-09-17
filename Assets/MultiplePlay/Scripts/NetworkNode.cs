using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using PretiaArCloud.Networking;


[RequireComponent(typeof(LineRenderer))]
public class NetworkNode : MonoBehaviour
{
    public LineRenderer line { get; private set; }
    public float lineWidth { get; set; }

    // These parameters are saved in history.
    Existence existence = Existence.BEFORE_GENERATION;
    public NetworkBubble startNetworkBubble { get; private set; }
    public NetworkBubble endNetworkBubble { get; private set; }
    public Color startColor { get; private set; }
    public Color endColor { get; private set; }

    public void SetNetworkBubbles(NetworkBubble start_bubble, NetworkBubble end_bubble)
    {
        if (!start_bubble || !end_bubble)
        {
            Debug.LogError("スタート[エンド]バブルに Null を代入できません。");
            return;
        }
        startNetworkBubble = start_bubble;
        endNetworkBubble = end_bubble;
    }

    Dictionary<int, NetworkNodeState> nodeHistory = new Dictionary<int, NetworkNodeState>();



    /// <summary> バブル同士を線で繋ぐ。</summary>
    public void DrawLineBetweenNetworkBubbles()
    {
        Vector3 start_position = startNetworkBubble.transform.position;
        Vector3 end_position = endNetworkBubble.transform.position;
        Vector3[] positions = new Vector3[] { start_position, end_position };
        line.SetPositions(positions);
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        UpdateColor(startNetworkBubble.bubbleColor, endNetworkBubble.bubbleColor);
        line.enabled = true;
        // 
        // ノード生成演出。
        // 
    }

    public void UpdatePosition(Vector3 start, Vector3 end)
    {
        Vector3[] new_positions = new Vector3[] { start, end };
        line.SetPositions(new_positions);
    }

    // If input is null, color will stay the same.
    public void UpdateColor(Color? start_color = null, Color? end_color = null)
    {
        startColor = start_color.HasValue ? (Color)start_color : startColor;
        endColor = end_color.HasValue ? (Color)end_color : endColor;
        if (start_color.HasValue) line.material.SetColor("_StartColor", startColor);
        if (end_color.HasValue) line.material.SetColor("_EndColor", endColor);
    }



    /// <summary> ノードのセットアップを行う。Instantiateの直後に必ず呼ぶこと！！ </summary>
    /// <param name="start_bubble"> ノードの開始バブル。</param>
    /// <param name="end_bubble"> ノードの終了バブル。</param>
    /// <param name="line_width"> ノードの太さ。</param>
    /// <param name="draw_immediate"> 線をノード生成時に引く。</param>
    public void Generate(NetworkBubble start_bubble, NetworkBubble end_bubble, float line_width = 0.005f, bool draw_immediate = true)
    {
        line = GetComponent<LineRenderer>();
        line.enabled = false;
        existence = Existence.EXISTS;
        SetNetworkBubbles(start_bubble, end_bubble);
        lineWidth = line_width;
        if (draw_immediate) DrawLineBetweenNetworkBubbles();

        // 
        // 
        // 
        identity = GetComponent<NetworkIdentity>();
    }



    ///<summary> ノードを削除する。ヒストリを戻す以外の方法で復活はできない。</summary>
    // NetworkNode.Delete() is called BEFORE bubble is destroyed for good.
    public void Delete(bool completely)
    {
        // Null check is necessary, because this node might be deleted when BEFORE_GENERATION, which does not have start[end] bubble.
        // Contained check is necessary, because this node might be already removed at start[end] bubbles.
        bool contained_in_start = startNetworkBubble && startNetworkBubble.connectedNetworkNodes.Contains(this);
        if (contained_in_start) startNetworkBubble.connectedNetworkNodes.Remove(this);
        bool contained_in_end = endNetworkBubble && endNetworkBubble.connectedNetworkNodes.Contains(this);
        if (contained_in_end) endNetworkBubble.connectedNetworkNodes.Remove(this);

        if (completely)
        {
            // 
            // ノード破壊演出。
            // 
            // Add this to cache to delete AFTER all nodes recording is done.
            NetworkBubbleController.I.deletedNodesCache.Add(this);
            Destroy(gameObject);
        }
        else
        {
            existence = Existence.DELETED;
            startNetworkBubble = null;
            endNetworkBubble = null;
            // 
            // ノード削除演出。
            // 
            gameObject.SetActive(false);
        }
    }



    /// <summary> ノードを復活させる。ヒストリを戻すときに使用する。</summary>
    void Revive()
    {
        existence = Existence.EXISTS;
        gameObject.SetActive(true);
        // 
        // ノード生成演出。
        // 
    }



    /// <summary> ノードの状態をヒストリに記録する。</summary>
    public void Record(int current_editing_history)
    {
        NetworkNodeState new_state = new NetworkNodeState(startNetworkBubble, endNetworkBubble, existence, startColor, endColor);

        // Branched from past history.
        if (nodeHistory.ContainsKey(current_editing_history))
        {
            // Replace past history.
            nodeHistory[current_editing_history] = new_state;

            // Delete histories which are later than new head history.
            nodeHistory.TakeWhile(s => s.Key <= current_editing_history);
        }

        // Recorded new history or Branched from history before generation.
        else
        {
            // Delete this node completely when branched from history before generation.
            if (existence == Existence.BEFORE_GENERATION)
            {
                Delete(true);
                return;
            }

            nodeHistory.Add(current_editing_history, new_state);
        }
    }



    /// <summary> ノードの状態を戻す(進める)。</summary>
    public void PlayBack(int editing_history)
    {
        // NetworkNode state should be at -1 from editing history.
        int state_history = editing_history - 1;

        // When played back to history before generation.
        if (!nodeHistory.ContainsKey(state_history))
        {
            // If already BEFORE_GENERATION, do nothing.
            if (existence == Existence.BEFORE_GENERATION) return;

            Delete(false);
            // Change existence AFTER Delete, because existence is also changed (to DELETE) in Delete.
            existence = Existence.BEFORE_GENERATION;
            return;
        }

        NetworkNodeState new_state = nodeHistory[state_history];

        // Update parameters.
        startNetworkBubble = new_state.startNetworkBubble;
        endNetworkBubble = new_state.endNetworkBubble;
        // This node might be already deleted in this history, which does not have start[end]bubble.
        if (startNetworkBubble && endNetworkBubble)
        {
            UpdatePosition(startNetworkBubble.transform.position, endNetworkBubble.transform.position);
            UpdateColor(new_state.startColor, new_state.endColor);
        }

        if (new_state.existence == existence) return;

        existence = new_state.existence;

        switch (new_state.existence)
        {
            // BEFORE_GENERATION does not reach here.
            case Existence.EXISTS: Revive(); break;
            case Existence.DELETED: Delete(false); break;
        }
    }



    // 
    // 
    //
    // Network.
    public NetworkIdentity identity { get; private set; }
}





public struct NetworkNodeState
{
    public NetworkBubble startNetworkBubble;
    public NetworkBubble endNetworkBubble;
    public Existence existence;
    public Color startColor, endColor;
    public NetworkNodeState(NetworkBubble startNetworkBubble, NetworkBubble endNetworkBubble, Existence existence, Color startColor, Color endColor)
    {
        this.startNetworkBubble = startNetworkBubble;
        this.endNetworkBubble = endNetworkBubble;
        this.existence = existence;
        this.startColor = startColor;
        this.endColor = endColor;
    }
}
