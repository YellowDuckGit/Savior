using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FB_CardDestroy : MonoBehaviour
{
    // Start is called before the first frame update
    public MMF_Player CardDestroyFeedBacks;
    public Transform target;
    public AnimationCurve curvePosition;

    private void Start()
    {
        setupFB();
    }

    public void setupFB()
    {
        MMF_Scale scale = new MMF_Scale();
        scale.AnimateScaleTarget = target;
        scale.Mode = MMF_Scale.Modes.Absolute;

        scale.FeedbackDuration = 0.2f;
        scale.RemapCurveZero = 0f;
        scale.RemapCurveOne = 0.1f;

        scale.AnimateX = true;
        scale.AnimateY = true;
        scale.AnimateZ = true;
        scale.AnimateScaleTweenX = new MMTweenType(curvePosition);
        scale.AnimateScaleTweenY = new MMTweenType(curvePosition);
        scale.AnimateScaleTweenZ = new MMTweenType(curvePosition);
        scale.AnimateScaleX = curvePosition;
        scale.AnimateScaleX = curvePosition;
        scale.AnimateScaleX = curvePosition;
        CardDestroyFeedBacks.AddFeedback(scale);

        CardDestroyFeedBacks.Initialization();

    }
}
