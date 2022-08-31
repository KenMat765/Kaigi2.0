using UnityEngine;
using UnityEngine.UI;
using PretiaArCloud;

public class StatusUpdater : MonoBehaviour
{

    [SerializeField]
    private ARSharedAnchorManager _relocManager;

    [SerializeField]
    private Text _statusLabel;

    void Start()
    {
        _statusLabel.text = "Stopped";
    }

    void OnEnable()
    {
        _relocManager.OnSharedAnchorStateChanged += OnSharedAnchorStateChanged;
    }

    void OnDisable()
    {
        _relocManager.OnSharedAnchorStateChanged -= OnSharedAnchorStateChanged;
    }

    private void OnSharedAnchorStateChanged(SharedAnchorState state)
    {
        switch (state)
        {
            case SharedAnchorState.Stopped:
                _statusLabel.text = "Stopped";
                break;

            case SharedAnchorState.Initializing:
                _statusLabel.text = "Initializing";
                break;

            case SharedAnchorState.Relocalizing:
                _statusLabel.text = "Relocalizing";
                break;

            case SharedAnchorState.Relocalized:
                _statusLabel.text = "Relocalized";
                break;
        }
    }
}