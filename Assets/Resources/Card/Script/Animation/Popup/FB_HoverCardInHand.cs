using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FB_HoverCardInHand : MonoBehaviour
{

    public MMF_Player PopupFeedbacks;
    public AnimationCurve curvePosition;

    public Transform target;

    // Update is called once per frame

    private void Start()
    {
        MMF_Position position = new MMF_Position();
        position.AnimatePositionTarget = target.gameObject;

        position.Mode = MMF_Position.Modes.AlongCurve;
        position.FeedbackDuration = 0.2f;
        position.RemapCurveZero = 0f;
        position.RemapCurveOne = 0.4f;

        position.AnimateX = false;
        position.AnimateY = true;
        position.AnimateZ = false;

        position.AnimatePositionTweenY = new MMTweenType(curvePosition);
        position.AnimatePositionCurveY = curvePosition;

        position. InitialPositionTransform  = target.transform;
        position.InitialPosition = new Vector3(0f, 0f, 0f);

        PopupFeedbacks.AddFeedback(position);

        

        //MMPropertyReceiver mMF_Receiver = new MMPropertyReceiver();
        //mMF_Receiver.TargetObject = target.gameObject;

        //MMF_Property mMF_Property = new MMF_Property();
        //mMF_Property.Target = mMF_Receiver;
        //mMF_Property.Target.TargetComponent = target.transform;
        //mMF_Property.Target.TargetPropertyName = "position";

        //mMF_Property.RelativeValues = true;
        //mMF_Property.Target.ModifyX = false;
        //mMF_Property.Target.ModifyY = true;
        //mMF_Property.Target.ModifyZ = false;


        //mMF_Property.Mode = MMF_Property.Modes.Instant;
        //mMF_Property.Target.Level = 1;
        //mMF_Property.Target.Vector3RemapZero = new Vector3(0f, 0f, 0f);
        //mMF_Property.Target.Vector3RemapOne = new Vector3(0f, 0.5f, 0f);

        //mMF_Property.InstantLevel = 1f;

        //PopupFeedbacks.AddFeedback(mMF_Property);


        PopupFeedbacks.Initialization();
    }
}