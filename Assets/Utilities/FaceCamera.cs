using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    public bool faceCamera = true;
    public bool gradually = false;
    public float angularVelocity;

    void Update()
    {
        if (!faceCamera) return;

        Vector3 relative_vector = DeviceInfo.I.transform.position - transform.position;

        if (relative_vector == Vector3.zero) return;

        Quaternion destination = Quaternion.LookRotation(relative_vector);
        if (gradually) transform.rotation = Quaternion.RotateTowards(transform.rotation, destination, angularVelocity * Time.deltaTime);
        else transform.rotation = destination;
    }
}
