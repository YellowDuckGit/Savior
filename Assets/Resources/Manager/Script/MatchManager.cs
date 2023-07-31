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
        Debug.Log("C28F5");
        if(instance != null && instance != this)
        {
            Debug.Log("C28F5-01 In if instance is different from null and this");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("C28F5-02 In if instance isn't different from null and this");
            instance = this;
        }
    }
    private void Start()
    {
        Debug.LogFormat("C28F57");
        turnPresent = K_PlayerSide.Blue;
        this.RegisterListener(EventID.OnMoveCardToSummonZone, param => MoveCardToSummonZoneEvent((string)param));
        this.RegisterListener(EventID.OnMoveCardToFightZone, param => MoveCardToFightZoneEvent((string)param));
        this.RegisterListener(EventID.OnSummonMonster, param => SummonCardEvent(param as SummonArgs));
        this.RegisterListener(EventID.OnMoveCardInTriggerSpell, param => MoveCardInTriggerSpellEvent(param as MoveCardInTriggerSpellArgs));
        StartCoroutine(InitalGameProcess());
    }

    public void StoreGainedAttributeAction(string attributeName)
    {
        Debug.LogFormat("C28F61");
        if(!AttributeGained.ContainsKey(round))
        {
            Debug.LogFormat("C28F61-01");
            AttributeGained[round].Add(attributeName);
        }
    }

    public void MoveCardInTriggerSpellEvent(MoveCardInTriggerSpellArgs args)
    {
        Debug.LogFormat("C28F30");
        object[] datas = new object[] { this.localPlayerSide, args.card.photonView.ViewID };
        Debug.LogFormat("C28F30 LocalPlayerSide is {0}, view id of card is {1}", this.localPlayerSide, args.card.photonView.ViewID);
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent((byte)RaiseEvent.MoveCardInTriggerSpell, datas, raiseEventOptions, SendOptions.SendUnreliable);
    }
    IEnumerator MoveCardInTriggerSpellAction(CardPlayer player, SpellCard card)
    {
        Debug.LogFormat("C28F29");
        card.IsSelected = false;
        var cardSelected = player.hand.Draw(card);
        card.Position = CardPosition.InTriggerSpellField;

        card.RemoveCardFormParentPresent();
        card.MoveCardIntoNewParent(player.spellZone.transform);

        yield return StartCoroutine(EffectManager.Instance.OnExecuteSpell(card));
        if(EffectManager.Instance.status == EffectManager.EffectStatus.success)
        {
            Debug.LogFormat("C28F29-01");
            player.spellZone.SpellCard = card;
            player.mana.Number -= card.Cost;

            Debug.LogFormat("C28F29-01 Used card is {0}", card);

            if(card.SpellType == SpellType.Slow)
            {
                Debug.LogFormat("C28F29-01-01 card spell type is slow");
                SwitchTurnAction();
            }
            else
            {
                Debug.LogFormat("C28F29-01-02 card spell type isn't slow");
            }
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            Debug.LogFormat("C28F29-02 Failed to use card spell, card is {0}", card);
        }
        yield return null;
    }
    private void OnEnable()
    {
        Debug.LogFormat("C28F39");
        PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
    }

    private void OnDisable()
    {
        Debug.LogFormat("C28F38");
        PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
    }

    public void OnclickLeftRoom()
    {
        Debug.Log("C28F35");
    }

    public void OnclickSwitchScene()
    {
        Debug.Log("C28F36");
    }



    #region Set Up Match
    /// <summary>
    /// Initial Player model
    /// </summary>
    /// <returns></returns>
    IEnumerator PlayerInit()
    {
        print(this.debug("C28F41 PlayerInit", new
        {
            PhotonNetwork.LocalPlayer.CustomProperties
        }));

        localPlayerSide = PhotonNetwork.LocalPlayer.CustomProperties[K_Player.K_PlayerSide.key].ToString();

        if(localPlayerSide.Equals(K_Player.K_PlayerSide.Blue))
        {
            Debug.LogFormat("C28F41-01 Local Player Side is Blue");
            LocalPlayer = PhotonNetwork.Instantiate("BluePlayer", positionBlue, Quaternion.Euler(rotationBlue)).GetComponent<CardPlayer>();
        }
        else if(localPlayerSide.Equals(K_Player.K_PlayerSide.Red))
        {
            Debug.LogFormat("C28F41-02 Local Player Side is Red");
            LocalPlayer = PhotonNetwork.Instantiate("RedPlayer", positionRed, Quaternion.Euler(rotationRed)).GetComponent<CardPlayer>();
        }
        yield return new WaitUntil(() => redPlayer != null && bluePlayer != null);
    }
    /// <summary>
    /// Set data all zone from player side (CardPlayer, MatchManager)
    /// </summary>
    void SetUpField()
    {
        Debug.LogFormat("C28F54");  
        print(this.debug("C28F54", new
        {
            playerNotNull = bluePlayer != null,
            NumberSummonZone = bluePlayer.summonZones.Count
        }));

        foreach(SummonZone summonZone in bluePlayer.summonZones)
        {
            Debug.LogFormat("C28F54 summonZone in blue player summonzone is {0}", summonZone);
            summonZone.player = bluePlayer;
            summonZone.matchManager = this;
        }

        foreach(FightZone fightZone in bluePlayer.fightZones)
        {
            Debug.LogFormat("C28F54 fightZone in blue player fightZones is {0}", fightZone);
            fightZone.player = bluePlayer;
            fightZone.matchManager = this;
        }

        bluePlayer.spellZone.player = bluePlayer;
        bluePlayer.spellZone.matchManager = this;

        print(this.debug("C28F54 Red SM count", new
        {
            playerNotNull = redPlayer != null,
            NumberSummonZone = redPlayer.summonZones.Count
        }));
        foreach(SummonZone summonZone in redPlayer.summonZones)
        {
            Debug.LogFormat("C28F54 summonZone in red player summonzone is {0}", summonZone);
            summonZone.player = redPlayer;
            summonZone.matchManager = this;
        }

        foreach(FightZone fightZone in redPlayer.fightZones)
        {
            Debug.LogFormat("C28F54 fightZone in red player fightZones is {0}", fightZone);
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
        Debug.Log("C28F22");
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
        Debug.LogFormat("C28F6");
        UIMatchManager.instance.TurnLoadingScene(true);

        yield return StartCoroutine(PlayerInit());

        PlayerSetTokken();

        SetUpField();

        string deckBlue = PhotonNetwork.CurrentRoom.CustomProperties[K_Player.DeckBlue].ToString();
        string deckRed = PhotonNetwork.CurrentRoom.CustomProperties[K_Player.DeckRed].ToString();
        Debug.LogFormat("C28F6 deckBlue is: {0}, deckRed is: {1}", deckBlue, deckRed);

        yield return StartCoroutine(bluePlayer.deck.getCardDataInDeck(deckBlue));

        yield return StartCoroutine(redPlayer.deck.getCardDataInDeck(deckRed));

        yield return StartCoroutine(LocalPlayer.deck.CreateMonsterCardsInDeckMatch());

        yield return new WaitUntil(() =>
        {
            Debug.LogFormat("C28F6 Check Deck of bluePlayer is Full: {0}, Check Deck of redplayer is full {1}", bluePlayer.deck.Full, redPlayer.deck.Full);
            return bluePlayer.deck.Full
             && redPlayer.deck.Full;
        });

        ExitGames.Client.Photon.Hashtable _myRoomCustomProperties = new ExitGames.Client.Photon.Hashtable();
        _myRoomCustomProperties = PhotonNetwork.CurrentRoom.CustomProperties;

        if(localPlayerSide == K_PlayerSide.Red)
        {
            Debug.LogFormat("C28F6-01 localPlayerSide is Red");
            _myRoomCustomProperties[K_Player.OrderInstanceCardRed] = string.Join("@", redPlayer.deck.GetAll().Select(card => card.photonView.ViewID));
            Debug.LogFormat("C28F6-01 send Template (Red): {0} ", _myRoomCustomProperties[K_Player.OrderInstanceCardRed]));
            PhotonNetwork.CurrentRoom.SetCustomProperties(_myRoomCustomProperties);
        }
        else if(localPlayerSide == K_PlayerSide.Blue)
        {
            Debug.LogFormat("C28F6-02 localPlayerSide is Blue");
            _myRoomCustomProperties[K_Player.OrderInstanceCardBlue] = string.Join("@", bluePlayer.deck.GetAll().Select(card => card.photonView.ViewID));
            Debug.LogFormat("C28F6-02 send Template (Blue): {0} ", _myRoomCustomProperties[K_Player.OrderInstanceCardBlue]));
            PhotonNetwork.CurrentRoom.SetCustomProperties(_myRoomCustomProperties);
        }
        else
        {
            Debug.LogFormat("C28F6-03 Local player side is not set");
        }

        yield return StartCoroutine(Next());

        yield return StartCoroutine(SyncOppositePlayerDeck());

        var deskBlue = bluePlayer.deck.GetAll();
        Debug.LogFormat("C28F6 BLUEDECK" + string.Join("\n", deskBlue.Select(c => string.Format("{0}. {1}", deskBlue.IndexOf(c), c))));

        var deskred = redPlayer.deck.GetAll();
        Debug.LogFormat("C28F6 REDDECK" + string.Join("\n", deskred.Select(c => string.Format("{0}. {1}", deskred.IndexOf(c), c))));

        UIMatchManager.instance.TurnLoadingScene(false);

        SoundManager.instance.PlayBackground_Match();

        //provide initial resource
        SetLimitHP(initialHP, bluePlayer);
        SetLimitHP(initialHP, redPlayer);

        ProvideHP(initialHP, bluePlayer);
        ProvideHP(initialHP, redPlayer);

        yield return StartCoroutine(Next());
        //draw amout of card when start match
        if(MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Blue))
        {
            Debug.LogFormat("C28F6-04 Draw amount of card when start match of blue player");
            yield return StartCoroutine(DrawPhase(5, bluePlayer));

        }
        else if(MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Red))
        {
            Debug.LogFormat("C28F6-05 Draw amount of card when start match of red");
            yield return StartCoroutine(DrawPhase(5, redPlayer));
        }
        yield return new WaitForSeconds(3);
    }

    private IEnumerator SyncOppositePlayerDeck()
    {
        Debug.LogFormat("C28F66");
        yield return new WaitUntil(() => PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(K_Player.OrderInstanceCardBlue) && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(K_Player.OrderInstanceCardRed));

        List<int> oppositeDeckOrder = null;
        if(localPlayerSide == K_PlayerSide.Red)
        {
            Debug.LogFormat("C28F66-01 local player is red");
            var deckOrder = PhotonNetwork.CurrentRoom.CustomProperties[K_Player.OrderInstanceCardBlue].ToString().Split('@');
            oppositeDeckOrder = deckOrder.Select(int.Parse).ToList();
        }
        else if(localPlayerSide == K_PlayerSide.Blue)
        {
            Debug.LogFormat("C28F66-02 local player is red");
            var deckOrder = PhotonNetwork.CurrentRoom.CustomProperties[K_Player.OrderInstanceCardRed].ToString().Split('@');
            oppositeDeckOrder = deckOrder.Select(int.Parse).ToList();
        }
        else
        {
            Debug.LogError("C28F66-03 Local player side is not set");
        }
        yield return StartCoroutine(OpponentPlayer.deck.SetOrderInstanceCard(oppositeDeckOrder));
        CameraManager.instance.OnclickSwitchCameraNormal();
    }

    private IEnumerator Next()
    {
        Debug.LogFormat("C28F34");
        LocalPlayer.isNEXT_STEP = true;
        object[] datas = new object[] { localPlayerSide, LocalPlayer.isNEXT_STEP };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        var requestComplete = PhotonNetwork.RaiseEvent((byte)RaiseEvent.NEXT_STEP, datas, raiseEventOptions, SendOptions.SendUnreliable);

        yield return new WaitUntil(() =>
        {
            Debug.LogFormat("C28F34 local is {0}, opponent is {1}", LocalPlayer.isNEXT_STEP, OpponentPlayer.isNEXT_STEP, );
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
        Debug.LogFormat("C28F27");
        UIMatchManager.instance.SkipTurn_Interactive = false;
        UIMatchManager.instance.setEventSkipTurn(SkipTurnEvent);

        yield return StartCoroutine(Next());
        int i = 0; 
        do
        {
            Debug.LogFormat("C28F27 {0} times", ++i);
            yield return StartCoroutine(BeginRound());
            yield return StartCoroutine(MiddleRound());
            yield return StartCoroutine(EndRound());
        } while(!isEndMatch);
        yield return null;
    }

    IEnumerator EndMatch()
    {
        Debug.Log("C28F14");
        switch(FindMatchSystem.instance.gameMode)
        {
            case GameMode.Normal:
                Debug.Log("C28F14-01 Game mode is normal");
                yield return StartCoroutine(ProvideReward(false));
                break;
            case GameMode.Rank:
                Debug.Log("C28F14-02 Game mode is rank");
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
        Debug.LogFormat("C28F7");
        round = round + 1; //increase round number
        Debug.LogFormat("C28F7 Begin Round: {0}", round);
        gamePhase = GamePhase.Normal; //set game phase to defaut

        if(MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Blue))
        {
            Debug.LogFormat("C28F7-01 Local side is blue player");
            yield return StartCoroutine(DrawPhase(1, bluePlayer));
        }
        else if(MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Red))
        {
            Debug.LogFormat("C28F7-02 Local side is red player");
            yield return StartCoroutine(DrawPhase(1, redPlayer));
        }
        else
        {
            Debug.LogFormat("C28F7-03 Local side is not yet");
        }

        //provide Mana
        ProvideMP(1, bluePlayer);
        ProvideMP(1, redPlayer);
        SetLimitMP(1, bluePlayer);
        SetLimitMP(1, redPlayer);

        if(round > 1)
        {
            Debug.LogFormat("C28F7-04 Change Token")
            ChangeToken();
        }

        Debug.LogFormat("C28F7 Set Player get first TURN BEFORE include round: {0}, turnPresent: {1}", round, turnPresent);

        this.turnPresent = bluePlayer.tokken == GameTokken.Attack ? K_Player.K_PlayerSide.Blue : K_Player.K_PlayerSide.Red;

        Debug.LogFormat("C28F7 Set Player get first TURN AFTER include round: {0}, turnPresent: {1}", round, turnPresent);

        UIMatchManager.instance.ChangeTokken();

        SetRightToAttack();
        if(!AttributeGained.ContainsKey(round))
        {
            Debug.LogFormat("C28F7-05")
            {
                AttributeGained.Add(round, new List<string>());
            }
        }

        this.PostEvent(EventID.OnStartRound, this);
    }
    /// <summary>
    /// Turn process here
    /// </summary>
    /// <returns></returns>
    IEnumerator MiddleRound()
    {
        Debug.Log("C28F28");
        yield return StartCoroutine(TurnProcess());
    }
    /// <summary>
    /// end a round
    /// </summary>
    /// <returns></returns>
    IEnumerator EndRound()
    {
        Debug.Log("C28F15");
        this.PostEvent(EventID.OnEndRound, this);
        yield return StartCoroutine(EffectManager.Instance.ExecuteOnEndRound(this));
        //TODO: Event end round
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
        Debug.LogFormat("C28F67");
        int i= 0;
        do
        {
            Debug.LogFormat("C28F67 excute do while i = ", ++i);
            yield return StartCoroutine(StartTurn());
            yield return StartCoroutine(EndTurn());
            print(this.debug(null, new
            {
                redSkip = redPlayer.isSkipTurn,
                blueSkip = bluePlayer.isSkipTurn
            })); ;
        } while(!isNextRound() && !isEndMatch); //run until 2 player skip turn or end match
        ResetSkipTurn();
        yield return null;
    }

    private bool isNextRound()
    {
        Debug.LogFormat("C28F23");
        Debug.LogFormat("C28F23 redPlayer is Skip Turn {0}, bluePlayer is skip Turn: {1}", redPlayer.isSkipTurn, bluePlayer.isSkipTurn);
        return redPlayer.isSkipTurn && bluePlayer.isSkipTurn;
    }

    IEnumerator StartTurn()
    {
        Debug.LogFormat("C28F60");
        Debug.LogFormat("C28F60 local player side is ", localPlayerSide);
        Debug.LogFormat("C28F60 turnPresent is ", turnPresent);

        if (localPlayerSide.Equals(turnPresent))
        {
            Debug.LogFormat("C28F60-01 local Player Side equal turn present");
            SoundManager.instance.PlayYourTurn();
            UIMatchManager.instance.Turn(turnPresent);
            UIMatchManager.instance.SkipTurn_Interactive = true;
            UIMatchManager.instance.PrintYourTurn();
        }
        else
        {
            Debug.LogFormat("C28F60-02 local Player Side not equal turn present");
            UIMatchManager.instance.Turn(turnPresent);
            UIMatchManager.instance.SkipTurn_Interactive = false;
            UIMatchManager.instance.PrintopponnetTurn();
        }
        this.PostEvent(EventID.OnStartTurn, this);
        yield return StartCoroutine(WaitUntilPlayerSwitchTurn());
    }

    IEnumerator EndTurn()
    {
        Debug.Log("C28F16");
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
        Debug.LogFormat("C28F12");
        object[] datas = new object[] { amount, player.photonView.ViewID };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        var result = PhotonNetwork.RaiseEvent((byte)RaiseEvent.DRAW_CARD_EVENT, datas, raiseEventOptions, SendOptions.SendUnreliable);
        yield return new WaitUntil(() => result);
    }

    IEnumerator AttackAndDefensePhase()
    {
        Debug.Log("C28F2");
        var AttackPlayer = GetAttackPlayer();
        var DefensePlayer = GetDefensePlayer();
        Debug.LogFormat("C28F2 Get AttackPlayer fightZones : {0}, Get DefensePlayer fightZones: {1}", AttackPlayer.fightZones, DefensePlayer.fightZones);
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
        Debug.LogFormat("C28F20");
        CardPlayer DefensePlayer = redPlayer.tokken == GameTokken.Defend ? redPlayer : bluePlayer;
        Debug.LogFormat("C28F20 Defense Player is {0}", DefensePlayer);
        return DefensePlayer;
    }

    /// <summary>
    /// get player who have attack tokken
    /// </summary>
    /// <returns></returns>
    public CardPlayer GetAttackPlayer()
    {
        Debug.LogFormat("C28F18");
        CardPlayer AttackPlayer = redPlayer.tokken == GameTokken.Attack ? redPlayer : bluePlayer;
        Debug.LogFormat("C28F18 Attack Player is {0}", AttackPlayer);
        return AttackPlayer;
    }

    /// <summary>
    /// Get current player in turn
    /// </summary>
    /// <returns></returns>
    public CardPlayer getCurrenPlayer()
    {
        Debug.LogFormat("C28F19");
        Debug.LogFormat("C28F19 turnPresent is {0}", turnPresent);
        if (turnPresent == K_PlayerSide.Red)
        {
            Debug.LogFormat("C28F19-01 turnPresent is red: ", turnPresent == K_PlayerSide.Red);
            return redPlayer;
        }
        else
        {
            Debug.LogFormat("C28F19-02 turnPresent is blue: ", turnPresent == K_PlayerSide.Blue);
            return bluePlayer;
        }
    }


    /// <summary>
    /// Player get the tokken for attack or defense role
    /// </summary>
    private void PlayerSetTokken()
    {
        Debug.LogFormat("C28F42");
        //check who is current player then set the tokken attack to the player 
        if(bluePlayer != null && redPlayer != null)
        {
            Debug.LogFormat("C28F42-01 Set token success");
            bluePlayer.tokken = GameTokken.Attack; //blue player alway set attack tokken first
            redPlayer.tokken = GameTokken.Defend;
            bluePlayer.isAttackAvaliable = true;
            redPlayer.isAttackAvaliable = false;
        }
        else
        {
            Debug.LogFormat("C28F42-02 Set token fail");
        }
    }

    /// <summary>
    /// auto switching player's tokken between 2 player (attack, defense)
    /// </summary>
    private void ChangeToken()
    {
        Debug.LogFormat("C28F8");
        if(bluePlayer != null && redPlayer != null)
        {
            Debug.LogFormat("C28F8-01 bluePlayer and redPlayer exist, start change Token");
            ((bluePlayer.tokken, bluePlayer.isAttackAvaliable), (redPlayer.tokken, redPlayer.isAttackAvaliable)) = bluePlayer.tokken == GameTokken.Attack ? ((GameTokken.Defend, false), (GameTokken.Attack, true)) : ((GameTokken.Attack, true), (GameTokken.Defend, false));
            UIMatchManager.instance.ChangeTokken();
        }
        else
        {
            Debug.LogFormat("C28F8-02 change tokken fail");
        }
    }

    /// <summary>
    /// waitting until current player click skip turn or Skip turn Action
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitUntilPlayerSwitchTurn()
    {
        Debug.LogFormat("C28F68");
        print(this.debug("Waiting player Action"));
        yield return new WaitUntil(() => ActionInTurn == PlayerAction.SwitchTurn || isEndMatch);
        Debug.LogFormat("C28F68 Player Action {0}", PlayerActionName = ActionInTurn.ToString());
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
        Debug.LogFormat("C28F50");
        Debug.LogFormat("C28F50 card player is {0}, limit hp is {1}", cardPlayer.hp.Limit);
        cardPlayer.hp.Limit = amount;
    }

    public void SetLimitMP(int amount, CardPlayer cardPlayer)
    {
        Debug.LogFormat("C28F51");
        Debug.LogFormat("C28F51 card player is {0}, limit mana is {1}", cardPlayer.mana.Limit);
        cardPlayer.mana.Limit += amount;
    }

    void ProvideHP(int amount, CardPlayer cardPlayer)
    {
        Debug.LogFormat("C28F43");
        cardPlayer.hp.Number += amount;
        Debug.LogFormat("C28F43 Hp of {0} is {1}", cardPlayer, cardPlayer.hp.Number);
    }
    /// <summary>
    /// Provide MP for player
    /// </summary>
    /// <returns></returns>
    void ProvideMP(int amount, CardPlayer cardPlayer)
    {
        Debug.LogFormat("C28F44");
        cardPlayer.mana.Number = cardPlayer.mana.Limit + amount;
        Debug.LogFormat("C28F44 Mana of {0} is {1}", cardPlayer, cardPlayer.mana.Number);
    }
    #endregion

    #region Function with resource for player
    IEnumerator ProvideReward(bool isRanked)
    {
        Debug.LogFormat("C28F45");
        string rewardWinRankedID = "B1";
        string rewardWinNormalID = "B2";
        string rewardLoseRankedID = "B1.1";
        string rewardLoseNormalID = "B2.2";
        string rewardID = "";

        Debug.LogFormat("C28F45 Elo of Blue: {0}", PhotonNetwork.CurrentRoom.CustomProperties[K_Player.EloBlue].ToString());
        Debug.LogFormat("C28F45 Elo of Red: {0}", PhotonNetwork.CurrentRoom.CustomProperties[K_Player.EloRed].ToString());

        int eloBlue = Int32.Parse(PhotonNetwork.CurrentRoom.CustomProperties[K_Player.EloBlue].ToString());
        int eloRed = Int32.Parse(PhotonNetwork.CurrentRoom.CustomProperties[K_Player.EloRed].ToString());

        if(isBlueWin)
        {
            Debug.LogFormat("C28F45-01 Blue win");
            if (localPlayerSide.Equals(K_PlayerSide.Blue)) //win
            {
                Debug.LogFormat("C28F45-01-01 Local Player Side is Blue Win");
                switch (FindMatchSystem.instance.gameMode)
                {
                    case GameMode.Normal:
                        Debug.LogFormat("C28F45-01-01-01 Game mode is Normal");
                        rewardID = rewardWinNormalID;
                        Debug.LogFormat("C28F45-01-01-01 rewardID is {0}", rewardID);
                        break;
                    case GameMode.Rank:
                        Debug.LogFormat("C28F45-01-01-02 Game mode is Rank");
                        rewardID = rewardWinRankedID;
                        PlayfabManager.instance.CalElo(true, eloBlue, eloRed);
                        Debug.LogFormat("C28F45-01-01-02 rewardID is {0}", rewardID);
                        break;
                }
            }
            else if(localPlayerSide.Equals(K_PlayerSide.Red)) //lose
            {
                Debug.LogFormat("C28F45-01-02 Local Player Side is Red Lose");
                switch (FindMatchSystem.instance.gameMode)
                {
                    case GameMode.Normal:
                        Debug.LogFormat("C28F45-01-02-01 Game mode is Normal");
                        rewardID = rewardLoseNormalID;
                        Debug.LogFormat("C28F45-01-02-01 rewardID is {0}", rewardID);
                        break;

                    case GameMode.Rank:
                        Debug.LogFormat("C28F45-01-02-02 Game mode is Rank");
                        rewardID = rewardLoseRankedID;
                        PlayfabManager.instance.CalElo(false, eloRed, eloBlue);
                        Debug.LogFormat("C28F45-01-02-02 rewardID is {0}", rewardID);
                        break;
                }
            }
            else
            {
                Debug.LogFormat("C28F45-01-03 Undefinition");
            }
        }
        else if(isRedWin)
        {
            Debug.LogFormat("C28F45-02 Red win");
            if (localPlayerSide.Equals(K_PlayerSide.Blue)) //lose
            {
                Debug.LogFormat("C28F45-02-01 Local Player Side is Blue Lose", localPlayerSide);
                switch (FindMatchSystem.instance.gameMode)
                {
                    case GameMode.Normal:
                        Debug.LogFormat("C28F45-02-01-01 Game mode is Normal");
                        rewardID = rewardLoseNormalID;
                        Debug.LogFormat("C28F45-02-01-01 rewardID is {0}", rewardID);
                        break;
                    case GameMode.Rank:
                        Debug.LogFormat("C28F45-02-01-02 Game mode is Rank");
                        rewardID = rewardLoseRankedID;
                        PlayfabManager.instance.CalElo(false, eloBlue, eloRed);
                        Debug.LogFormat("C28F45-02-01-02 rewardID is {0}", rewardID);
                        break;
                }
            }
            else if(localPlayerSide.Equals(K_PlayerSide.Red)) //win
            {
                Debug.LogFormat("C28F45-02-02 Local Player Side is Red Win", localPlayerSide);
                switch (FindMatchSystem.instance.gameMode)
                {
                    case GameMode.Normal:
                        Debug.LogFormat("C28F45-02-02-01 Game mode is Normal");
                        rewardID = rewardWinNormalID;
                        Debug.LogFormat("C28F45-02-02-01 rewardID is {0}", rewardID);
                        break;
                    case GameMode.Rank:
                        Debug.LogFormat("C28F45-02-02-02 Game mode is Rank");
                        rewardID = rewardWinRankedID;
                        PlayfabManager.instance.CalElo(true, eloRed, eloBlue);
                        Debug.LogFormat("C28F45-02-02-02 rewardID is {0}", rewardID);
                        break;
                }
            }
            else
            {
                Debug.LogFormat("C28F45-02-03 Undefinition");
            }
        }
        else
        {
            Debug.LogFormat("C28F45-03 Undefinition");
        }

        if(rewardID != "")
        {
            Debug.LogFormat("C28F45-04 rewardID = {0}", rewardID);
            StartCoroutine(PlayfabManager.instance.BuyItems(catalog: "Reward", storeId: "BS1", new List<ItemPurchaseRequest>()
            {
                new ItemPurchaseRequest() {ItemId = rewardID, Quantity = 1}
            }, currency: "MC"));
        }
        else
        {
            Debug.LogFormat("C28F45-05 rewardID is null, rewardID is {0}", rewardID);
        }

        //set UI
        if (localPlayerSide.Equals(K_PlayerSide.Blue))
        {
            Debug.LogFormat("C28F45-06 Set UI of Blue");
            yield return StartCoroutine(UIMatchManager.instance.setResultMatch(isBlueWin, isRanked, 2));
        }
        else if(localPlayerSide.Equals(K_PlayerSide.Red))
        {
            Debug.LogFormat("C28F45-07 Set UI of Red");
            yield return StartCoroutine(UIMatchManager.instance.setResultMatch(isRedWin, isRanked, 2));
        }
        else
        {
            Debug.LogFormat("C28F45-08 Undefinition");
        }
        yield return null;
    }
    public void ResultMatch(WinCondition winCondition)
    {
        Debug.LogFormat("C28F47");
        if(!isEndMatch)
        {
            Debug.LogFormat("C28F47-01 Wincondition is {0}", winCondition);
            isEndMatch = true;
            switch(winCondition)
            {
                case WinCondition.EnemyLoseAllHp:
                    Debug.LogFormat("C28F47-01-01 Wincondition is enemyLoseAllHp");
                    if (redPlayer.hp.Number <= 0)
                    {
                        Debug.LogFormat("C28F47-01-01-01 Hp of red < 0 is {0}", redPlayer.hp.Number);
                        isBlueWin = true;
                        isRedWin = false;
                    }
                    else if(bluePlayer.hp.Number <= 0)
                    {
                        Debug.LogFormat("C28F47-01-01-02 Hp of blue < 0 is {0}", bluePlayer.hp.Number);
                        isBlueWin = false;
                        isRedWin = true;
                    }
                    else
                    {
                        Debug.LogFormat("C28F47-01-01-03 Hp of blue > 0 is {0} or hp of red > 0", bluePlayer.hp.Number, redPlayer.hp.Number);
                    }
                    break;
            }
        }
        else
        {
            Debug.LogFormat("C28F47-02 isEndMatch is {0}", isEndMatch);
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
        Debug.LogFormat("C28F33");
        var args = obj.GetData();
        
        switch((RaiseEvent)obj.Code)
        {
            case RaiseEvent.NEXT_STEP:
                {
                    Debug.LogFormat("C28F33-01 Raise Event is next step");
                    if (args.senderPlayerSide != localPlayerSide)
                    {
                        Debug.LogFormat("C28F33-01-01 sender Player Side != local Player Side");
                        StartCoroutine(OpponentPlayer.NextStepAction(args.isNEXT_STEP));
                    }
                    else
                    {
                        Debug.LogFormat("C28F33-01-02 sender Player Side == local Player Side");
                    }
                    break;
                }
            case RaiseEvent.SKIP_TURN:
                {
                    Debug.LogFormat("C28F33-02 Raise Event is Skip turn");
                    SkipTurnAction(args.playerSide as string);
                    SwitchTurnAction();
                    break;
                }
            case RaiseEvent.SWITCH_TURN:
                {
                    Debug.LogFormat("C28F33-03 Raise Event is Switch turn");
                    SwitchTurnAction();
                    break;
                }
            case RaiseEvent.ATTACK:
                {
                    Debug.LogFormat("C28F33-04 Raise Event is attack");
                    StartCoroutine(AttackAction(args.playerSide as string));
                    break;
                }
            case RaiseEvent.DEFENSE:
                {
                    Debug.LogFormat("C28F33-05 Raise Event is defense");
                    StartCoroutine(DefenseAction(args.playerSide as string));
                    break;
                }
            case RaiseEvent.SUMMON_MONSTER:
                {
                    Debug.LogFormat("C28F33-06 Raise Event is Summon monster");
                    print(this.debug("C28F33-06 Start Summon Monster Execute", new
                    {
                        args.zoneID,
                        args.cardID,
                        args.playerSide,
                        args.cardPosition,
                        args.isSpecialSummon
                    }));
                    var zoneRequest = args.playerSide == K_PlayerSide.Blue ? bluePlayer.summonZones.Find(a => a.photonView.ViewID.Equals(args.zoneID)) : redPlayer.summonZones.Find(a => a.photonView.ViewID.Equals(args.zoneID));
                    print(this.debug("C28F33-06 Zone request", new
                    {
                        zoneRequest = zoneRequest
                    }));

                    if(zoneRequest != null)
                    {
                        Debug.LogFormat("C28F33-06-01 zone Request not equal null");
                        MonsterCard card = null;
                        switch((CardPosition)args.cardPosition)
                        {
                            case CardPosition.InDeck:
                                Debug.LogFormat("C28F33-06-01-01 card position in deck");
                                card = zoneRequest.player.deck.PeekWithPhoton(args.cardID);
                                break;
                            case CardPosition.InHand:
                                Debug.LogFormat("C28F33-06-01-02 card position in hand");
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
                                Debug.LogFormat("C28F33-06-01-03 card position is defind");
                                break;
                        }

                        Debug.LogFormat("C28F33-06-01 card is {0}", card);

                        if (card != null)
                        {
                            print(this.debug("C28F33-06-01-04", new
                            {
                                zoneid = zoneRequest.photonView.ViewID,
                                nameCard = card.ToString()
                            }));
                            ExecuteSummonCardAction(zoneRequest, card, args.isSpecialSummon);
                        }
                        else
                        {
                            Debug.LogFormat("C28F33-06-01-05 Card null");
                        }
                    }
                    else
                    {
                        Debug.LogFormat("C28F33-06-02 Zone null, zone request is {0}", zoneRequest);
                    }
                    break;
                }
            case RaiseEvent.MoveCardInTriggerSpell:
                {
                    Debug.LogFormat("C28F33-07 Raise Event is Move Card in Trigger Spell");
                    CardPlayer player = args.playerSide == K_PlayerSide.Red ? redPlayer : bluePlayer;
                    Debug.LogFormat("C28F33-07 player is {0}", player);
                    if (player != null)
                    {
                        Debug.LogFormat("C28F33-07-01 player diff null");
                        object card = player.hand.PeekWithPhoton(args.cardID);

                        if(card != null && card is SpellCard spellCard)
                        {
                            Debug.LogFormat("C28F33-07-01-01 card is {0}, spellCard is {1}", card, spellCard);
                            StartCoroutine(MoveCardInTriggerSpellAction(player, spellCard));
                        }
                        else
                        {
                            Debug.LogFormat("C28F33-07-01-02 card is null {0}", card);
                        }
                    }
                    else
                    {
                        Debug.LogFormat("C28F33-07-02 player not diff null");
                    }
                    break;
                }
        }
    }

    public void ExecuteSummonCardAction(SummonZone zoneRequest, MonsterCard card, bool isSpecialSummon)
    {
        Debug.LogFormat("C28F17");
        if(zoneRequest != null && card != null)
        {
            Debug.LogFormat("C28F17-01 zoneRequest: {0}, card: {1}, isSpecialSummon: {2}", zoneRequest, card, isSpecialSummon);
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
        Debug.LogFormat("C28F63");
        print(this.debug("C28F63 Summon card to summon zone", new
        {
            args.card,
            isFilled = args.summonZone.isFilled(),
            args.cardPosition,
            args.isSpecialSummon
        }));
        if(args.card == null || args.summonZone.isFilled())
        {
            print(this.debug("C28F63-01 not valid card or summon zone is fill", new
            {
                card = args.card,
                isFill = args.summonZone.isFilled()
            }));
            return false;
        }
        else
        {
            print(this.debug("C28F63-02 valid card or summon zone is fill", new
            {
                card = args.card,
                isFill = args.summonZone.isFilled()
            }));
        }

        //tru mana
        if(!args.isSpecialSummon && args.card.Cost > args.summonZone.player.mana.Number)
        {
            Debug.LogFormat("C28F63-03 not enough mana to summon");
            return false;
        }
        else
        {
            Debug.LogFormat("C28F63-04 enough mana to summon");
        }
        // ID zone, ID cardTarget
        object[] datas = new object[] { args.summonZone.photonView.ViewID, args.card.photonView.ViewID, args.summonZone.player.side, (int)args.cardPosition, args.isSpecialSummon ? 1 : 0 };
        print(this.debug("C28F63 zone and target id", new
        {
            zoneID = args.summonZone.photonView.ViewID,
            cardID = args.card.photonView.ViewID,
            playerSide = args.summonZone.player.side
        }));
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent((byte)RaiseEvent.SUMMON_MONSTER, datas, raiseEventOptions, SendOptions.SendUnreliable);
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
        Debug.LogFormat("C28F62");
        card.IsSelected = false;
        card.CardPlayer.hand.RemoveCardFormHand(card);

        zoneRequest.SetMonsterCard(card);
        zoneRequest.isSelected = false;
        if (!isSpecialSummon)
        {
            Debug.LogFormat("C28F62-01");
            zoneRequest.player.mana.Number -= card.Cost;
            Debug.LogFormat("C28F62-01 zoneRequest.player.mana.Number : {0} ", zoneRequest.player.mana.Number);
        }
        else
        {
            Debug.LogFormat("C28F62-02 is Special Summon is {0}", isSpecialSummon);
        }
        yield return StartCoroutine(EffectManager.Instance.OnAfterSummon(card));
        SwitchTurnAction();
    }
    #endregion

    #region ATTACK Event - Action
    public void AttackEvent()
    {
        Debug.Log("C28F3");
        if (LocalPlayer.tokken == GameTokken.Attack)
        {
            Debug.Log("C28F3-01 You can attack");
            object[] datas = new object[] { localPlayerSide };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent((byte)RaiseEvent.ATTACK, datas, raiseEventOptions, SendOptions.SendUnreliable);
        }
        else
        {
            Debug.Log("C28F3-02 You can not attack");
        }
    }
    /// <summary>
    /// Phat dong attack
    /// </summary>
    /// <param name="playerSide"></param>
    public IEnumerator AttackAction(string playerSide)
    {
        Debug.Log("C28F1");
        yield return StartCoroutine(StartAttackPhase());
        yield return StartCoroutine(EndAttackPhase());
        SwitchTurnAction();
    }

    private IEnumerator EndAttackPhase()
    {
        Debug.Log("C28F13");
        yield return null;
    }

    private IEnumerator StartDefendPhase()
    {
        Debug.Log("C28F59");
        yield return null;
    }

    private IEnumerator StartAttackPhase()
    {
        Debug.LogFormat("C28F58");
        this.gamePhase = GamePhase.Attack;
        var playerAttack = GetAttackPlayer();
        var playerDefense = GetDefensePlayer();

        playerAttack.playerAction = PlayerAction.Attack;
        playerAttack.isAttackAvaliable = false;
        if(LocalPlayer.tokken == GameTokken.Defend)
        {
            Debug.LogFormat("C28F58-01 local player token is defense");
            SetDefenseAction();
        }
        else 
        {
            Debug.LogFormat("C28F58-02 local player token is {0}", _LocalPlayer.tokken);
        }
        yield return StartCoroutine(EffectManager.Instance.None()); //effect
        SwitchTurnEvent();
        yield return null;
    }

    #endregion

    #region DEFEND Event - Action
    public void DefenseEvent()
    {
        Debug.LogFormat("C28F11");
        object[] datas = new object[] { localPlayerSide };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent((byte)RaiseEvent.DEFENSE, datas, raiseEventOptions, SendOptions.SendUnreliable);
    }

    public IEnumerator DefenseAction(string playerSide)
    {
        Debug.LogFormat("C28F10");
        yield return StartCoroutine(AttackAndDefensePhase());
        SetSkipAction();
    }
    #endregion

    #region SWITCH_TURN Event - Action
    private void SwitchTurnEvent()
    {
        Debug.LogFormat("C28F65");
        object[] datas = new object[] { };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent((byte)RaiseEvent.SWITCH_TURN, datas, raiseEventOptions, SendOptions.SendUnreliable);
    }
    /// <summary>
    /// Turn chi qua turn moi khi ma dung switch turn
    /// </summary>
    public void SwitchTurnAction()
    {
        Debug.LogFormat("C28F64");
        print(this.debug("C28F64 Before turn change", new
        {
            this.localPlayerSide,
            turnPresent,
            round
        }));

        turnPresent = (turnPresent == K_PlayerSide.Blue ? K_PlayerSide.Red : K_PlayerSide.Blue);
        print(this.debug("C28F64 Affter turn change", new
        {
            this.localPlayerSide,
            turnPresent,
            round
        }));

        ActionInTurn = PlayerAction.SwitchTurn;
    }
    #endregion

    #region SKIP_TURN Event - Action
    public void SkipTurnEvent()
    {
        Debug.LogFormat("C28F55");
        Debug.LogFormat("C28F55 {0} click skip turn", LocalPlayer.side);
        object[] datas = new object[] { localPlayerSide };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent((byte)RaiseEvent.SKIP_TURN, datas, raiseEventOptions, SendOptions.SendUnreliable);
    }
    void SkipTurnAction(string playerSide)
    {
        Debug.LogFormat("C28F56");
        Debug.LogFormat("C28F56 player Side is {0}", playerSide);
        if(playerSide.Equals(K_PlayerSide.Blue))
        {
            Debug.LogFormat("C28F56-01 player is Blue");
            bluePlayer.isSkipTurn = true;
        }
        else if(playerSide.Equals(K_PlayerSide.Red))
        {
            Debug.LogFormat("C28F56-02 player is Red");
            redPlayer.isSkipTurn = true;
        }
        else
        {
            Debug.LogFormat("C28F56-03 player is unidentified");
        }
    }
    #endregion

    #endregion

    #region *LOCAL* Event - Action
    private void MoveCardToSummonZoneEvent(string playerSide)
    {
        Debug.LogFormat("C28F32");
        if(playerSide.Equals(localPlayerSide))
        {
            Debug.LogFormat("C28F32-01 Local Player Side is {0}", localPlayerSide);
            if (localPlayerSide.Equals(K_PlayerSide.Blue))
            {
                Debug.LogFormat("C28F32-01-01 local Player Side is Blue");
                int count = bluePlayer.fightZones.FindAll(a => a.monsterCard != null).Count;
                Debug.LogFormat("C28F32-01-01 Number Card in Fight Field BLUE: {0} ", count);
                if (count == 0)
                {
                    Debug.LogFormat("C28F32-01-01-01 Number Card in Fight Field BLUE: 0 ");
                    SetSkipAction();
                }
                else
                {
                    Debug.LogFormat("C28F32-01-01-02 Number Card in Fight Field BLUE: {0}", count);
                }
            }
            else if(localPlayerSide.Equals(K_PlayerSide.Red))
            {
                Debug.LogFormat("C28F32-01-02 local Player Side is Red");
                int count = redPlayer.fightZones.FindAll(a => a.monsterCard != null).Count;
                Debug.LogFormat("C28F32-01-02 Number Card in Fight Field RED: {0} ", count);
                if(count == 0)
                {
                    Debug.LogFormat("C28F32-01-02-01 Number Card in Fight Field RED: 0 ");
                    SetSkipAction();
                }
                else
                {
                    Debug.LogFormat("C28F32-01-02-02 Number Card in Fight Field RED: {0}", count);
                }
            }
            else
            {
                Debug.LogFormat("C28F32-01-03 local player side not equal red or blue, local player side is {0}", localPlayerSide);
            }
        }
        else
        {
            Debug.LogFormat("C28F32-02 player side not equal local player side,player side is {0}, local player side is {1}",playerSide ,localPlayerSide);
        }
    }

    private void MoveCardToFightZoneEvent(string playerSide)
    {
        Debug.LogFormat("C28F31");
        if(playerSide.Equals(localPlayerSide) && isRightToDefense(playerSide))
        {
            Debug.LogFormat("C28F31-01 playerside is {0} defense", playerSide);
            SetDefenseAction();
        }
        else if(playerSide.Equals(localPlayerSide) && isRightToAttack(playerSide))
        {
            Debug.LogFormat("C28F31-02 playerside is {0} attack", playerSide);
            SetAttackAction();
        }
        else
        {
            Debug.LogFormat("C28F31-03 Move Card To Fight Zone FAIL");
        }
    }
    #endregion

    #region Functions Check State
    public bool isPlayerTurn(string playerSide)
    {
        Debug.LogFormat("C28F24");
        if(turnPresent.Equals(playerSide))
        {
            Debug.LogFormat("C28F24-01 equal turnPresent: {0}, playerside: {1}", turnPresent, playerSide);
            return true;
        }
        else
        {
            Debug.LogFormat("C28F24-02 Not equal turnPresent: {0}, playerside: {1}", turnPresent, playerSide);
            return false;
        }
    }

    public bool isRightToAttack(string playerSide)
    {
        Debug.LogFormat("C28F25");
        if(playerSide.Equals(K_PlayerSide.Blue))
        {
            Debug.LogFormat("C28F25-01 playSide equal blue, playside is: {0}", playerSide);
            Debug.LogFormat("C28F25-01 bluePlayer.tokken == GameTokken.Attack is {0}, blueplayer.token is: {1}", bluePlayer.tokken == GameTokken.Attack, bluePlayer.tokken);
            Debug.LogFormat("C28F25-01 bluePlayer.isAttackAvaliable is {0}", bluePlayer.isAttackAvaliable);
            return bluePlayer.tokken == GameTokken.Attack && bluePlayer.isAttackAvaliable;
        }
        else if(playerSide.Equals(K_PlayerSide.Red))
        {
            Debug.LogFormat("C28F25-02 playSide equal red, playside is: {0}", playerSide);
            Debug.LogFormat("C28F25-02 redPlayer.tokken == GameTokken.Attack is {0}, redplayer.token is: {1}", redPlayer.tokken == GameTokken.Attack, redPlayer.tokken);
            Debug.LogFormat("C28F25-02 redPlayer.isAttackAvaliable is {0}", redPlayer.isAttackAvaliable);
            return redPlayer.tokken == GameTokken.Attack && redPlayer.isAttackAvaliable;
        }
        return false;
    }

    public bool isRightToDefense(string playerSide)
    {
        Debug.LogFormat("C28F26");

        if (playerSide.Equals(K_PlayerSide.Blue))
        {
            Debug.LogFormat("C28F26-01 playSide equal blue, playside is: {0}", playerSide);
            Debug.LogFormat("C28F26-01 bluePlayer.tokken == GameTokken.Defend is {0}, blueplayer.token is: {1}", bluePlayer.tokken == GameTokken.Defend, bluePlayer.tokken);
            return bluePlayer.tokken == GameTokken.Defend;
        }
        else if(playerSide.Equals(K_PlayerSide.Red))
        {
            Debug.LogFormat("C28F26-02 playSide equal red, playside is: {0}", playerSide);
            Debug.LogFormat("C28F26-02 redPlayer.tokken == GameTokken.Defend is {0}, redplayer.token is: {1}", redPlayer.tokken == GameTokken.Defend, redPlayer.tokken);
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
        Debug.LogFormat("C28F52");
        print(this.debug("C28F52", new
        {
            turnPresent,
            localPlayerSide
        }));
        //UI
        if(LocalPlayer.tokken == GameTokken.Attack)
        {
            Debug.LogFormat("C28F52-01 local player Attack");
            UIMatchManager.instance.RightAttack();
            UIMatchManager.instance.PrintYourAttack();
        }
        else
        {
            Debug.LogFormat("C28F52-02 local player Defense");
            UIMatchManager.instance.RightAttack();
            UIMatchManager.instance.PrintYourDefense();
        }
    }


    IEnumerator ClearAttackField(string playerSide)
    {
        Debug.LogFormat("C28F9");
        yield return new WaitForSeconds(3f);

        if(playerSide.Equals(K_PlayerSide.Blue))
        {
            Debug.LogFormat("C28F9-01 player side is Blue");
            foreach (FightZone fightZone in bluePlayer.fightZones)
            {
                Debug.LogFormat("C28F9-01 fight zone of blue player: ", FightZone fightZone in bluePlayer.fightZones);
                if (fightZone.monsterCard != null && !fightZone.monsterCard.Position.Equals(CardPosition.InGraveyard))
                {
                    Debug.LogFormat("C28F9-01-01 BLUEPLAYER The card's fight zone is not in the grave position");
                    SummonZone summonZone = bluePlayer.summonZones.FirstOrDefault(zone => !zone.isFilled() && !zone.isSelected);
                    Debug.LogFormat("C28F9-01-01 summon zone of BLUEPLAYER is selected, ", summonZone);
                    if (summonZone != null)
                    {
                        Debug.LogFormat("C28F9-01-01-01 summonZone of BLUEPLAYER is ", summonZone);
                        summonZone.RaiseMoveCardFromAttackFieldToSummonField(fightZone, fightZone.monsterCard);
                    }
                    else
                    {
                        Debug.LogFormat("C28F9-01-01-02 summonZone of BLUEPLAYER isn't selected");
                    }
                }
                else
                {
                    Debug.LogFormat("C28F9-01-02 BLUEPLAYER card is null");
                }
            }
        }
        else if(playerSide.Equals(K_PlayerSide.Red))
        {
            Debug.LogFormat("C28F9-02 player side is Red");
            foreach (FightZone fightZone in redPlayer.fightZones)
            {
                Debug.LogFormat("C28F9-02 fight zone of red player: ", FightZone fightZone in redPlayer.fightZones);
                if (fightZone.monsterCard != null && !fightZone.monsterCard.Position.Equals(CardPosition.InGraveyard))
                {
                    Debug.LogFormat("C28F9-02-01 REDPLAYER The card's fight zone is not in the grave position");
                    SummonZone summonZone = redPlayer.summonZones.FirstOrDefault(zone => !zone.isFilled() && !zone.isSelected);
                    Debug.LogFormat("C28F9-02-01 summon zone of REDPLAYER is selected, ", summonZone);

                    if (summonZone != null)
                    {
                        Debug.LogFormat("C28F9-02-01-01 summonZone of REDPLAYER is ", summonZone);
                        summonZone.RaiseMoveCardFromAttackFieldToSummonField(fightZone, fightZone.monsterCard);
                    }
                    else
                    {
                        Debug.LogFormat("C28F9-02-01-02 summonZone of REDPLAYER isn't selected");
                    }
                }
                else
                {
                    Debug.LogFormat("C28F9-02-02 REDPLAYER card is null");
                }
            }
        }
    }

    IEnumerator AttackZoneOpposite(List<FightZone> attackZones, List<FightZone> defenseZones)
    {
        Debug.LogFormat("C28F4");
        foreach(FightZone attackZone in attackZones)
        {
            Debug.LogFormat("C28F4 In Foreach FightZone attackZone in attackZones: {0}", FightZone attackZone in attackZones);
            if (attackZone.monsterCard != null)
            {
                Debug.LogFormat("C28F4-01 In if attackZone have monsterCard {0}", attackZone.monsterCard);
                int indexAttackZone = attackZones.IndexOf(attackZone);
                MonsterCard monsterAttack = attackZone.monsterCard;

                FightZone defenseZone = defenseZones.ElementAt(indexAttackZone);
                MonsterCard monsterDefense = defenseZone.GetMonsterCard();

                if(monsterDefense != null)
                {
                    Debug.LogFormat("C28F4-01-01 In if defense zone have monster card to defense");
                    this.PostEvent(EventID.OnCardAttack, new AnimationAttackArgs(monsterDefense, monsterAttack));
                    yield return new WaitForSeconds(0.6f);

                    monsterAttack.attack(monsterDefense);
                    yield return new WaitForSeconds(0.6f);

                    this.PostEvent(EventID.OnCardAttack, new AnimationAttackArgs(monsterAttack, monsterDefense));
                    yield return new WaitForSeconds(0.6f);
                    monsterDefense.attack(monsterAttack);
                }
                else
                {
                    Debug.LogFormat("C28F4-01-02 If denfense zone have not monster card to denfese, monsterAttack will attack player, Start attack player");
                    defenseZone.player.hp.decrease(monsterAttack.Attack);
                }
            }
            else
            {
                Debug.LogFormat("C28F4-02 In if attackZone have not monsterCard {0}", attackZone.monsterCard);
            }
        }
    }

    void SetSkipAction()
    {
        Debug.LogFormat("C28F53");
        UIMatchManager.instance.removeAllEventSkipTurn();
        UIMatchManager.instance.setEventSkipTurn(SkipTurnEvent);
    }

    void SetDefenseAction()
    {
        Debug.LogFormat("C28F49");
        UIMatchManager.instance.removeAllEventSkipTurn();
        UIMatchManager.instance.setEventDefense(DefenseEvent);
    }

    void SetAttackAction()
    {
        Debug.LogFormat("C28F48")
        UIMatchManager.instance.removeAllEventSkipTurn();
        UIMatchManager.instance.setEventAtk(AttackEvent);
    }
    /// <summary>
    /// set to default player skip turn action
    /// </summary>
    public void ResetSkipTurn()
    {
        Debug.LogFormat("C28F46");
        redPlayer.isSkipTurn = false;
        bluePlayer.isSkipTurn = false;
    }
    #endregion

    #region ObserverPattern-Register Function
    public override void OnLeftRoom()
    {
        Debug.LogFormat("C28F40");
        if(PhotonNetwork.IsConnected)
        {
            Debug.LogFormat("C28F40-01 Connected");
            PhotonNetwork.LoadLevel("Home");
        }
        else
        {
            Debug.LogFormat("C28F40-02 Not Connected");
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.LogFormat("C28F37");
    }
    #endregion

    public void GetPlayerInfo()
    {
        Debug.Log("C28F21");
        Debug.Log(this.debug("C28F21 Players", new
        {
            LocalPlayer,
            OpponentPlayer
        }));
        Debug.Log(this.debug("C28F21 LocalPlayer details", new
        {
            LocalPlayer.tokken,
            LocalPlayer.isSkipTurn,
            LocalPlayer.isAttackAvaliable,
            LocalPlayer._IsSelectAble,
            LocalPlayer.IsSelected,
            LocalPlayer.isNEXT_STEP
        }));
        Debug.Log(this.debug("C28F21 OpponentPlayer details", new
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
