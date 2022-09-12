using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PretiaArCloud;

public class RelocManager : Singleton<RelocManager>
{
    protected override bool dont_destroy_on_load { get; set; } = false;

    [SerializeField] ARSharedAnchorManager aRSharedAnchorManager;
    public bool relocComplete { get; private set; }
    public float relocScore { get; private set; }
    public delegate void OnRelocalizedDelegate();
    public event OnRelocalizedDelegate onRelocalized = null;
    public delegate void OnScoreUpdatedDelegate(float score);
    public event OnScoreUpdatedDelegate onScoreUpdated = null;
    public delegate void OnStateChangedDelegate(SharedAnchorState state);
    public event OnStateChangedDelegate onStateChanged = null;
    public delegate void OnStartReloc();
    public event OnStartReloc onStartReloc = null;
    public delegate void OnStopReloc();
    public event OnStopReloc onStopReloc = null;
    bool relocOnGoing = false;

    void OnEnable()
    {
        aRSharedAnchorManager.OnRelocalized += OnRelocalized;
        aRSharedAnchorManager.OnScoreUpdated += OnScoreUpdated;
        aRSharedAnchorManager.OnSharedAnchorStateChanged += OnStateChanged;
    }

    void OnDisable()
    {
        aRSharedAnchorManager.OnRelocalized -= OnRelocalized;
        aRSharedAnchorManager.OnScoreUpdated -= OnScoreUpdated;
        aRSharedAnchorManager.OnSharedAnchorStateChanged -= OnStateChanged;
    }

    ///<summary>Start relocalization. When relocalization is ongoing, terminate the process.</summary>
    public void StartRelocalize()
    {
        if (relocOnGoing)
        {
            relocOnGoing = false;
            aRSharedAnchorManager.ResetSharedAnchor();
            if (onStopReloc != null) onStopReloc();
        }
        else
        {
            relocOnGoing = true;
            aRSharedAnchorManager.StartCloudMapRelocalization();
            if (onStartReloc != null) onStartReloc();
        }
    }

    void OnRelocalized()
    {
        relocComplete = true;
        if (onRelocalized != null) onRelocalized();
    }

    void OnScoreUpdated(float score)
    {
        relocScore = score * 100;
        if (onScoreUpdated != null) onScoreUpdated(score);
    }

    void OnStateChanged(SharedAnchorState newState)
    {
        if (onStateChanged != null) onStateChanged(newState);
    }
}
