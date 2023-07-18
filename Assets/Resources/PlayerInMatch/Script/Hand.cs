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
    private int MaxholdNumber;

    //[SerializeField]
    public List<CardBase> _cards = new();

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
        get { return _cards.Count; }
    }

    // Implementing the IsReadOnly property
    public bool IsReadOnly
    {
        get { return false; }
    }

    // Implementing the indexer
    public CardBase this[int index]
    {
        get { return _cards[index]; }
        set { _cards[index] = value; }
    }

    // Implementing the Add method
    public void Add(CardBase item)
    {
        _cards.Add(item);
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
    }

    // Implementing the Remove method
    public bool Remove(CardBase item)
    {
        return _cards.Remove(item);
    }

    // Implementing the RemoveAt method
    public void RemoveAt(int index)
    {
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
        foreach (CardBase card in cards)
        {
            _cards.Add(card);
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
        for (int i = _list.Count - 1; i > 0; i--)
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
        return card;
    }
    //draw many card
    public List<CardBase> Draw(int count, bool random = false)
    {
        List<CardBase> cards = new List<CardBase>();
        if (random)
        {
            Random rnd = new Random();
            for (int i = 0; i < count; i++)
            {
                int j = rnd.Next(0, _cards.Count);
                CardBase card = _cards[j];
                _cards.RemoveAt(j);
                cards.Add(card);
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                CardBase card = _cards[0];
                _cards.RemoveAt(0);
                cards.Add(card);
            }
        }
        return cards;
    }

    //draw card
    public CardBase Draw(int index)
    {
        CardBase card = _cards[index];
        _cards.RemoveAt(index);
        return card;
    }

    //draw card
    public CardBase Draw(CardBase card)
    {
        _cards.Remove(card);
        return card;
    }

    //draw card
    public CardBase Draw(string name)
    {
        CardBase card = _cards.Find(c => c.Name == name);
        _cards.Remove(card);
        return card;
    }

    //draw card most expensive
    public CardBase DrawMostExpensive()
    {
        CardBase card = _cards.OrderByDescending(c => c.Cost).First();
        _cards.Remove(card);
        return card;
    }

    //draw card Monster most powerful
    public CardBase DrawMostPowerfulMonster()
    {
        CardBase card = _cards.Where(c => c is MonsterCard).OrderByDescending(c => ((MonsterCard)c).Attack).First();
        _cards.Remove(card);
        return card;
    }


    #endregion

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Mouse0))
        //{
        //    if (photonView.IsMine)
        //    {
        //        testRaiseEvent();
        //    }
        //}
    }
    public void clickLeftMouse()
    {
        //if (CameraSwitcher.IsActiveCamera(camera.lookCardCamera))
        //{
        //    //Check Select status card in hand
        //    CardTarget cardTarget = monsterCardsInHand.FirstOrDefault(a => a.cardTarget.isSelected == true).cardTarget;
        //    if (cardTarget != null)
        //    {
        //        if (!cardTarget.isSelectedCard)
        //        {
        //            cardTarget.UnSelectCard();
        //            cardTarget.isSelected = false;
        //        }
        //        else
        //        {
        //            cardTarget.isSelectedCard = false;
        //        }
        //    }
        //}
        //else if (CameraSwitcher.IsActiveCamera(camera.lookFieldCamera))
        //{
        //    //click summon
        //}
    }

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
        if ((RaiseEvent)obj.Code == RaiseEvent.DRAW_CARD_EVENT)
        {
            object[] datas = (object[])obj.CustomData;
            int amount = (int)datas[0];
            int photonViewID = (int)datas[1];
            CardPlayer player = this.gameObject.GetComponentInParent<CardPlayer>();
            if (photonViewID.Equals(player.photonView.ViewID))
            {
                for (int i = 0; i < amount; i++)
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
        if (card != null)
        {
            _cards.Add(card);
            card.Position = CardPosition.InHand;
            card.gameObject.transform.parent = this.gameObject.transform;
            if (_cards.Count > 0)
            {
                ScaleCardInHand();
                SortPostionRotationCardInHand();
            }
        }
        //int indexTopCardDeck = deck.cards.Count - 1;
        //if (indexTopCardDeck >= 0)
        //{
        //    MonsterCard monsterCard = deck._cards[indexTopCardDeck];

        //    monsterCardsInHand.Add(monsterCard);

        //    monsterCard.Position = CardPosition.InHand;

        //    //set parent = hand
        //    deck._cards[indexTopCardDeck].gameObject.transform.parent = this.gameObject.transform;

        //    deck.cards.RemoveAt(indexTopCardDeck);

        //    #region Postion & Rotation

        //    if (monsterCardsInHand.Count > 0)
        //    {
        //        ScaleCardInHand();
        //        SortPostionRotationCardInHand();
        //    }
        //    //rotation 
        //    #endregion
        //}
    }

    public void SortPostionRotationCardInHand()
    {
        int numberCardInHand = _cards.Count;

        if (numberCardInHand > 1)
        {
            //so card chan
            if (numberCardInHand % 2 == 0)
            {
                //case card chan co 2 card center
                int centerElement1 = numberCardInHand / 2;
                int centerElement2 = centerElement1 + 1;

                int indexE1 = centerElement1 - 1;
                int indexE2 = centerElement2 - 1;

                //set center postion card
                _cards[indexE1].gameObject.transform.localPosition = new Vector3(-distanceBetweenCard.x / 2, 0f, 0f);
                _cards[indexE2].gameObject.transform.localPosition = new Vector3(distanceBetweenCard.x / 2, 0f, 0f);

                //set roation center card
                _cards[indexE1].gameObject.transform.localRotation = Quaternion.EulerRotation(rotationBetweenCard.x, -rotationBetweenCard.y / 2, -rotationBetweenCard.z / 2);
                _cards[indexE2].gameObject.transform.localRotation = Quaternion.EulerRotation(rotationBetweenCard.x, rotationBetweenCard.y / 2, rotationBetweenCard.z / 2);

                //set left side postion of cards
                for (int i = indexE1 - 1; i >= 0; i--)
                {
                    Vector3 previousCardPosition = _cards[i + 1].gameObject.transform.localPosition;
                    _cards[i].gameObject.transform.localPosition = new Vector3(previousCardPosition.x - distanceBetweenCard.x, previousCardPosition.y - distanceBetweenCard.y, previousCardPosition.z + distanceBetweenCard.z);

                    Quaternion previousCardRoatation = _cards[i + 1].gameObject.transform.localRotation;
                    _cards[i].gameObject.transform.localRotation = Quaternion.EulerRotation(previousCardRoatation.x - rotationBetweenCard.x, previousCardRoatation.y - rotationBetweenCard.y, previousCardRoatation.z - rotationBetweenCard.z);
                }

                //set right side postion of cards
                for (int i = indexE2 + 1; i <= numberCardInHand - 1; i++)
                {
                    Vector3 behideCardPosition = _cards[i - 1].gameObject.transform.localPosition;
                    _cards[i].gameObject.transform.localPosition = new Vector3(behideCardPosition.x + distanceBetweenCard.x, behideCardPosition.y + distanceBetweenCard.y, behideCardPosition.z + distanceBetweenCard.z);

                    Quaternion previousCardRoatation = _cards[i - 1].gameObject.transform.localRotation;
                    _cards[i].gameObject.transform.localRotation = Quaternion.EulerRotation(previousCardRoatation.x + rotationBetweenCard.x, previousCardRoatation.y + rotationBetweenCard.y, previousCardRoatation.z + rotationBetweenCard.z);
                }
            }
            else //so card le
            {
                int centerElement = (int)((numberCardInHand / 2) + 1);


                int indexCenterElement = centerElement - 1;

                //set center postion card
                _cards[indexCenterElement].gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);

                //set roation center card
                _cards[indexCenterElement].gameObject.transform.localRotation = Quaternion.identity;

                //set left side postion of cards
                for (int i = indexCenterElement - 1; i >= 0; i--)
                {
                    Vector3 previousCardPosition = _cards[i + 1].gameObject.transform.localPosition;
                    _cards[i].gameObject.transform.localPosition = new Vector3(previousCardPosition.x - distanceBetweenCard.x, previousCardPosition.y - distanceBetweenCard.y, previousCardPosition.z + distanceBetweenCard.z);

                    Quaternion previousCardRoatation = _cards[i + 1].gameObject.transform.localRotation;
                    _cards[i].gameObject.transform.localRotation = Quaternion.EulerRotation(previousCardRoatation.x - rotationBetweenCard.x, previousCardRoatation.y - rotationBetweenCard.y, previousCardRoatation.z - rotationBetweenCard.z);
                }

                //set right side postion of cards
                for (int i = indexCenterElement + 1; i <= numberCardInHand - 1; i++)
                {
                    Vector3 behideCardPosition = _cards[i - 1].gameObject.transform.localPosition;
                    _cards[i].gameObject.transform.localPosition = new Vector3(behideCardPosition.x + distanceBetweenCard.x, behideCardPosition.y + distanceBetweenCard.y, behideCardPosition.z + distanceBetweenCard.z);

                    Quaternion previousCardRoatation = _cards[i - 1].gameObject.transform.localRotation;
                    _cards[i].gameObject.transform.localRotation = Quaternion.EulerRotation(previousCardRoatation.x + rotationBetweenCard.x, previousCardRoatation.y + rotationBetweenCard.y, previousCardRoatation.z + rotationBetweenCard.z);
                }
            }
        }
        else
        {
            if (_cards.Count >= 1)
            {
                _cards[numberCardInHand - 1].gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
                _cards[numberCardInHand - 1].gameObject.transform.localRotation = Quaternion.identity;
            }
        }

        if (_cards.Count >= 1)
        {
            //sort Y hand card
            _cards[0].gameObject.transform.localPosition = new Vector3(_cards[0].gameObject.transform.localPosition.x, distanceBetweenCard.y, _cards[0].gameObject.transform.localPosition.z);
            for (int i = 1; i <= numberCardInHand - 1; i++)
            {
                Vector3 previousCardPosition = _cards[i - 1].gameObject.transform.localPosition;
                _cards[i].gameObject.transform.localPosition = new Vector3(_cards[i].gameObject.transform.localPosition.x, previousCardPosition.y + distanceBetweenCard.y, _cards[i].gameObject.transform.localPosition.z);

                //Quaternion previousCardRoatation = monsterCardsInHand[i - 1].gameObject.transform.localRotation;
                //monsterCardsInHand[i].gameObject.transform.localRotation = Quaternion.EulerRotation(previousCardRoatation.x + rotationBetweenCard.x, previousCardRoatation.y + rotationBetweenCard.y, previousCardRoatation.z + rotationBetweenCard.z);

            }
        }

    }

    public void ScaleCardInHand()
    {
        for (int i = 0; i <= _cards.Count - 1; i++)
        {
            _cards[i].transform.localScale = scaleBetweenCard;
        }
    }

    public void RemoveCardFormHand(MonsterCard card)
    {
        _cards.Remove(card);
    }
    #endregion

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
