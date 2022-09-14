using UnityEngine;
using UnityEngine.UI;

namespace PretiaArCloud.Samples.ShooterSample
{
    public class ErrorPrompt : MonoBehaviour
    {
        [SerializeField] private Button _button = default;

        private void OnEnable()
        {
            _button.onClick.AddListener(Hide);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(Hide);
        }

        private void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}