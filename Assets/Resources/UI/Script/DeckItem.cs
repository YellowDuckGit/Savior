using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeckItem : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI text_DeckName;

    Data_Deck data;

    private string id = CommonFunction.getNewId();

    public string Id
    {
        get { return id; }
        set { id = value; }
    }

    public Data_Deck Data
    {
        get { return this.data; }
        set
        {
            this.id = Id;
            this.data = value;
            text_DeckName.text = this.data.deckName;
        }
    }

    public IEnumerator LoadDeckData()
    {
        if(data != null && data.deckItemsId.Count() > 0)
        {
            yield return StartCoroutine(UIManager.instance.ShowPopupDeckDetailed(this));
        }

        yield return null;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        //deckitem in Playscene
        if (UIManager.instance.isCollection_Decks) //DeckItem in collection
        {
            //clear and destroy odd list card in deck pack
            foreach (CardInDeckPack cardInDeckPack in GameData.instance.listCardInDeckPack)
            {
                Destroy(cardInDeckPack.gameObject);
            }
            GameData.instance.listCardInDeckPack.Clear();
            
            //select deck
            GameData.instance.selectDeck = this;
            UIManager.instance.DeckName = this.data.deckName;
            print(this.data.deckName);
            print("Selected Deck");

            if (data.ListCardID.Count > 0)
            {
                StartCoroutine(CreateDeck());
            }
        }else if (UIManager.instance.isChooseDeckPVF)
        {
            UIManager.instance.LoadSeletedDeck(this.transform,UIManager.instance.CollectionDeckPVF_PlayScene,UIManager.instance.SelectFramePVF);
        }
        else if(UIManager.instance.isChooseDeck)
        {
            UIManager.instance.LoadSeletedDeck(this.transform,UIManager.instance.CollectionDeck_PlayScene,UIManager.instance.SelectFrame);
        }
        else if(UIManager.instance.isStoreDecks)
        {
            StartCoroutine(LoadDeckData());
            print(data.deckName);
        }
    }

    public IEnumerator CreateDeck()
    {
        while (true)
        {
            yield return StartCoroutine(GameData.instance.LoadCardInDeckPack(data.ListCardID.ToList()));

            this.PostEvent(EventID.OnChangeDeckName);
            this.PostEvent(EventID.OnChangeNumberCardInDeck, GameData.instance.getNumberCardInDeck());
            UIManager.instance.TurnOnUpdateDeckScene();
            break;
        }
    }
}

[Serializable]
public class Data_Deck
{
    public string id;
    //field save in server
    public string deckCode;
    public string deckName;
    //

    public Data_Deck(string id)
    {
        this.id = id;
    }

    private List<string> listCardID = new List<string>();
    public List<string> deckItemsId = new List<string>();



    public List<string> ListCardID
    {
        get { return listCardID; }
        set { listCardID = value; }
    }
    public void setListCardItem()
    {
        ListCardID = deckCode.Split('%').ToList();
    }

    public Data_Deck(string deckCode, string deckName)
    {
        this.deckCode = deckCode;
        this.deckName = deckName;
    }

}

[Serializable]
public class ListDeck
{
    public Data_Deck[] listDeck;
}