using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverCardManager : MonoBehaviour
{
    public MMF_Player FeedBack_Hover;
    public MMF_Player FeedBack_UnHover;

    bool isHover = false;

    private void OnMouseExit()
    {
        if (isHover)
        {
            //print("FeedBack_UnHover");
            //isHover = false;
            //FeedBack_UnHover.PlayFeedbacks();
            print("FeedBack_Exit Hover");

            isHover = false;
            FeedBack_Hover.StopFeedbacks();
            FeedBack_Hover.Direction = MMFeedbacks.Directions.BottomToTop;
            FeedBack_Hover.PlayFeedbacks();
        }
    }

    private void OnMouseOver()
    {
        if (!isHover)
        {
            print("FeedBack_Hover");
            isHover = true;
            FeedBack_Hover.StopFeedbacks();
            FeedBack_Hover.Direction = MMFeedbacks.Directions.TopToBottom;
            FeedBack_Hover.PlayFeedbacks();

        }
    }
}
