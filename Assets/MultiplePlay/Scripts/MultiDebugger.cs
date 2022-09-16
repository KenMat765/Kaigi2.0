using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MultiDebugger : MonoBehaviour
{
    static TextMeshProUGUI debug;
    void Awake() => debug = transform.Find("Debug").GetComponent<TextMeshProUGUI>();
    public static void Log(string message) => debug.text += message + "\n";
    public static void Clear() => debug.text = "\n";
}