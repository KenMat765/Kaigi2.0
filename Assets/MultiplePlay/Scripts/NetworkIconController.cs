using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;
using Utilities.ScreenCanvasExtention;
using TMPro;

public class NetworkIconController : Singleton<NetworkIconController>
{
    protected override bool dont_destroy_on_load { get; set; } = false;

    public float duration = 0.2f;
    public int historySliderValue { get { return (int)historySlider.value; } }

    [SerializeField] RectTransform graphActions;
    Button generateButton, selectButton, historyButton;

    [SerializeField] RectTransform triggers;
    Button voiceInputButton, raycastButton;
    Slider historySlider;
    RectTransform[] blackDots;

    [SerializeField] RectTransform editMenu;
    Button moveButton, connectButton, coloringButton, deleteButton, exitButton;
    Image coloringButtonImage;
    RectTransform[] colorButtonRects;

    [SerializeField] RectTransform selectRing;

    [SerializeField] GameObject reloc;
    TextMeshProUGUI relocStateText;
    TextMeshProUGUI relocStartText;



    public void ShowGraphActionIcons(bool show)
    {
        float destinationY = show ? 200 : -100;
        graphActions.DOAnchorPosY(destinationY, duration);
    }

    ///<param name="icon_number">０＝Voice。１＝Raycast。２＝History。その他＝全アイコンを非表示。</param>
    public void ShowTriggerIcons(int icon_number)
    {
        voiceInputButton.gameObject.SetActive(false);
        raycastButton.gameObject.SetActive(false);
        historySlider.gameObject.SetActive(false);
        switch (icon_number)
        {
            case 0: voiceInputButton.gameObject.SetActive(true); break;
            case 1: raycastButton.gameObject.SetActive(true); break;
            case 2:
                historySlider.gameObject.SetActive(true);

                int currentEditingHistory = NetworkBubbleController.I.currentEditingHistory;
                int headEditingHistory = NetworkBubbleController.I.headEditingHistory;
                int tailEditingHistory = NetworkBubbleController.I.tailEditingHistory;
                int maxHistoryCount = NetworkBubbleController.I.maxHistoryCount;
                historySlider.maxValue = headEditingHistory < maxHistoryCount ? headEditingHistory : maxHistoryCount - 1;
                historySlider.value = currentEditingHistory - tailEditingHistory;

                int dot_count = headEditingHistory - tailEditingHistory - 1;
                RectTransform[] dots_to_show = blackDots.Take(dot_count).ToArray();
                RectTransform[] dots_to_unshow = blackDots.Skip(dot_count).ToArray();
                for (int m = 0; m < dots_to_unshow.Length; m++) dots_to_unshow[m].gameObject.SetActive(false);
                for (int k = 0; k < dot_count; k++)
                {
                    float interval = 280.0f * 2 / (dot_count + 1);
                    float X = -280.0f + interval * (k + 1);
                    dots_to_show[k].gameObject.SetActive(true);
                    dots_to_show[k].anchoredPosition = new Vector2(X, 0);
                }
                break;
        }
    }

    public void ShowEditMenuIcons(bool show)
    {
        float destinationX = show ? -90 : 90;
        editMenu.DOAnchorPosX(destinationX, duration);
    }

    public void ShowColorPallete(bool show)
    {
        Sequence seq = DOTween.Sequence();
        if (show)
        {
            for (int k = 0; k < colorButtonRects.Length; k++)
            {
                int m = k;
                Vector2 destination = new Vector2(-135, 225 - 150 * k);
                seq.Join(colorButtonRects[k].DOLocalMove(destination, duration));
                seq.OnStart(() => colorButtonRects[m].gameObject.SetActive(true));
            }
        }
        else
        {
            for (int k = 0; k < colorButtonRects.Length; k++)
            {
                int m = k;
                Vector2 destination = Vector2.zero;
                seq.Join(colorButtonRects[k].DOLocalMove(destination, duration));
                seq.OnComplete(() => colorButtonRects[m].gameObject.SetActive(false));
            }
        }
        seq.Play();
    }

    public void InteractHistoryButton(bool enable)
    {
        if (historyButton.interactable == enable) return;
        historyButton.interactable = enable;
    }
    public void ColorColoringButton(Color color) => coloringButtonImage.color = color;
    public void InteractRaycastButton(bool interact)
    {
        if (raycastButton.interactable == interact) return;
        raycastButton.interactable = interact;
    }

    ///<param name="icon_number">０＝Generate。１＝Select。２＝History。３＝Move。４＝Connect。５＝Coloring。６＝Delete。その他＝非表示</param>
    public void MoveSelectRing(int icon_number)
    {
        selectRing.gameObject.SetActive(true);
        Vector3 size = icon_number < 3 ? new Vector3(2.2f, 2.2f, 1) : new Vector3(1.8f, 1.8f, 1);
        selectRing.localScale = size;
        switch (icon_number)
        {
            case 0: selectRing.position = graphActions.position + new Vector3(-250, 0, 0).RescaleCanvas2Screen(); break;
            case 1: selectRing.position = graphActions.position; break;
            case 2: selectRing.position = graphActions.position + new Vector3(250, 0, 0).RescaleCanvas2Screen(); break;
            case 3: selectRing.position = editMenu.position + new Vector3(0, 300, 0).RescaleCanvas2Screen(); break;
            case 4: selectRing.position = editMenu.position + new Vector3(0, 150, 0).RescaleCanvas2Screen(); break;
            case 5: selectRing.position = editMenu.position; break;
            case 6: selectRing.position = editMenu.position + new Vector3(0, -150, 0).RescaleCanvas2Screen(); break;
            default: selectRing.gameObject.SetActive(false); break;
        }
    }

    void OnStartReloc()
    {
        relocStateText.text = "Score : 0";
        relocStartText.text = "Stop";
    }
    void OnStopReloc() => relocStartText.text = "Start";
    void WhileRelocalizing(float score) => relocStateText.text = $"Score : {(int)(score * 100)}";
    void OnRelocalized()
    {
        reloc.SetActive(false);
        ShowGraphActionIcons(true);
    }



    protected override void Awake()
    {
        base.Awake();

        generateButton = graphActions.Find("GenerateButton").GetComponent<Button>();
        selectButton = graphActions.Find("SelectButton").GetComponent<Button>();
        historyButton = graphActions.Find("HistoryButton").GetComponent<Button>();

        voiceInputButton = triggers.Find("VoiceInputButton").GetComponent<Button>();
        raycastButton = triggers.Find("RaycastButton").GetComponent<Button>();
        historySlider = triggers.Find("HistorySlider").GetComponent<Slider>();

        blackDots = new RectTransform[NetworkBubbleController.I.maxHistoryCount - 2];
        Transform black_dots_parent = triggers.Find("HistorySlider/Background/BlackDots");
        GameObject black_dot = black_dots_parent.Find("BlackDot").gameObject;
        blackDots[0] = black_dot.GetComponent<RectTransform>();
        for (int k = 1; k < NetworkBubbleController.I.maxHistoryCount - 2; k++)
        {
            blackDots[k] = Instantiate(black_dot, black_dots_parent).GetComponent<RectTransform>();
        }

        moveButton = editMenu.Find("MoveButton").GetComponent<Button>();
        connectButton = editMenu.Find("ConnectButton").GetComponent<Button>();
        coloringButton = editMenu.Find("ColoringButton").GetComponent<Button>();
        deleteButton = editMenu.Find("DeleteButton").GetComponent<Button>();
        exitButton = editMenu.Find("ExitButton").GetComponent<Button>();

        coloringButtonImage = coloringButton.GetComponent<Image>();
        Transform colorPallete = editMenu.Find("ColorPallete");
        int color_count = colorPallete.transform.childCount;
        colorButtonRects = new RectTransform[color_count];
        for (int k = 0; k < color_count; k++)
        {
            colorButtonRects[k] = colorPallete.GetChild(k).GetComponent<RectTransform>();
            colorPallete.transform.GetComponentsInChildren<Image>()[k].color = NetworkBubbleController.I.colors[k];
        }

        relocStateText = reloc.transform.Find("State/Text").GetComponent<TextMeshProUGUI>();
        relocStartText = reloc.transform.Find("StartButton/Text").GetComponent<TextMeshProUGUI>();
        RelocManager.I.onStartReloc += OnStartReloc;
        RelocManager.I.onStopReloc += OnStopReloc;
        RelocManager.I.onScoreUpdated += WhileRelocalizing;
        RelocManager.I.onRelocalized += OnRelocalized;

        // 
        // 
        // 
        reloc.SetActive(true);
        ShowGraphActionIcons(false);

        ShowTriggerIcons(-1);
        ShowEditMenuIcons(false);
        ShowColorPallete(false);
        InteractHistoryButton(false);
        MoveSelectRing(-1);
    }

    void OnDestroy()
    {
        RelocManager.I.onStartReloc -= OnStartReloc;
        RelocManager.I.onStopReloc -= OnStopReloc;
        RelocManager.I.onScoreUpdated -= WhileRelocalizing;
        RelocManager.I.onRelocalized -= OnRelocalized;
    }
}
