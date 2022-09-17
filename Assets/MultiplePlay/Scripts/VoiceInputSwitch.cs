using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TextSpeech;
using System;

public class VoiceInputSwitch : Singleton<VoiceInputSwitch>
{
    protected override bool dont_destroy_on_load { get; set; } = false;

    // const string LANG_CODE = "en-US";
    const string LANG_CODE = "ja-JP";

    uint actionNumberCount = 0;
    uint actionSwitch;
    Dictionary<uint, Action<string>> onResultActions = new Dictionary<uint, Action<string>>();

    protected override void Awake()
    {
        SpeechToText.Instance.Setting(LANG_CODE);
    }

    void OnEnable()
    {
        SpeechToText.Instance.onResultCallback += OnVoiceResult;
    }

    void OnDisable()
    {
        SpeechToText.Instance.onResultCallback -= OnVoiceResult;
    }

    void OnVoiceResult(string result)
    {
        if (!onResultActions.ContainsKey(actionSwitch)) return;

        Action<string> action = onResultActions[actionSwitch];
        action(result);
    }

    public uint RegisterAction(Action<string> action)
    {
        // 付与する番号。
        uint number = actionNumberCount;
        actionNumberCount++;

        // その番号で実行するAction。
        onResultActions.Add(number, action);

        return number;
    }

    public void SwitchAction(uint action_number) => actionSwitch = action_number;
}
