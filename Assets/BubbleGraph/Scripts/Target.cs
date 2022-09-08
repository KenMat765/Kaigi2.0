using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Target : MonoBehaviour
{
    // 
    // 
    // 
    public TargetState targetState; //{ get; private set; }

    public void ChangeTargetState(TargetState new_state)
    {
        TargetState old_state = targetState;
        targetState = new_state;
        OnTargetStateChanged(new_state, old_state);
    }
    void OnTargetStateChanged(TargetState new_state, TargetState old_state)
    {
        switch (old_state)
        {
            case TargetState.NOT_ACTIVE: break;
            case TargetState.READY: break;
            case TargetState.FOCUSING: break;
            case TargetState.SELECTING: break;
        }
        switch (new_state)
        {
            case TargetState.NOT_ACTIVE: break;
            case TargetState.READY: break;
            case TargetState.FOCUSING: break;
            case TargetState.SELECTING: break;
        }
    }

    public GameObject focusingObject { get; set; }
    public GameObject selectingObject { get; set; }

    Canvas canvas;
    public Image image;

    void Start()
    {
        canvas = GetComponent<Canvas>();
        canvas.worldCamera = DeviceInfo.I.cam;
        // image = GetComponentInChildren<Image>();
    }
}



public enum TargetState
{
    NOT_ACTIVE,
    READY,
    FOCUSING,
    SELECTING
}
