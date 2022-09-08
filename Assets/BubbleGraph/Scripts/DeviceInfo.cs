using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceInfo : Singleton<DeviceInfo>
{
    protected override bool dont_destroy_on_load { get; set; } = false;
    public Camera cam;

    protected override void Awake()
    {
        base.Awake();
        cam = GetComponent<Camera>();
    }
}
