using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PretiaArCloud.Networking;
using UnityEngine.SceneManagement;

public class TokenGenerator : MonoBehaviour
{
    [SerializeField] Button tokenButton;
    [SerializeField] GameObject errorPrompt;

    private void OnEnable() => tokenButton.onClick.AddListener(Login);
    private void OnDisable() => tokenButton.onClick.RemoveListener(Login);

    async void Login()
    {
        try
        {
            if (await NetworkManager.Instance.ConnectAsync())
            {
                string guest = $"{SystemInfo.deviceUniqueIdentifier}_{System.DateTime.Now.Second}";
                var (statusCode, token, displayName) = await NetworkManager.Instance.GuestLoginAsync(guest);

                if (statusCode == NetworkStatusCode.Success)
                {
                    PlayerPrefs.SetString(ConstantVariables.ACCESS_TOKEN_KEY, StringEncoder.Instance.GetString(token));
                    SceneManager.LoadScene("MultipleMain");
                }
                else
                {
                    Debug.LogError($"Failed to login: {statusCode}");
                    errorPrompt.SetActive(true);
                }
            }
        }
        catch
        {
            errorPrompt.SetActive(true);
            throw;
        }
    }
}
