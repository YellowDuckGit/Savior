using DG.Tweening;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class FB_DrawCard : MonoBehaviour
{
    [MMInspectorButton("setUpFB")]
    public bool create; 

    public MMF_Player drawCardFeedBack;
    public DOTweenAnimation drawCardDot;
    public MMPathMovement path;

    public AnimationCurve pathMovementCurve;

    public Transform cardTarget;
    private void Start()
    {
        setUpFB();
    }

    public void setUpFB()
    {
        if (CameraManager.instance != null) print("!+null");

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
        rotation1.FeedbackDuration = 1f;
        rotation1.RemapCurveZero = 0f;
        rotation1.RemapCurveOne = -30f;
        rotation1.AnimateX = true;
        rotation1.AnimateY = true;
        rotation1.AnimateZ = false;
        rotation1.AnimateRotationX = pathMovementCurve;
        rotation1.AnimateRotationY = pathMovementCurve;
        rotation1.AnimateRotationTarget = cardTarget;
        rotation1.Mode = MMF_Rotation.Modes.Additive;

        drawCardFeedBack.AddFeedback(rotation1);

        MMF_Pause pause2 = new MMF_Pause();
        pause2.PauseDuration = 1.2f;
        drawCardFeedBack.AddFeedback(pause2);

        MMF_Events event2 = new MMF_Events();
        UnityEvent ev2 = new UnityEvent();
        ev2.AddListener(() => CameraManager.instance.SwitchCamera(CameraManager.ChanelCamera.Normal));
        ev2.AddListener(() => drawCardDot.DOPlay());

        event2.PlayEvents = ev2;
        drawCardFeedBack.AddFeedback(event2);

    }
}
