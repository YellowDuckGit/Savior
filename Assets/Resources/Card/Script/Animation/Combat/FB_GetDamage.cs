using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FB_GetDamage : MonoBehaviour
{

    [MMInspectorButton("setUpFB")]
    public bool create;

    public MMF_Player GetDamageFeedbacks;
    public AnimationCurve curvePosition;

    public Transform target;

    public MMPositionShaker positionShaker;
    public bool direction;
    public Vector3 DestinationPosition;

    private void Start()
    {
        setUpFB();
    }

    // Update is called once per frame

    public void setUpFB()
    {
        if (direction)
        {
            positionShaker.ShakeMainDirection = new Vector3(0f, 0f, 1f);
        }
        else positionShaker.ShakeMainDirection = new Vector3(0f, 0f, -1f);

        MMF_Events event1 = new MMF_Events();
        UnityEvent ev1 = new UnityEvent();
        ev1.AddListener(() => positionShaker.Play());
        event1.PlayEvents = ev1;
        GetDamageFeedbacks.AddFeedback(event1);
     
    }
}
