using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HandTracking : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI tmp;
    [SerializeField] GameObject sphere;

    void Update()
    {
        // var gesture = ManomotionManager.Instance.Hand_infos[0].hand_info.gesture_info.mano_gesture_continuous;
        // switch (gesture)
        // {
        //     case ManoGestureContinuous.NO_GESTURE:
        //         tmp.text = "No Gesture";
        //         break;

        //     case ManoGestureContinuous.HOLD_GESTURE:
        //         tmp.text = "Hold Gesture";
        //         break;

        //     case ManoGestureContinuous.OPEN_HAND_GESTURE:
        //         tmp.text = "Open Hand";
        //         break;

        //     case ManoGestureContinuous.OPEN_PINCH_GESTURE:
        //         tmp.text = "Pinch";
        //         break;

        //     case ManoGestureContinuous.CLOSED_HAND_GESTURE:
        //         tmp.text = "Closed";
        //         break;

        //     case ManoGestureContinuous.POINTER_GESTURE:
        //         tmp.text = "Pointer";
        //         break;

        //     default:
        //         tmp.text = "";
        //         break;
        // }

        TrackingInfo trackingInfo = ManomotionManager.Instance.Hand_infos[0].hand_info.tracking_info;
        Vector3 indexPoint = ManoUtils.Instance.CalculateNewPositionSkeletonJointDepth(trackingInfo.skeleton.joints[8], trackingInfo.depth_estimation);
        sphere.transform.position = indexPoint;
    }
}
