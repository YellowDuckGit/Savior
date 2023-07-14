using DG.Tweening;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class FB_DrawCard : MonoBehaviour
{
    [MMInspectorButton("setUpFB")]
    public bool create;

    public MMF_Player drawCardFeedBack;
    public MMPathMovement path;

    public AnimationCurve pathMovementCurve;

    public Transform cardTarget;
    public Transform hand;
    private void Start()
    {
        setUpFB();
    }

    public void setUpFB()
    {
        MMF_Events event1 = new MMF_Events();
        UnityEvent ev1 = new UnityEvent();
        ev1.AddListener(() => CameraManager.instance.SwitchCamera(CameraManager.ChanelCamera.SkipTurn));
        ev1.AddListener(() => path.MovementSpeed = 3);
        event1.PlayEvents = ev1;
        drawCardFeedBack.AddFeedback(event1);

        MMF_Pause pause1 = new MMF_Pause();
        pause1.PauseDuration = 0.5f;
        drawCardFeedBack.AddFeedback(pause1);


        MMF_Rotation rotation1 = new MMF_Rotation();
        rotation1.AnimateRotationTarget = cardTarget;

        rotation1.FeedbackDuration = 1f;
        rotation1.RemapCurveZero = 90f;
        rotation1.RemapCurveOne = 110f;

        rotation1.AnimateX = true;
        rotation1.AnimateY = false;
        rotation1.AnimateZ = false;
        rotation1.AnimateRotationTweenX = new MMTweenType(pathMovementCurve);

        rotation1.AnimateRotationX = pathMovementCurve;
        rotation1.Mode = MMF_Rotation.Modes.Additive;

        drawCardFeedBack.AddFeedback(rotation1);

        MMF_HoldingPause pause2 = new MMF_HoldingPause();
        pause2.PauseDuration = 0.8f;
        drawCardFeedBack.AddFeedback(pause2);

        MMF_Events event2 = new MMF_Events();
        UnityEvent ev2 = new UnityEvent();
        ev2.AddListener(() => SetCardInHand());
        ev2.AddListener(() => CameraManager.instance.SwitchCamera(CameraManager.ChanelCamera.Normal));
        event2.PlayEvents = ev2;
        drawCardFeedBack.AddFeedback(event2);

        //MMF_Rotation rotation2 = new MMF_Rotation();
        //rotation2.AnimateRotationTarget = cardTarget;

        //rotation2.FeedbackDuration = 0.2f;
        //rotation2.RemapCurveZero = 0f;
        //rotation2.RemapCurveOne = 90f;

        //rotation2.AnimateX = true;
        //rotation2.AnimateY = false;
        //rotation2.AnimateZ = false;
        //rotation2.AnimateRotationTweenX = new MMTweenType(pathMovementCurve);

        //rotation2.AnimateRotationX = pathMovementCurve;
        //rotation2.Mode = MMF_Rotation.Modes.Absolute;

        //drawCardFeedBack.AddFeedback(rotation2);


        //Target Object
        //MMPropertyReceiver mMF_Receiver = new MMPropertyReceiver();
        //mMF_Receiver.TargetObject = cardTarget.gameObject;

        //MMF_Property mMF_Property = new MMF_Property();
        //mMF_Property.Target = mMF_Receiver;
        //mMF_Property.Target.TargetComponent = cardTarget.transform;
        //mMF_Property.Target.TargetPropertyName = "rotation";

        //mMF_Property.RelativeValues = true;
        //mMF_Property.Target.ModifyX = true;
        //mMF_Property.Target.ModifyY = false;
        //mMF_Property.Target.ModifyZ = false;


        //mMF_Property.Mode = MMF_Property.Modes.Instant;
        //mMF_Property.Target.Level = 1;
        //mMF_Property.Target.QuaternionRemapOne = new Vector3(0f, 0f, 0f);
        //mMF_Property.Target.QuaternionRemapOne = new Vector3(-100f, 0f, 180f);

        //mMF_Property.InstantLevel = 1f;

        //drawCardFeedBack.AddFeedback(mMF_Property);


        drawCardFeedBack.Initialization();
    }


    public void SetCardInHand()
    {
        GameObject ParentCard = new GameObject();
        ParentCard.transform.position = cardTarget.transform.position;
        cardTarget.transform.parent = ParentCard.transform;
        ParentCard.transform.parent = hand.transform;
        cardTarget.transform.localPosition = Vector3.zero;
        //cardTarget.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
    }
}
