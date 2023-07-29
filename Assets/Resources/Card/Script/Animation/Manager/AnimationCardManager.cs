using Cinemachine;
using MoreMountains.Feedbacks;
using MoreMountains.FeedbacksForThirdParty;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;
using UnityEngine.XR;

public class AnimationCardManager : MonoBehaviour
{
    public enum SpellColor
    {
        Violet, White, Blue, Green
    }

    public static AnimationCardManager instance;

    [Header("Draw Card")]
    public MMPathMovement path;
    public AnimationCurve DrawCard_Curve;

    [Header("ATK Card")]
    public AnimationCurve curveDestination;
    public AnimationCurve ATK_curveScaleXDestination;
    public AnimationCurve ATK_curveScaleYDestination;
    public AnimationCurve ATK_curveScaleZDestination;

    public AnimationCurve ATK_BackPositionZRed;
    public AnimationCurve ATK_BackPositionZBlue;
    public AnimationCurve ATK_BackPositionY;



    [Header("Destroy Card")]
    public AnimationCurve Destroy_curve;

    [Header("Hover Card")]

    public AnimationCurve Hover_curveY;

    public LayoutGroup3D redHand;
    public LayoutGroup3D blueHand;


    [Header("Get Damage")]
    public AnimationCurve GetDamage_curve;


    [Header("SpellShot")]
    public GameObject SpellViolet;
    public GameObject SpellWhite;
    public GameObject SpellBlue;
    public GameObject SpellGreen;

    public AnimationCurve SpellShot_curveDestination;
    public AnimationCurve SpellShot_curveScaleXDestination;
    public AnimationCurve SpellShot_curveScaleYDestination;
    public AnimationCurve SpellShot_curveScaleZDestination;


    private void Awake()
    {
        //DontDestroyOnLoad(gameObject);

        if (instance != null && instance != this)
        {
            Debug.LogError("UIManager have 2");
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }


    public MMF_Player CreateAnimationFB_DrawCard(Transform targetAnimated, Transform Hand)
    {
        string name = "FB_DrawCard";

        MMF_Player mMF_Player = CreateMMF_PlayerContainer(targetAnimated, name, true);

        MMF_Events event1 = new MMF_Events();
        UnityEvent ev1 = new UnityEvent();
        ev1.AddListener(() => CameraManager.instance.SwitchCamera(CameraManager.ChanelCamera.SkipTurn));
        ev1.AddListener(() => path.MovementSpeed = 3);
        event1.PlayEvents = ev1;
        mMF_Player.AddFeedback(event1);

        MMF_Pause pause1 = new MMF_Pause();
        pause1.PauseDuration = 0.5f;
        mMF_Player.AddFeedback(pause1);


        MMF_Rotation rotation1 = new MMF_Rotation();
        rotation1.AnimateRotationTarget = targetAnimated;

        rotation1.FeedbackDuration = 1f;
        rotation1.RemapCurveZero = 90f;
        rotation1.RemapCurveOne = 110f;

        rotation1.AnimateX = true;
        rotation1.AnimateY = false;
        rotation1.AnimateZ = false;
        rotation1.AnimateRotationTweenX = new MMTweenType(DrawCard_Curve);

        rotation1.AnimateRotationX = DrawCard_Curve;
        rotation1.Mode = MMF_Rotation.Modes.Additive;

        mMF_Player.AddFeedback(rotation1);

        MMF_HoldingPause pause2 = new MMF_HoldingPause();
        pause2.PauseDuration = 0.8f;
        mMF_Player.AddFeedback(pause2);

        MMF_Events event2 = new MMF_Events();
        UnityEvent ev2 = new UnityEvent();
        ev2.AddListener(() => SetCardInHand(targetAnimated,Hand));
        ev2.AddListener(() => CameraManager.instance.SwitchCamera(CameraManager.ChanelCamera.Normal));
        event2.PlayEvents = ev2;
        mMF_Player.AddFeedback(event2);

        //MMF_Rotation rotation2 = new MMF_Rotation();
        //rotation2.AnimateRotationTarget = cardTarget;

        //rotation2.FeedbackDuration = 0.2f;
        //rotation2.RemapCurveZero = 0f;
        //rotation2.RemapCurveOne = 90f;

        //rotation2.AnimateX = true;
        //rotation2.AnimateY = false;
        //rotation2.AnimateZ = false;
        //rotation2.AnimateRotationTweenX = new MMTweenType(pathMovementCurve);

        //rotation2.AnimateRotationX = pathMovementCurve;
        //rotation2.Mode = MMF_Rotation.Modes.Absolute;

        //drawCardFeedBack.AddFeedback(rotation2);


        //Target Object
        //MMPropertyReceiver mMF_Receiver = new MMPropertyReceiver();
        //mMF_Receiver.TargetObject = cardTarget.gameObject;

        //MMF_Property mMF_Property = new MMF_Property();
        //mMF_Property.Target = mMF_Receiver;
        //mMF_Property.Target.TargetComponent = cardTarget.transform;
        //mMF_Property.Target.TargetPropertyName = "rotation";

        //mMF_Property.RelativeValues = true;
        //mMF_Property.Target.ModifyX = true;
        //mMF_Property.Target.ModifyY = false;
        //mMF_Property.Target.ModifyZ = false;


        //mMF_Property.Mode = MMF_Property.Modes.Instant;
        //mMF_Property.Target.Level = 1;
        //mMF_Property.Target.QuaternionRemapOne = new Vector3(0f, 0f, 0f);
        //mMF_Property.Target.QuaternionRemapOne = new Vector3(-100f, 0f, 180f);

        //mMF_Property.InstantLevel = 1f;

        //drawCardFeedBack.AddFeedback(mMF_Property);
        mMF_Player.Initialization();

        return mMF_Player;
    }

    public MMF_Player CreateAnimationFB_ATK(CardBase targetAnimated, CardBase targetOpponent)
    {
        string name = "FB_ATK";

        MMF_Player mMF_Player = CreateMMF_PlayerContainer(targetAnimated.transform, name, true);

        //MMF_Scale scale = new MMF_Scale();
        //scale.AnimateScaleTarget = targetAnimated.transform;
        //scale.Mode = MMF_Scale.Modes.Additive;

        //scale.FeedbackDuration = 0.2f;
        //scale.RemapCurveZero = 0f;
        //scale.RemapCurveOne = 0.02f;

        //scale.AnimateX = true;
        //scale.AnimateY = true;
        //scale.AnimateZ = true;
        //scale.AnimateScaleTweenX = new MMTweenType(ATK_curveScaleXDestination);
        //scale.AnimateScaleTweenY = new MMTweenType(ATK_curveScaleYDestination);
        //scale.AnimateScaleTweenZ = new MMTweenType(ATK_curveScaleZDestination);
        //scale.AnimateScaleX = ATK_curveScaleXDestination;
        //scale.AnimateScaleX = ATK_curveScaleYDestination;
        //scale.AnimateScaleX = ATK_curveScaleZDestination;
        //mMF_Player.AddFeedback(scale);



        //MMF_Position position = new MMF_Position();
        //position.AnimatePositionTarget = targetAnimated.gameObject;

        //position.Mode = MMF_Position.Modes.AlongCurve;
        //position.FeedbackDuration = 0.2f;
        //position.RemapCurveZero = 0f;
        //position.RemapCurveOne = 0.2f;

        //position.AnimateX = false;
        //position.AnimateY = false;
        //position.AnimateZ = true;

        //if (targetAnimated.CardPlayer.side.Equals(K_Player.K_PlayerSide.Blue))
        //{
        //    position.AnimatePositionTweenZ = new MMTweenType(ATK_BackPositionZBlue);
        //    position.AnimatePositionCurveZ = ATK_BackPositionZBlue;
        //}
        //else if (targetAnimated.CardPlayer.side.Equals(K_Player.K_PlayerSide.Red))
        //{
        //    position.AnimatePositionTweenZ = new MMTweenType(ATK_BackPositionZRed);
        //    position.AnimatePositionCurveZ = ATK_BackPositionZRed;
        //}


        //position.AnimatePositionTweenY = new MMTweenType(ATK_BackPositionY);
        //position.AnimatePositionCurveY = ATK_BackPositionY;

        //position.InitialPositionTransform = targetAnimated.transform;
        //position.InitialPosition = new Vector3(0f, 0f, 0f);

        //mMF_Player.AddFeedback(position);



        //MMF_HoldingPause pause1 = new MMF_HoldingPause();
        //pause1.PauseDuration = 0.4f;
        //mMF_Player.AddFeedback(pause1);

        MMF_DestinationTransform mMFeedbackDestinationTransform = new MMF_DestinationTransform();
        mMFeedbackDestinationTransform.TargetTransform = targetAnimated.transform;
        mMFeedbackDestinationTransform.Destination = targetOpponent.transform;

        mMFeedbackDestinationTransform.SeparatePositionCurve = true;
        mMFeedbackDestinationTransform.AnimatePositionTween = new MMTweenType(curveDestination);
        mMFeedbackDestinationTransform.GlobalAnimationCurve = curveDestination;
        mMFeedbackDestinationTransform.Duration = 0.6f;

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

        mMF_Player.AddFeedback(mMFeedbackDestinationTransform);   
        
        mMF_Player.Initialization();

        return mMF_Player;
    }

    public MMF_Player CreateAnimationFB_Destroy(Transform targetAnimated)
    {
        string name = "FB_CardDestroy";

        MMF_Player mMF_Player = CreateMMF_PlayerContainer(targetAnimated, name, false);

        MMF_Scale scale = new MMF_Scale();
        scale.AnimateScaleTarget = targetAnimated;
        scale.Mode = MMF_Scale.Modes.Absolute;

        scale.FeedbackDuration = 0.2f;
        scale.RemapCurveZero = 0f;
        scale.RemapCurveOne = 0.1f;

        scale.AnimateX = true;
        scale.AnimateY = true;
        scale.AnimateZ = true;
        scale.AnimateScaleTweenX = new MMTweenType(Destroy_curve);
        scale.AnimateScaleTweenY = new MMTweenType(Destroy_curve);
        scale.AnimateScaleTweenZ = new MMTweenType(Destroy_curve);
        scale.AnimateScaleX = Destroy_curve;
        scale.AnimateScaleX = Destroy_curve;
        scale.AnimateScaleX = Destroy_curve;
        mMF_Player.AddFeedback(scale);

        mMF_Player.Initialization();

        return mMF_Player;

    }

    public MMF_Player CreateAnimationFB_Hover(CardBase targetAnimated)
    {
        string name = "FB_HoverCard";

        MMF_Player mMF_Player = CreateMMF_PlayerContainer(targetAnimated.transform, name, true);

     

        MMF_Position position = new MMF_Position();
        position.AnimatePositionTarget = targetAnimated.gameObject;
        position.Mode = MMF_Position.Modes.AlongCurve;
        position.FeedbackDuration = 0.3f;
        position.RemapCurveZero = 0f;
        position.RemapCurveOne = 0.6f;
        position.AnimateX = false;
        position.AnimateY = true;
        position.AnimateZ = true;
        position.AnimatePositionTweenY = new MMTweenType(Hover_curveY);
        position.AnimatePositionCurveY = Hover_curveY;
        position.AllowAdditivePlays = false;


        if (targetAnimated.CardPlayer.side.Equals(K_Player.K_PlayerSide.Blue))
        {
            AnimationCurve curve = new AnimationCurve(new Keyframe[2] { new Keyframe(0, 0), new Keyframe(1, -0.2f) });
            position.AnimatePositionTweenZ = new MMTweenType(curve);
            position.AnimatePositionCurveZ = curve;
        }
        else if (targetAnimated.CardPlayer.side.Equals(K_Player.K_PlayerSide.Red))
        {
            AnimationCurve curve = new AnimationCurve(new Keyframe[2] { new Keyframe(0, 0), new Keyframe(1, 0.2f) });
            position.AnimatePositionTweenZ = new MMTweenType(curve);
            position.AnimatePositionCurveZ = curve;
        }

        position.InitialPositionTransform = targetAnimated.transform;
        //position.InitialPosition = new Vector3(0f, 0f, 0f);
        mMF_Player.AddFeedback(position);


        //MMF_Rotation rotation = new MMF_Rotation();
        //rotation.RotationSpace = Space.World;
        //rotation.AnimateRotationDuration = 0.3f;

        //rotation.AnimateRotationTarget = targetAnimated.transform.parent.transform;
        //rotation.Mode = MMF_Rotation.Modes.ToDestination;

        //if (targetAnimated.CardPlayer.side.Equals(K_Player.K_PlayerSide.Blue))
        //{
        //    rotation.DestinationAngles = new Vector3(0, 180, 0);
        //}
        //else if (targetAnimated.CardPlayer.side.Equals(K_Player.K_PlayerSide.Red))
        //{
        //    rotation.DestinationAngles = new Vector3(0, 0, 0);
        //}

        //mMF_Player.AddFeedback(rotation);


        MMF_Events event1 = new MMF_Events();
        UnityEvent ev1 = new UnityEvent();

        if (targetAnimated.CardPlayer.side.Equals(K_Player.K_PlayerSide.Blue))
        {
            if (blueHand == null) blueHand = targetAnimated.CardPlayer.hand.gameObject.GetComponent<LayoutGroup3D>();
            ev1.AddListener(() => blueHand.RebuildLayout());
        }
        else if (targetAnimated.CardPlayer.side.Equals(K_Player.K_PlayerSide.Red))
        {
            if (redHand == null) redHand = targetAnimated.CardPlayer.hand.gameObject.GetComponent<LayoutGroup3D>();
            ev1.AddListener(() => redHand.RebuildLayout());
        }
        event1.PlayEvents = ev1;
        mMF_Player.AddFeedback(event1);

        //MMF_Pause pause1 = new MMF_Pause();
        //pause1.PauseDuration = 0.2f;
        //mMF_Player.AddFeedback(pause1);
        mMF_Player.Initialization();


        //rotation.Timing.MMFeedbacksDirectionCondition = MMFeedbackTiming.MMFeedbacksDirectionConditions.OnlyWhenForwards;
        event1.Timing.MMFeedbacksDirectionCondition = MMFeedbackTiming.MMFeedbacksDirectionConditions.OnlyWhenBackwards;

        return mMF_Player;
    }

    public MMF_Player GetAnimationFB_Hover(CardBase targetAnimated)
    {
        string name = "FB_HoverCard";
        Transform containerOld = targetAnimated.transform.Find("MMF_PlayerContainer").gameObject.transform.Find(name);
        if(containerOld != null)
        {
            MMF_Player player = containerOld.GetComponent<MMF_Player>();
            return player;
        }
        return null;
    }

    public MMF_Player CreateAnimationFB_GetDamage(Transform targetAnimated)
    {
        string name = "FB_GetDamage";

        MMF_Player mMF_Player = CreateMMF_PlayerContainer(targetAnimated, name, false);


        MMPositionShaker shake = targetAnimated.AddComponent<MMPositionShaker>();
        shake.Mode = MMPositionShaker.Modes.Transform;
        shake.TargetTransform = targetAnimated.transform;

        shake.ShakeSpeed = 20;
        shake.ShakeRange = 0.1f;
        shake.ShakeMainDirection = new Vector3(0f, 0f, -1f);

        shake.UseAttenuation = true;
        shake.AttenuationCurve = GetDamage_curve;

        MMF_PositionShake shake1 = new MMF_PositionShake();
        shake1.TargetShaker = shake;



        shake1.ShakeSpeed = 20;
        shake1.ShakeRange = 0.1f;
        shake1.ShakeMainDirection = new Vector3(0.01f, 0f, 0f);

        shake1.UseAttenuation = true;
        shake1.AttenuationCurve = GetDamage_curve;

        shake1.DirectionalNoiseStrengthMin = new Vector3(0.25f, 0f, 0.25f);
        shake1.DirectionalNoiseStrengthMax = new Vector3(0.25f, 0f, 0.25f);


        mMF_Player.AddFeedback(shake1);
        mMF_Player.Initialization();

        return mMF_Player;
    }

    public MMF_Player CreateAnimationFB_SpellShot(Transform Card,Transform targetOpponent, SpellColor spellColor)
    {
        GameObject prefab = null;

        switch (spellColor)
        {
            case SpellColor.Violet:
                prefab = SpellViolet;
                break;
            case SpellColor.White:
                prefab = SpellWhite;
                break;
            case SpellColor.Blue:
                prefab = SpellBlue;
                break;
            case SpellColor.Green:
                prefab = SpellGreen;
                break;
        }

        GameObject effect = GameObject.Instantiate(prefab);

        string name = "FB_SpellShot";

        MMF_Player mMF_Player = CreateMMF_PlayerContainer(Card, name, true, effect);

        MMF_Scale scale = new MMF_Scale();
        scale.AnimateScaleTarget = effect.transform;
        scale.Mode = MMF_Scale.Modes.Additive;

        scale.FeedbackDuration = 0.2f;
        scale.RemapCurveZero = 0f;
        scale.RemapCurveOne = 0.02f;

        scale.AnimateX = true;
        scale.AnimateY = true;
        scale.AnimateZ = true;
        scale.AnimateScaleTweenX = new MMTweenType(SpellShot_curveScaleXDestination);
        scale.AnimateScaleTweenY = new MMTweenType(SpellShot_curveScaleYDestination);
        scale.AnimateScaleTweenZ = new MMTweenType(SpellShot_curveScaleZDestination);
        scale.AnimateScaleX = SpellShot_curveScaleXDestination;
        scale.AnimateScaleX = SpellShot_curveScaleYDestination;
        scale.AnimateScaleX = SpellShot_curveScaleZDestination;

        //scale.AnimateScaleTweenZ = new MMTweenType(curveScaleDestination);
        //scale.AnimateScaleZ = curveScaleDestination;

        mMF_Player.AddFeedback(scale);

        MMF_HoldingPause pause1 = new MMF_HoldingPause();
        pause1.PauseDuration = 0.2f;
        mMF_Player.AddFeedback(pause1);

        MMF_DestinationTransform mMFeedbackDestinationTransform = new MMF_DestinationTransform();
        mMFeedbackDestinationTransform.TargetTransform = effect.transform;
        mMFeedbackDestinationTransform.ForceOrigin = false;
        mMFeedbackDestinationTransform.Destination = targetOpponent;

        mMFeedbackDestinationTransform.SeparatePositionCurve = true;
        mMFeedbackDestinationTransform.AnimatePositionTween = new MMTweenType(SpellShot_curveDestination);
        mMFeedbackDestinationTransform.GlobalAnimationCurve = SpellShot_curveDestination;
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

        mMF_Player.AddFeedback(mMFeedbackDestinationTransform);

        mMF_Player.Initialization();

        return mMF_Player;
    }

    public void CreateAniamtionCamera(ref MMF_Player mMF_Player, int firstChanel)
    {

        MMF_Events cameraNormal = new MMF_Events();
       
        MMF_CinemachineTransition mMF_CinemachineTransition0 = new MMF_CinemachineTransition();
        mMF_CinemachineTransition0.TargetVirtualCamera = mMF_Player.gameObject.transform.Find("NormalCamera").GetComponent<CinemachineVirtualCamera>();
        mMF_CinemachineTransition0.Channel = firstChanel;
        firstChanel++;

        MMF_CinemachineTransition mMF_CinemachineTransition1 = new MMF_CinemachineTransition();
        mMF_CinemachineTransition1.TargetVirtualCamera = mMF_Player.gameObject.transform.Find("BoardCamera").GetComponent<CinemachineVirtualCamera>();
        mMF_CinemachineTransition1.Channel = firstChanel;
        firstChanel++;


        MMF_CinemachineTransition mMF_CinemachineTransition2 = new MMF_CinemachineTransition();
        mMF_CinemachineTransition2.TargetVirtualCamera = mMF_Player.gameObject.transform.Find("HandCamera").GetComponent<CinemachineVirtualCamera>();
        mMF_CinemachineTransition2.Channel = firstChanel;
        firstChanel++;


        MMF_CinemachineTransition mMF_CinemachineTransition3 = new MMF_CinemachineTransition();
        mMF_CinemachineTransition3.TargetVirtualCamera = mMF_Player.gameObject.transform.Find("MP_HPCamera").GetComponent<CinemachineVirtualCamera>();
        mMF_CinemachineTransition3.Channel = firstChanel;
        firstChanel++;


        MMF_CinemachineTransition mMF_CinemachineTransition4 = new MMF_CinemachineTransition();
        mMF_CinemachineTransition4.TargetVirtualCamera = mMF_Player.gameObject.transform.Find("SkipTurn_Camera").GetComponent<CinemachineVirtualCamera>();
        mMF_CinemachineTransition4.Channel = firstChanel;
        firstChanel++;


        MMF_CinemachineTransition mMF_CinemachineTransition5 = new MMF_CinemachineTransition();
        mMF_CinemachineTransition5.TargetVirtualCamera = mMF_Player.gameObject.transform.Find("CardCamera").GetComponent<CinemachineVirtualCamera>();
        mMF_CinemachineTransition5.Channel = firstChanel;
        firstChanel++;


        mMF_Player.AddFeedback(mMF_CinemachineTransition0);
        mMF_Player.AddFeedback(mMF_CinemachineTransition1);
        mMF_Player.AddFeedback(mMF_CinemachineTransition2);
        mMF_Player.AddFeedback(mMF_CinemachineTransition3);
        mMF_Player.AddFeedback(mMF_CinemachineTransition4);
        mMF_Player.AddFeedback(mMF_CinemachineTransition5);

        mMF_Player.Initialization();
    }

    private void SetCardInHand(Transform CardAnimated, Transform Hand)
    {
        GameObject ParentCard = new GameObject();
        ParentCard.transform.position = CardAnimated.transform.position;
        CardAnimated.transform.parent = ParentCard.transform;
        ParentCard.transform.parent = Hand.transform;
        CardAnimated.transform.localPosition = Vector3.zero;
        //cardTarget.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
    }

    private MMF_Player CreateMMF_PlayerContainer(Transform targetAnimated, string name, bool destroyOld, GameObject childGameObject = null)
    {
        Transform containerOld = targetAnimated.Find("MMF_PlayerContainer").gameObject.transform.Find(name);
        if (containerOld != null)
        {
            if(destroyOld)
            Destroy(containerOld.gameObject);
            else return containerOld.GetComponent<MMF_Player>();    
        }

        GameObject container = targetAnimated.Find("MMF_PlayerContainer").gameObject;
        GameObject MMF_PlayerContainer = new GameObject();
        MMF_PlayerContainer.name = name;
        MMF_PlayerContainer.transform.parent = container.transform;
        MMF_PlayerContainer.AddComponent<MMF_Player>();
        
        if(childGameObject!= null)
        childGameObject.transform.parent = MMF_PlayerContainer.transform;

        return MMF_PlayerContainer.GetComponent<MMF_Player>();

    }


}
