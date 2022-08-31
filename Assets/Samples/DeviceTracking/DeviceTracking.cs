using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DeviceTracking : MonoBehaviour
{
    [SerializeField] GameObject arCamera;
    [SerializeField] TextMeshProUGUI tmp;

    void Update()
    {
        tmp.text = arCamera.transform.localPosition.ToString();
    }
}
