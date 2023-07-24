using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
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

    public CardBase Card;

    //public Transform Hand;
    //[Space(10)]
    //[Header("MMF")]
    //private MMF_Player MMF_DrawCard;
    //private MMF_Player MMF_ATKCard;
    private MMF_Player MMF_DestroyCard;
    private MMF_Player MMF_GetDamage;
    private MMF_Player MMF_Hover;

    private static bool isHoverCardAnimation = false;

    private void Start()
    {
        MMF_DestroyCard = AnimationCardManager.instance.CreateAnimationFB_Destroy(Card.transform);
        MMF_GetDamage = AnimationCardManager.instance.CreateAnimationFB_GetDamage(Card.transform);
        this.RegisterListener(EventID.OnRightClickHoverCard, (param) => rightClickHover(param as CardBase));

    }

    public void PlayDrawCard(Transform hand)
    {
        MMF_Player mMF_Player = AnimationCardManager.instance.CreateAnimationFB_DrawCard(Card.transform, hand);
        mMF_Player.PlayFeedbacks();
    }

    public void PlayATKCard(CardBase own , CardBase Opponent)
    {
        print("AnimationATK: "+own.Name + "ATK "+ Opponent.Name);
        MMF_Player mMF_Player = AnimationCardManager.instance.CreateAnimationFB_ATK(own, Opponent);
        mMF_Player.PlayFeedbacks();
    }

    public void Test()
    {

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
        isHoverCardAnimation = true;
    }

    public void rightClickHover(CardBase card)
    {
        print("rightClickHover");

        if (card.Position.Equals(CardPosition.InHand))
        {
            if(isHoverCardAnimation)
            CameraManager.instance.SwitchCamera(CameraManager.ChanelCamera.Hand, card);
        }
    }



    public void PlayUnHover()
    {
        if (MMF_Hover == null)
            MMF_Hover = AnimationCardManager.instance.CreateAnimationFB_Hover(Card);

        MMF_Hover.StopFeedbacks();
        MMF_Hover.Direction = MMFeedbacks.Directions.BottomToTop;
        MMF_Hover.PlayFeedbacks();

        isHoverCardAnimation = false;

        StartCoroutine(DelayUnHoverSwitchCamera(2));
    }


    public void PlayGetDame()
    {
        MMF_GetDamage.PlayFeedbacks();
        print("PlayGetDame");
    }

    public IEnumerator DelayHoverSwitchCamera(float seccond, CardBase card= null)
    {
        yield return new WaitForSeconds(seccond);
        CameraManager.instance.SwitchCamera(CameraManager.ChanelCamera.Hand, card);
    }

    public IEnumerator DelayUnHoverSwitchCamera(float seccond)
    {
        yield return new WaitForSeconds(seccond);
        
        if(!isHoverCardAnimation)
        CameraManager.instance.SwitchCamera(CameraManager.ChanelCamera.Normal);
    }
}
