using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowProxy : MonoBehaviour
{
    public Transform targetProxy { get; set; }
    [SerializeField] float smoothing = 0.95f;

    void Update()
    {
        if (targetProxy == null) return;

        Vector3 target = targetProxy.position;
        Vector3 current = transform.position;
        Vector3 next = current + (target - current) * smoothing;
        transform.position = next;
    }
}
