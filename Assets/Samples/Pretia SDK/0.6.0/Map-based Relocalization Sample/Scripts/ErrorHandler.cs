using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PretiaArCloud;

public class ErrorHandler : MonoBehaviour
{

    [SerializeField]
    private ARSharedAnchorManager _relocManager;

    [SerializeField]
    private GameObject _errorPanel;

    [SerializeField]
    private Text _errorLabel;

    void Start()
    {
        _errorPanel.SetActive(false);
    }

    void OnEnable()
    {
        _relocManager.OnException += OnException;
        _relocManager.OnLocationPermissionDenied += ShowLocationPermissionDeniedError;
    }

    void Update()
    {
        HideErrorIfClicked();
    }

    void OnDisable()
    {
        _relocManager.OnException -= OnException;
        _relocManager.OnLocationPermissionDenied -= ShowLocationPermissionDeniedError;
    }

    private void HideErrorIfClicked()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            List<RaycastResult> results = new List<RaycastResult>();

            pointerData.position = Input.GetTouch(0).position;
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (RaycastResult result in results)
            {
                if (result.gameObject.name == "ErrorLabel" || result.gameObject.name == "ErrorPanel")
                {
                    _errorPanel.SetActive(false);
                }
            }
        }
    }

    private void OnException(Exception exception)
    {
        ShowError(exception.Message);
    }

    private void ShowLocationPermissionDeniedError()
    {
        ShowError("Location permission is denied for this application. Please enable it manually");
    }

    private void ShowError(string errorMessage)
    {
        _errorLabel.text = errorMessage;
        _errorPanel.SetActive(true);
    }

}