using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PretiaArCloud.Networking;

public class SessionInitializer : MonoBehaviour
{
    private IGameSession _gameSession = default;

    private async void Awake()
    {
        var token = PlayerPrefs.GetString(ConstantVariables.ACCESS_TOKEN_KEY);
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
