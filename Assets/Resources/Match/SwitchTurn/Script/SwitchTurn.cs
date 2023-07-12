using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchTurn : MonoBehaviour
{
    [SerializeField] MMFeedbacks RedEffect;
    [SerializeField] MMFeedbacks BlueEffect;


    void OnMouseDown()
    {
        RedEffect.PlayFeedbacks();
    }
}
