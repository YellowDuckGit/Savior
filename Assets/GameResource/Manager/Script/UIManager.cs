using Assets.GameComponent.UI.CreateDeck.UI.Script;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Drawing2D;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using UnityEngine.VFX;

public enum SceneType
{
    SignIn, SignUp, Recovery, Home, Loading, Play, ChooseDeckPVF, StorePacks, StoreDecks, StoreCards
        , CollectionDecks, CollectionCards, CreateDeck, ChooseDeck, WaitingMatch, Matching
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

    //Play scene
    [SerializeField] GameObject collectionDeck_PlayScene;
    [SerializeField] GameObject collectionDeckPVF_PlayScene;
    [SerializeField] GameObject selectFrame;
    [SerializeField] GameObject selectFramePVF;


    [Space(10)]


    [Header("Panel")]
    [Space(5)]
    [SerializeField] GameObject SettingPanel;
    [SerializeField] GameObject LoadingAPIPanel;
    [SerializeField] GameObject PopupPackDetailed;
    [SerializeField] GameObject PopupDeckDetailed;
    [SerializeField] GameObject PopupCardDetailed;

    [SerializeField] CountdownTimer WaitingAcceptMatch;
    [SerializeField] CounterTime counterTimeWating;
    [SerializeField] GameObject PanelErrorMessage;




    /// <summary>
    /// Variables bellow will be updated one or more times in game
    /// </summary>
    [Header("Loading Data")]
    [Space(5)]
    //Create Card Scene
    [SerializeField] List<TextMeshProUGUI> numberCardInDeck;
    // [SerializeField] List<TMP_InputField> deckNameCraeteDeck;
    [SerializeField] List<TMP_InputField> deckName;
    [SerializeField] List<TextMeshProUGUI> gameMode;
    [SerializeField] List<TextMeshProUGUI> elo;
    [SerializeField] List<TextMeshProUGUI> virtualMoney;
    [SerializeField] List<TextMeshProUGUI> username;
    [SerializeField] List<TextMeshProUGUI> usernameOpponent;


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
    [SerializeField] Button ACT_Store_CancelCard;

    [SerializeField] Button ACT_NormalMode;
    [SerializeField] Button ACT_RankedMode;
    [SerializeField] Button ACT_PlayWithFriendMode;
    [SerializeField] Button ACT_TutorialMode;

    [SerializeField] Button ACT_FindMatch;
    [SerializeField] Button ACT_AcceptMatch;
    [SerializeField] Button ACT_DeclineMatch;
    [SerializeField] Button ACT_LeaveRoom;
    [SerializeField] Button ACT_StopFind;



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

        switchSceneBack.ForEach(a => a.onClick.AddListener(() => TurnOnBackScene()));


        ACT_Login.onClick.AddListener(() => PlayfabManager.instance.Login(loginUsername.text, loginPassword.text));
        ACT_Register.onClick.AddListener(() => PlayfabManager.instance.Register(regEmail.text, regUsername.text, regPassword.text, regRePassword.text));
        ACT_RecoverPassword.onClick.AddListener(() => PlayfabManager.instance.RecoverUser(recoverEmail.text));

        ACT_Store_BuyPack.onClick.AddListener(() =>
        {
            print("click to buy pack");
            StartCoroutine(PlayfabManager.instance.BuyItems("Card", "BS1", GameData.instance.itemPurchaseRequests, "MC"));
            PopupPackDetailed.SetActive(false);
        });
        ACT_Store_CancelPack.onClick.AddListener(() => PopupPackDetailed.SetActive(false));

        ACT_Store_BuyDeck.onClick.AddListener(() =>
        {
            print("click to buy deck");
            StartCoroutine(PlayfabManager.instance.BuyItems("Card", "DS1", GameData.instance.itemPurchaseRequests, "MC"));
            PopupDeckDetailed.SetActive(false);
        });
        ACT_Store_CancelDeck.onClick.AddListener(() => PopupDeckDetailed.SetActive(false));

        ACT_Store_BuyCard.onClick.AddListener(() =>
        {
            print("click to buy card");
            StartCoroutine(PlayfabManager.instance.BuyItems("Card", "CS1", GameData.instance.itemPurchaseRequests, "MC"));
            PopupCardDetailed.SetActive(false);
        });
        ACT_Store_CancelCard.onClick.AddListener(() => PopupCardDetailed.SetActive(false));

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
        //ACT_DeleteDeck.onClick.AddListener(() => { PlayfabManager.instance.RemoveFriend("vanphu02"); });
        #endregion

        TurnOn(SceneType.SignIn, true); //Default
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
        if (type != SceneType.Home && turn)
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
                        // LOAD MONEY VIRTUAL

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
                        // LOAD MONEY VIRTUAL
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
                        // LOAD MONEY VIRTUAL

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
                        FaderRound.PlayFeedbacks();
                        TurnOffSceneAlreadyShow();
                        // LOAD MONEY VIRTUAL

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
                    }
                    isPlay = turn;
                    foreach (GameObject obj in PlayScene)
                    {
                        obj.SetActive(turn);
                    }
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
                    }
                    isStorePacks = turn;
                    foreach (GameObject obj in StorePacksScene)
                    {
                        obj.SetActive(turn);
                    }
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
                    }
                    isStoreDecks = turn;
                    foreach (GameObject obj in StoreDecksScene)
                    {
                        obj.SetActive(turn);
                    }
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
                        StartCoroutine(GameData.instance.LoadCardCollection(CreateDeck_CollectionCard));
                        StartCoroutine(GameData.instance.LoadCardInDeckPack(CreateDeck_CardInDeck));
                        //StartCoroutine(GameData.instance.LoadCardInInventoryUser(CardInventory));
                        LoadDeckName();
                        LoadNumberCardInDeck(GameData.instance.getNumberCardInDeck());

                        TurnOffSceneAlreadyShow();
                    }
                    isCreateDeck = turn;
                    foreach (GameObject obj in CreateDeckScene)
                    {
                        obj.SetActive(turn);
                    }
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
            TurnOn(SceneType.SignIn, false);
        }

        if (isSignUp)
        {
            TurnOn(SceneType.SignUp, false);
        }

        if (isRecovery)
        {
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
            TurnOn(SceneType.CreateDeck, false);
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
        TurnOn(SceneType.CreateDeck, true);
        ACT_SaveDeck.onClick.RemoveAllListeners();
        GameData.instance.selectDeck = null;
        ACT_SaveDeck.onClick.AddListener(() => StartCoroutine(CollectionManager.instance.CreateDeck()));
    }
    public void TurnOnUpdateDeckScene()
    {
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
        yield return StartCoroutine(PlayfabManager.instance.GetElo());
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
        set { this.elo.ForEach(a => a.text = "Ranking Score: " + value); }
    }

    public string NumberCardInDeck
    {
        get { return numberCardInDeck[0].text; }
        private set { this.numberCardInDeck.ForEach(a => a.text = value); }
    }


    public string VirtualMoney
    {
        get { return virtualMoney[0].text; }
        private set { virtualMoney.ForEach(a => a.text = value); }
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

    public void ShowPopupCard(CardInInventory item)
    {
        GameData.instance.itemPurchaseRequests.Clear();
        GameData.instance.itemPurchaseRequests.Add(new PlayFab.ClientModels.ItemPurchaseRequest()
        {
            ItemId = item.CardItem.cardData.Id,
            Quantity = 1
        });
        var popup = PopupCardDetailed.GetComponent<popupDetail>();
        PopupCardDetailed.SetActive(true);

        popup.description.text = item.CardItem.cardData.Name + "\n" + item.CardItem.cardData.Description;
    }
    public void ShowPopupDeckDetailed(DeckItem item)
    {
        GameData.instance.itemPurchaseRequests.Clear();
        GameData.instance.itemPurchaseRequests.Add(new PlayFab.ClientModels.ItemPurchaseRequest()
        {
            ItemId = item.Id,
            Quantity = 1
        });
        //print("DECK ID:" + item.Id);
        var popup = PopupDeckDetailed.GetComponent<popupDetail>();
        PopupDeckDetailed.SetActive(true);
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
        popup.description.text = string.Join("\n", dic.Select(x => $"{x.Key} : {x.Value}x"));
        popup.description.text += "\n";
    }
    public void ShowPopupPackDetailed(PackItem item)
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
            if (dic.ContainsKey(key))
            {
                dic[key]++;
            }
            else
            {
                dic.Add(key, 1);
            }
        }

        print($"popup dic count: {dic.Count}");
        popup.description.text = string.Join("\n", dic.Select(x => $"{x.Key} : {x.Value}x"));
        popup.description.text += "\n";

        print("droptableinfor count: " + GameData.instance.dropTableInforList.Count);
        var matchingItems = GameData.instance.dropTableInforList.Where(x => dic.ContainsKey(x.id)); // get the items from the list that have the same id as the keys in dic
        popup.description.text += string.Join("\n", matchingItems.Select(item => $"{item.id}: {item.ItemsToString()}")); // append the items to the text
        print("pack data items" + packData.cardItemsId.Count);

        print("pack data items" + packData.cardItemsId.Count);
    }

    public void WatingAcceptMatch(bool enable)
    {
        if (ACT_AcceptMatch.gameObject.activeSelf != enable)
            ACT_AcceptMatch.gameObject.SetActive(enable);

        if (enable)
        {
            if (!WaitingAcceptMatch.isTimerRunning)
            {
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
