using UnityEngine;
using UnityEngine.UI;
using PretiaArCloud;

public class MapBasedRelocSceneController : MonoBehaviour
{

    [SerializeField]
    private ARSharedAnchorManager _relocManager;

    [SerializeField]
    private Button _startButton;

    [SerializeField]
    private Text _startButtonLabel;

    private bool _isRunning = false;

    private bool isRunning
    {
        get
        {
            return _isRunning;
        }

        set
        {
            _isRunning = value;
            if (value)
            {
                _startButtonLabel.text = "Stop";
            }
            else
            {
                _startButtonLabel.text = "Start";
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        isRunning = false;
    }

    void OnEnable()
    {
        _startButton.onClick.AddListener(OnStartClicked);
        _relocManager.OnSharedAnchorStateChanged += OnSharedAnchorStateChanged;
        _relocManager.OnRelocalized += OnRelocalized;
    }

    void OnDisable()
    {
        _startButton.onClick.RemoveListener(OnStartClicked);
        _relocManager.OnSharedAnchorStateChanged -= OnSharedAnchorStateChanged;
        _relocManager.OnRelocalized -= OnRelocalized;
    }

    private void OnStartClicked()
    {
        if (isRunning)
        {
            _relocManager.ResetSharedAnchor();
        }
        else
        {
            _relocManager.StartCloudMapRelocalization();
        }
    }

    private void OnSharedAnchorStateChanged(SharedAnchorState state)
    {
        if (state == SharedAnchorState.Initializing)
        {
            isRunning = true;
        }
        else if (state == SharedAnchorState.Stopped)
        {
            isRunning = false;
        }
    }

    private void OnRelocalized()
    {
        _startButton.gameObject.SetActive(false);
    }
}
