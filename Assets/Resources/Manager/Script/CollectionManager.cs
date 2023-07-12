using Photon.Realtime;
using PlayFab.ClientModels;
using PlayFab.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class CollectionManager : MonoBehaviour
{

    public static CollectionManager instance;

    //[Header("Manager Data")]
    //[SerializeField] List<CardItem> gameCard = new List<CardItem>(); //all of card in the game

    ////UI
    //[SerializeField] List<CardInInventory> inventory = new List<CardInInventory>();
    //[SerializeField] List<CardInDeckPack> deck= new List<CardInDeckPack>();

    [Header("Prefab")]
    [SerializeField] GameObject CardItemPrefab;
    [SerializeField] GameObject CardInInventoryPrefab;
    [SerializeField] GameObject cardInDeckPackPrefab;


    [Header("Parent Tranform")]
    [SerializeField] GameObject gameObjetDeck;
    [SerializeField] GameObject gameObjectInventory;

    public int LimitNumberCardInDeck = 20;

    void Start()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("MonsterDataManager have 2");
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }

        this.RegisterListener(EventID.OnPutCardInDeck, (param) => PutCardInDeck((string)param));
        this.RegisterListener(EventID.OnRemoveCardInDeck, (param) => RemoveCardInDeck((string)param));
    }

    //public void loadData(string json)
    //{
    //    ///list Deck 


    //    //gen cardItem in game
    //    foreach (MonsterData monsterData in GameData.instance.listCardDataInGame)
    //    {
    //        CardItem item = GameObject.Instantiate(CardItemPrefab,Vector3.zero,Quaternion.identity).GetComponent<CardItem>();
    //        item.transform.parent = this.transform;

    //        item.cardData = monsterData; 
    //        gameCard.Add(item);
    //    }

    //    //load amount of card in inventory player 
    //    foreach (MonsterData monsterData in GameData.instance.listMonsterDataUser)
    //    {
    //        print(monsterData.id);

    //        CardItem item = gameCard.Single(a => a.cardData.id.Equals(monsterData.id));
    //        item.amount++;
    //    }

    //    //gen cardInventory form cardItem above
    //    foreach (CardItem cardItem in gameCard)
    //    {
    //        GameObject item = GameObject.Instantiate(CardInInventoryPrefab,Vector3.zero,Quaternion.identity);
    //        CardInInventory card = item.GetComponent<CardInInventory>();
    //        card.CardItem = cardItem;
    //        card.transform.parent = gameObjectInventory.transform;
    //        card.transform.localScale = new Vector3(1f, 1f, 1f);
    //        inventory.Add(card);
    //    }
    //}

    public void PutCardInDeck(string cardDataID)
    {
        //CardItem cardItem = GameData.instance.listCardItem.First(a => a.cardData.id.Equals(cardDataID));
        CardInInventory cardInventory = GameData.instance.listCardInInventory.Single(a => a.CardItem.cardData.Id == cardDataID);
        if (cardInventory.NumberCard > 0 && GameData.instance.getNumberCardInDeck() < LimitNumberCardInDeck && cardInventory != null)
        {
            CardInDeckPack cardInDeckPack = GameData.instance.listCardInDeckPack.SingleOrDefault(a => a.CardItem.cardData.Id == cardDataID);

            //deck do not have any this card before
            if (cardInDeckPack == null)
            {
                GameObject item = GameObject.Instantiate(cardInDeckPackPrefab, Vector3.zero, Quaternion.identity);
                CardInDeckPack card = item.GetComponent<CardInDeckPack>();
                card.CardItem = cardInventory.CardItem;
                card.NumberCard = 1;
                card.transform.parent = gameObjetDeck.transform;
                card.transform.localScale = new Vector3(1f, 1f, 1f);
                card.transform.localPosition = new Vector3(0f, 0f, 0f);
                GameData.instance.listCardInDeckPack.Add(card);

            }
            else
            {
                cardInDeckPack.NumberCard++;
            }

            cardInventory.NumberCard--;

            this.PostEvent(EventID.OnChangeNumberCardInDeck, GameData.instance.getNumberCardInDeck());

        }
    }

    public void RemoveCardInDeck(string cardDataID)
    {
        CardInDeckPack cardItemInDeck = GameData.instance.listCardInDeckPack.First(a => a.CardItem.cardData.Id.Equals(cardDataID));
        if (cardItemInDeck != null)
        {
            cardItemInDeck.NumberCard--;

            if (cardItemInDeck.NumberCard <= 0)
            {
                GameData.instance.listCardInDeckPack.Remove(cardItemInDeck);
                Destroy(cardItemInDeck.gameObject);
            }
            else
            {

            }

            CardInInventory cardInventory = GameData.instance.listCardInInventory.Single(a => a.CardItem.cardData.Id == cardDataID);
            cardInventory.NumberCard++;

            this.PostEvent(EventID.OnChangeNumberCardInDeck, GameData.instance.getNumberCardInDeck());

        }

    }

    public IEnumerator CreateDeckFromStore(Dictionary<string, int> dic, string deckName)
    {
        print($"149 {deckName}");
        string deckCode = "";
        foreach ((var key, var value) in dic)
        {
            deckCode += key + ":" + value + "%";
        }

        deckCode = deckCode.Substring(0, deckCode.Length - 1);

        Data_Deck newDeck;
        print("DeckName:" + UIManager.instance.DeckName + "|");

        if (string.IsNullOrEmpty(deckName))
        {
            newDeck = new Data_Deck(deckCode, "DeckName");
        }
        else
        {
            newDeck = new Data_Deck(deckCode, deckName);
        }
        print("DeckName:" + newDeck.deckName);

        List<Data_Deck> listNewDeck = GameData.instance.listDeck.listDeck.ToList();
        listNewDeck.Add(newDeck);

        string json = JsonHelper.ToJson(listNewDeck.ToArray()).Replace("Items", "listDeck");
        yield return StartCoroutine(PlayfabManager.instance.SetUserData("Decks", json));
        yield return StartCoroutine(GameData.instance.LoadDeckItems());
        UIManager.instance.TurnOnCollectionDeckScene();
        yield return null;
    }

    public IEnumerator CreateDeck()
    {
        var NumberCardInDeck = GameData.instance.getNumberCardInDeck();
        if (NumberCardInDeck == LimitNumberCardInDeck)
        {
            string deckCode = "";
            foreach (CardInDeckPack card in GameData.instance.listCardInDeckPack)
            {
                deckCode += card.CardItem.cardData.Id + ":" + card.NumberCard + "%";
            }
            deckCode = deckCode.Substring(0, deckCode.Length - 1);

            //create deck item
            Data_Deck newDeck;
            print("DeckName:" + UIManager.instance.DeckName + "|");

            if (UIManager.instance.DeckName.Equals(""))
            {
                newDeck = new Data_Deck(deckCode, "DeckName");
            }
            else
            {
                newDeck = new Data_Deck(deckCode, UIManager.instance.DeckName);
            }
            print("DeckName:" + newDeck.deckName);

            List<Data_Deck> listNewDeck = GameData.instance.listDeck.listDeck.ToList();
            listNewDeck.Add(newDeck);

            string json = JsonHelper.ToJson(listNewDeck.ToArray()).Replace("Items", "listDeck");
            yield return StartCoroutine(PlayfabManager.instance.SetUserData("Decks", json));
            yield return StartCoroutine(GameData.instance.LoadDeckItems());
            UIManager.instance.TurnOnCollectionDeckScene();
        }
        else
        {
            UIManager.instance.EnablePanelErrorMessage(true, "The number of cards in the deck must be " + LimitNumberCardInDeck);
            print(this.debug("NumberCardInDeck invalid", new
            {
                NumberCardInDeck
            }));
        }
    }

    public IEnumerator UpdateDeck()
    {
        if (GameData.instance.getNumberCardInDeck() == 20)
        {
            #region Create new Deck
            string deckCode = "";
            foreach (CardInDeckPack card in GameData.instance.listCardInDeckPack)
            {
                deckCode += card.CardItem.cardData.Id + ":" + card.NumberCard + "%";
            }
            deckCode = deckCode.Substring(0, deckCode.Length - 1);

            //create deck item
            Data_Deck newDeck;

            if (UIManager.instance.DeckName.Equals(""))
            {
                newDeck = new Data_Deck(deckCode, "DeckName");
            }
            else
            {
                newDeck = new Data_Deck(deckCode, UIManager.instance.DeckName);
            }
            #endregion

            #region Remove Odd Deck
            List<Data_Deck> listDataDeck = GameData.instance.listDeck.listDeck.ToList();
            listDataDeck.Remove(GameData.instance.selectDeck.Data);

            listDataDeck.Add(newDeck);
            string json = JsonHelper.ToJson(listDataDeck.ToArray()).Replace("Items", "listDeck");
            yield return StartCoroutine(PlayfabManager.instance.SetUserData("Decks", json));
            #endregion

            yield return StartCoroutine(GameData.instance.LoadDeckItems());

            UIManager.instance.TurnOnCollectionDeckScene();
            print("INIT DECK ITEM");

        }
        else
        {
            UIManager.instance.EnablePanelErrorMessage(true, "The number of cards in the deck must be " + LimitNumberCardInDeck);
        }
        //load list deck
    }

    public IEnumerator DeleteDeck()
    {
        List<Data_Deck> listDataDeck = GameData.instance.listDeck.listDeck.ToList();
        print(listDataDeck.Count);

        listDataDeck.Remove(GameData.instance.selectDeck.Data);

        print(listDataDeck);

        string json = JsonHelper.ToJson(listDataDeck.ToArray()).Replace("Items", "listDeck");
        yield return StartCoroutine(PlayfabManager.instance.SetUserData("Decks", json));
        yield return StartCoroutine(GameData.instance.LoadDeckItems());
        UIManager.instance.TurnOnCollectionDeckScene();
        print("INIT DECK ITEM");
    }

}
