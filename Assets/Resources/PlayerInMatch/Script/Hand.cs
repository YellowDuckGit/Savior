using Assets.GameComponent.Manager;
using Card;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;
using Random = System.Random;

public class Hand : MonoBehaviourPun, IList<CardBase>, IPunObservable
{
    private const int MaxholdNumber = 8;

    [SerializeField]
    private List<CardBase> _cards = new();

    [SerializeField]
    private Deck deck;

    [SerializeField]
    public Vector3 distanceBetweenCard;

    [SerializeField]
    public Vector3 rotationBetweenCard;

    [SerializeField]
    public Vector3 scaleBetweenCard;

    [Header("Camera")]
    [SerializeField] PlayerCamera camera;

    #region MyRegion
    // Implementing the Count property
    public int Count
    {
        get
        {
            return _cards.Count;
        }
    }

    // Implementing the IsReadOnly property
    public bool IsReadOnly
    {
        get
        {
            return false;
        }
    }

    // Implementing the indexer
    public CardBase this[int index]
    {
        get
        {
            return _cards[index];
        }
        set
        {
            _cards[index] = value;
        }
    }

    // Implementing the Add method
    public void Add(CardBase item)
    {
        Debug.Log(this.debug($"Add card to hand {item}", new
        {
            count = this.Count,
            max = MaxholdNumber
        }));
        if(this.Count < MaxholdNumber)
        {
            //SFX: DrawCard
            _cards.Add(item);
            SelectManager.Instance.CheckSelectAble(MatchManager.instance);
            item.Parents = this;
            CreateParentSortingForCard(item);
        }
        else
        {
            Debug.Log(this.debug($"Hand hold to reach max number card, discard {item}"));
            item.Discard();
        }

    }

    // Implementing the Clear method
    public void Clear()
    {
        _cards.Clear();
    }

    // Implementing the Contains method
    public bool Contains(CardBase item)
    {
        return _cards.Contains(item);
    }

    // Implementing the CopyTo method
    public void CopyTo(CardBase[] array, int arrayIndex)
    {
        _cards.CopyTo(array, arrayIndex);
    }

    // Implementing the GetEnumerator method
    public IEnumerator<CardBase> GetEnumerator()
    {
        return _cards.GetEnumerator();
    }

    // Implementing the IndexOf method
    public int IndexOf(CardBase item)
    {
        return _cards.IndexOf(item);
    }

    // Implementing the Insert method
    [Obsolete("This method is obsolete and should not be used.")]
    public void Insert(int index, CardBase item)
    {
        print("#error Insert method is not allowed in Hand class please use Add instead.");
    }

    //Insert random position
    public void InsertRandom(CardBase item)
    {
        Random rnd = new Random();
        int index = rnd.Next(0, _cards.Count);
        _cards.Insert(index, item);
        CreateParentSortingForCard(item);

    }

    // Implementing the Remove method
    public bool Remove(CardBase item)
    {
        RemoveParentSortingForCard(item);
        return _cards.Remove(item);
    }

    // Implementing the RemoveAt method
    public void RemoveAt(int index)
    {
        RemoveParentSortingForCard(_cards[index]);
        _cards.RemoveAt(index);
    }

    // Implementing the non-generic GetEnumerator method
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void AddRange(ICollection<CardBase> cards)
    {
        // Loop through the cards and add each one to the _cards field
        foreach(CardBase card in cards)
        {
            this.Add(card);
        }
    }

    public CardBase PeekWithPhoton(int photonID)
    {
        return _cards.Where(card => card != null && card.photonView.ViewID == photonID).FirstOrDefault();
    }

    public List<CardBase> GetAllCardSelected()
    {
        return _cards.Where(card => card != null && card.IsSelected).ToList();
    }

    //shuffle cards
    private void Shuffle(IList _list)
    {
        // Use a random number generator
        System.Random rng = new Random();
        // Loop through the list from the last element to the first
        for(int i = _list.Count - 1; i > 0; i--)
        {
            // Pick a random index between 0 and i
            int j = rng.Next(i + 1);
            // Swap the elements at i and j
            object temp = _list[i];
            _list[i] = _list[j];
            _list[j] = temp;
        }
    }

    //draw a top card
    public CardBase Draw()
    {
        CardBase card = _cards[0];
        _cards.RemoveAt(0);
        RemoveParentSortingForCard(card);
        return card;
    }
    //draw many card
    public List<CardBase> Draw(int count, bool random = false)
    {
        List<CardBase> cards = new List<CardBase>();
        if(random)
        {
            Random rnd = new Random();
            for(int i = 0; i < count; i++)
            {
                int j = rnd.Next(0, _cards.Count);
                CardBase card = _cards[j];
                _cards.RemoveAt(j);
                RemoveParentSortingForCard(card);
                cards.Add(card);
            }
        }
        else
        {
            for(int i = 0; i < count; i++)
            {
                CardBase card = _cards[0];
                _cards.RemoveAt(0);
                cards.Add(card);
                RemoveParentSortingForCard(card);
            }
        }
        return cards;
    }

    //draw card
    public CardBase Draw(int index)
    {
        CardBase card = _cards[index];
        _cards.RemoveAt(index);
        RemoveParentSortingForCard(card);
        return card;
    }

    //draw card
    public CardBase Draw(CardBase card)
    {
        _cards.Remove(card);
        RemoveParentSortingForCard(card);
        return card;
    }

    //draw card
    public CardBase Draw(string name)
    {
        CardBase card = _cards.Find(c => c.Name == name);
        _cards.Remove(card);
        RemoveParentSortingForCard(card);
        return card;
    }

    //draw card most expensive
    public CardBase DrawMostExpensive()
    {
        CardBase card = _cards.OrderByDescending(c => c.Cost).First();
        _cards.Remove(card);
        RemoveParentSortingForCard(card);

        return card;
    }

    //draw card Monster most powerful
    public CardBase DrawMostPowerfulMonster()
    {
        CardBase card = _cards.Where(c => c is MonsterCard).OrderByDescending(c => ((MonsterCard)c).Attack).First();
        _cards.Remove(card);
        RemoveParentSortingForCard(card);

        return card;
    }


    #endregion

    #region Raise Event
    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
    }

    private void NetworkingClient_EventReceived(EventData obj)
    {
        if((RaiseEvent)obj.Code == RaiseEvent.DRAW_CARD_EVENT)
        {
            object[] datas = (object[])obj.CustomData;
            int amount = (int)datas[0];
            int photonViewID = (int)datas[1];
            CardPlayer player = this.gameObject.GetComponentInParent<CardPlayer>();
            if(photonViewID.Equals(player.photonView.ViewID))
            {
                for(int i = 0; i < amount; i++)
                {
                    DrawCard();
                }
            }
        }
    }
    #endregion

    #region Card
    void DrawCard()
    {
        var card = deck.Draw();
        if(card != null)
        {
            //card.Position = CardPosition.InHand;
            this.Add(card);
            //CreateParentSortingForCard(card);
            //card.gameObject.transform.parent = this.gameObject.transform;
        }
    }

    public void RemoveCardFormHand(MonsterCard card)
    {
        _cards.Remove(card);
        RemoveParentSortingForCard(card);
        SelectManager.Instance.CheckSelectAble(MatchManager.instance);
    }
    #endregion

    public void CreateParentSortingForCard(CardBase Card)
    {
        GameObject parentCard = new GameObject();
        parentCard.transform.parent = this.gameObject.transform;

        Card.transform.parent = parentCard.transform;
        Card.transform.position = Vector3.zero;
        Card.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    public void RemoveParentSortingForCard(CardBase Card)
    {
        GameObject parentCard = Card.transform.parent.gameObject;
        Card.transform.parent = null;
        GameObject.Destroy(parentCard);
    }
    #region GET SET
    public List<CardBase> GetAllCardInHand()
    {
        return _cards;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }

    #endregion
}
