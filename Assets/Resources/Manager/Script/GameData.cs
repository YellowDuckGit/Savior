using Assets.GameComponent.Card.CardComponents.Script;
using Assets.GameComponent.UI.CreateDeck.UI.Script;
using ExitGames.Client.Photon.StructWrapping;
using MoreMountains.Feel;
using Photon.Pun;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
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
    //list card data form resource
    public List<ICardData> listCardDataInGame = new List<ICardData>();

    ///list card id of user, get it form playfab
    public List<string> listCard = new List<string>();
    public List<string> listCardInDeck = new List<string>();


    public ListDeck listDeck;

    public List<CardItem> listCardItem = new List<CardItem>();
    public List<DeckItem> listDeckItem = new List<DeckItem>();
    public List<DeckItem> listDeckItemInStore = new List<DeckItem>();
    public List<Data_Deck> listDeckDataInStore = new List<Data_Deck>();
    public List<PackItem> listPackItem = new List<PackItem>(); //list object
    public List<Data_Pack> listPackData = new List<Data_Pack>(); //list instance
    public List<Data_Deck> listDeckData = new List<Data_Deck>();
    public List<FriendInfo> listFriendData = new List<FriendInfo>();
    public List<FriendItem> listFriendItem = new List<FriendItem>();
    public List<ItemPurchaseRequest> itemPurchaseRequests = new List<ItemPurchaseRequest>();
    public List<DropTableInfor> dropTableInforList = new List<DropTableInfor>();
    public List<CardInInventory> listCardInInventory = new List<CardInInventory>();
    public List<CardInDeckPack> listCardInDeckPack = new List<CardInDeckPack>();
    public List<CardInPack> listCardInPack = new List<CardInPack>();
    public List<List<string>> listCardOpenedInPack = new List<List<string>>();
    public Dictionary<string, string> listCardPrice = new Dictionary<string, string>();



    [Header("Prefab")]
    [SerializeField] GameObject MonsterCardPrefFab;
    [SerializeField] GameObject SpellCardPrefFab;
    [SerializeField] GameObject DeckItemPrefFab;
    [SerializeField] GameObject CardItemPrefab;
    [SerializeField] GameObject CardInInventoryPrefab_M;
    [SerializeField] GameObject CardInInventoryPrefab_SP;
    [SerializeField] GameObject cardInDeckPackPrefab;
    [SerializeField] GameObject packPrefab;
    [SerializeField] GameObject deckPrefab;
    [SerializeField] GameObject FriendPrefab;
    [SerializeField] GameObject CardInPack;



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

        yield return StartCoroutine(LoadDeck());

        yield return StartCoroutine(PlayfabManager.instance.CalElo(false,0,0));


        //Load Price of Card
        yield return StartCoroutine(LoadingCardPrice());
        
        yield return StartCoroutine(LoadFriendItem());

        //yield return StartCoroutine(LoadDeck());
        #endregion

        UIManager.instance.UserName = ChatManager.instance.nickName;

        yield return StartCoroutine(UIManager.instance.LoadVirtualMoney());

        yield return StartCoroutine(UIManager.instance.LoadElo());

        //yield return StartCoroutine(UIManager.instance.LoadElo());

        //UIManager.instance.TurnOnLoadingScene();
        UIManager.instance.TurnOnHomeScene();
        UIManager.instance.EnableLoadingAPI(false);
        print("=====================================================================");
    }

    #region Load Data Function
    private IEnumerator GetCardDatasFormResoure()
    {
        //Materials\CardAvatar_Material\CardAvatar_Card\ForestBeast\Part1
        //MonsterData[] cardDatas = Resources.FindObjectsOfTypeAll(typeof(MonsterData)) as MonsterData[];
        //Resources\Card\Materials\CardAvatar_Material\CardAvatar_Card\ForestBeast\Part1
        //Debug.Log("/Resources/Card/Materials/CardAvatar_Material/CardAvatar_Card/ForestBeast/Part1");
        Debug.Log("LoadMonsterDatas");
        var cardDatas = Resources.LoadAll<ScriptableObject>("Card/Data");
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

    private IEnumerator LoadingCardPrice()
    {
        foreach (KeyValuePair<string, string> entry in listCardPrice)
        {
            print(entry.Key + ": " + entry.Value);
            CardItem card = listCardItem.SingleOrDefault(a=>a.cardData.Id == entry.Key);
            if(card != null) card.price = Int32.Parse(entry.Value);
        }

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
    public IEnumerator LoadCardInPackItem(DropTableInfor dropTableInfor)
    {

        UnLoadListCardInPackItem();
        foreach (KeyValuePair<string, int> entry in dropTableInfor.items)
        {
            print(entry.Key + ": " + entry.Value);  
            CardInPack card = GameObject.Instantiate(CardInPack, Vector3.zero, Quaternion.identity).GetComponent<CardInPack>();
            card.CardItem = listCardItem.Single(a=>a.cardData.Id.Equals(entry.Key));
            if(card.CardItem != null ) print(card.CardItem.cardData.Id);
            listCardInPack.Add(card);
        }


        foreach (CardInPack card in listCardInPack)
        {
            switch (card.CardItem.cardData.RarityCard)
            {
                case Rarity.Normal:
                    card.transform.parent = UIManager.instance.CollectionCardInPackNormal.transform;
                    print("Rarity.Normal:");
                    break;
                case Rarity.Elite:
                    card.transform.parent = UIManager.instance.CollectionCardInPackElite.transform;
                    print("Rarity.Elite:");

                    break;
                case Rarity.Epic:
                    card.transform.parent = UIManager.instance.CollectionCardInPackEpic.transform;
                    print("Rarity.Epic:");

                    break;
                case Rarity.Legendary:
                    card.transform.parent = UIManager.instance.CollectionCardInPackLegendary.transform;
                    print("Rarity.Legendary:");

                    break;
            }
            card.transform.localScale = new Vector3(1f, 1f, 1f);
            card.transform.localPosition = Vector3.zero;

        }
        yield return null;
    }

    public IEnumerator LoadCardInDeckStoreItem(Dictionary<string, int> dic)
    {

        UnLoadCardInDeckPack();

        foreach (CardItem items in listCardItem)
        {
            print(items.cardData.Id);
        }

        foreach (KeyValuePair<string, int> entry in dic)
        {
            print(entry.Key + ": " + entry.Value);
            CardInDeckPack card = GameObject.Instantiate(cardInDeckPackPrefab, Vector3.zero, Quaternion.identity).GetComponent<CardInDeckPack>();
            card.CardItem = listCardItem.Single(a => a.cardData.Id.Equals(entry.Key));
            card.NumberCard = entry.Value;
            if (card.CardItem != null) print(card.CardItem.cardData.Id);
            listCardInDeckPack.Add(card);
        }


        foreach (CardInDeckPack card in listCardInDeckPack)
        {
            switch (card.CardItem.cardData.RarityCard)
            {
                case Rarity.Normal:
                    card.transform.parent = UIManager.instance.CollectionCardInDeckNormal.transform;
                    print("Rarity.Normal:");
                    break;
                case Rarity.Elite:
                    card.transform.parent = UIManager.instance.CollectionCardInDeckElite.transform;
                    print("Rarity.Elite:");

                    break;
                case Rarity.Epic:
                    card.transform.parent = UIManager.instance.CollectionCardInDeckEpic.transform;
                    print("Rarity.Epic:");

                    break;
                case Rarity.Legendary:
                    card.transform.parent = UIManager.instance.CollectionCardInDeckLegendary.transform;
                    print("Rarity.Legendary:");

                    break;
            }
            card.transform.localScale = new Vector3(1f, 1f, 1f);
            card.transform.localPosition = Vector3.zero;

        }
        yield return null;
    }


    public CardInInventory InitCard2D(CardItem card)
    {
        print("InitCard2D");
        GameObject prefab = null;

        if(card.cardData.CardType == CardType.Monster)
        {
            prefab = CardInInventoryPrefab_M;
        }
        else if (card.cardData.CardType == CardType.Spell)
        {
            prefab = CardInInventoryPrefab_SP;
        }
      
        CardInInventory cardInInventory = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity).GetComponent<CardInInventory>();
        cardInInventory.CardItem = card;

        CardInDeckPack cardInDeckPack = listCardInDeckPack.SingleOrDefault(a => a.CardItem == card);
        if (cardInDeckPack != null)
        {
            cardInInventory.NumberCard = card.amount - cardInDeckPack.NumberCard;
        }
        else
        {
            cardInInventory.NumberCard = card.amount;
        }


        return cardInInventory;
    }

    private IEnumerator InitDeckItem()
    {
        print("INIT DECK ITEM");

        UnLoadListDeckItem();

        foreach (Data_Deck data in listDeck.listDeck)
        {
            DeckItem deckItem = GameObject.Instantiate(DeckItemPrefFab, Vector3.zero, Quaternion.identity).GetComponent<DeckItem>();
            deckItem.price.gameObject.SetActive(false);
            deckItem.gameObject.SetActive(false);
            deckItem.Data = data;
            deckItem.text_DeckName.text = data.deckName;
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
            print("ICardData: " + cardData.Name);

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
            pack.text_packName.text = item.packName;
            pack.Data = item;
            pack.price.text = item.price;
            pack.gameObject.SetActive(false);
            listPackItem.Add(pack);
            Debug.Log("END PACK ITEMS");
        }
        Debug.Log("END INIT PACK");

        yield return null;
    }

    private IEnumerator InitDeck()
    {
        Debug.Log("START INIT DECK");
        UnLoadListDeckItemInStore();
        foreach (Data_Deck item in listDeckDataInStore)
        {
            Debug.Log("DECK ITEMS");
            DeckItem deck = GameObject.Instantiate(deckPrefab, Vector3.zero, Quaternion.identity).GetComponent<DeckItem>();
            deck.Id = item.id;
            deck.Data = item;
            deck.text_DeckName.text = item.deckName;
            Debug.Log("DECK NAME: " + item.deckName);
            deck.price.text = item.price;
            deck.gameObject.SetActive(false);
            listDeckItemInStore.Add(deck);
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
            friend.userName.text = data.Username;
            friend.Status = 0; //offline
            friend.gameObject.SetActive(false);
            listFriendItem.Add(friend);
            Debug.Log("END Friend ITEMS");
        }
        Debug.Log("END Friend PACK");

        yield return null;
    }
    #endregion

    #region UnLoad Data 
    public void UnLoadListCardInPackItem()
    {
        if (listCardInPack.Count > 0)
        {
            print("UnLoadListCardInPackItem");
            if (listCardInPack[0] == null)
            {
                listCardInPack.Clear();
            }
            else
            {
                foreach (CardInPack data in listCardInPack)
                {
                    Destroy(data.gameObject);
                }
                listCardInPack.Clear();
            }
        }
    }


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

    public void UnLoadListDeckItemInStore()
    {
        if (listDeckItemInStore.Count > 0)
        {
            print("UnLoadListDeckItem");
            if (listDeckItemInStore[0] == null)
            {
                listDeckItemInStore.Clear();
            }
            else
            {
                foreach (DeckItem data in listDeckItemInStore)
                {
                    Destroy(data.gameObject);
                }
                listDeckItemInStore.Clear();
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

                //delete  optionDropdown
                Destroy(UIManager.instance.CollectionFriend.transform.GetChild(0).gameObject);
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
                    deckItem.text_DeckName.text = deckItem.Data.deckName;
                    print("LOAD DECK ITEM: " + deckItem.Data.deckName);
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
                card.transform.localPosition = Vector3.zero;
                //number card
              
            }
        }

        foreach (CardInInventory card in listCardInInventory)
        {
            CardInDeckPack cardInDeckPack = listCardInDeckPack.SingleOrDefault(a => a.CardItem == card.CardItem);
            if (cardInDeckPack != null)
            {
                print("if (cardInDeckPack != null)");
                card.NumberCard = card.CardItem.amount - cardInDeckPack.NumberCard;
            }
            else
            {
                print("if (cardInDeckPack -= null)");
                print(card.CardItem.amount);
                card.NumberCard = card.CardItem.amount;
            }
        }

        yield return null;


        //if (listCardInInventory[0].transform.parent != parent.transform)
        //{
        //    foreach (CardInInventory card in listCardInInventory)
        //    {
        //        card.gameObject.SetActive(true);
        //        card.transform.parent = parent.transform;
        //        card.transform.localScale = new Vector3(1f, 1f, 1f);

        //        card.transform.localPosition = Vector3.zero;
        //        //numer card
        //        card.NumberCard = card.CardItem.amount;
        //    }
        //}
        //yield return null;
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

    public IEnumerator LoadDeck() //load deck in store
    {
        yield return StartCoroutine(PlayfabManager.instance.GetStores(cataLog: "Card", storeId: "DS1"));
        yield return StartCoroutine(InitDeck());
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
                item.text_packName.text = item.Data.packName;
                item.price.text = item.Data.price;
            }
            Debug.Log("END LOAD PACK");
        }
        yield return null;
    }

    public IEnumerator LoadDeck(GameObject parent)
    {
        if (listDeckItem[0].transform.parent != parent.transform)
        {
            Debug.Log("START LOAD DECK");
            foreach (DeckItem item in listDeckItem)
            {
                item.gameObject.SetActive(true);
                item.transform.parent = parent.transform;
                item.transform.localScale = new Vector3(1f, 1f, 1f);
                item.transform.localPosition = new Vector3(0f, 0f, 0f);
            }
            Debug.Log("END LOAD DECK");
        }
        yield return null;
    }

    public IEnumerator LoadDeckInStore(GameObject parent)
    {
        if (listDeckItemInStore[0].transform.parent != parent.transform)
        {
            Debug.Log("START LOAD DECK IN STORE");
            foreach (DeckItem item in listDeckItemInStore)
            {
                item.gameObject.SetActive(true);
                item.transform.parent = parent.transform;
                item.transform.localScale = new Vector3(1f, 1f, 1f);
                item.transform.localPosition = new Vector3(0f, 0f, 0f);
                item.text_DeckName.text = item.Data.deckName;
                item.price.text = item.Data.price;
            }
            Debug.Log("END LOAD DECK IN STORE");
        }
        yield return null;
    }

    public IEnumerator LoadCardItemInGame()
    {
        yield return StartCoroutine(GetCardDatasFormResoure());
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
        PhotonManager.instance.HandlerFriendUpdate(GameData.instance.listFriendData);
        yield return StartCoroutine(LoadFriendItem(UIManager.instance.CollectionFriend));

    }

    public IEnumerator LoadFriendItem(GameObject parent)
    {
        print("LoadFriendItem");
        if (listFriendItem.Count > 0 && listFriendItem[0].transform.parent != parent.transform)
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

    public void SearchByText(string text)
    {
        if (UIManager.instance.isCollection_Cards || UIManager.instance.isStoreCards)
        {
            print("Call");
            List<CardInInventory> listShow = listCardInInventory.Where(a => a.CardItem.cardData.Name.ToLower().Contains(text.ToLower())).ToList();
            List<CardInInventory> listHide = listCardInInventory.Except(listShow).ToList();


            foreach (CardInInventory card in listShow)
            {
                card.gameObject.SetActive(true);
            }

            foreach (CardInInventory card in listHide)
            {
                card.gameObject.SetActive(false);
            }
        }
        else if (UIManager.instance.isCollection_Decks)
        {
            print("Call");
            List<DeckItem> listShow = listDeckItem.Where(a => a.text_DeckName.text.ToLower().Contains(text.ToLower())).ToList();
            List<DeckItem> listHide = listDeckItem.Except(listShow).ToList();


            foreach (DeckItem deck in listShow)
            {
                deck.gameObject.SetActive(true);
            }

            foreach (DeckItem deck in listHide)
            {
                deck.gameObject.SetActive(false);
            }
        }
        else if (UIManager.instance.isStoreDecks)
        {
            print("Call");
            List<DeckItem> listShow = listDeckItemInStore.Where(a => a.text_DeckName.text.ToLower().Contains(text.ToLower())).ToList();
            List<DeckItem> listHide = listDeckItemInStore.Except(listShow).ToList();


            foreach (DeckItem deck in listShow)
            {
                deck.gameObject.SetActive(true);
            }

            foreach (DeckItem deck in listHide)
            {
                deck.gameObject.SetActive(false);
            }
        }
        else if (UIManager.instance.isStorePacks)
        {
            print("Call");
            List<PackItem> listShow = listPackItem.Where(a => a.text_packName.text.ToLower().Contains(text.ToLower())).ToList();
            List<PackItem> listHide = listPackItem.Except(listShow).ToList();


            foreach (PackItem deck in listShow)
            {
                deck.gameObject.SetActive(true);
            }

            foreach (PackItem deck in listHide)
            {
                deck.gameObject.SetActive(false);
            }
        }
    }


    #region Get Set
    public int Elo
    {
        get { return _elo; }
        set { 
            
            _elo = value;
        }
    }

    public int Coin
    {
        get { return _coin; }
        set { _coin = value; }
    }
    #endregion
}
