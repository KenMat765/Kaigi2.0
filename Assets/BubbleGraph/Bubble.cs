using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Bubble : MonoBehaviour
{
    public int bubbleId { get; set; }
    public bool focused { get; set; }
    public bool selected { get; set; }
    TextMeshProUGUI tmp;


    // 
    // 
    // 
    [SerializeField] GameObject red;
    [SerializeField] GameObject green;


    void Awake()
    {
        tmp = GetComponentInChildren<TextMeshProUGUI>();
    }

    void Update()
    {
        // 
        // 
        // 
        red.SetActive(focused);
        green.SetActive(selected);
        tmp.text = selected.ToString();
    }
}
