using PretiaArCloud.Networking;
using UnityEngine;

namespace PretiaArCloud.Samples.ShooterSample
{
    public class GameSessionInitializer : MonoBehaviour
    {
        private IGameSession _gameSession = default;

        private async void Awake()
        {
            var token = PlayerPrefs.GetString(Constants.ACCESS_TOKEN_KEY);
            _gameSession = await NetworkManager.Instance.RequestRandomMatchAsync(token);
        }

        private void OnApplicationQuit()
        {
            if (!_gameSession.Disposed)
            {
                _gameSession.Dispose();
            }
        }
    }
}