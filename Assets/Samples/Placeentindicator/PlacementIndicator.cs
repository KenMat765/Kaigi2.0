using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlacementIndicator : MonoBehaviour
{
    ARRaycastManager rayManager;
    GameObject indicator;

    void Start()
    {
        rayManager = FindObjectOfType<ARRaycastManager>();
        indicator = transform.GetChild(0).gameObject;

        indicator.SetActive(false);
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            rayManager.Raycast(Input.touches[0].position, hits, TrackableType.Planes);
            if (hits.Count > 0)
            {
                ARRaycastHit hit = hits[0];
                indicator.SetActive(true);
                indicator.transform.position = hit.pose.position;
                indicator.transform.rotation = hit.pose.rotation;
            }
        }
        else
        {
            if (indicator.activeSelf) indicator.SetActive(false);
        }
    }
}
