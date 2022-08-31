using UnityEngine;
using UnityEngine.UI;
using PretiaArCloud;

public class ScoreUpdater : MonoBehaviour
{

    [SerializeField]
    private ARSharedAnchorManager _relocManager;

    [SerializeField]
    private Text _scoreLabel;

    void Start() {
        OnScoreUpdated(0);
    }

    void OnEnable() {
        _relocManager.OnScoreUpdated += OnScoreUpdated;
    }

    void OnDisable() {
        _relocManager.OnScoreUpdated -= OnScoreUpdated;
    }

    void OnScoreUpdated(float score)
    {
        _scoreLabel.text = "Score: " + Mathf.Floor(score * 100);
    }
}