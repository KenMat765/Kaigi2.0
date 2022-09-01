using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConfirmBox : MonoBehaviour
{
    static GameObject confirmBoxObject;
    static TextMeshProUGUI explanationText;
    static Button yesButton, noButton;

    void Awake()
    {
        confirmBoxObject = gameObject;
        confirmBoxObject.SetActive(false);
        explanationText = transform.Find("Explanation").GetComponent<TextMeshProUGUI>();
        yesButton = transform.Find("Yes").GetComponent<Button>();
        noButton = transform.Find("No").GetComponent<Button>();
    }

    public static void OpenConfirmBox(string explanation, Action on_yes = null, Action on_no = null)
    {
        explanationText.text = explanation;
        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();
        yesButton.onClick.AddListener(() =>
        {
            if (on_yes != null) on_yes();
            CloseConfirmBox();
        });
        noButton.onClick.AddListener(() =>
        {
            if (on_no != null) on_no();
            CloseConfirmBox();
        });
        confirmBoxObject.SetActive(true);
    }

    static void CloseConfirmBox()
    {
        confirmBoxObject.SetActive(false);
    }
}
