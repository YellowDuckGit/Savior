using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum SceneType
{
    SignIn, SignUp, Recovery, Home, LoadingProcessAPI, Play, Store, Collection, CollectionDecks, CollectionCards, CreateDeck
}
public class UIManager : MonoBehaviour
{
    public static UIManager instance;

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
    [SerializeField] private List<GameObject> LoadingProcessAPI;
    [SerializeField] private List<GameObject> PlayScene;
    [SerializeField] private List<GameObject> CollectionScene;
    [SerializeField] private List<GameObject> Collection_DecksScene;
    [SerializeField] private List<GameObject> Collection_CardsScene;
    [SerializeField] private List<GameObject> StoreScene;
    [SerializeField] private List<GameObject> CreateDeckScene;
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
    [SerializeField] GameObject DeckPack;
    [SerializeField] GameObject CardInventory;
    [SerializeField] GameObject CollectionPack;
    //Play scene
    [SerializeField] GameObject CollectionDeck_PlayScene;
    [SerializeField] GameObject SelectFrame;

    [SerializeField] GameObject PopupPackDetailed;
    [Space(10)]

    /// <summary>
    /// Variables bellow will be updated one or more times in game
    /// </summary>
    [Header("Loading Data")]
    [Space(5)]
    //Create Card Scene
    [SerializeField] TextMeshProUGUI numberCardInDeck;
    [SerializeField] TMP_InputField deckName;
    [SerializeField] TextMeshProUGUI elo;
    [SerializeField] TextMeshProUGUI waitingTime;
    [SerializeField] TextMeshProUGUI virtualMoneyHome;
    [SerializeField] TextMeshProUGUI virtualMoneyStore;

    [Space(10)]

    [Header("Button Event")]
    [Space(5)]
    //switch Scene Button
    [SerializeField] Button switchSceneSignIn;
    [SerializeField] Button switchSceneSignUp;
    [SerializeField] Button switchSceneRecovery;
    [SerializeField] Button switchSceneHome;
    [SerializeField] Button switchScenePlay;
    [SerializeField] Button switchSceneCollection;
    [SerializeField] Button switchSceneStore;
    [SerializeField] Button switchSceneCollectionDecks;
    [SerializeField] Button switchSceneCollectionCards;
    [SerializeField] Button switchSceneCreateDeck;
    [SerializeField] Button switchSceneBack;

    //Action Button
    [SerializeField] Button ACT_SaveDeck;
    [SerializeField] Button ACT_DeleteDeck;
    [SerializeField] Button ACT_NormalMode;
    [SerializeField] Button ACT_RankedMode;
    [SerializeField] Button ACT_Play;
    [SerializeField] Button ACT_Buy;
    [SerializeField] Button ACT_Cancel;

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
    public bool isLoadingProcessAPI;
    public bool isPlay;
    public bool isCollection;
    public bool isCollection_Decks;
    public bool isCollection_Cards;
    public bool isStore;
    public bool isCreateDeck;


    //[Space(10)]
    //[Header("Notification UI")]
    //[Space(5)]
    //[SerializeField] Outline selectCardOutLine;



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
            //this.RegisterListener(EventID.OnChangeDeckName, (param) => LoadDeckName());
            //this.RegisterListener(EventID.OnChangeNumberCardInDeck, (param) => LoadNumberCardInDeck((int)param));
            #endregion

            #region Add Button Event
            switchSceneSignUp.onClick.AddListener(() => TurnOnSignUpScene());
            switchSceneRecovery.onClick.AddListener(() => TurnOnRecoveryScene());
            switchSceneSignIn.onClick.AddListener(() => TurnOnSignInScene());


            //home.onClick.AddListener(() => TurnOnHomeScene());
            //play.onClick.AddListener(() => TurnOnPlayScene());
            //collection.onClick.AddListener(() => TurnOnCollectionScene());
            //store.onClick.AddListener(() => TurnOnStoreScene());
            //collectionDecks.onClick.AddListener(() => TurnOnCollectionDeckScene());
            //collectionCards.onClick.AddListener(() => TurnOnCollectionCardScene());
            //createDeck.onClick.AddListener(() => TurnOnCreateDeckScene());
            //back.onClick.AddListener(() => TurnOnBackScene());
            //ACT_DeleteDeck.onClick.AddListener(() => StartCoroutine(CollectionManager.instance.DeleteDeck()));
            //ACT_Buy.onClick.AddListener(() =>
            //{
            //    print("click to buy");
            //    StartCoroutine(PlayFabAuth.instance.BuyPacks("Card", "BS1", GameData.instance.itemPurchaseRequests, "MC"));
            //    PopupPackDetailed.SetActive(false);
            //});
            //ACT_Cancel.onClick.AddListener(() => PopupPackDetailed.SetActive(false));


            //ACT_NormalMode.onClick.AddListener(() => OnClickNormalMode());
            //ACT_RankedMode.onClick.AddListener(() => OnClickRankedMode());
            //ACT_FindMatch.onClick.AddListener(() => OnClickFindMatch());
            //ACT_AcceptMatch.onClick.AddListener(() => OnClickAcceptMatch());
            //ACT_DeclinetMatch.onClick.AddListener(() => OnClickDeclineMatch());
            //ACT_StopFind.onClick.AddListener(() => OnClickDeclineMatch());

            //OnClickNormalMode();
            //EventSystem.current.SetSelectedGameObject(null);
            //EventSystem.current.SetSelectedGameObject(buttonNormalMode.gameObject);
            #endregion
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region TurnOn, TurnOff Scene
    private void TurnOn(SceneType type, bool turn)
    {
        print("Type: "+type+" Turn: "+turn);
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
                    isHome = turn;

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
                    isHome = turn;

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
                    isHome = turn;

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
            case SceneType.LoadingProcessAPI:
                if (isLoadingProcessAPI ^ turn)
                {
                    if (turn) TurnOffSceneAlreadyShow();
                    print("loading :" + turn);

                    isLoadingProcessAPI = turn;
                    foreach (GameObject obj in LoadingProcessAPI)
                    {
                        obj.SetActive(turn);
                    }
                }

                if (isLoadingProcessAPI)
                {
                    lastScence = presentScene;
                    presentScene = SceneType.LoadingProcessAPI;
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
            case SceneType.Collection:

                if (isCollection ^ turn)
                {
                    if (turn) TurnOffSceneAlreadyShow();
                    isCollection = turn;
                    foreach (GameObject obj in CollectionScene)
                    {
                        obj.SetActive(turn);
                    }
                }

                if (isCollection)
                {
                    lastScence = presentScene;
                    presentScene = SceneType.Collection;
                }
                break;
            case SceneType.CollectionDecks:
                if (isCollection_Decks ^ turn)
                {
                    if (turn)
                    {
                        //StartCoroutine(GameData.instance.LoadDeckItems(CollectionDeck));
                        //GameData.instance.UnLoadCardInDeckPack();
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
                        //StartCoroutine(GameData.instance.LoadCardCollection(CollectionCard));
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
            case SceneType.Store:

                if (isStore ^ turn)
                {
                    if (turn)
                    {
                        //StartCoroutine(GameData.instance.LoadPack(CollectionPack));
                        TurnOffSceneAlreadyShow();
                    }
                    isStore = turn;
                    foreach (GameObject obj in StoreScene)
                    {
                        if (obj.name != "Popup_PackDetail")
                        {
                            obj.SetActive(turn);
                        }
                    }
                }

                if (isStore)
                {
                    lastScence = presentScene;
                    presentScene = SceneType.Store;
                }
                break;
            case SceneType.CreateDeck:

                if (isCreateDeck ^ turn)
                {
                    if (turn)
                    {
                        //reset deck name
                        //LoadDeckName();
                        //StartCoroutine(GameData.instance.LoadCardInDeckPack(DeckPack));

                        //StartCoroutine(GameData.instance.LoadCardInInventoryUser(CardInventory));
                        //LoadNumberCardInDeck(GameData.instance.getNumberCardInDeck());
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
        if (isLoadingProcessAPI)
        {
            TurnOn(SceneType.LoadingProcessAPI, false);
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

        if (isStore)
        {
            TurnOn(SceneType.Store, false);
        }

        if (isCreateDeck)
        {
            TurnOn(SceneType.CreateDeck, false);
        }

        if (isCollection)
        {
            TurnOn(SceneType.Collection, false);
        }

        if (isCollection_Cards)
        {
            TurnOn(SceneType.CollectionCards, false);
        }

        if (isCollection_Decks)
        {
            TurnOn(SceneType.CollectionDecks, false);
        }
    }
    #endregion

    #region Switch Scene Function
    public void TurnOnBackScene()
    {
        print(lastScence);
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
        TurnOn(SceneType.SignUp, true);
    }

    public void TurnOnRecoveryScene()
    {
        TurnOn(SceneType.Recovery, true);
    }

    public void TurnOnLoadingScene()
    {
        TurnOn(SceneType.LoadingProcessAPI, true);
    }

    public void TurnOnPlayScene()
    {
        TurnOn(SceneType.Play, true);
    }
    public void TurnOnCollectionScene()
    {
        TurnOn(SceneType.Collection, true);
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
        //ACT_SaveDeck.onClick.RemoveAllListeners();
        //ACT_SaveDeck.onClick.AddListener(() => StartCoroutine(CollectionManager.instance.CreateDeck()));
    }
    public void TurnOnUpdateDeckScene()
    {
        TurnOn(SceneType.CreateDeck, true);
        //ACT_SaveDeck.onClick.RemoveAllListeners();
        //ACT_SaveDeck.onClick.AddListener(() => StartCoroutine(CollectionManager.instance.UpdateDeck()));

    }

    public void TurnOnStoreScene()
    {
        TurnOn(SceneType.Store, true);
    }

    #endregion
}
