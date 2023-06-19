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
            this.data = value;
            text_DeckName.text = this.data.deckName;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //deckitem in Playscene
        if (transform.parent.gameObject.name.Equals("DeckCollection")) //DeckItem in collection
        {
            //clear and destroy odd list card in deck pack
            foreach (CardInDeckPack cardInDeckPack in GameData.instance.listCardInDeckPack)
            {
                Destroy(cardInDeckPack.gameObject);
            }
            GameData.instance.listCardInDeckPack.Clear();

            //select deck
            GameData.instance.selectDeck = this;
            print("Selected Deck");

            if (data.ListCardID.Count > 0)
            {
                StartCoroutine(CreateDeck());
            }
        }
        else
        {
            //UIManager.instance.LoadSeletedDeck(this.transform);
        }
    }

    IEnumerator CreateDeck()
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
    //field save in server
    public string deckCode;
    public string deckName;
    //



    private List<string> listCardID = new List<string>();



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