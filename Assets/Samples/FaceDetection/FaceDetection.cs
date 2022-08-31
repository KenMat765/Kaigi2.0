using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class FaceDetection : MonoBehaviour
{
    ARFace arFace;
    [SerializeField] GameObject dotPrefab;
    List<GameObject> dots;

    void Start()
    {
        dots = new List<GameObject>();
        arFace = GetComponent<ARFace>();
    }

    void Update()
    {
        if (arFace.vertices.IsCreated)
        {
            foreach (Vector3 verticle in arFace.vertices)
            {
                var dot = GetFreeDot();
                dot.transform.localPosition = verticle;
            }
        }
        else
        {
            foreach (GameObject dot in dots)
            {
                dot.SetActive(false);
            }
        }
    }

    GameObject GetFreeDot()
    {
        if (dots.Count == 0)
        {
            GameObject new_dot = Instantiate(dotPrefab, transform);
            new_dot.SetActive(true);
            dots.Add(new_dot);
            return new_dot;
        }
        else
        {
            foreach (GameObject dot in dots)
            {
                if (!dot.activeSelf)
                {
                    dot.SetActive(true);
                    return dot;
                }
            }
            GameObject new_dot = Instantiate(dotPrefab, transform);
            new_dot.SetActive(true);
            dots.Add(new_dot);
            return new_dot;
        }
    }
}
