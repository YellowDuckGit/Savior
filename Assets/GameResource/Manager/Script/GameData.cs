using Assets.GameComponent.Card.CardComponents.Script;
using Assets.GameComponent.UI.CreateDeck.UI.Script;
using Photon.Pun;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using WebSocketSharp;
using static Data_Pack;

public class GameData : MonoBehaviour
{
    public static GameData instance;

    [Header("Manager Data")]
    //list monster data form resource
    public List<ICardData> listCardDataInGame = new List<ICardData>();

    ///list card id of user, get it form playfab
    public List<string> listCard = new List<string>();

    public ListDeck listDeck;

    public List<CardItem> listCardItem = new List<CardItem>();
    public List<DeckItem> listDeckItem = new List<DeckItem>();
    public List<PackItem> listPackItem = new List<PackItem>(); //list object
    public List<Data_Pack> listPackData = new List<Data_Pack>(); //list instance
    public List<FriendInfo> listFriendData = new List<FriendInfo>();
    public List<FriendItem> listFriendItem = new List<FriendItem>();
    public List<ItemPurchaseRequest> itemPurchaseRequests = new List<ItemPurchaseRequest>();
    public List<DropTableInfor> dropTableInforList = new List<DropTableInfor>();


    public List<CardInInventory> listCardInInventory = new List<CardInInventory>();
    public List<CardInDeckPack> listCardInDeckPack = new List<CardInDeckPack>();

    [Header("Prefab")]
    [SerializeField] GameObject MonsterCardPrefFab;
    [SerializeField] GameObject SpellCardPrefFab;

    [SerializeField] GameObject DeckItemPrefFab;
    [SerializeField] GameObject CardItemPrefab;
    [SerializeField] GameObject CardInInventoryPrefab_M;
    [SerializeField] GameObject CardInInventoryPrefab_SP;
    [SerializeField] GameObject cardInDeckPackPrefab;
    [SerializeField] GameObject packPrefab;
    [SerializeField] GameObject FriendPrefab;


    [Header("Choose Deck")]
    public DeckItem selectDeck;
    private bool triggerLoadingGameProcess = false;
    public string packName;

    //player information
    private int _elo;
    private int _coin;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (instance != null && instance != this)
        {
            Debug.LogError("MonsterDataManager have 2");
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    void Start()
    {

    }

    public IEnumerator LoadingGameProcess()
    {
        print("Loading Game Process");
        print("=====================================================================");

        //UIManager.instance.TurnOnLoadingScene();

        #region Init GameObject
        yield return StartCoroutine(LoadCardItemInGame());

        yield return StartCoroutine(LoadCardInInventoryInGame());

        yield return StartCoroutine(LoadCardInInventoryUser());

        yield return StartCoroutine(LoadDeckItems());

        yield return StartCoroutine(LoadPack());

        yield return StartCoroutine(LoadFriendItem());
        #endregion

        yield return StartCoroutine(UIManager.instance.LoadVirtualMoney());

        //yield return StartCoroutine(UIManager.instance.LoadElo());

        //UIManager.instance.TurnOnLoadingScene();
        UIManager.instance.TurnOnHomeScene();

        print("=====================================================================");
    }

    #region Load Data Function
    private IEnumerator GetMonsterDatasFormResoure()
    {
        //MonsterData[] cardDatas = Resources.FindObjectsOfTypeAll(typeof(MonsterData)) as MonsterData[];
        Debug.Log("LoadMonsterDatas");
        var cardDatas = Resources.LoadAll<ScriptableObject>("CardsData");
        while (true)
        {
            if (cardDatas.Length > 0)
            {

                listCardDataInGame = cardDatas.Where(data => data is ICardData).Select(data => (ICardData)data).ToList();
                break;
            }
            yield return null;
        }
    }

    private void GetListDeck(string json)
    {
        listDeck = JsonUtility.FromJson<ListDeck>(json);
        print("Number Desk: " + listDeck.listDeck.Length);
        foreach (Data_Deck data in listDeck.listDeck)
        {
            data.setListCardItem();
        }
    }

    private IEnumerator LoadingCardItemInInventory()
    {
        foreach (string str in listCard)
        {
            string[] arr = str.Split(':');
            string id = arr[0];
            int amount = Int32.Parse(arr[1]);
            print("Card ID : " + id);
            CardItem item = listCardItem.Single(a => a.cardData.Id.Equals(id));
            CardInInventory card = listCardInInventory.Single(a => a.CardItem.cardData.Id.Equals(id));
            item.amount = amount;
            card.NumberCard = amount;
        }
        print("CARD AMOUNT: " + listCard.Count());

        yield return null;
    }

    public int getNumberCardInDeck()
    {
        int count = 0;
        List<CardInDeckPack> list = GameData.instance.listCardInDeckPack;
        if (list.Count > 0)
        {
            foreach (CardInDeckPack card in GameData.instance.listCardInDeckPack)
            {
                count += card.NumberCard;
            }
        }
        return count;
    }
    #endregion

    #region Inint Function
    private IEnumerator InitDeckItem()
    {
        print("LoadingGameObjectDeck");

        UnLoadListDeckItem();

        foreach (Data_Deck data in listDeck.listDeck)
        {
            DeckItem deckItem = GameObject.Instantiate(DeckItemPrefFab, Vector3.zero, Quaternion.identity).GetComponent<DeckItem>();
            deckItem.gameObject.SetActive(false);
            deckItem.Data = data;
            listDeckItem.Add(deckItem);
            print("ININT");
        }
        yield return null;
    }

    private IEnumerator InitCardItemInGame()
    {
        UnLoadListCardItem();
        foreach (ICardData cardData in listCardDataInGame)
        {

            CardItem cardItem = GameObject.Instantiate(CardItemPrefab, Vector3.zero, Quaternion.identity).GetComponent<CardItem>();
            cardItem.gameObject.SetActive(false);
            cardItem.cardData = cardData;
            listCardItem.Add(cardItem);
        }
        yield return null;
    }

    private IEnumerator InitCardInInventory()
    {
        UnLoadListCardInventory();
        foreach (CardItem cardItem in listCardItem)
        {
            if (cardItem.cardData is MonsterData)
            {
                CardInInventory card = GameObject.Instantiate(CardInInventoryPrefab_M, Vector3.zero, Quaternion.identity).GetComponent<CardInInventory>();
                card.gameObject.SetActive(false);
                card.CardItem = cardItem;
                listCardInInventory.Add(card);
            }
            else if (cardItem.cardData is SpellData)
            {
                CardInInventory card = GameObject.Instantiate(CardInInventoryPrefab_SP, Vector3.zero, Quaternion.identity).GetComponent<CardInInventory>();
                card.gameObject.SetActive(false);
                card.CardItem = cardItem;
                listCardInInventory.Add(card);
            }
            else
            {
                print(this.debug("Not found type of Card Data"));
            }
        }
        yield return null;
    }

    private IEnumerator InitCardInDeckPack(List<string> listCardID)
    {

        foreach (string cardID in listCardID)
        {
            print(this.debug("listCardID", new
            {
                cardID
            }));
            string[] arr = cardID.Split(":");
            string id = arr[0];
            string amount = arr[1];

            CardInDeckPack card = GameObject.Instantiate(cardInDeckPackPrefab, Vector3.zero, Quaternion.identity).GetComponent<CardInDeckPack>();
            card.gameObject.SetActive(false);
            card.CardItem = listCardItem.Single(a => a.cardData.Id.Equals(id));
            card.NumberCard = Int32.Parse(amount);
            listCardInDeckPack.Add(card);
        }
        yield return null;
    }

    private IEnumerator InitPack()
    {
        Debug.Log("START INIT PACK");
        UnLoadListPackItem();
        foreach (Data_Pack item in listPackData)
        {
            Debug.Log("PACK ITEMS");
            PackItem pack = GameObject.Instantiate(packPrefab, Vector3.zero, Quaternion.identity).GetComponent<PackItem>();
            pack.ID = item.id;
            pack.Data = item;
            pack.gameObject.SetActive(false);
            listPackItem.Add(pack);
            Debug.Log("END PACK ITEMS");
        }
        Debug.Log("END INIT PACK");

        yield return null;
    }

    private IEnumerator InitFriendItem()
    {
        UnLoadListFriendItem();
        foreach (FriendInfo data in GameData.instance.listFriendData)
        {
            Debug.Log("Friend ITEMS");
            FriendItem friend = GameObject.Instantiate(FriendPrefab, Vector3.zero, Quaternion.identity).GetComponent<FriendItem>();
            friend.Name.text = data.Username;
            friend.gameObject.SetActive(false);
            listFriendItem.Add(friend);
            Debug.Log("END Friend ITEMS");
        }
        Debug.Log("END Friend PACK");

        yield return null;
    }
    #endregion

    #region UnLoad Data 
    public void UnLoadListDeckItem()
    {
        if (listDeckItem.Count > 0)
        {
            print("UnLoadListDeckItem");
            if (listDeckItem[0] == null)
            {
                listDeckItem.Clear();
            }
            else
            {
                foreach (DeckItem data in listDeckItem)
                {
                    Destroy(data.gameObject);
                }
                listDeckItem.Clear();
            }
        }
    }

    public void UnLoadListCardInventory()
    {
        if (listCardInInventory.Count > 0)
        {

            if (listCardInInventory[0] == null)
            {
                listCardInInventory.Clear();
            }
            else
            {
                foreach (CardInInventory data in listCardInInventory)
                {
                    Destroy(data.gameObject);
                }
                listCardInInventory.Clear();
            }
        }
    }

    public void UnLoadCardInDeckPack()
    {
        if (listCardInDeckPack.Count > 0)
        {
            foreach (CardInDeckPack data in listCardInDeckPack)
            {
                Destroy(data.gameObject);
            }
            listCardInDeckPack.Clear();
        }
    }

    public void UnLoadListCardItem()
    {
        if (listCardItem.Count > 0)
        {

            if (listCardItem[0] == null)
            {
                listCardItem.Clear();
            }
            else
            {
                foreach (CardItem data in listCardItem)
                {
                    Destroy(data.gameObject);
                }
                listCardItem.Clear();
            }
        }
    }

    public void UnLoadListPackItem()
    {
        if (listPackItem.Count > 0)
        {

            if (listPackItem[0] == null)
            {
                listPackItem.Clear();
            }
            else
            {
                foreach (PackItem data in listPackItem)
                {
                    Destroy(data.gameObject);
                }
                listPackItem.Clear();
            }
        }
    }

    public void UnLoadListFriendItem()
    {
        if (listFriendItem.Count > 0)
        {

            if (listFriendItem[0] == null)
            {
                listFriendItem.Clear();
            }
            else
            {
                foreach (FriendItem data in listFriendItem)
                {
                    Destroy(data.gameObject);
                }
                listFriendItem.Clear();
            }
        }
    }
    #endregion

    #region Scene Manager
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnLoaded;

    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnLoaded;

    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("LoadScene: " + scene.name);
        if (scene.name.Equals("Home"))
        {
            StartCoroutine(LoadingGameProcess());
        }
    }

    void OnSceneUnLoaded(Scene scene)
    {
        Debug.Log("UnScene: " + scene.name);
        if (scene.name.Equals("Match"))
        {
            print("===================DDDDDDDDDDD+++++++++++++++");

        }
    }
    #endregion
    public IEnumerator LoadDeckItems()
    {
        yield return StartCoroutine(PlayfabManager.instance.GetUserData("Decks", GetListDeck));
        StartCoroutine(InitDeckItem());
    }

    public IEnumerator LoadDeckItems(GameObject parent)
    {
        print("LoadDeckItem");
        if (listDeckItem.Count > 0)
        {
            if (listDeckItem[0].transform.parent != parent.transform)
            {
                foreach (DeckItem deckItem in listDeckItem)
                {
                    deckItem.gameObject.SetActive(true);
                    deckItem.transform.parent = parent.transform;
                    deckItem.transform.localScale = new Vector3(1f, 1f, 1f);
                    deckItem.transform.localPosition = new Vector3(0f, 0f, 0f);

                }
            }
        }
        else
        {
            print(this.debug("listDeckItem Null"));
        }
        yield return null;
    }

    public IEnumerator LoadCardInInventoryUser()
    {
        yield return StartCoroutine(PlayfabManager.instance.GetCards());
        //yield return StartCoroutine(GetMonsterDataPlayer());
        yield return StartCoroutine(LoadingCardItemInInventory());
    }

    public IEnumerator LoadCardInInventoryUser(GameObject parent)
    {
        if (listCardInInventory[0].transform.parent != parent.transform)
        {
            foreach (CardInInventory card in listCardInInventory)
            {
                card.gameObject.SetActive(true);
                card.transform.parent = parent.transform;
                card.transform.localScale = new Vector3(1f, 1f, 1f);

                //number card
                CardInDeckPack cardInDeckPack = listCardInDeckPack.SingleOrDefault(a => a.CardItem == card.CardItem);
                if (cardInDeckPack != null)
                {
                    card.NumberCard = card.CardItem.amount - cardInDeckPack.NumberCard;
                }
                else
                {
                    card.NumberCard = card.CardItem.amount;
                }
            }
        }
        yield return null;
    }

    public IEnumerator LoadCardInDeckPack(List<string> listCardID)
    {
        yield return StartCoroutine(InitCardInDeckPack(listCardID));
    }

    public IEnumerator LoadCardInDeckPack(GameObject parent)
    {
        if (listCardInDeckPack.Count > 0 && listCardInDeckPack[0].transform.parent != parent.transform)  
        {
            foreach (CardInDeckPack card in listCardInDeckPack)
            {
                card.gameObject.SetActive(true);
                card.transform.parent = parent.transform;
                card.transform.localScale = new Vector3(1f, 1f, 1f);
                card.transform.localPosition = new Vector3(0f, 0f, 0f);

            }
        }
        yield return null;
    }

    public IEnumerator LoadCardCollection(GameObject parent)
    {
        if (listCardInInventory[0].transform.parent != parent.transform)
        {
            foreach (CardInInventory card in listCardInInventory)
            {
                card.gameObject.SetActive(true);
                card.transform.parent = parent.transform;
                card.transform.localScale = new Vector3(1f, 1f, 1f);

                card.transform.localPosition = Vector3.zero;
                //numer card
                card.NumberCard = card.CardItem.amount;
            }
        }
        yield return null;
    }

    public IEnumerator LoadPack()
    {
        yield return StartCoroutine(PlayfabManager.instance.GetStores(cataLog: "Card", storeId: "BS1"));
        yield return StartCoroutine(InitPack());
        //yield return StartCoroutine(PlayFabAuth.instance.GetDropTable());


    }

    public IEnumerator LoadPack(GameObject parent)
    {
        if (listPackItem[0].transform.parent != parent.transform)
        {
            Debug.Log("START LOAD PACK");
            foreach (PackItem item in listPackItem)
            {
                item.gameObject.SetActive(true);
                item.transform.parent = parent.transform;
                item.transform.localScale = new Vector3(1f, 1f, 1f);
                item.transform.localPosition = new Vector3(0f, 0f, 0f);
                item.text_packName.text = item.ID.ToString();
            }
            Debug.Log("END LOAD PACK");
        }
        yield return null;
    }

    public IEnumerator LoadCardItemInGame()
    {
        yield return StartCoroutine(GetMonsterDatasFormResoure());
        yield return StartCoroutine(InitCardItemInGame());
    }

    public IEnumerator LoadCardInInventoryInGame()
    {
        yield return StartCoroutine(InitCardInInventory());
    }

    public IEnumerator LoadFriendItem()
    {
        yield return StartCoroutine(PlayfabManager.instance.GetFriends());
        yield return StartCoroutine(InitFriendItem());
        print("aaaaaaaa");
        yield return StartCoroutine(LoadFriendItem(UIManager.instance.CollectionFriend));
        print("bbbbb");

    }

    public IEnumerator LoadFriendItem(GameObject parent)
    {
        print("LoadFriendItem");
        if (listFriendItem[0].transform.parent != parent.transform)
        {
            Debug.Log("START LOAD Friend");
            foreach (FriendItem item in listFriendItem)
            {
                item.gameObject.SetActive(true);
                item.transform.parent = parent.transform;
                item.transform.localScale = new Vector3(1f, 1f, 1f);
                item.transform.localPosition = new Vector3(0f, 0f, 0f);
            }
            Debug.Log("END LOAD Friend");
        }
        yield return null;
    }

    #region Get Set
    public int Elo
    {
        get { return _elo; }
        set { _elo = value; }
    }

    public int Coin
    {
        get { return _coin; }
        set { _coin = value; }
    }
    #endregion
}
