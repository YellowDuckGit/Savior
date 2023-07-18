using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FB_ATK : MonoBehaviour
{
    [MMInspectorButton("setUpFB")]
    public bool create;

    public MMF_Player ATKFeedBacks;

    public AnimationCurve curveDestination;
    public AnimationCurve curveScaleXDestination;
    public AnimationCurve curveScaleYDestination;
    public AnimationCurve curveScaleZDestination;

    public Transform CardATK;
    public Transform CardOpponent;
    // Start is called before the first frame update
    void Start()
    {
        setupFeedback();
    }

    public void setupFeedback()
    {
        MMF_Scale scale = new MMF_Scale();
        scale.AnimateScaleTarget = CardATK;
        scale.Mode = MMF_Scale.Modes.Additive;

        scale.FeedbackDuration = 0.2f;
        scale.RemapCurveZero = 0f;
        scale.RemapCurveOne = 0.02f;

        scale.AnimateX = true;
        scale.AnimateY = true;
        scale.AnimateZ = true;
        scale.AnimateScaleTweenX = new MMTweenType(curveScaleXDestination);
        scale.AnimateScaleTweenY = new MMTweenType(curveScaleYDestination);
        scale.AnimateScaleTweenZ = new MMTweenType(curveScaleZDestination);
        scale.AnimateScaleX = curveScaleXDestination;
        scale.AnimateScaleX = curveScaleYDestination;
        scale.AnimateScaleX = curveScaleZDestination;

        //scale.AnimateScaleTweenZ = new MMTweenType(curveScaleDestination);
        //scale.AnimateScaleZ = curveScaleDestination;

        ATKFeedBacks.AddFeedback(scale);

        MMF_HoldingPause pause1 = new MMF_HoldingPause();
        pause1.PauseDuration = 0.2f;
        ATKFeedBacks.AddFeedback(pause1);

        MMF_DestinationTransform mMFeedbackDestinationTransform = new MMF_DestinationTransform();
        mMFeedbackDestinationTransform.TargetTransform = CardATK;
        mMFeedbackDestinationTransform.ForceOrigin = false; 
        mMFeedbackDestinationTransform.Destination = CardOpponent;

        mMFeedbackDestinationTransform.SeparatePositionCurve = true;
        mMFeedbackDestinationTransform.AnimatePositionTween = new MMTweenType(curveDestination);
        mMFeedbackDestinationTransform.GlobalAnimationCurve = curveDestination;
        mMFeedbackDestinationTransform.Duration = 0.2f;

        mMFeedbackDestinationTransform.AnimatePositionX = true;
        mMFeedbackDestinationTransform.AnimatePositionY = true;
        mMFeedbackDestinationTransform.AnimatePositionZ = true;
        mMFeedbackDestinationTransform.AnimateRotationX = false;
        mMFeedbackDestinationTransform.AnimateRotationY = false;
        mMFeedbackDestinationTransform.AnimateRotationZ = false;
        mMFeedbackDestinationTransform.AnimateRotationW = false;
        mMFeedbackDestinationTransform.AnimateScaleX = false;
        mMFeedbackDestinationTransform.AnimateScaleY = false;
        mMFeedbackDestinationTransform.AnimateScaleZ = false;

        ATKFeedBacks.AddFeedback(mMFeedbackDestinationTransform);



        ATKFeedBacks.Initialization();
    }
}
