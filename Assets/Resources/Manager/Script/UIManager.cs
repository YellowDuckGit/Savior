using Assets.GameComponent.UI.CreateDeck.UI.Script;
using DG.Tweening;
using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;


public enum SceneType
{
    SignIn, SignUp, Recovery, Home, Loading, Play, ChooseDeckPVF, StorePacks, StoreDecks, StoreCards
        , CollectionDecks, CollectionCards, CreateDeck, ChooseDeck, WaitingMatch, Matching, OpenPack
}
public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("Login")]
    [SerializeField] private TMP_InputField loginUsername;
    [SerializeField] private TMP_InputField loginPassword;
    [SerializeField] private TextMeshProUGUI loginMessage;
    [SerializeField] Button ACT_Login;

    [Space(5)]
    [Header("Register")]
    [SerializeField] private TMP_InputField regUsername;
    [SerializeField] private TMP_InputField regPassword;
    [SerializeField] private TMP_InputField regRePassword;
    [SerializeField] private TMP_InputField regEmail;
    [SerializeField] private TextMeshProUGUI regMessage;
    [SerializeField] Button ACT_Register;


    [Space(5)]
    [Header("Recover")]
    [SerializeField] private TMP_InputField recoverEmail;
    [SerializeField] private TextMeshProUGUI recoverMessage;
    [SerializeField] Button ACT_RecoverPassword;

    [Space(5)]
    [Header("Friend")]
    [SerializeField] private TMP_InputField friendUserName;
    [SerializeField] Button ACT_ShowFriend;
    [SerializeField] Button ACT_AddFriend;
    [SerializeField] Button ACT_AcceptRequest;
    [SerializeField] Button ACT_DeclineRequest;
    [SerializeField] Button ACT_LeftRoom;
    [SerializeField] Button ACT_Confirm;

    [SerializeField] TextMeshProUGUI addFriendMessage;
    [SerializeField] GameObject friendContainer;
    [SerializeField] GameObject requestPanelContainer;

    /// <summary>
    /// Each Variable bellow is present to one scene in game.
    /// Data type is List<GameObject> support to store list GameObject in that scene.
    /// </summary>
    [Header("Scene")]
    [Space(5)]
    [SerializeField] private List<GameObject> SignInScene;
    [SerializeField] private List<GameObject> SignUpScene;
    [SerializeField] private List<GameObject> RecoveryScene;
    [SerializeField] private List<GameObject> HomeScene;
    [SerializeField] private List<GameObject> LoadingScene;

    [SerializeField] private List<GameObject> PlayScene;
    [SerializeField] private List<GameObject> PlayPVPScene;
    [SerializeField] private List<GameObject> PlayPVEScene;
    [SerializeField] private List<GameObject> Collection_DecksScene;
    [SerializeField] private List<GameObject> Collection_CardsScene;

    [SerializeField] private List<GameObject> StorePacksScene;
    [SerializeField] private List<GameObject> StoreDecksScene;
    [SerializeField] private List<GameObject> StoreCardsScene;

    [SerializeField] private List<GameObject> CreateDeckScene;
    [SerializeField] private List<GameObject> ChooseDeckScene;
    [SerializeField] private List<GameObject> ChooseDeckScenePVF;


    [SerializeField] private List<GameObject> WatingMatchScene;
    [SerializeField] private List<GameObject> MatchingScene;

    [SerializeField] private List<GameObject> OpenPackScene;


    [Space(10)]

    /// <summary>
    /// Parent GameObject is GameObject which store list children GameObject
    /// Variable bellow use for case you need load list gameobject into parent GameObject
    /// EX: Load list Card into CollectionCard
    /// </summary>
    [Header("Parent GameObject")]
    [Space(5)]
    [SerializeField] GameObject CollectionDeck;
    [SerializeField] GameObject CollectionCard;
    [SerializeField] GameObject CreateDeck_CollectionCard;
    [SerializeField] GameObject CreateDeck_CardInDeck;

    [SerializeField] GameObject StorePacks;
    [SerializeField] GameObject StoreDecks;
    [SerializeField] GameObject StoreCards;

    [SerializeField] GameObject collectionFriend;

    [SerializeField] GameObject collectionCardInPackNormal;
    [SerializeField] GameObject collectionCardInPackElite;
    [SerializeField] GameObject collectionCardInPackEpic;
    [SerializeField] GameObject collectionCardInPackLegendary;

    [SerializeField] GameObject collectionCardInDeckNormal;
    [SerializeField] GameObject collectionCardInDeckElite;
    [SerializeField] GameObject collectionCardInDeckEpic;
    [SerializeField] GameObject collectionCardInDeckLegendary;

    //Play scene
    [SerializeField] GameObject collectionDeck_PlayScene;
    [SerializeField] GameObject collectionDeckPVF_PlayScene;
    [SerializeField] GameObject selectFrame;
    [SerializeField] GameObject selectFramePVF;

    [Space(10)]
    [Header("Panel")]
    [Space(5)]
    [SerializeField] GameObject LoadingAPIPanel;
    [SerializeField] GameObject PopupPackDetailed;
    [SerializeField] GameObject PopupDeckDetailed;



    [SerializeField] CountdownTimer WaitingAcceptMatch;
    [SerializeField] CounterTime counterTimeWating;
    [SerializeField] GameObject PanelErrorMessage;

    [SerializeField] public GameObject PanelCardDetails;

    [SerializeField] public GameObject PanelSkipTutorial;



    /// <summary>
    /// Variables bellow will be updated one or more times in game
    /// </summary>
    [Header("Loading Data")]
    [Space(5)]
    //Create Card Scene
    [SerializeField] List<TextMeshProUGUI> numberCardInDeck;
    // [SerializeField] List<TMP_InputField> deckNameCraeteDeck;
    [SerializeField] List<TMP_InputField> deckName;
    [SerializeField] TextMeshProUGUI packName;
    [SerializeField] TextMeshProUGUI cardPrice;
    [SerializeField] List<TextMeshProUGUI> gameMode;
    [SerializeField] List<TextMeshProUGUI> elo;
    [SerializeField] List<TextMeshProUGUI> virtualMoney;
    [SerializeField] List<TextMeshProUGUI> username;
    [SerializeField] List<TextMeshProUGUI> usernameOpponent;
    [SerializeField] List<TMP_InputField> SearchText;


    [Space(10)]
    [Header("Switch Scnene")]
    [Space(5)]
    //switch Scene Button
    [SerializeField] List<Button> switchSceneSignIn;
    [SerializeField] List<Button> switchSceneSignUp;
    [SerializeField] List<Button> switchSceneRecovery;
    [SerializeField] List<Button> switchSceneHome;
    [SerializeField] List<Button> switchScenePlay;

    [SerializeField] List<Button> switchSceneStorePacks;
    [SerializeField] List<Button> switchSceneStoreDecks;
    [SerializeField] List<Button> switchSceneStoreCards;

    [SerializeField] List<Button> switchSceneCollectionDecks;
    [SerializeField] List<Button> switchSceneCollectionCards;
    [SerializeField] List<Button> switchSceneCreateDeck;
    [SerializeField] List<Button> switchSceneChooseDeck;
    [SerializeField] List<Button> switchSceneChooseDeckPVF;


    [SerializeField] List<Button> switchSceneWaitingMatch;
    [SerializeField] List<Button> switchSceneMatching;

    [SerializeField] List<Button> switchSceneOpenPack;

    [SerializeField] List<Button> switchSceneBack;


    //Action Button
    [Header("Button-Account")]


    [SerializeField] Button ACT_SaveDeck;
    [SerializeField] Button ACT_DeleteDeck;

    [SerializeField] Button ACT_Store_BuyPack;
    [SerializeField] Button ACT_Store_CancelPack;

    [SerializeField] Button ACT_Store_BuyDeck;
    [SerializeField] Button ACT_Store_CancelDeck;

    [SerializeField] Button ACT_Store_BuyCard;

    [SerializeField] Button ACT_NormalMode;
    [SerializeField] Button ACT_RankedMode;
    [SerializeField] Button ACT_PlayWithFriendMode;
    [SerializeField] Button ACT_TutorialMode;

    [SerializeField] Button ACT_FindMatch;
    [SerializeField] Button ACT_AcceptMatch;
    [SerializeField] Button ACT_DeclineMatch;
    [SerializeField] Button ACT_LeaveRoom;
    [SerializeField] Button ACT_StopFind;

    [SerializeField] Button ACT_SkipTutorial;
    [SerializeField] Button ACT_NotSkipTutorial;

    [Space(10)]
    [Header("Scene State")]
    [Space(5)]

    /// <summary>
    /// Store state which scene is active
    /// </summary>

    public bool isSignIn;
    public bool isSignUp;
    public bool isRecovery;
    public bool isHome;
    public bool isLoading;
    public bool isPlay;

    public bool isCollection_Decks;
    public bool isCollection_Cards;

    public bool isStorePacks;
    public bool isStoreDecks;
    public bool isStoreCards;

    public bool isCreateDeck;
    public bool isChooseDeck;
    public bool isChooseDeckPVF;

    public bool isWatingMatch;
    public bool isMatchingMatch;
    public bool isOpenPack;

    public bool isLoadCoin;



    //[Space(10)]
    //[Header("Notification UI")]
    //[Space(5)]
    //[SerializeField] Outline selectCardOutLine;

    #region Feedback
    [Space(10)]
    [Header("Feedback")]
    [Space(5)]
    [SerializeField]
    private MMFeedbacks FaderRound;
    [SerializeField]
    private MMFeedbacks Fader;
    [SerializeField]
    private MMFeedbacks FaderDirectional;

    [SerializeField]
    public MMFeedbacks FeedBackOpenPack;


    [SerializeField]
    public MMFeedbacks FeedBackFlipCard;

    #endregion

    [Space(10)]
    [Header("VFX")]
    [Space(5)]
    [SerializeField]
    private VisualEffect VFXTimer;

    /// <summary>
    /// Store Last Scene Name and Present Scene Name
    /// </summary>
    SceneType lastScence;
    SceneType presentScene;
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
        SearchText.ForEach(a => a.onValueChanged.AddListener((String) => GameData.instance.SearchByText(a.text)));

        //isSignIn = isSignUp = is isHome = isLoading = isPlay = isCollection = isCollection_Decks = isCollection_Cards = isStore = isCreateDeck = false;

        #region Register Event Load Data
        this.RegisterListener(EventID.OnChangeDeckName, (param) => LoadDeckName());
        this.RegisterListener(EventID.OnChangeNumberCardInDeck, (param) => LoadNumberCardInDeck((int)param));
        #endregion

        #region Add Button Event
        switchSceneHome.ForEach(a => a.onClick.AddListener(() => TurnOnHomeScene()));
        switchSceneSignUp.ForEach(a => a.onClick.AddListener(() => TurnOnSignUpScene()));
        switchSceneRecovery.ForEach(a => a.onClick.AddListener(() => TurnOnRecoveryScene()));
        switchSceneSignIn.ForEach(a => a.onClick.AddListener(() => TurnOnSignInScene()));
        switchScenePlay.ForEach(a => a.onClick.AddListener(() => TurnOnPlayScene()));
        switchSceneStorePacks.ForEach(a => a.onClick.AddListener(() => TurnOnStorePacksScene()));
        switchSceneStoreDecks.ForEach(a => a.onClick.AddListener(() => TurnOnStoreDecksScene()));
        switchSceneStoreCards.ForEach(a => a.onClick.AddListener(() => TurnOnStoreCardsScene()));
        switchSceneCollectionDecks.ForEach(a => a.onClick.AddListener(() => TurnOnCollectionDeckScene()));
        switchSceneCollectionCards.ForEach(a => a.onClick.AddListener(() => TurnOnCollectionCardScene()));
        switchSceneChooseDeck.ForEach(a => a.onClick.AddListener(() => TurnOnChooseDeckScene()));
        switchSceneCreateDeck.ForEach(a => a.onClick.AddListener(() => TurnOnCreateDeckScene()));
        switchSceneWaitingMatch.ForEach(a => a.onClick.AddListener(() => TurnOnWatingMatchScene()));
        switchSceneOpenPack.ForEach(a => a.onClick.AddListener(() => TurnOnOpenPackScene()));

        switchSceneBack.ForEach(a => a.onClick.AddListener(() =>
        {
            TurnOnBackScene();
            SoundManager.instance.PlayClick_Back();
        }));


        ACT_Login.onClick.AddListener(() => PlayfabManager.instance.Login(loginUsername.text, loginPassword.text));
        ACT_Register.onClick.AddListener(() => PlayfabManager.instance.Register(regEmail.text, regUsername.text, regPassword.text, regRePassword.text));
        ACT_RecoverPassword.onClick.AddListener(() => PlayfabManager.instance.RecoverUser(recoverEmail.text));

        ACT_Store_BuyPack.onClick.AddListener(() =>
        {
            print("click to buy pack");
            StartCoroutine(PlayfabManager.instance.BuyPack(GameData.instance.itemPurchaseRequests));
            PopupPackDetailed.SetActive(false);

            SoundManager.instance.PlayClick_Payment();
        });
        ACT_Store_CancelPack.onClick.AddListener(() => PopupPackDetailed.SetActive(false));

        ACT_Store_BuyDeck.onClick.AddListener(() =>
        {
            print("click to buy deck");
            StartCoroutine(PlayfabManager.instance.BuyItems("Card", "DS1", GameData.instance.itemPurchaseRequests, "MC"));
            PopupDeckDetailed.SetActive(false);

            SoundManager.instance.PlayClick_Payment();
        });
        ACT_Store_CancelDeck.onClick.AddListener(() => PopupDeckDetailed.SetActive(false));

        ACT_Store_BuyCard.onClick.AddListener(() =>
        {
            print("click to buy card");
            StartCoroutine(PlayfabManager.instance.BuyItems("Card", "CS1", GameData.instance.itemPurchaseRequests, "MC"));
            PanelCardDetails.SetActive(false);

            SoundManager.instance.PlayClick_Payment();
        });

        ACT_NormalMode.onClick.AddListener(() => { FindMatchSystem.instance.gameMode = global::GameMode.Normal; GameMode = "Normal"; });
        ACT_RankedMode.onClick.AddListener(() => { FindMatchSystem.instance.gameMode = global::GameMode.Rank; GameMode = "Rank"; });
        //ACT_PlayWithFriendMode.onClick.AddListener(() => { FindMatchSystem.instance.gameMode = global::GameMode.PlayWithFriend; GameMode = "PlayWithFriend"; });
        //ACT_TutorialMode.onClick.AddListener(() => { FindMatchSystem.instance.gameMode = global::GameMode.Tutorial; GameMode = "Tutorial"; });

        //ACT_ShowFriend.onClick.AddListener(() => StartCoroutine(GameData.instance.LoadFriendItem(CollectionFriend)));
        //ACT_ShowFriend.onClick.AddListener(() => { FriendContainer.SetActive(!FriendContainer.activeSelf); });
        ACT_AddFriend.onClick.AddListener(() =>
        {
            PlayfabManager.instance.AddFriend(PlayfabManager.FriendIdType.Username, friendUserName.text);
        });
        //ACT_DeleteDeck.onClick.AddListener(() => { PlayfabManager.instance.RemoveFriend("vanphu02"); });]

        // TUTORIAL
        ACT_SkipTutorial.onClick.AddListener(() =>
        {
            TutorialManager.instance.isSkip = false;
            TutorialManager.instance.PlayTutorialChain();
        });

        ACT_NotSkipTutorial.onClick.AddListener(() =>
        {
            TutorialManager.instance.isSkip = true;
            TutorialManager.instance.Skip();
        });

        #endregion

        if (PlayfabManager.instance.isAuthented == false)
        {
            TurnOn(SceneType.SignIn, true); //Default
        }
        else
        {
            StartCoroutine(GameData.instance.LoadingGameProcess());
        }
    }


    //test
    public void LoadHomeScene()
    {
        print("load home scene");
        TurnOffSceneAlreadyShow();
        TurnOn(SceneType.Home, true);
    }

    // Update is called once per frame
    void Update()
    {

    }

    #region TurnOn, TurnOff Scene
    private void TurnOn(SceneType type, bool turn)
    {
        if (turn)
        {

            Fader.PlayFeedbacks();
            //FaderDirectional.PlayFeedbacks();
        }
        print("Type: " + type + " Turn: " + turn);
        switch (type)
        {
            case SceneType.SignIn:
                if (isSignIn ^ turn)
                {
                    if (turn)
                    {
                        TurnOffSceneAlreadyShow();

                        if (UIManager.instance.loginMessage.transform.parent.gameObject.activeSelf)
                        {
                            UIManager.instance.loginMessage.transform.parent.gameObject.SetActive(false);
                        }

                        //SOUND MANAGER
                        SoundManager.instance.PlayBackground_Login();

                    }
                    isSignIn = turn;

                    foreach (GameObject obj in SignInScene)
                    {
                        obj.SetActive(turn);
                    }
                }

                if (isSignIn)
                {
                    lastScence = presentScene;
                    presentScene = SceneType.SignIn;
                }
                break;

            case SceneType.SignUp:
                if (isSignUp ^ turn)
                {
                    if (turn)
                    {
                        TurnOffSceneAlreadyShow();

                        if (UIManager.instance.RegisterMessage.transform.parent.gameObject.activeSelf)
                            UIManager.instance.RegisterMessage.transform.parent.gameObject.SetActive(false);
                    }
                    isSignUp = turn;

                    foreach (GameObject obj in SignUpScene)
                    {
                        obj.SetActive(turn);
                    }
                }

                if (isSignUp)
                {
                    lastScence = presentScene;
                    presentScene = SceneType.SignUp;
                }
                break;

            case SceneType.Recovery:
                if (isRecovery ^ turn)
                {
                    if (turn)
                    {

                        TurnOffSceneAlreadyShow();

                        if (UIManager.instance.recoverMessage.transform.parent.gameObject.activeSelf)
                            UIManager.instance.recoverMessage.transform.parent.gameObject.SetActive(false);

                    }
                    isRecovery = turn;

                    foreach (GameObject obj in RecoveryScene)
                    {
                        obj.SetActive(turn);
                    }
                }

                if (isRecovery)
                {
                    lastScence = presentScene;
                    presentScene = SceneType.Recovery;
                }
                break;

            case SceneType.Home:
                if (isHome ^ turn)
                {
                    if (turn)
                    {
                        if (lastScence == SceneType.SignUp || lastScence == SceneType.SignIn)
                            FaderRound.PlayFeedbacks();

                        TurnOffSceneAlreadyShow();
                        // LOAD MONEY VIRTUAL

                        // TUTORIAL
                        TutorialManager.instance.PlayTutorialChain();

                        //SOUND
                        SoundManager.instance.PLayBackground_Home();
                    }
                    isHome = turn;

                    foreach (GameObject obj in HomeScene)
                    {
                        obj.SetActive(turn);
                    }
                }

                if (isHome)
                {
                    lastScence = presentScene;
                    presentScene = SceneType.Home;
                }
                break;
            case SceneType.Loading:
                if (isLoading ^ turn)
                {
                    if (turn) TurnOffSceneAlreadyShow();
                    print("loading :" + turn);

                    isLoading = turn;
                    foreach (GameObject obj in LoadingScene)
                    {
                        obj.SetActive(turn);
                    }
                }

                if (isLoading)
                {
                    lastScence = presentScene;
                    presentScene = SceneType.Loading;
                }
                break;
            case SceneType.Play:
                if (isPlay ^ turn)
                {
                    if (turn)
                    {
                        //StartCoroutine(GameData.instance.LoadDeckItems(CollectionDeck_PlayScene));
                        //GameData.instance.UnLoadCardInDeckPack();
                        TurnOffSceneAlreadyShow();

                        // TUTORIAL
                        TutorialManager.instance.PlayTutorialChain();
                    }
                    isPlay = turn;
                    foreach (GameObject obj in PlayScene)
                    {
                        obj.SetActive(turn);
                    }
                    HeaderTitle.title = "Select Game Mode";
                }

                if (isPlay)
                {
                    lastScence = presentScene;
                    presentScene = SceneType.Play;
                }
                break;

            case SceneType.CollectionDecks:
                if (isCollection_Decks ^ turn)
                {
                    if (turn)
                    {
                        StartCoroutine(GameData.instance.LoadDeckItems(CollectionDeck));
                        GameData.instance.UnLoadCardInDeckPack();
                        TurnOffSceneAlreadyShow();
                    }
                    isCollection_Decks = turn;
                    foreach (GameObject obj in Collection_DecksScene)
                    {
                        obj.SetActive(turn);
                    }
                    HeaderTitle.title = "Collection Deck";
                }

                if (isCollection_Decks)
                {
                    lastScence = presentScene;
                    presentScene = SceneType.CollectionDecks;
                }
                break;
            case SceneType.CollectionCards:
                if (isCollection_Cards ^ turn)
                {
                    if (turn)
                    {
                        StartCoroutine(GameData.instance.LoadCardCollection(CollectionCard));
                        TurnOffSceneAlreadyShow();
                    }
                    isCollection_Cards = turn;
                    foreach (GameObject obj in Collection_CardsScene)
                    {
                        obj.SetActive(turn);
                    }
                    HeaderTitle.title = "Collection Card";
                }

                if (isCollection_Cards)
                {
                    lastScence = presentScene;
                    presentScene = SceneType.CollectionCards;
                }
                break;

            case SceneType.StorePacks:

                if (isStorePacks ^ turn)
                {
                    if (turn)
                    {
                        StartCoroutine(GameData.instance.LoadPack(StorePacks));
                        TurnOffSceneAlreadyShow();

                        //TUTORIAL
                        Debug.Log("START TUTORIAL IN STORE");
                        TutorialManager.instance.PlayTutorialChain();
                    }
                    isStorePacks = turn;
                    foreach (GameObject obj in StorePacksScene)
                    {
                        obj.SetActive(turn);
                    }
                    HeaderTitle.title = "Pack Store";

                }

                if (isStorePacks)
                {
                    lastScence = presentScene;
                    presentScene = SceneType.StorePacks;
                }
                break;
            case SceneType.StoreDecks:

                if (isStoreDecks ^ turn)
                {
                    if (turn)
                    {
                        StartCoroutine(GameData.instance.LoadDeckInStore(StoreDecks));
                        TurnOffSceneAlreadyShow();

                        // TUTORIAL
                        TutorialManager.instance.PlayTutorialChain();
                    }
                    isStoreDecks = turn;
                    foreach (GameObject obj in StoreDecksScene)
                    {
                        obj.SetActive(turn);
                    }
                    HeaderTitle.title = "Deck Store";
                }

                if (isStoreDecks)
                {
                    lastScence = presentScene;
                    presentScene = SceneType.StoreDecks;
                }
                break;
            case SceneType.StoreCards:

                if (isStoreCards ^ turn)
                {
                    if (turn)
                    {
                        StartCoroutine(GameData.instance.LoadCardCollection(StoreCards));
                        TurnOffSceneAlreadyShow();
                    }
                    isStoreCards = turn;
                    foreach (GameObject obj in StoreCardsScene)
                    {
                        obj.SetActive(turn);
                    }
                    HeaderTitle.title = "Card Store";
                }

                if (isStoreCards)
                {
                    lastScence = presentScene;
                    presentScene = SceneType.StoreCards;
                }
                break;
            case SceneType.CreateDeck:

                Debug.Log("CreateDeck: " + turn);
                if (isCreateDeck ^ turn)
                {
                    if (turn)
                    {
                        //StartCoroutine(GameData.instance.LoadCardCollection(CreateDeck_CollectionCard));
                        StartCoroutine(GameData.instance.LoadCardInDeckPack(CreateDeck_CardInDeck));
                        StartCoroutine(GameData.instance.LoadCardInInventoryUser(CreateDeck_CollectionCard));
                        LoadDeckName();
                        LoadNumberCardInDeck(GameData.instance.getNumberCardInDeck());

                        TurnOffSceneAlreadyShow();

                        //TUTORIAL
                        TutorialManager.instance.PlayTutorialChain();
                    }
                    isCreateDeck = turn;
                    foreach (GameObject obj in CreateDeckScene)
                    {
                        obj.SetActive(turn);
                    }
                    HeaderTitle.title = "Create New Deck";
                }

                if (isCreateDeck)
                {
                    lastScence = presentScene;
                    presentScene = SceneType.CreateDeck;
                }

                break;
            case SceneType.ChooseDeck:

                if (isChooseDeck ^ turn)
                {
                    if (turn)
                    {
                        //reset deck name

                        StartCoroutine(GameData.instance.LoadDeckItems(CollectionDeck_PlayScene));
                        TurnOffSceneAlreadyShow();
                    }
                    isChooseDeck = turn;
                    foreach (GameObject obj in ChooseDeckScene)
                    {
                        obj.SetActive(turn);
                    }
                    HeaderTitle.title = "PVP - Choose Deck";
                }

                if (isChooseDeck)
                {
                    lastScence = presentScene;
                    presentScene = SceneType.ChooseDeck;
                }

                break;
            case SceneType.ChooseDeckPVF:

                if (isChooseDeckPVF ^ turn)
                {
                    if (turn)
                    {
                        //reset deck name

                        StartCoroutine(GameData.instance.LoadDeckItems(CollectionDeckPVF_PlayScene));
                        TurnOffSceneAlreadyShow();
                    }
                    isChooseDeckPVF = turn;
                    foreach (GameObject obj in ChooseDeckScenePVF)
                    {
                        obj.SetActive(turn);
                    }
                    HeaderTitle.title = "PVF - Choose Deck";

                }

                if (isChooseDeckPVF)
                {
                    lastScence = presentScene;
                    presentScene = SceneType.ChooseDeckPVF;
                }

                break;

            case SceneType.WaitingMatch:

                if (isWatingMatch ^ turn)
                {
                    if (turn)
                    {
                        TurnOffSceneAlreadyShow();
                        SoundManager.instance.PLayBackground_FindMatch();
                    }
                    isWatingMatch = turn;
                    foreach (GameObject obj in WatingMatchScene)
                    {
                        obj.SetActive(turn);
                    }
                }

                if (isWatingMatch)
                {
                    lastScence = presentScene;
                    presentScene = SceneType.WaitingMatch;
                }
                break;

            case SceneType.Matching:

                if (isMatchingMatch ^ turn)
                {
                    if (turn)
                    {
                        TurnOffSceneAlreadyShow();
                    }
                    isMatchingMatch = turn;
                    foreach (GameObject obj in MatchingScene)
                    {
                        obj.SetActive(turn);
                    }
                }

                if (isMatchingMatch)
                {
                    lastScence = presentScene;
                    presentScene = SceneType.Matching;
                }
                break;

            case SceneType.OpenPack:

                if (isOpenPack ^ turn)
                {
                    if (turn)
                    {
                        TurnOffSceneAlreadyShow();
                    }
                    isOpenPack = turn;
                    foreach (GameObject obj in OpenPackScene)
                    {
                        obj.SetActive(turn);
                    }
                }

                if (isOpenPack)
                {
                    lastScence = presentScene;
                    presentScene = SceneType.OpenPack;
                }
                break;

            default:
                Debug.LogError("Can't find Scene");
                break;


        }
    }

    /// <summary>
    /// Turn Off Scene already show
    /// </summary>
    private void TurnOffSceneAlreadyShow()
    {
        if (isLoading)
        {
            TurnOn(SceneType.Loading, false);
        }

        if (isSignIn)
        {
            loginUsername.text = "";
            loginPassword.text = "";
            TurnOn(SceneType.SignIn, false);
        }

        if (isSignUp)
        {
            regUsername.text = "";
            regPassword.text = "";
            regEmail.text = "";
            regRePassword.text = "";
            TurnOn(SceneType.SignUp, false);
        }

        if (isRecovery)
        {
            regEmail.text = "";
            TurnOn(SceneType.Recovery, false);
        }

        if (isHome)
        {
            TurnOn(SceneType.Home, false);
        }

        if (isPlay)
        {
            TurnOn(SceneType.Play, false);
        }


        if (isStorePacks)
        {
            TurnOn(SceneType.StorePacks, false);
        }

        if (isStoreDecks)
        {
            TurnOn(SceneType.StoreDecks, false);
        }

        if (isStoreCards)
        {
            TurnOn(SceneType.StoreCards, false);
        }

        if (isCreateDeck)
        {
            GameData.instance.selectDeck = null;
            GameData.instance.UnLoadCardInDeckPack();

            TurnOn(SceneType.CreateDeck, false);
        }

        if (isOpenPack)
        {
            TurnOn(SceneType.OpenPack, false);
        }

        if (isCollection_Cards)
        {
            TurnOn(SceneType.CollectionCards, false);
        }

        if (isCollection_Decks)
        {
            TurnOn(SceneType.CollectionDecks, false);
        }

        if (isChooseDeck)
        {
            TurnOn(SceneType.ChooseDeck, false);
        }

        if (isChooseDeckPVF)
        {
            TurnOn(SceneType.ChooseDeckPVF, false);
        }

        if (isWatingMatch)
        {
            TurnOn(SceneType.WaitingMatch, false);
            FindMatchSystem.instance.OnClickDeclineMatch();
        }

        if (isMatchingMatch)
        {
            TurnOn(SceneType.Matching, false);
        }
    }
    #endregion

    #region Switch Scene Function
    public void TurnOnBackScene()
    {
        print("lastScene" + lastScence);
        TurnOn(lastScence, true);
    }

    public void TurnOnHomeScene()
    {
        TurnOn(SceneType.Home, true);
    }

    public void TurnOnSignInScene()
    {
        TurnOn(SceneType.SignIn, true);
    }
    public void TurnOnSignUpScene()
    {
        print("Turn on sign up");
        TurnOn(SceneType.SignUp, true);
    }

    public void TurnOnRecoveryScene()
    {
        TurnOn(SceneType.Recovery, true);
    }

    public void TurnOnLoadingScene()
    {
        TurnOn(SceneType.Loading, true);
    }

    public void TurnOnPlayScene()
    {
        TurnOn(SceneType.Play, true);
    }

    public void TurnOnCollectionDeckScene()
    {
        TurnOn(SceneType.CollectionDecks, true);
    }

    public void TurnOnCollectionCardScene()
    {
        TurnOn(SceneType.CollectionCards, true);

    }
    public void TurnOnCreateDeckScene()
    {
        print("TurnOnCreateDeckScene");
        TurnOn(SceneType.CreateDeck, true);
        ACT_SaveDeck.onClick.RemoveAllListeners();
        ACT_SaveDeck.onClick.AddListener(() => StartCoroutine(CollectionManager.instance.CreateDeck()));
    }
    public void TurnOnUpdateDeckScene()
    {
        print("TurnOnUpdateDeckScene");
        TurnOn(SceneType.CreateDeck, true);
        ACT_SaveDeck.onClick.RemoveAllListeners();
        ACT_SaveDeck.onClick.AddListener(() => StartCoroutine(CollectionManager.instance.UpdateDeck()));

    }

    public void TurnOnStorePacksScene()
    {
        TurnOn(SceneType.StorePacks, true);
    }
    public void TurnOnStoreDecksScene()
    {
        TurnOn(SceneType.StoreDecks, true);
    }

    public void TurnOnStoreCardsScene()
    {
        TurnOn(SceneType.StoreCards, true);
    }

    public void TurnOnChooseDeckScene()
    {
        TurnOn(SceneType.ChooseDeck, true);
    }
    public void TurnOnChooseDeckPVFScene()
    {
        TurnOn(SceneType.ChooseDeckPVF, true);
    }
    public void TurnOnWatingMatchScene()
    {
        TurnOn(SceneType.WaitingMatch, true);
    }

    public void TurnOnOpenPackScene()
    {
        TurnOn(SceneType.OpenPack, true);
    }

    public void TurnOnMatchingScene()
    {
        TurnOn(SceneType.Matching, true);
    }

    #endregion

    #region LoadDataFunction
    public IEnumerator LoadVirtualMoney()
    {
        print("LOAD MONEY");
        yield return StartCoroutine(PlayfabManager.instance.GetVirtualCurrency());
        foreach (var money in virtualMoney)
        {
            VirtualMoney = GameData.instance.Coin.ToString();
        }


        print("END LOAD MONEY");
        yield return null;
    }

    public IEnumerator LoadElo()
    {
        yield return StartCoroutine(PlayfabManager.instance.GetScore());
        Elo = GameData.instance.Elo.ToString();
        yield return null;
    }

    public void LoadNumberCardInDeck(int amount)
    {
        numberCardInDeck[0].text = amount + "/" + CollectionManager.instance.LimitNumberCardInDeck;
    }

    public void LoadDeckName()
    {
        if (GameData.instance.selectDeck != null)
            deckName.ForEach(a => a.text = GameData.instance.selectDeck.Data.deckName);
        else
        {
            deckName.ForEach(a => a.text = "");
        }
    }

    public void LoadSeletedDeck(Transform deck, GameObject oldParent, GameObject newParent)
    {

        DeckItem deckItemChildren = newParent.GetComponentInChildren<DeckItem>();

        if (deckItemChildren == null)
        {
            deck.parent = newParent.transform;
            GameData.instance.selectDeck = deck.gameObject.GetComponent<DeckItem>();
        }
        else
        {
            //get children form select frame

            //click deckItem in select frame
            if (deckItemChildren.transform == deck.transform)
            {
                deck.parent = oldParent.transform;
                GameData.instance.selectDeck = null;
            }
            else
            {
                deckItemChildren.transform.parent = oldParent.transform;
                deck.parent = newParent.transform;
                GameData.instance.selectDeck = deck.gameObject.GetComponent<DeckItem>();
            }
        }
    }

    public void LoadCardDetail(CardItem cardItem)
    {

        CardInInventory cardInInventory = GameData.instance.InitCard2D(cardItem);
        cardInInventory.transform.parent = PanelCardDetails.transform;
        cardInInventory.gameObject.transform.localPosition = Vector3.zero;
        cardInInventory.gameObject.transform.localScale = new Vector3(2f, 2f, 2f);
        PanelCardDetails.gameObject.SetActive(true);

        if (isStoreCards)
        {
            print($"UIManager 1087 {isStoreCards}");
            print(cardItem.amount);

            if (cardItem.amount != 3)
                ACT_Store_BuyCard.gameObject.SetActive(true);
            else
            {
                ACT_Store_BuyCard.gameObject.SetActive(false);
            }
            cardPrice.transform.parent.gameObject.SetActive(true);
            cardPrice.text = cardItem.price.ToString();
            GameData.instance.itemPurchaseRequests.Clear();
            GameData.instance.itemPurchaseRequests.Add(new PlayFab.ClientModels.ItemPurchaseRequest()
            {
                ItemId = cardItem.cardData.Id,
                Quantity = 1
            });
        }
        if (isCollection_Cards)
        {
            ACT_Store_BuyCard.gameObject.SetActive(false);
            cardPrice.transform.parent.gameObject.SetActive(false);
        }
    }

    public void UnLoadCardDetail()
    {
        print("Unload: " + PanelCardDetails.transform.childCount);
        if (PanelCardDetails.transform.childCount > 0)
        {
            Transform children = PanelCardDetails.transform.GetComponentInChildren<CardInInventory>().transform;
            Destroy(children.gameObject);
            PanelCardDetails.gameObject.SetActive(false);
            // TUTORIAL
            print("UN LOAD TUTORIAL");
            TutorialManager.instance.PlayTutorialChain();
        }

    }
    #endregion

    #region Get Set


    public string DeckName
    {
        get { return deckName[0].text; }
        set
        {
            this.deckName.ForEach(a => a.text = value);
        }
    }

    string Elo
    {
        get { return elo[0].text; }
        set { this.elo.ForEach(a => a.text = "Elo: " + value); }
    }

    public string NumberCardInDeck
    {
        get { return numberCardInDeck[0].text; }
        private set { this.numberCardInDeck.ForEach(a => a.text = value); }
    }


    public string VirtualMoney
    {
        get { return virtualMoney[0].text; }
        private set
        {
            Coroutine a = null;
            if (isLoadCoin)
            {
                StopCoroutine(a);
                isLoadCoin = false;
            }

            isLoadCoin = true;
            a = StartCoroutine(IntegerLerpCoroutine(Int32.Parse(virtualMoney[0].text), Int32.Parse(value), 2f));
        }
    }

    public string UserName
    {
        get { return username[0].text; }
        set { username.ForEach(a => a.text = value); }
    }


    public string GameMode
    {
        get { return this.gameMode[0].text; }
        private set { this.gameMode.ForEach(a => a.text = value); }
    }


    //ACT_NormalMode.onClick.AddListener(() => OnClickNormalMode());
    //        ACT_RankedMode.onClick.AddListener(() => OnClickRankedMode());
    //        ACT_FindMatch.onClick.AddListener(() => OnClickFindMatch());
    //        ACT_AcceptMatch.onClick.AddListener(() => OnClickAcceptMatch());
    //        ACT_DeclinetMatch.onClick.AddListener(() => OnClickDeclineMatch());
    //        ACT_StopFind.onClick.AddListener(() => OnClickDeclineMatch());
    public Button Button_NormalMode
    {
        get { return ACT_NormalMode; }
    }

    public Button Button_RankedMode
    {
        get { return ACT_RankedMode; }
    }

    public Button Button_FindMatch
    {
        get { return ACT_FindMatch; }
    }

    public Button Button_AcceptMatch
    {
        get { return ACT_AcceptMatch; }
    }

    public Button Button_DelineMatch
    {
        get { return ACT_DeclineMatch; }
    }

    public Button Button_StopFind
    {
        get { return ACT_StopFind; }
    }

    public CounterTime CounterTimeWating
    {
        get { return counterTimeWating; }
    }
    public CountdownTimer CountdownTimer
    {
        get { return WaitingAcceptMatch; }
    }

    public TextMeshProUGUI AddFriendMessage
    {
        get { return this.addFriendMessage; }
    }

    public GameObject CollectionFriend
    {
        get { return this.collectionFriend; }

    }

    public TMP_InputField LoginUsername
    {
        get { return this.loginUsername; }
    }

    public GameObject RequestPanelContainer
    {
        get { return this.requestPanelContainer; }
    }

    public GameObject CollectionDeck_PlayScene
    {
        get { return this.collectionDeck_PlayScene; }
    }

    public GameObject CollectionDeckPVF_PlayScene
    {
        get { return this.collectionDeckPVF_PlayScene; }
    }
    public GameObject SelectFrame
    {
        get { return this.selectFrame; }
    }

    public GameObject SelectFramePVF
    {
        get { return this.selectFramePVF; }
    }

    public GameObject CollectionCardInPackNormal
    {
        get { return this.collectionCardInPackNormal; }
    }

    public GameObject CollectionCardInPackElite
    {
        get { return this.collectionCardInPackElite; }
    }

    public GameObject CollectionCardInPackEpic
    {
        get { return this.collectionCardInPackEpic; }
    }
    public GameObject CollectionCardInPackLegendary
    {
        get { return this.collectionCardInPackLegendary; }
    }

    public GameObject CollectionCardInDeckNormal
    {
        get { return this.collectionCardInDeckNormal; }
    }

    public GameObject CollectionCardInDeckElite
    {
        get { return this.collectionCardInDeckElite; }
    }

    public GameObject CollectionCardInDeckEpic
    {
        get { return this.collectionCardInDeckEpic; }
    }
    public GameObject CollectionCardInDeckLegendary
    {
        get { return this.collectionCardInDeckLegendary; }
    }
    //[SerializeField] GameObject collectionDeck_PlayScene;
    //[SerializeField] GameObject collectionDeckPVF_PlayScene;
    //[SerializeField] GameObject selectFrame;
    //[SerializeField] GameObject selectFramePVF;
    #endregion

    #region Notification UI Function
    //public void UI_FindMatch(bool enable)
    //{
    //    if (enable)
    //    {
    //        ACT_FindMatch.gameObject.SetActive(true);
    //        ACT_FindMatch.interactable = true;
    //    }
    //    else
    //    {
    //        ACT_FindMatch.gameObject.SetActive(false);
    //        ACT_FindMatch.interactable = false;
    //    }
    //}

    //public void UI_WaitingOppenent(bool enable)
    //{
    //    if (enable)
    //    {
    //        ACT_StopFind.gameObject.SetActive(true);
    //        ACT_StopFind.interactable = true;

    //    }
    //    else
    //    {
    //        ACT_StopFind.gameObject.SetActive(false);
    //        ACT_StopFind.interactable = false;

    //    }
    //}

    //public void UI_ConfirmMatchmaking(bool enable)
    //{
    //    if (enable)
    //    {
    //        ACT_AcceptMatch.gameObject.SetActive(true);
    //        ACT_DeclineMatch.gameObject.SetActive(true);
    //    }
    //    else
    //    {
    //        ACT_AcceptMatch.gameObject.SetActive(false);
    //        ACT_DeclineMatch.gameObject.SetActive(false);
    //    }
    //}

    //public void UI_StartMatch(bool enable)
    //{
    //    ///////////
    //}
    #endregion

    #region Information Update

    public TextMeshProUGUI LoginMessage
    {
        get { return loginMessage; }
        private set
        {
            loginMessage = value;
            EnableLoadingAPI(false);

        }
    }

    public TextMeshProUGUI RegisterMessage
    {
        get { return regMessage; }
        private set
        {
            regMessage = value;
            EnableLoadingAPI(false);
        }
    }

    public TextMeshProUGUI RecoverMessage
    {
        get { return recoverMessage; }
        private set
        {
            recoverMessage = value;
            EnableLoadingAPI(false);

        }
    }
    #endregion

    #region MatchUI

    #endregion

    #region Other

    private IEnumerator IntegerLerpCoroutine(int fromValue, int toValue, float duration)
    {
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            int result = Mathf.RoundToInt(Mathf.Lerp(fromValue, toValue, t));
            virtualMoney.ForEach(a => a.text = result.ToString());
            //if (toValue != 0)
            //    liquid.CompensateShapeAmount = (float)result / (float)MatchManager.instance.maxMana;
            SoundManager.instance.PlayCoinSound();
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        isLoadCoin = false;
    }


    public void EnablePanelErrorMessage(bool enable, string mess = null)
    {
        TextMeshProUGUI text = PanelErrorMessage.GetComponentInChildren<TextMeshProUGUI>();
        if (enable)
        {
            text.text = mess;
            PanelErrorMessage.gameObject.SetActive(true);
            PanelErrorMessage.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            text.text = mess;
            PanelErrorMessage.gameObject.SetActive(false);
            PanelErrorMessage.transform.parent.gameObject.SetActive(false);
        }

    }
    public void EnableLoadingAPI(bool enable)
    {
        LoadingAPIPanel.SetActive(enable);
    }

    public void ShowPopupCardInStore(CardInInventory item)
    {
        GameData.instance.itemPurchaseRequests.Clear();
        GameData.instance.itemPurchaseRequests.Add(new PlayFab.ClientModels.ItemPurchaseRequest()
        {
            ItemId = item.CardItem.cardData.Id,
            Quantity = 1
        });

        //popup.description.text = item.CardItem.cardData.Name + "\n" + item.CardItem.cardData.Description;
    }

    public IEnumerator ShowPopupDeckDetailed(DeckItem item)
    {
        GameData.instance.itemPurchaseRequests.Clear();
        GameData.instance.itemPurchaseRequests.Add(new PlayFab.ClientModels.ItemPurchaseRequest()
        {
            ItemId = item.Id,
            Quantity = 1
        });
        PopupDeckDetailed.SetActive(true);
        print($"UIManager 1408 {item.Data.deckName}");
        var deckData = GameData.instance.listDeckDataInStore.Find(i => i.id == item.Id);
        Dictionary<string, int> dic = new Dictionary<string, int>();
        foreach (var key in deckData.deckItemsId)
        {
            if (dic.ContainsKey(key))
            {
                dic[key]++;
            }
            else
            {
                dic.Add(key, 1);
            }
        }

        yield return StartCoroutine(GameData.instance.LoadCardInDeckStoreItem(dic));
        UIManager.instance.DeckName = item.Data.deckName;

    }

    public IEnumerator ShowPopupPackDetailed(PackItem item)
    {
        GameData.instance.itemPurchaseRequests.Clear();
        GameData.instance.itemPurchaseRequests.Add(new PlayFab.ClientModels.ItemPurchaseRequest()
        {
            ItemId = item.ID,
            Quantity = 1
        });
        var popup = PopupPackDetailed.GetComponent<popupDetail>();
        PopupPackDetailed.SetActive(true);

        print("show popup");
        print("GameData.instance.listPackData: " + GameData.instance.listPackData.Count);
        var packData = GameData.instance.listPackData.Find(i => i.id == item.ID);
        Dictionary<string, int> dic = new Dictionary<string, int>();
        foreach (var key in packData.dropTableId)
        {
            print(key);
        }

        foreach (var key in packData.dropTableId)
        {
            print(key);
            if (dic.ContainsKey(key))
            {
                dic[key]++;
            }
            else
            {
                dic.Add(key, 1);
            }
        }

        DropTableInfor matchingitems = GameData.instance.dropTableInforList.FirstOrDefault(x => dic.ContainsKey(x.id)); // get the items from the list that have the same id as the keys in dic
        if (matchingitems != null) print("Not null");
        print($"1464 {item.Data.packName}");
        packName.text = item.Data.packName;
        yield return StartCoroutine(GameData.instance.LoadCardInPackItem(matchingitems));

    }

    public void WatingAcceptMatch(bool enable)
    {
        if (ACT_AcceptMatch.gameObject.activeSelf != enable)
            ACT_AcceptMatch.gameObject.SetActive(enable);

        if (enable)
        {
            if (!WaitingAcceptMatch.isTimerRunning)
            {
                //SFX: Notify Found Match
                SoundManager.instance.PlayFoundMatch();
                WaitingAcceptMatch.StartTimer();
                VFXTimer.SendEvent("OnPlay");
            }
        }
        else
        {
            {
                WaitingAcceptMatch.PauseTimer();
                VFXTimer.SendEvent("OnStop");
            }
        }

    }


    #endregion
}
