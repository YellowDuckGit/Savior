using DG.Tweening;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR;
using static CameraManager;

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

    private bool isHoverCardAnimation = false;
    private static bool isScaleHand = false;
    private static bool isHoverCard = false;
    private static float delayUnScale = 0.5f;

    private CardAnimation cardAnimation;
    private LayoutGroup3D layoutGroup3D;
    private void Start()
    {
        MMF_DestroyCard = AnimationCardManager.instance.CreateAnimationFB_Destroy(Card.transform);
        MMF_GetDamage = AnimationCardManager.instance.CreateAnimationFB_GetDamage(Card.transform);
        this.RegisterListener(EventID.OnRightClickHoverCard, (param) => rightClickHover(param as CardBase));

    }

    private void Update()
    {
    
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

    public void PlayDestroyCard()
    {
        MMF_DestroyCard.PlayFeedbacks();
        print("PlayDestroyCard");
    }

    public void PlayHover()
    {
        print("PlayHover");
        isHoverCardAnimation = true;
        isHoverCard = true;

        if (isScaleHand)
        {
            StartCoroutine(PlayAnimationHover(0f));
        }
        else
        {
            StartCoroutine(ScaleHandUp(0));
            StartCoroutine(PlayAnimationHover(0.4f));

        }

    }

    IEnumerator PlayAnimationHover(float seconds)
    {

        yield return new WaitForSeconds(seconds);

        if (isHoverCardAnimation)
        {
            if (MMF_Hover == null)
                MMF_Hover = AnimationCardManager.instance.CreateAnimationFB_Hover(Card);
            MMF_Hover.StopFeedbacks();
            MMF_Hover.Direction = MMFeedbacks.Directions.TopToBottom;
  
            MMF_Position mMF_Position = (MMF_Position)MMF_Hover.FeedbacksList.Find(a => a is MMF_Position);
            if (mMF_Position != null)
            {
                print("change position");
                mMF_Position.InitialPositionTransform = Card.transform;
                //mMF_Position.InitialPosition = Card.transform.position;
            }
            yield return new WaitForSeconds(0.1f);

            MMF_Hover.PlayFeedbacks();

            if (CameraManager.instance.presentChannel == (int)ChanelCamera.Hand)
            {
                print("LookAtHand");
                if (Card.CardPlayer.side.Equals(K_Player.K_PlayerSide.Blue))
                {
                    CameraManager.instance.LookAtBlueHand(Card);

                }
                else if (Card.CardPlayer.side.Equals(K_Player.K_PlayerSide.Red))
                {
                    CameraManager.instance.LookAtRedHand(Card);
                }
            }
        }
        yield return new WaitForSeconds(0.5f);
    }

    public void PlayUnHover()
    {
        isHoverCardAnimation = false;
        isHoverCard = false;
        StartCoroutine(PlayAnimationUnHover(0));
        StartCoroutine(ScaleHandDown(2f));

    }

    IEnumerator PlayAnimationUnHover(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (!isHoverCardAnimation)
        {
            if (MMF_Hover == null)
                MMF_Hover = AnimationCardManager.instance.CreateAnimationFB_Hover(Card);
            MMF_Hover.StopFeedbacks();
            MMF_Hover.Direction = MMFeedbacks.Directions.BottomToTop;

            MMF_Position mMF_Position = (MMF_Position)MMF_Hover.FeedbacksList.Find(a => a is MMF_Position);
            if (mMF_Position != null)
            {
                print("change position");
                mMF_Position.InitialPositionTransform = Card.transform;
                //mMF_Position.InitialPosition = Card.transform.position;
            }
            MMF_Hover.PlayFeedbacks();
        }
    }

    IEnumerator ScaleHandUp(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (isHoverCardAnimation && !isScaleHand)
        {
            if (layoutGroup3D == null) layoutGroup3D = Card.CardPlayer.hand.GetComponent<LayoutGroup3D>();
            layoutGroup3D.RadiusSpace = 5.5f;
            layoutGroup3D.RebuildLayout();
            isScaleHand = true;
        }
    }

    IEnumerator ScaleHandDown(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (!isHoverCard)
        {
            Card.CardPlayer.hand.resetPositionCardInHand();

            if (layoutGroup3D == null) layoutGroup3D = Card.CardPlayer.hand.GetComponent<LayoutGroup3D>();
            layoutGroup3D.RadiusSpace = 4f;
            layoutGroup3D.RebuildLayout();
            isScaleHand = false;
            
        }
    }

    public void rightClickHover(CardBase card)
    {
        if (card.Position.Equals(CardPosition.InHand))
        {
            if (isHoverCardAnimation)
            {
                print("CameraManager.instance.SwitchCamera(CameraManager.ChanelCamera.Hand, card);");
                CameraManager.instance.SwitchCamera(CameraManager.ChanelCamera.Hand, card);
            }
        }
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
