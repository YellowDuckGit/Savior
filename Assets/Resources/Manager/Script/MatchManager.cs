using Assets.GameComponent.Manager;
using ExitGames.Client.Photon;
using JetBrains.Annotations;
using MoreMountains.Tools;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using PlayFab.ClientModels;
using PlayFab.EconomyModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Policy;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WebSocketSharp;
using static Cinemachine.CinemachineConfiner;
using static Cinemachine.CinemachineTriggerAction.ActionSettings;
using static EnumDefine;
using static EventGameHandler;
using static K_Player;
using static MatchManager;
using static MonsterCard;
using Debug = UnityEngine.Debug;

public enum WinCondition
{
    None, EnemyLoseAllHp
}



public class MatchManager : MonoBehaviourPunCallbacks
{

    [MMInspectorButton("GetPlayerInfo")]
    public bool Play;

    public static MatchManager instance;

    public CardPlayer redPlayer;
    public CardPlayer bluePlayer;

    private CardPlayer _LocalPlayer;
    private CardPlayer _OppnentPlayer;

    public float EloResult;

    public CardPlayer LocalPlayer
    {
        get
        {
            return _LocalPlayer;
        }
        set
        {
            _LocalPlayer = value;
        }
    }
    public CardPlayer OpponentPlayer
    {
        get
        {
            if(_OppnentPlayer != null)
            {
                return _OppnentPlayer;
            }
            _OppnentPlayer = LocalPlayer == redPlayer ? bluePlayer : redPlayer;
            return _OppnentPlayer;
        }
    }

    private CardPlayer currentInTurnPlayer;

    public Dictionary<int, List<string>> AttributeGained = new();

    public Vector3 positionBlue;
    public Vector3 rotationBlue;

    public Vector3 positionRed;
    public Vector3 rotationRed;

    public GameObject blueSide;
    public GameObject redSide;

    #region Match State
    public int round = 0;
    public string turnPresent;

    //public bool blueTriggerAttack;
    //public bool redTriggerAttack;

    //public bool isBlueSkip = false;
    //public bool isRedSkip = false;

    public bool isEndMatch = false;
    public bool isRedWin = false;
    public bool isBlueWin = false;

    public PlayerAction ActionInTurn = PlayerAction.Normal;

    #endregion


    #region Match Setting
    [SerializeField] int numberCardDrawBeginMatch = 1;

    public int minMana = 0;
    public int maxMana = 10;
    [SerializeField] int initialMana = 0;
    [SerializeField] int manaIncreasePerRound = 1;


    public int minHP = 0;
    public int maxHP = 20;
    [SerializeField] int initialHP = 20;

    /// <summary>
    /// store the current game phase
    /// </summary>
    public GamePhase gamePhase = GamePhase.Normal;
    #endregion

    //defind what side of local client side
    public string localPlayerSide;

    private void Awake()
    {
        //DontDestroyOnLoad(gameObject);
        if(instance != null && instance != this)
        {
            UnityEngine.Debug.LogError("MatchManager have 2");
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }
    private void Start()
    {
        //UIMatchManager.instance.GetACT_SkipTurn.onClick.AddListener(() => SkipTurnEvent());
        //UIMatchManager.instance.GetACT_SkipTurn.interactable = false;

        turnPresent = K_PlayerSide.Blue;
        /*
         * Regist function process for local event
         */
        this.RegisterListener(EventID.OnMoveCardToSummonZone, param => MoveCardToSummonZoneEvent((string)param));
        this.RegisterListener(EventID.OnMoveCardToFightZone, param => MoveCardToFightZoneEvent((string)param));
        this.RegisterListener(EventID.OnSummonMonster, param => SummonCardEvent(param as SummonArgs));
        this.RegisterListener(EventID.OnMoveCardInTriggerSpell, param => MoveCardInTriggerSpellEvent(param as MoveCardInTriggerSpellArgs));
        StartCoroutine(InitalGameProcess());
    }

    public void StoreGainedAttributeAction(string attributeName)
    {
        if(!AttributeGained.ContainsKey(round))
        {
            AttributeGained[round].Add(attributeName);
        }

    }

    public void MoveCardInTriggerSpellEvent(MoveCardInTriggerSpellArgs args)
    {
        object[] datas = new object[] { this.localPlayerSide, args.card.photonView.ViewID };

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent((byte)RaiseEvent.MoveCardInTriggerSpell, datas, raiseEventOptions, SendOptions.SendUnreliable);

    }
    IEnumerator MoveCardInTriggerSpellAction(CardPlayer player, SpellCard card)
    {
        print(this.debug());
        card.IsSelected = false;
        //change card from hand to summon zone 
        var cardSelected = player.hand.Draw(card);
        //player.hand.SortPostionRotationCardInHand();
        card.Position = CardPosition.InTriggerSpellField;

        card.RemoveCardFormParentPresent();
        card.MoveCardIntoNewParent(player.spellZone.transform);

        player.spellZone.SpellCard = card;
        player.mana.Number -= card.Cost;

        yield return StartCoroutine(EffectManager.Instance.OnAfterSummon(card));

        yield return new WaitForSeconds(0.5f);

        /*
         * just local player get and excute effect first, after that async player opposite
         */
        Debug.Log(this.debug("Used card spell", new
        {
            card.SpellType
        }));

        if(card.SpellType == SpellType.Slow)
        {
            SwitchTurnAction();
        }

        if(card != null)
        {
            card.transform.SetParent(null);
            card.gameObject.SetActive(false); //destroy spell card after use
        }
        yield return null;
    }
    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
    }

    public void OnclickLeftRoom()
    {
    }

    public void OnclickSwitchScene()
    {
    }



    #region Set Up Match
    /// <summary>
    /// Initial Player model
    /// </summary>
    /// <returns></returns>
    IEnumerator PlayerInit()
    {
        print(this.debug("PlayerInit", new
        {
            PhotonNetwork.LocalPlayer.CustomProperties
        }));

        localPlayerSide = PhotonNetwork.LocalPlayer.CustomProperties[K_Player.K_PlayerSide.key].ToString();

        if(localPlayerSide.Equals(K_Player.K_PlayerSide.Blue))
        {
            LocalPlayer = PhotonNetwork.Instantiate("BluePlayer", positionBlue, Quaternion.Euler(rotationBlue)).GetComponent<CardPlayer>();
        }
        else if(localPlayerSide.Equals(K_Player.K_PlayerSide.Red))
        {
            LocalPlayer = PhotonNetwork.Instantiate("RedPlayer", positionRed, Quaternion.Euler(rotationRed)).GetComponent<CardPlayer>();
        }
        yield return new WaitUntil(() => redPlayer != null && bluePlayer != null);
    }
    /// <summary>
    /// Set data all zone from player side (CardPlayer, MatchManager)
    /// </summary>
    void SetUpField()
    {
        print(this.debug("SetUp Field"));
        print(this.debug("Blue SM count", new
        {
            playerNotNull = bluePlayer != null,
            NumberSummonZone = bluePlayer.summonZones.Count
        }));

        foreach(SummonZone summonZone in bluePlayer.summonZones)
        {
            summonZone.player = bluePlayer;
            summonZone.matchManager = this;
        }

        foreach(FightZone fightZone in bluePlayer.fightZones)
        {
            fightZone.player = bluePlayer;
            fightZone.matchManager = this;
        }

        bluePlayer.spellZone.player = bluePlayer;
        bluePlayer.spellZone.matchManager = this;

        print(this.debug("Red SM count", new
        {
            playerNotNull = redPlayer != null,
            NumberSummonZone = redPlayer.summonZones.Count
        }));
        foreach(SummonZone summonZone in redPlayer.summonZones)
        {
            summonZone.player = redPlayer;
            summonZone.matchManager = this;
        }

        foreach(FightZone fightZone in redPlayer.fightZones)
        {
            fightZone.player = redPlayer;
            fightZone.matchManager = this;
        }
        redPlayer.spellZone.player = redPlayer;
        redPlayer.spellZone.matchManager = this;

    }
    #endregion

    #region TURN PHASE in Match SYSTEM
    /// <summary>
    /// Match process happend
    /// </summary>
    /// <returns></returns>
    private IEnumerator InitalGameProcess()
    {
        yield return StartCoroutine(BeginMatch());
        yield return StartCoroutine(MiddleMatch());
        yield return StartCoroutine(EndMatch());
    }
    #region Match Process

    /// <summary>
    /// Begin Match initial resource for player(Desk, card etc.)
    /// </summary>
    /// <returns></returns>
    IEnumerator BeginMatch()
    {
        print(this.debug($"Begin Match"));
        UIMatchManager.instance.TurnLoadingScene(true);

        yield return StartCoroutine(PlayerInit());

        PlayerSetTokken();

        SetUpField();

        string deckBlue = PhotonNetwork.CurrentRoom.CustomProperties[K_Player.DeckBlue].ToString(); //get desk Serialize from photon custom property
        string deckRed = PhotonNetwork.CurrentRoom.CustomProperties[K_Player.DeckRed].ToString();

        yield return StartCoroutine(bluePlayer.deck.getCardDataInDeck(deckBlue)); //Init desk in match for blue

        yield return StartCoroutine(redPlayer.deck.getCardDataInDeck(deckRed)); //Init desk in match for red

        /*
         * Create and set up for each card in desk
         */

        yield return StartCoroutine(LocalPlayer.deck.CreateMonsterCardsInDeckMatch());


        yield return new WaitUntil(() =>
        {
            print(this.debug("Wait until all card in deck is created", new
            {
                blue = bluePlayer.deck.Count,
                red = redPlayer.deck.Count
            }));
            return bluePlayer.deck.Full
             && redPlayer.deck.Full;
        });

        ExitGames.Client.Photon.Hashtable _myRoomCustomProperties = new ExitGames.Client.Photon.Hashtable();
        _myRoomCustomProperties = PhotonNetwork.CurrentRoom.CustomProperties;

        //upload local instancd deck to custom room properties
        if(localPlayerSide == K_PlayerSide.Red)
        {
            _myRoomCustomProperties[K_Player.OrderInstanceCardRed] = string.Join("@", redPlayer.deck.GetAll().Select(card => card.photonView.ViewID));
            print(this.debug("send Template (Red): \n" + _myRoomCustomProperties[K_Player.OrderInstanceCardRed]));
            PhotonNetwork.CurrentRoom.SetCustomProperties(_myRoomCustomProperties);
        }
        else if(localPlayerSide == K_PlayerSide.Blue)
        {
            _myRoomCustomProperties[K_Player.OrderInstanceCardBlue] = string.Join("@", bluePlayer.deck.GetAll().Select(card => card.photonView.ViewID));
            print(this.debug("send Template (Blue): \n" + _myRoomCustomProperties[K_Player.OrderInstanceCardBlue]));
            PhotonNetwork.CurrentRoom.SetCustomProperties(_myRoomCustomProperties);
        }
        else
        {
            Debug.LogError("Local player side is not set");
        }

        /*
         * Sync opposite player's deck photon view id and card in deck
         */

        yield return StartCoroutine(Next());

        yield return StartCoroutine(SyncOppositePlayerDeck());


        //print string of card in desk with format <index>. <card> [id] \newline
        var deskBlue = bluePlayer.deck.GetAll();
        print("BLUEDECK" + string.Join("\n", deskBlue.Select(c => string.Format("{0}. {1}", deskBlue.IndexOf(c), c))));
        var deskred = redPlayer.deck.GetAll();
        print("REDDECK" + string.Join("\n", deskred.Select(c => string.Format("{0}. {1}", deskred.IndexOf(c), c))));

        //yield return new WaitUntil(() => !bluePlayer.deck.cards.Any(a => a.BaseMonsterData == null)
        //        && !redPlayer.deck.cards.Any(a => a.BaseMonsterData == null));

        //yield return StartCoroutine(UIMatchManager.instance.flipUI());

        //provide initial resource
        SetLimitHP(initialHP, bluePlayer);
        SetLimitHP(initialHP, redPlayer);
  

        ProvideHP(initialHP, bluePlayer);
        ProvideHP(initialHP, redPlayer);




        yield return StartCoroutine(Next());
        //draw amout of card when start match
        if(MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Blue))
        {
            yield return StartCoroutine(DrawPhase(5, bluePlayer));

        }
        else if(MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Red))
        {
            yield return StartCoroutine(DrawPhase(5, redPlayer));
        }

        yield return new WaitForSeconds(3);

        UIMatchManager.instance.TurnLoadingScene(false);

        SoundManager.instance.PlayBackground_Match();
    }

    private IEnumerator SyncOppositePlayerDeck()
    {
        yield return new WaitUntil(() => PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(K_Player.OrderInstanceCardBlue) && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(K_Player.OrderInstanceCardRed));

        List<int> oppositeDeckOrder = null;
        if(localPlayerSide == K_PlayerSide.Red)
        {
            var deckOrder = PhotonNetwork.CurrentRoom.CustomProperties[K_Player.OrderInstanceCardBlue].ToString().Split('@');
            oppositeDeckOrder = deckOrder.Select(int.Parse).ToList();
        }
        else if(localPlayerSide == K_PlayerSide.Blue)
        {
            var deckOrder = PhotonNetwork.CurrentRoom.CustomProperties[K_Player.OrderInstanceCardRed].ToString().Split('@');
            oppositeDeckOrder = deckOrder.Select(int.Parse).ToList();
        }
        else
        {
            Debug.LogError("Local player side is not set");
        }
        yield return StartCoroutine(OpponentPlayer.deck.SetOrderInstanceCard(oppositeDeckOrder));
        CameraManager.instance.OnclickSwitchCameraNormal();
    }

    private IEnumerator Next()
    {
        print("Send next");
        LocalPlayer.isNEXT_STEP = true;
        object[] datas = new object[] { localPlayerSide, LocalPlayer.isNEXT_STEP };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        var requestComplete = PhotonNetwork.RaiseEvent((byte)RaiseEvent.NEXT_STEP, datas, raiseEventOptions, SendOptions.SendUnreliable);

        yield return new WaitUntil(() =>
        {
            print(this.debug("wait next", new
            {
                local = OpponentPlayer.isNEXT_STEP,
                opponent = LocalPlayer.isNEXT_STEP
            }));
            return requestComplete && OpponentPlayer.isNEXT_STEP && LocalPlayer.isNEXT_STEP;
        });

        OpponentPlayer.isNEXT_STEP = false;
        LocalPlayer.isNEXT_STEP = false;
    }

    /// <summary>
    /// Middle Match process where player can play card
    /// </summary>
    /// <returns></returns>
    IEnumerator MiddleMatch()
    {
        UIMatchManager.instance.SkipTurn_Interactive = false;
        UIMatchManager.instance.setEventSkipTurn(SkipTurnEvent);

        yield return StartCoroutine(Next());

        /*
         * game loop until end match
         */
        do
        {
            yield return StartCoroutine(BeginRound());
            yield return StartCoroutine(MiddleRound());
            yield return StartCoroutine(EndRound());
        } while(!isEndMatch);
        yield return null;
    }
    IEnumerator EndMatch()
    {
        print(this.debug("End Match"));
        switch(FindMatchSystem.instance.gameMode)
        {
            case GameMode.Normal:
                yield return StartCoroutine(ProvideReward(false));
                break;
            case GameMode.Rank:
                yield return StartCoroutine(ProvideReward(true));
                break;
        }

        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("GameScene");
        yield return null;
    }
    #endregion

    #region Round System
    /// <summary>
    /// start a round provide resource as mana and draw 1 card begin round
    /// </summary>
    /// <returns></returns>
    IEnumerator BeginRound()
    {
        /*/ 
         * round number increase
         * reset round attribute
         * switch tokken
         * switch turn
         * draw 1 card for all player and increase mana
         * start event start round
         /*/


        round = round + 1; //increase round number


        print(this.debug($"Begin Round {round}", new
        {
            round
        }));

        gamePhase = GamePhase.Normal; //set game phase to defaut

        print(this.debug("Local side", new
        {
            MatchManager.instance.localPlayerSide
        }));

        if(MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Blue))
        {
            yield return StartCoroutine(DrawPhase(1, bluePlayer));
        }
        else if(MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Red))
        {
            yield return StartCoroutine(DrawPhase(1, redPlayer));
        }

        //provide Mana
        ProvideMP(1, bluePlayer);
        ProvideMP(1, redPlayer);
        SetLimitMP(1, bluePlayer);
        SetLimitMP(1, redPlayer);


        print(this.debug("Change Tokken at new round", new
        {
            round
        }));
        /*
         * at the first round not change tokken by using default tokken (Blue attack first)
         * at the another round when new round then 2 player change tokken
         */
        if(round > 1)
            ChangeToken();

        /*
         * in new round, the player have tokken attack is the player get turn first
         */
        print(this.debug("Set Player get first turn before", new
        {
            round,
            turnPresent,
            bluePlayerTokken = (Enum.GetName(typeof(GameTokken), bluePlayer.tokken))
        }));

        this.turnPresent = bluePlayer.tokken == GameTokken.Attack ? K_Player.K_PlayerSide.Blue : K_Player.K_PlayerSide.Red;

        print(this.debug("Set Player get first turn after", new
        {
            round,
            turnPresent,
            bluePlayerTokken = (Enum.GetName(typeof(GameTokken), bluePlayer.tokken))
        }));

        UIMatchManager.instance.ChangeTokken();


        SetRightToAttack();
        if(!AttributeGained.ContainsKey(round))
        {
            {
                AttributeGained.Add(round, new List<string>());
            }
        }

        //TODO: EVENT START ROUND
        this.PostEvent(EventID.OnStartRound, this);
    }
    /// <summary>
    /// Turn process here
    /// </summary>
    /// <returns></returns>
    IEnumerator MiddleRound()
    {
        print(this.debug());
        //get frist turn 
        yield return StartCoroutine(TurnProcess());
        print(this.debug("End Middle round"));

    }
    /// <summary>
    /// end a round
    /// </summary>
    /// <returns></returns>
    IEnumerator EndRound()
    {
        this.PostEvent(EventID.OnEndRound, this);
        yield return StartCoroutine(EffectManager.Instance.ExecuteOnEndRound(this));
        //TODO: Event end round
        print(this.debug());
        yield return null;
    }
    #endregion

    #region Turn System
    /// <summary>
    /// process turn and their event
    /// StartTurn: player action here
    /// End Turn: player who have attack tokken, have been attacked and defend
    /// </summary>
    /// <returns></returns>
    IEnumerator TurnProcess()
    {
        do
        {
            yield return StartCoroutine(StartTurn());
            yield return StartCoroutine(EndTurn());
            print(this.debug(null, new
            {
                redSkip = redPlayer.isSkipTurn,
                blueSkip = bluePlayer.isSkipTurn
            }));
        } while(!isNextRound() && !isEndMatch); //run until 2 player skip turn or end match
        ResetSkipTurn();
        yield return null;
    }

    private bool isNextRound()
    {
        return redPlayer.isSkipTurn && bluePlayer.isSkipTurn;
    }

    IEnumerator StartTurn()
    {
        /*
         * SetUp UI for player
         */
        if(localPlayerSide.Equals(turnPresent))
        {
            //UIMatchManager.instance.Turn = "Your Turn";
            //UIMatchManager.instance.GetACT_SkipTurn.interactable = true;

            // SFX: Your Turn
            SoundManager.instance.PlayYourTurn();
            UIMatchManager.instance.Turn(turnPresent);
            UIMatchManager.instance.SkipTurn_Interactive = true;
        }
        else
        {
            //UIMatchManager.instance.Turn = "Opponent turn";
            //UIMatchManager.instance.GetACT_SkipTurn.interactable = false;
            UIMatchManager.instance.Turn(turnPresent);
            UIMatchManager.instance.SkipTurn_Interactive = false;

        }
        //TODO: check player can be summon or use card
        //TODO: Player can attack

        //check player can summon card
        this.PostEvent(EventID.OnStartTurn, this);
        yield return StartCoroutine(WaitUntilPlayerSwitchTurn());
    }

    IEnumerator EndTurn()
    {
        yield return null;
    }

    #endregion

    #region Phase System
    /// <summary>
    /// Draw amount of card when begin of Match
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="bluePlayer"></param>
    /// <param name="redPlayer"></param>
    /// <returns></returns>
    IEnumerator DrawPhase(int amount, CardPlayer player)
    {
        object[] datas = new object[] { amount, player.photonView.ViewID };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        var result = PhotonNetwork.RaiseEvent((byte)RaiseEvent.DRAW_CARD_EVENT, datas, raiseEventOptions, SendOptions.SendUnreliable);
        yield return new WaitUntil(() => result);
    }

    IEnumerator AttackAndDefensePhase()
    {
        print(this.debug("Attack And Defense Phase"));
        var AttackPlayer = GetAttackPlayer();
        var DefensePlayer = GetDefensePlayer();

        yield return StartCoroutine(AttackZoneOpposite(AttackPlayer.fightZones, DefensePlayer.fightZones));
        yield return StartCoroutine(ClearAttackField(localPlayerSide));
        SetSkipAction();
        gamePhase = GamePhase.Normal;
        this.PostEvent(EventID.EndAttackAndDefensePhase, this);

        yield return null;
    }
    #endregion

    #endregion

    #region Util function
    /// <summary>
    /// get player who have tokken defense
    /// </summary>
    /// <returns></returns>
    public CardPlayer GetDefensePlayer()
    {
        CardPlayer DefensePlayer = redPlayer.tokken == GameTokken.Defend ? redPlayer : bluePlayer;
        return DefensePlayer;
    }

    /// <summary>
    /// get player who have attack tokken
    /// </summary>
    /// <returns></returns>
    public CardPlayer GetAttackPlayer()
    {
        CardPlayer AttackPlayer = redPlayer.tokken == GameTokken.Attack ? redPlayer : bluePlayer;
        return AttackPlayer;
    }

    /// <summary>
    /// Get current player in turn
    /// </summary>
    /// <returns></returns>
    public CardPlayer getCurrenPlayer()
    {
        if(turnPresent == K_PlayerSide.Red)
            return redPlayer;

        return bluePlayer;
    }


    /// <summary>
    /// Player get the tokken for attack or defense role
    /// </summary>
    private void PlayerSetTokken()
    {
        print(this.debug());
        //check who is current player then set the tokken attack to the player 
        if(bluePlayer != null && redPlayer != null)
        {
            bluePlayer.tokken = GameTokken.Attack; //blue player alway set attack tokken first
            redPlayer.tokken = GameTokken.Defend;
            bluePlayer.isAttackAvaliable = true;
            redPlayer.isAttackAvaliable = false;
        }
        else
        {
            print(this.debug("Set tokken fail", new
            {
                isBluePlayerValid = bluePlayer != null,
                isRedPlayerValid = redPlayer != null
            }));
        }

    }

    /// <summary>
    /// auto switching player's tokken between 2 player (attack, defense)
    /// </summary>
    private void ChangeToken()
    {
        print(this.debug("Before change tokken", new
        {
            blueTokken = Enum.GetName(typeof(GameTokken), bluePlayer.tokken),
            redTokken = Enum.GetName(typeof(GameTokken), redPlayer.tokken)
        }));
        if(bluePlayer != null && redPlayer != null)
        {
            ((bluePlayer.tokken, bluePlayer.isAttackAvaliable), (redPlayer.tokken, redPlayer.isAttackAvaliable)) = bluePlayer.tokken == GameTokken.Attack ? ((GameTokken.Defend, false), (GameTokken.Attack, true)) : ((GameTokken.Attack, true), (GameTokken.Defend, false));
            print(this.debug("After change tokken", new
            {
                blueTokken = Enum.GetName(typeof(GameTokken), bluePlayer.tokken),
                redTokken = Enum.GetName(typeof(GameTokken), redPlayer.tokken)
            }));

            UIMatchManager.instance.ChangeTokken();
        }
        else
        {
            print(this.debug("change tokken fail", new
            {
                isBluePlayerValid = bluePlayer != null,
                isRedPlayerValid = redPlayer != null
            }));
        }

    }

    /// <summary>
    /// waitting until current player click skip turn or Skip turn Action
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitUntilPlayerSwitchTurn()
    {
        print(this.debug("Waiting player Action"));
        yield return new WaitUntil(() => ActionInTurn == PlayerAction.SwitchTurn || isEndMatch);
        print(this.debug("Player Action", new
        {
            PlayerActionName = ActionInTurn.ToString()
        }));
        ActionInTurn = PlayerAction.Normal;
    }
    #endregion

    #region Functions with resource in Match
    /// <summary>
    /// Provide HP for player
    /// </summary>
    /// <returns></returns>
    /// 

    public void SetLimitHP(int amount, CardPlayer cardPlayer)
    {
        //Provide HP Step
        cardPlayer.hp.Limit = amount;
    }

    public void SetLimitMP(int amount, CardPlayer cardPlayer)
    {
        //Provide HP Step
        cardPlayer.mana.Limit+= amount;
    }

    void ProvideHP(int amount, CardPlayer cardPlayer)
    {
        cardPlayer.hp.Number += amount;
    }
    /// <summary>
    /// Provide MP for player
    /// </summary>
    /// <returns></returns>
    void ProvideMP(int amount, CardPlayer cardPlayer)
    {
        cardPlayer.mana.Number = cardPlayer.mana.Limit + amount; 

    }
    #endregion

    #region Function with resource for player
    IEnumerator ProvideReward(bool isRanked)
    {
        print("ProvideReward");
        string rewardWinRankedID = "B1";
        string rewardWinNormalID = "B2";
        string rewardLoseRankedID = "B1.1";
        string rewardLoseNormalID = "B2.2";
        string rewardID = "";

        print(PhotonNetwork.CurrentRoom.CustomProperties[K_Player.EloBlue].ToString());
        print(PhotonNetwork.CurrentRoom.CustomProperties[K_Player.EloRed].ToString());


        int eloBlue = Int32.Parse(PhotonNetwork.CurrentRoom.CustomProperties[K_Player.EloBlue].ToString());
        int eloRed = Int32.Parse(PhotonNetwork.CurrentRoom.CustomProperties[K_Player.EloRed].ToString());

        if(isBlueWin)
        {
            if(localPlayerSide.Equals(K_PlayerSide.Blue)) //win
            {
                switch(FindMatchSystem.instance.gameMode)
                {
                    case GameMode.Normal:
                        rewardID = rewardWinNormalID;
                        break;
                    case GameMode.Rank:
                        rewardID = rewardWinRankedID;
                        PlayfabManager.instance.CalElo(true, eloBlue, eloRed);
                        break;
                }
            }
            else if(localPlayerSide.Equals(K_PlayerSide.Red)) //lose
            {
                switch(FindMatchSystem.instance.gameMode)
                {
                    case GameMode.Normal:
                        rewardID = rewardLoseNormalID;
                        break;

                    case GameMode.Rank:
                        rewardID = rewardLoseRankedID;
                        PlayfabManager.instance.CalElo(false, eloRed, eloBlue);
                        break;
                }
            }
        }
        else if(isRedWin)
        {
            if(localPlayerSide.Equals(K_PlayerSide.Blue)) //lose
            {
                switch(FindMatchSystem.instance.gameMode)
                {
                    case GameMode.Normal:
                        rewardID = rewardLoseNormalID;
                        break;
                    case GameMode.Rank:
                        rewardID = rewardLoseRankedID;
                        PlayfabManager.instance.CalElo(false, eloBlue, eloRed);
                        break;
                }
            }
            else if(localPlayerSide.Equals(K_PlayerSide.Red)) //win
            {
                switch(FindMatchSystem.instance.gameMode)
                {
                    case GameMode.Normal:
                        rewardID = rewardWinNormalID;
                        break;
                    case GameMode.Rank:
                        rewardID = rewardWinRankedID;
                        PlayfabManager.instance.CalElo(true, eloRed, eloBlue);
                        break;
                }
            }
        }

        if(rewardID != "")
        {
            StartCoroutine(PlayfabManager.instance.BuyItems(catalog: "Reward", storeId: "BS1", new List<ItemPurchaseRequest>()
            {
                new ItemPurchaseRequest() {ItemId = rewardID, Quantity = 1}
            }, currency: "MC"));
        }

        //set UI
        if(localPlayerSide.Equals(K_PlayerSide.Blue))
        {
            yield return StartCoroutine(UIMatchManager.instance.setResultMatch(isBlueWin, isRanked, 2));
        }
        else if(localPlayerSide.Equals(K_PlayerSide.Red))
        {
            yield return StartCoroutine(UIMatchManager.instance.setResultMatch(isRedWin, isRanked, 2));
        }

        yield return null;
    }
    public void ResultMatch(WinCondition winCondition)
    {
        if(!isEndMatch)
        {
            print("Wincondition");
            isEndMatch = true;
            switch(winCondition)
            {
                case WinCondition.EnemyLoseAllHp:
                    if(redPlayer.hp.Number <= 0)
                    {
                        isBlueWin = true;
                        isRedWin = false;
                    }
                    else if(bluePlayer.hp.Number <= 0)
                    {
                        isBlueWin = false;
                        isRedWin = true;
                    }
                    break;
            }
        }
    }
    #endregion

    #region *ROOM* Event - Action
    /// <summary>
    /// Process for all action need to async between 2 player in match Action
    /// </summary>
    /// <param name="obj"></param>
    private void NetworkingClient_EventReceived(EventData obj)
    {
        //object[] datas = null;
        var args = obj.GetData();
        print(this.debug("Event process", new
        {
            eventName = Enum.GetName(typeof(RaiseEvent), (byte)obj.Code),
            code = obj.Code
        }));
        switch((RaiseEvent)obj.Code)
        {
            case RaiseEvent.NEXT_STEP:
                {
                    if(args.senderPlayerSide != localPlayerSide)
                    {
                        StartCoroutine(OpponentPlayer.NextStepAction(args.isNEXT_STEP));
                    }
                    break;
                }
            case RaiseEvent.SKIP_TURN:
                {
                    SkipTurnAction(args.playerSide as string);
                    SwitchTurnAction();
                    break;
                }
            case RaiseEvent.SWITCH_TURN:
                {
                    SwitchTurnAction();
                    break;
                }
            case RaiseEvent.ATTACK:
                {
                    StartCoroutine(AttackAction(args.playerSide as string));

                    break;
                }
            case RaiseEvent.DEFENSE:
                {
                    StartCoroutine(DefenseAction(args.playerSide as string));
                    break;
                }
            case RaiseEvent.SUMMON_MONSTER:
                {
                    print(this.debug("Start Summon Monster Execute", new
                    {
                        args.zoneID,
                        args.cardID,
                        args.playerSide,
                        args.cardPosition,
                        args.isSpecialSummon
                    }));

                    var zoneRequest = args.playerSide == K_PlayerSide.Blue ? bluePlayer.summonZones.Find(a => a.photonView.ViewID.Equals(args.zoneID)) : redPlayer.summonZones.Find(a => a.photonView.ViewID.Equals(args.zoneID));
                    print(this.debug("Zone request", new
                    {
                        zoneRequest = zoneRequest
                    }));

                    if(zoneRequest != null)
                    {
                        MonsterCard card = null;
                        switch((CardPosition)args.cardPosition)
                        {
                            case CardPosition.InDeck:
                                card = zoneRequest.player.deck.PeekWithPhoton(args.cardID);
                                break;
                            case CardPosition.InHand:
                                card = zoneRequest.player.hand.PeekWithPhoton(args.cardID);
                                break;
                            case CardPosition.InFightField:
                                break;
                            case CardPosition.InSummonField:
                                break;
                            case CardPosition.InGraveyard:
                                break;
                            case CardPosition.Any:
                                break;
                            case CardPosition.InTriggerSpellField:
                                break;
                            default:
                                break;
                        }

                        /*zoneRequest.player.hand.GetAllCardInHand().Find(a => a.photonView.ViewID.Equals(args.cardID));*/
                        print(this.debug("Card selected", new
                        {
                            card
                        }));

                        if(card != null)
                        {
                            print(this.debug("", new
                            {
                                zoneid = zoneRequest.photonView.ViewID,
                                nameCard = card.ToString()
                            }));
                            ExecuteSummonCardAction(zoneRequest, card, args.isSpecialSummon);

                        }
                        else
                        {
                            print(this.debug("Card null", card));
                        }

                    }
                    else
                    {
                        print(this.debug("Zone null", zoneRequest));
                    }
                    print(this.debug("End Summon Monster Execute"));
                    break;
                }
            case RaiseEvent.MoveCardInTriggerSpell:
                {
                    CardPlayer player = args.playerSide == K_PlayerSide.Red ? redPlayer : bluePlayer;
                    if(player != null)
                    {
                        object card = player.hand.PeekWithPhoton(args.cardID);
                        /*zoneRequest.player.hand.GetAllCardInHand().Find(a => a.photonView.ViewID.Equals(args.cardID));*/

                        if(card != null && card is SpellCard spellCard)
                        {
                            StartCoroutine(MoveCardInTriggerSpellAction(player, spellCard));
                        }
                        else
                        {
                            print(this.debug("Card null", card));
                        }
                    }
                    break;
                }
        }
    }

    public void ExecuteSummonCardAction(SummonZone zoneRequest, MonsterCard card, bool isSpecialSummon)
    {
        print(this.debug("execute summon ", new
        {
            zoneRequest,
            card,
            card.CardPlayer.side,
            isSpecialSummon
        }));
        if(zoneRequest != null && card != null)
        {
            StartCoroutine(SummonCardAction(zoneRequest, card, isSpecialSummon));
        }
    }

    #region SUMMON Event - Action
    /// <summary>
    /// Raise event summon cho phep hai nguoi choi cung dong bo su kien
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public bool SummonCardEvent(SummonArgs args)
    {
        print(this.debug("Summon card to summon zone", new
        {
            args.card,
            isFilled = args.summonZone.isFilled(),
            args.cardPosition,
            args.isSpecialSummon
        }));
        //player select card -> player select summon field -> invoke summon even
        //TODO: check condition summon
        //TODO: summon
        //TODO: raise even for oppoence player to update UI
        if(args.card == null || args.summonZone.isFilled())
        {
            print(this.debug("not valid card or summon zone is fill", new
            {
                card = args.card,
                isFill = args.summonZone.isFilled()
            }));
            return false;
        }
        print(this.debug("check enough mana to summon", new
        {
            args.card.Cost,
            args.summonZone.player.mana.Number
        }));
        //tru mana
        if(!args.isSpecialSummon && args.card.Cost > args.summonZone.player.mana.Number)
        {
            print(this.debug("not enough mana to summon"));
            return false;
        }
        // ID zone, ID cardTarget
        object[] datas = new object[] { args.summonZone.photonView.ViewID, args.card.photonView.ViewID, args.summonZone.player.side, (int)args.cardPosition, args.isSpecialSummon ? 1 : 0 };
        print(this.debug("zone and target id", new
        {
            zoneID = args.summonZone.photonView.ViewID,
            //args.card.photonView.ViewID,
            cardID = args.card.photonView.ViewID,
            playerSide = args.summonZone.player.side
        }));
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent((byte)RaiseEvent.SUMMON_MONSTER, datas, raiseEventOptions, SendOptions.SendUnreliable);

        print(this.debug("Photon RaiseEvent SUMMON_MONSTER", new
        {
            args.summonZone.photonView.ViewID
        }));


        //await OnClickSwitchTurnCallback(false, OnClickSwitchTurn);

        return true;
    }
    /// <summary>
    /// thuc hien viet trieu hoi monster
    /// </summary>
    /// <param name="zoneRequest"></param>
    /// <param name="card"></param>
    /// <returns></returns>
    public IEnumerator SummonCardAction(SummonZone zoneRequest, MonsterCard card, bool isSpecialSummon)
    {
        print(this.debug());
        card.IsSelected = false;
        //change card from hand to summon zone 
        card.CardPlayer.hand.RemoveCardFormHand(card);
        //card.CardPlayer.hand.SortPostionRotationCardInHand();

        zoneRequest.SetMonsterCard(card);
        zoneRequest.isSelected = false;
        if(!isSpecialSummon)
            zoneRequest.player.mana.Number -= card.Cost;
        /*
         * just local player get and excute effect first, after that async player opposite
         */
        yield return StartCoroutine(EffectManager.Instance.OnAfterSummon(card));

        print(this.debug("End Summon Monster Action", new
        {
            card
        }));
        SwitchTurnAction();
    }
    #endregion

    #region ATTACK Event - Action
    public void AttackEvent()
    {
        print("OnclickAttack");
        if(LocalPlayer.tokken == GameTokken.Attack)
        {
            object[] datas = new object[] { localPlayerSide };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent((byte)RaiseEvent.ATTACK, datas, raiseEventOptions, SendOptions.SendUnreliable);
        }
        else
        {
            print(this.debug("You can not attack", new
            {
                tokken = Enum.GetName(typeof(GameTokken), LocalPlayer.tokken),
                NumberMonsterInAttackZone = LocalPlayer.fightZones.Where(zone => zone.monsterCard != null)
            }));
            ;
        }
        //code raise event
    }
    /// <summary>
    /// Phat dong attack
    /// </summary>
    /// <param name="playerSide"></param>
    public IEnumerator AttackAction(string playerSide)
    {

        yield return StartCoroutine(StartAttackPhase());
        yield return StartCoroutine(EndAttackPhase());
        SwitchTurnAction();
    }

    private IEnumerator EndAttackPhase()
    {
        print(this.debug());
        yield return null;
    }

    private IEnumerator StartDefendPhase()
    {
        yield return null;
    }

    private IEnumerator StartAttackPhase()
    {
        print(this.debug());
        this.gamePhase = GamePhase.Attack;
        var playerAttack = GetAttackPlayer();
        var playerDefense = GetDefensePlayer();

        playerAttack.playerAction = PlayerAction.Attack;
        playerAttack.isAttackAvaliable = false;

        /*
         * when attack Event have been processed 
         * set denfense Event to local player
         */
        if(LocalPlayer.tokken == GameTokken.Defend)
        {
            /*
             * Change the skip button to defense button
             */
            SetDefenseAction();
        }
        //yield return new WaitUntil(() => gamePhase == GamePhase.Normal);
        yield return StartCoroutine(EffectManager.Instance.None()); //effect
        SwitchTurnEvent();
        yield return null;
    }

    #endregion

    #region DEFEND Event - Action
    public void DefenseEvent()
    {
        print("OnclickDefense");

        object[] datas = new object[] { localPlayerSide };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent((byte)RaiseEvent.DEFENSE, datas, raiseEventOptions, SendOptions.SendUnreliable);
    }

    public IEnumerator DefenseAction(string playerSide)
    {
        yield return StartCoroutine(AttackAndDefensePhase());

        SetSkipAction();
    }
    #endregion

    #region SWITCH_TURN Event - Action
    private void SwitchTurnEvent()
    {
        print(this.debug("PhotonNetwork RaiseEvent SWITCH_TURN"));
        object[] datas = new object[] { };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent((byte)RaiseEvent.SWITCH_TURN, datas, raiseEventOptions, SendOptions.SendUnreliable);
    }
    /// <summary>
    /// Turn chi qua turn moi khi ma dung switch turn
    /// </summary>
    public void SwitchTurnAction()
    {
        print(this.debug("Before turn change", new
        {
            this.localPlayerSide,
            turnPresent,
            round
        }));
        turnPresent = (turnPresent == K_PlayerSide.Blue ? K_PlayerSide.Red : K_PlayerSide.Blue);
        print(this.debug("Affter turn change", new
        {
            this.localPlayerSide,
            turnPresent,
            round
        }));


        print(this.debug(localPlayerSide, new
        {
            islocalPlayerSideInAttack = localPlayerSide.Equals(turnPresent),
            turnPresent,
            tokken = Enum.GetName(typeof(GameTokken), LocalPlayer.tokken)
        }));
        ActionInTurn = PlayerAction.SwitchTurn;
    }
    #endregion

    #region SKIP_TURN Event - Action
    public void SkipTurnEvent()
    {
        print(this.debug($"{LocalPlayer.side} click skip turn"));
        object[] datas = new object[] { localPlayerSide };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent((byte)RaiseEvent.SKIP_TURN, datas, raiseEventOptions, SendOptions.SendUnreliable);
    }
    void SkipTurnAction(string playerSide)
    {
        print(this.debug("Skip Turn", new
        {
            playerSide
        }));
        if(playerSide.Equals(K_PlayerSide.Blue))
        {
            bluePlayer.isSkipTurn = true;
        }
        else if(playerSide.Equals(K_PlayerSide.Red))
        {
            redPlayer.isSkipTurn = true;
        }
    }
    #endregion

    #endregion

    #region *LOCAL* Event - Action
    private void MoveCardToSummonZoneEvent(string playerSide)
    {
        if(playerSide.Equals(localPlayerSide))
        {
            if(localPlayerSide.Equals(K_PlayerSide.Blue))
            {
                int count = bluePlayer.fightZones.FindAll(a => a.monsterCard != null).Count;
                print("Number Card in Fight Field: " + count);
                if(count == 0)
                {
                    SetSkipAction();
                }
            }
            else if(localPlayerSide.Equals(K_PlayerSide.Red))
            {
                int count = redPlayer.fightZones.FindAll(a => a.monsterCard != null).Count;
                print("Number Card in Fight Field: " + count);
                if(count == 0)
                {
                    SetSkipAction();
                }
            }
        }
    }

    private void MoveCardToFightZoneEvent(string playerSide)
    {
        if(playerSide.Equals(localPlayerSide) && isRightToDefense(playerSide))
        {
            SetDefenseAction();
        }
        else if(playerSide.Equals(localPlayerSide) && isRightToAttack(playerSide))
        {
            SetAttackAction();
        }
    }
    #endregion

    #region Functions Check State
    public bool isPlayerTurn(string playerSide)
    {
        if(turnPresent.Equals(playerSide))
        {
            return true;
        }
        else
            return false;
    }

    public bool isRightToAttack(string playerSide)
    {
        if(playerSide.Equals(K_PlayerSide.Blue))
        {
            return bluePlayer.tokken == GameTokken.Attack && bluePlayer.isAttackAvaliable;
        }
        else if(playerSide.Equals(K_PlayerSide.Red))
        {
            return redPlayer.tokken == GameTokken.Attack && redPlayer.isAttackAvaliable;
        }

        return false;
    }

    public bool isRightToDefense(string playerSide)
    {
        if(playerSide.Equals(K_PlayerSide.Blue))
        {
            return bluePlayer.tokken == GameTokken.Defend;
        }
        else if(playerSide.Equals(K_PlayerSide.Red))
        {
            return redPlayer.tokken == GameTokken.Defend;
        }

        return false;
    }
    #endregion

    #region Functions In Attack and Defense phase
    public enum PlayerAction
    {
        Normal,
        Attack,
        Defend,
        Summon,
        SkipTurn,
        SwitchTurn
    }

    public enum GamePhase
    {
        /// <summary>
        /// summon or use card, draw card, move card to attack zone, move card to defense zone
        /// </summary>
        Normal,
        /// <summary>
        /// in Attact and defend phase, where 2 players' card will fight
        /// </summary>
        Attack
    }


    /// <summary>
    /// UI setup when player got attack tokken
    /// </summary>
    void SetRightToAttack()
    {
        print(this.debug(localPlayerSide, new
        {
            turnPresent
        }));

        //if (turnPresent.Equals(K_PlayerSide.Red))
        //{
        //    rightAttackRedSide = true;
        //}
        //else if (turnPresent.Equals(K_PlayerSide.Blue))
        //{
        //    rightAttackBlueSide = true;
        //}

        print(this.debug("", new
        {
            turnPresent,
            localPlayerSide
        }));
        //UI
        if(LocalPlayer.tokken == GameTokken.Attack)
        {
            //UIMatchManager.instance.RightAttack = $"{LocalPlayer.side} Your Attack";
            // SFX: Your Attack
            SoundManager.instance.PlayYourAttack();
            UIMatchManager.instance.RightAttack();
        }
        else
        {
            //UIMatchManager.instance.RightAttack = $"{LocalPlayer.side} Your Defense";
            // SFX: Your Defense
            SoundManager.instance.PlayYourDefense();
            UIMatchManager.instance.RightAttack();
        }

        //if (localPlayerSide.Equals(K_PlayerSide.Blue))
        //{
        //    if (rightAttackBlueSide)
        //    {
        //        UIMatchManager.instance.RightAttack = "Blue Your Attack";
        //    }
        //    else
        //    {
        //        UIMatchManager.instance.RightAttack = "Blue Your Defense";
        //    }
        //}
        //else if (localPlayerSide.Equals(K_PlayerSide.Red))
        //{
        //    if (rightAttackRedSide)
        //    {
        //        UIMatchManager.instance.RightAttack = "Red Your Attack";
        //    }
        //    else
        //    {
        //        UIMatchManager.instance.RightAttack = "Red Your Defense";
        //    }
        //}
        print(this.debug(null, new
        {
            turnPresent
        }));
    }


    IEnumerator ClearAttackField(string playerSide)
    {
        yield return new WaitForSeconds(3f);

        if(playerSide.Equals(K_PlayerSide.Blue))
        {
            foreach(FightZone fightZone in bluePlayer.fightZones)
            {
                if(fightZone.monsterCard != null && !fightZone.monsterCard.Position.Equals(CardPosition.InGraveyard))
                {
                    //int indexFightZone = bluePlayer.fightZones.IndexOf(fightZone);
                    //int indexSummonZone = indexFightZone;
                    SummonZone summonZone = bluePlayer.summonZones.FirstOrDefault(zone => !zone.isFilled() && !zone.isSelected);
                    if(summonZone != null)
                        summonZone.RaiseMoveCardFromAttackFieldToSummonField(fightZone, fightZone.monsterCard);
                    else
                        Debug.LogError(this.debug("not any summon empty"));
                    //fightZone.monsterCard.MoveToSummonZone(fightZone, bluePlayer.summonZones.ElementAt(indexSummonZone));
                }
                else
                {
                    print(this.debug("card is null"));
                }
            }
        }
        else if(playerSide.Equals(K_PlayerSide.Red))
        {
            foreach(FightZone fightZone in redPlayer.fightZones)
            {
                if(fightZone.monsterCard != null && !fightZone.monsterCard.Position.Equals(CardPosition.InGraveyard))
                {
                    //int indexFightZone = redPlayer.fightZones.IndexOf(fightZone);
                    //int indexSummonZone = indexFightZone;
                    SummonZone summonZone = redPlayer.summonZones.FirstOrDefault(zone => !zone.isFilled() && !zone.isSelected);
                    if(summonZone != null)
                        summonZone.RaiseMoveCardFromAttackFieldToSummonField(fightZone, fightZone.monsterCard);
                    else
                        Debug.LogError(this.debug("not any summon empty"));
                }
                else
                {
                    print(this.debug("card is null"));
                }
            }
        }
    }

    IEnumerator AttackZoneOpposite(List<FightZone> attackZones, List<FightZone> defenseZones)
    {
        foreach(FightZone attackZone in attackZones)
        {
            if(attackZone.monsterCard != null)
            {
                int indexAttackZone = attackZones.IndexOf(attackZone);
                MonsterCard monsterAttack = attackZone.monsterCard;

                FightZone defenseZone = defenseZones.ElementAt(indexAttackZone);
                MonsterCard monsterDefense = defenseZone.GetMonsterCard();

                //true if defense zone opposite exist monster card
                if(monsterDefense != null)
                {
                    this.PostEvent(EventID.OnCardAttack, new AnimationAttackArgs(monsterDefense , monsterAttack));
                    yield return new WaitForSeconds(0.6f);
                    monsterAttack.attack(monsterDefense);

                    yield return new WaitForSeconds(0.6f);


                    this.PostEvent(EventID.OnCardAttack, new AnimationAttackArgs(monsterAttack, monsterDefense));
                    yield return new WaitForSeconds(0.6f);
                    monsterDefense.attack(monsterAttack);
                }
                else //attack to hp player
                {
                    //animation missing || attack to player
                    defenseZone.player.hp.decrease(monsterAttack.Attack);
                }

            }
        }
    }

    void SetSkipAction()
    {
        //UIMatchManager.instance.GetACT_SkipTurn.onClick.RemoveAllListeners();
        //UIMatchManager.instance.GetACT_SkipTurn.onClick.AddListener(() => SkipTurnEvent());
        //UIMatchManager.instance.TextButton_ACT_SkipTurn = "Skip";

        UIMatchManager.instance.removeAllEventSkipTurn();
        UIMatchManager.instance.setEventSkipTurn(SkipTurnEvent);

    }

    void SetDefenseAction()
    {
        //UIMatchManager.instance.GetACT_SkipTurn.onClick.RemoveAllListeners();
        //UIMatchManager.instance.GetACT_SkipTurn.onClick.AddListener(() => DefenseEvent());
        //UIMatchManager.instance.TextButton_ACT_SkipTurn = "Defense";

        UIMatchManager.instance.removeAllEventSkipTurn();
        UIMatchManager.instance.setEventDefense(DefenseEvent);
    }

    void SetAttackAction()
    {
        //UIMatchManager.instance.GetACT_SkipTurn.onClick.RemoveAllListeners();
        //UIMatchManager.instance.GetACT_SkipTurn.onClick.AddListener(() => AttackEvent());
        //UIMatchManager.instance.TextButton_ACT_SkipTurn = "Attack";

        UIMatchManager.instance.removeAllEventSkipTurn();
        UIMatchManager.instance.setEventAtk(AttackEvent);
    }
    /// <summary>
    /// set to default player skip turn action
    /// </summary>
    public void ResetSkipTurn()
    {
        redPlayer.isSkipTurn = false;
        bluePlayer.isSkipTurn = false;
    }
    #endregion

    #region ObserverPattern-Register Function
    public override void OnLeftRoom()
    {
        //UI_roomID.text = "";
        print("LeftRoom");
        if(PhotonNetwork.IsConnected)
        {
            print("Connected");
            PhotonNetwork.LoadLevel("Home");
        }
        else
        {
            print("Not Connected");
        }
    }

    public override void OnConnectedToMaster()
    {
        //PhotonNetwork.JoinLobby();
        print("OnConnectedToServer");
    }
    #endregion

    public void GetPlayerInfo()
    {
        Debug.Log(this.debug("Players", new
        {
            LocalPlayer,
            OpponentPlayer
        }));
        Debug.Log(this.debug("LocalPlayer details", new
        {
            LocalPlayer.tokken,
            LocalPlayer.isSkipTurn,
            LocalPlayer.isAttackAvaliable,
            LocalPlayer._IsSelectAble,
            LocalPlayer.IsSelected,
            LocalPlayer.isNEXT_STEP
        }));
        Debug.Log(this.debug("OpponentPlayer details", new
        {
            OpponentPlayer.tokken,
            OpponentPlayer.isSkipTurn,
            OpponentPlayer.isAttackAvaliable,
            OpponentPlayer._IsSelectAble,
            OpponentPlayer.IsSelected,
            OpponentPlayer.isNEXT_STEP
        }));
    }
}
