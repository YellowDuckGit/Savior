using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonEvent : MonoBehaviour
{

    public MMFeedbacks overFeedbacks;
    public MMFeedbacks exitFeedbacks;
    bool isOver;
    // Start is called before the first frame update

    private void OnMouseOver()
    {
        if (!isOver)
        {
            isOver = true;
            overFeedbacks.PlayFeedbacks();
        }
    }

    private void OnMouseExit()
    {
        if (isOver)
        {
            isOver = false;
            exitFeedbacks.PlayFeedbacks();
        }
    }
}
