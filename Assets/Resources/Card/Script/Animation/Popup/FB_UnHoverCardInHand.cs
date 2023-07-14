using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FB_UnHoverCardInHand : MonoBehaviour
{

    public MMF_Player UnPopupFeedbacks;
    //public AnimationCurve curvePosition;
    public AnimationCurve curvePosition;
    public Transform target;

    // Update is called once per frame
    void Start()
    {
        MMF_Position position = new MMF_Position();
        position.AnimatePositionTarget = target.gameObject;

        position.Mode = MMF_Position.Modes.AlongCurve;
        position.FeedbackDuration = 0.2f;
        position.RemapCurveZero = 0.8f;
        position.RemapCurveOne = 0.6f;

        position.AnimateX = false;
        position.AnimateY = true;
        position.AnimateZ = false;

        position.AnimatePositionTweenY = new MMTweenType(curvePosition);
        position.AnimatePositionCurveY = curvePosition;

        position.InitialPositionTransform = target.transform;
        position.InitialPosition = new Vector3(0f, 0f, 0f);

        UnPopupFeedbacks.AddFeedback(position);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
