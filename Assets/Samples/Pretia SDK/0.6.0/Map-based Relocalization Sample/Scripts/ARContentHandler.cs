using UnityEngine;
using PretiaArCloud;
using System;

public class ARContentHandler : MonoBehaviour
{

    [SerializeField]
    private ARSharedAnchorManager _relocManager;

    [SerializeField]
    private GameObject _arContents;

    void OnEnable()
    {
        _relocManager.OnMapRelocalized += OnMapRelocalized;
        _relocManager.OnRelocalized += OnRelocalized;
    }

    void OnDisable()
    {
        _relocManager.OnMapRelocalized -= OnMapRelocalized;
        _relocManager.OnRelocalized -= OnRelocalized;
    }

    private void OnRelocalized()
    {
        _arContents.SetActive(true);
    }

    private void OnMapRelocalized(string mapKey)
    {
        Debug.Log($"Successfully relocalized {mapKey}");
    }
}