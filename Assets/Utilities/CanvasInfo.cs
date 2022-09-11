using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>Attach this component to Canvas in screen to access to canvas infos easily.</summary>
public class CanvasInfo : MonoBehaviour
{
    public static Canvas canvas;
    public static RectTransform canvasRect;
    public static float width { get { return canvasRect.rect.width; } }
    public static float height { get { return canvasRect.rect.height; } }

    void Awake()
    {
        canvas = GetComponent<Canvas>();
        canvasRect = GetComponent<RectTransform>();
    }
}
