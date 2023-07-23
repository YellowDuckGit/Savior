using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class CardAnimationController : MonoBehaviour
{
    public enum CardAnimation
    {
        DrawCard
    }

    //[MMInspectorButton("Spell")]
    //public bool Play;

    //[Space(10)]
    //[Header("Init Animation")]
    //public bool drawCard;
    //public bool ATKCard;

    [Space(10)]
    [Header("Properties")]

    public Transform Card;
    public Transform OpponentTest;

    //public Transform Hand;
    //[Space(10)]
    //[Header("MMF")]
    //private MMF_Player MMF_DrawCard;
    //private MMF_Player MMF_ATKCard;
    private MMF_Player MMF_DestroyCard;
    private MMF_Player MMF_GetDamage;
    private MMF_Player MMF_Hover;

    private void Start()
    {
        MMF_DestroyCard = AnimationCardManager.instance.CreateAnimationFB_Destroy(Card);
        MMF_GetDamage = AnimationCardManager.instance.CreateAnimationFB_GetDamage(Card);
        

    }

    public void PlayDrawCard(Transform hand)
    {
        MMF_Player mMF_Player = AnimationCardManager.instance.CreateAnimationFB_DrawCard(Card, hand);
        mMF_Player.PlayFeedbacks();
    }

    public void PlayATKCard(Transform own ,Transform Opponent)
    {
       
        MMF_Player mMF_Player = AnimationCardManager.instance.CreateAnimationFB_ATK(own, Opponent);
        mMF_Player.PlayFeedbacks();
        print("PlayATKCard");
    }
    public void PlayDestroyCard()
    {
        MMF_DestroyCard.PlayFeedbacks();
        print("PlayDestroyCard");
    }

    public void PlayHover()
    {
        if(MMF_Hover == null)
            MMF_Hover = AnimationCardManager.instance.CreateAnimationFB_Hover(Card);

        MMF_Hover.StopFeedbacks();
        MMF_Hover.Direction = MMFeedbacks.Directions.TopToBottom;
        MMF_Hover.PlayFeedbacks();

    }

    public void PlayUnHover()
    {
        if (MMF_Hover == null)
            MMF_Hover = AnimationCardManager.instance.CreateAnimationFB_Hover(Card);

        MMF_Hover.StopFeedbacks();
        MMF_Hover.Direction = MMFeedbacks.Directions.BottomToTop;
        MMF_Hover.PlayFeedbacks();
    }
      

    public void PlayGetDame()
    {
        MMF_GetDamage.PlayFeedbacks();
        print("PlayGetDame");

    }
}
