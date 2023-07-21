using JetBrains.Annotations;
using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static K_Player;

public class UIMatchManager : MonoBehaviour
{
    public static UIMatchManager instance;

    [SerializeField] MatchManager matchManager;
    /// <summary>
    /// Parent GameObject is GameObject which store list children GameObject
    /// Variable bellow use for case you need load list gameobject into parent GameObject
    /// EX: Load list Card into CollectionCard
    /// </summary>
    [Space(5)]
    [SerializeField] GameObject PanelResult;
    [SerializeField] GameObject PanelLoading;
    [SerializeField] GameObject PanelSaviorLogicOption;



    [Space(10)]

    /// <summary>
    /// Variables bellow will be updated one or more times in game
    /// </summary>
    [Header("Loading Data")]
    [Space(5)]
    //Create Card Scene
    //[SerializeField] TextMeshProUGUI T_Turn;
    [SerializeField] MMF_Player lightBlue;
    [SerializeField] MMF_Player lightRed;

    [SerializeField] MMF_Player SkipTurnRed;
    [SerializeField] MMF_Player SkipTurnBlue;

    [SerializeField] MMF_Player ATKRed;
    [SerializeField] MMF_Player ATKBlue;

    [SerializeField] MMF_Player DefenseRed;
    [SerializeField] MMF_Player DefenseBlue;

    public bool SkipTurn_Interactive = true;

    [SerializeField] MatchManager.PlayerAction phaseUI;

    //[SerializeField] TextMeshProUGUI T_RightAttack;
    //[SerializeField] TextMeshProUGUI T_ACT_SkipTurn;
    [SerializeField] TextMeshProUGUI T_ResultMatch;

    //[Space(10)]

    //[Header("Button Event")]
    //[Space(5)]
    //[SerializeField] Button ACT_SkipTurn;

    public MMFeedbacksSequencer MMFeedbacksSequencerAttackRed;
    public MMFeedbacksSequencer MMFeedbacksSequencerAttackBlue;
    public MMFeedbacksSequencer MMFeedbacksSequencerDefendRed;
    public MMFeedbacksSequencer MMFeedbacksSequencerDefendBlue;

    private EnumDefine.GameTokken lastRedTokken = EnumDefine.GameTokken.None;
    private EnumDefine.GameTokken lastBlueTokken = EnumDefine.GameTokken.None;


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

    void Start()
    {

        if (SceneManager.GetActiveScene().name.Equals("Home"))
        {

            #region Register Event Load Data
            #endregion

            #region Add Button Event

            #endregion
        }
    }


   

    #region LoadDataFunction
    #endregion

    #region Get Set
    

    //public string Turn
    //{
    //    get { return T_Turn.text; }
    //    set { this.T_Turn.text = value; }
    //}

    public void Turn(string turn)
    {
        print("Turn");
        if (K_Player.K_PlayerSide.Blue.Equals(turn))
        {
            lightBlue.PlayFeedbacks();
        }else if (K_Player.K_PlayerSide.Red.Equals(turn))
        {
            lightRed.PlayFeedbacks();
        }
    }

    public void ClickSkipTurnModel()
    {
        if (SkipTurn_Interactive)
        {
            switch (phaseUI)
            {
                case MatchManager.PlayerAction.SkipTurn:
                    if (K_Player.K_PlayerSide.Blue.Equals(MatchManager.instance.localPlayerSide))
                    {
                        print("SkipTurnBlue.PlayFeedbacks();");
                        SkipTurnBlue.PlayFeedbacks();
                    }
                    else if (K_Player.K_PlayerSide.Red.Equals(MatchManager.instance.localPlayerSide))
                    {
                        print("SkipTurnRed.PlayFeedbacks();");
                        SkipTurnRed.PlayFeedbacks();
                    }
                    break;
                case MatchManager.PlayerAction.Attack:
                    if (K_Player.K_PlayerSide.Blue.Equals(MatchManager.instance.localPlayerSide))
                    {
                        print("ATKBlue.PlayFeedbacks();");
                        ATKBlue.PlayFeedbacks();
                    }
                    else if (K_Player.K_PlayerSide.Red.Equals(MatchManager.instance.localPlayerSide))
                    {
                        print("ATKRed.PlayFeedbacks();");
                        ATKRed.PlayFeedbacks();
                    }
                    break;
                case MatchManager.PlayerAction.Defend:
                    if (K_Player.K_PlayerSide.Blue.Equals(MatchManager.instance.localPlayerSide))
                    {
                        print("DefenseBlue.PlayFeedbacks();");
                        DefenseBlue.PlayFeedbacks();
                    }
                    else if (K_Player.K_PlayerSide.Red.Equals(MatchManager.instance.localPlayerSide))
                    {
                        print("DefenseRed.PlayFeedbacks();");
                        DefenseRed.PlayFeedbacks();
                    }
                    break;
            }
          
        }
    }

    public void setEventSkipTurn(UnityAction function)
    {
        phaseUI = MatchManager.PlayerAction.SkipTurn;

        print("setEventSkipTurn");
        if (K_Player.K_PlayerSide.Blue.Equals(MatchManager.instance.localPlayerSide))
        {
            MMF_Events mMF_PlayerEvents = SkipTurnBlue.GetFeedbackOfType<MMF_Events>(MMF_Player.AccessMethods.Last,0);
            mMF_PlayerEvents.PlayEvents.RemoveAllListeners();
            UnityEvent ev2 = new UnityEvent();
            ev2.AddListener(() => function());
            mMF_PlayerEvents.PlayEvents = ev2;

            SkipTurnBlue.Initialization();
        }
        else if (K_Player.K_PlayerSide.Red.Equals(MatchManager.instance.localPlayerSide))
        {

            MMF_Events mMF_PlayerEvents = SkipTurnRed.GetFeedbackOfType<MMF_Events>(MMF_Player.AccessMethods.Last, 0);
            mMF_PlayerEvents.PlayEvents.RemoveAllListeners();
            UnityEvent ev2 = new UnityEvent();
            ev2.AddListener(() => function());
            mMF_PlayerEvents.PlayEvents = ev2;

            SkipTurnRed.Initialization();

        }
    }

    public void setEventAtk(UnityAction function)
    {
        phaseUI = MatchManager.PlayerAction.Attack;

        print("setEventAtk");
        if (K_Player.K_PlayerSide.Blue.Equals(MatchManager.instance.localPlayerSide))
        {
            MMF_Events mMF_PlayerEvents = ATKBlue.GetFeedbackOfType<MMF_Events>(MMF_Player.AccessMethods.Last, 0);
            mMF_PlayerEvents.PlayEvents.RemoveAllListeners();
            UnityEvent ev2 = new UnityEvent();
            ev2.AddListener(() => function());
            mMF_PlayerEvents.PlayEvents = ev2;
            ATKBlue.Initialization();
        }
        else if (K_Player.K_PlayerSide.Red.Equals(MatchManager.instance.localPlayerSide))
        {
            MMF_Events mMF_PlayerEvents = ATKRed.GetFeedbackOfType<MMF_Events>(MMF_Player.AccessMethods.Last, 0);
            mMF_PlayerEvents.PlayEvents.RemoveAllListeners();
            UnityEvent ev2 = new UnityEvent();
            ev2.AddListener(() => function());
            mMF_PlayerEvents.PlayEvents = ev2;
            ATKRed.Initialization();
        }
    }

    public void setEventDefense(UnityAction function)
    {
        phaseUI = MatchManager.PlayerAction.Defend;

        print("setEventDefense");
        if (K_Player.K_PlayerSide.Blue.Equals(MatchManager.instance.localPlayerSide))
        {
            MMF_Events mMF_PlayerEvents = DefenseBlue.GetFeedbackOfType<MMF_Events>(MMF_Player.AccessMethods.Last, 0);
            mMF_PlayerEvents.PlayEvents.RemoveAllListeners();
            UnityEvent ev2 = new UnityEvent();
            ev2.AddListener(() => function());
            mMF_PlayerEvents.PlayEvents = ev2;
            DefenseBlue.Initialization();
        }
        else if (K_Player.K_PlayerSide.Red.Equals(MatchManager.instance.localPlayerSide))
        {

            MMF_Events mMF_PlayerEvents = DefenseRed.GetFeedbackOfType<MMF_Events>(MMF_Player.AccessMethods.Last, 0);
            mMF_PlayerEvents.PlayEvents.RemoveAllListeners();
            UnityEvent ev2 = new UnityEvent();
            ev2.AddListener(() => function());
            mMF_PlayerEvents.PlayEvents = ev2;
            DefenseRed.Initialization();
        }
    }

    public void removeAllEventSkipTurn()
    {
        //if (K_Player.K_PlayerSide.Blue.Equals(MatchManager.instance.localPlayerSide))
        //{
        //    EventSkipTurnBlue.PlayEvents.RemoveAllListeners();
        //}
        //else if (K_Player.K_PlayerSide.Red.Equals(MatchManager.instance.localPlayerSide))
        //{
        //    EventSkipTurnRed.PlayEvents.RemoveAllListeners();
        //}
    }
    public void RightAttack()
    {
        ///Feedback Change right attach bang icon
        Debug.Log("RightAttack");
    }

    //public string RightAttack
    //{
    //    get { return T_RightAttack.text; }
    //    set { this.T_RightAttack.text = value; }
    //}

    //public string TextButton_ACT_SkipTurn
    //{
    //    get { return T_ACT_SkipTurn.text; }
    //    set { this.T_ACT_SkipTurn.text = value; }
    //}

  

    public void ChangeTokken()
    {
        if(MatchManager.instance.redPlayer.tokken == EnumDefine.GameTokken.Attack)
        {
            MMFeedbacksSequencerAttackRed.PlaySequence();
        }else if (MatchManager.instance.redPlayer.tokken == EnumDefine.GameTokken.Defend)
        {
            MMFeedbacksSequencerDefendRed.PlaySequence();
        }

        if (MatchManager.instance.bluePlayer.tokken == EnumDefine.GameTokken.Attack)
        {
            MMFeedbacksSequencerAttackBlue.PlaySequence();
        }
        else if (MatchManager.instance.bluePlayer.tokken == EnumDefine.GameTokken.Defend)
        {
            MMFeedbacksSequencerDefendBlue.PlaySequence();
        }


    }

    string ResultMatch
    {
        get { return T_ResultMatch.text; }
        set { this.T_ResultMatch.text = value; }
    }

    //public Button GetACT_SkipTurn
    //{
    //    get { return this.ACT_SkipTurn; }
    //}


    #endregion

    #region Function
    public IEnumerator flipUI()
    {
        Vector3 rotationUIParameter = new Vector3(0f, 180f, 0f);
        List<Transform> listFlipUI = new List<Transform>(); 
        if(MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Blue))
        {
            RectTransform rectTransformHp = matchManager.redPlayer.hp.gameObject.GetComponent<RectTransform>();
            RectTransform rectTransformMana = matchManager.redPlayer.mana.gameObject.GetComponent<RectTransform>();
            rectTransformMana.Rotate(rotationUIParameter);
            rectTransformHp.Rotate(rotationUIParameter);

        } else if (MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Red))
        {
            RectTransform rectTransformHp = matchManager.bluePlayer.hp.gameObject.GetComponent<RectTransform>();
            RectTransform rectTransformMana = matchManager.bluePlayer.mana.gameObject.GetComponent<RectTransform>();
            rectTransformMana.Rotate(rotationUIParameter);
            rectTransformHp.Rotate(rotationUIParameter);

            listFlipUI.AddRange(matchManager.bluePlayer.summonZones.Select(a => a.transform.parent.transform));
            listFlipUI.AddRange(matchManager.bluePlayer.fightZones.Select(a => a.transform.parent.transform));
            listFlipUI.AddRange(matchManager.redPlayer.summonZones.Select(a => a.transform.parent.transform));
            listFlipUI.AddRange(matchManager.redPlayer.fightZones.Select(a => a.transform.parent.transform));
        }

        foreach (Transform t in listFlipUI)
        {
            print("TRANSFORM");
            Vector3 rotationToAdd = new Vector3(0,-180f, 0f);
            t.localRotation *= Quaternion.Euler(rotationToAdd);
        }
        yield return null;
    }

    public IEnumerator setResultMatch(bool win, bool isRank, float showTime)
    {
        PanelResult.SetActive(true);
        if (win)
        {
            if(isRank)
                ResultMatch = "Your Win\n+250 Coin\n+10 Elo";
            else
            ResultMatch = "Your Win"; 
        }
        else
        {
            if(isRank)
                ResultMatch = "Your Lose";
            else
            ResultMatch = "Your Lose";
        }

        yield return new WaitForSeconds(showTime);
    }

    public void TurnLoadingScene(bool turn)
    {
        //PanelLoading.SetActive(turn);
    }
    #endregion

}
