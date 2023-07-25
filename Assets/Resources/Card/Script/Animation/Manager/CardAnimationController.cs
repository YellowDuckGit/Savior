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
    private bool lookingAt = false;
    private static bool isScaleHand = false;
    private static bool isHoverCard = false;
    private static bool isHandCamera = false;
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
        //if (lookingAt && !isHoverCardAnimation && Input.GetMouseButtonDown(1))
        //{
        //    lookingAt = false;
        //    isHandCamera = false;
        //    CameraManager.instance.SwitchCamera(CameraManager.ChanelCamera.Normal);
        //    StartCoroutine(ScaleHandDown());
        //}
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
       

        if (isScaleHand)
        {
            StartCoroutine(PlayAnimationHover(0));
        }
        else
        {
            StartCoroutine(PlayAnimationHover(0.3f));
        }

    }

    IEnumerator PlayAnimationHover(float seconds)
    {

        yield return new WaitForSeconds(seconds);

        StartCoroutine(ScaleHandUp(0));

        if (MMF_Hover == null)
            MMF_Hover = AnimationCardManager.instance.CreateAnimationFB_Hover(Card);
        MMF_Hover.StopFeedbacks();
        MMF_Hover.Direction = MMFeedbacks.Directions.TopToBottom;
        MMF_Hover.PlayFeedbacks();

        //if (isHandCamera)
        //{
        //    print("LookAtHand");
        //    CameraManager.instance.LookAtHand(Card);
        //}

        isHoverCardAnimation = true;
        isHoverCard = true;
    }

    public void PlayUnHover()
    {
        StartCoroutine(PlayAnimationUnHover(0));
    }


    IEnumerator PlayAnimationUnHover(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        StartCoroutine(ScaleHandDown(2f));
        if (MMF_Hover == null)
            MMF_Hover = AnimationCardManager.instance.CreateAnimationFB_Hover(Card);
        MMF_Hover.StopFeedbacks();
        MMF_Hover.Direction = MMFeedbacks.Directions.BottomToTop;
        MMF_Hover.PlayFeedbacks();

        isHoverCardAnimation = false;

        isHoverCard = false;
    }

    IEnumerator ScaleHandUp(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (isHoverCardAnimation && !isScaleHand)
        {
            if (layoutGroup3D == null) layoutGroup3D = Card.CardPlayer.hand.GetComponent<LayoutGroup3D>();
            layoutGroup3D.RadiusSpace = 4.5f;
            layoutGroup3D.RebuildLayout();
            isScaleHand = true;
        }
    }

    IEnumerator ScaleHandDown(float seconds)
    {
        yield return new WaitForSeconds(seconds);

      

        if (!isHoverCardAnimation && isScaleHand && !isHoverCard)
        {
            if (layoutGroup3D == null) layoutGroup3D = Card.CardPlayer.hand.GetComponent<LayoutGroup3D>();
            layoutGroup3D.RadiusSpace = 3.8f;
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
                lookingAt = true;
                isHandCamera = true;
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
