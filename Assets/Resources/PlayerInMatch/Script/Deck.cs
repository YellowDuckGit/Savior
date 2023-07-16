using Assets.GameComponent.Card.CardComponents.Script;
using Assets.GameComponent.Card.Logic.Effect.CreateCard;
using Assets.GameComponent.Card.Logic.TargetObject.Target.PlayerTarget;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static K_Player;
using Random = System.Random;
[Serializable]
public class Deck : MonoBehaviourPun, IList<CardBase>, IPunObservable
{

    //list data get by id cards 
    [SerializeField] public List<ICardData> cardDatas = new List<ICardData>();

    //list monsters card in deck
    [SerializeField] private List<CardBase> _cards = new List<CardBase>();

    #region List Card function
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
        if(_cards.Count < 20)
        {
            _cards.Add(item);

            var photonID = item.photonView.ViewID;
            var index = _cards.IndexOf(item);
        }

        else
        {
            Debug.LogError(this.debug("Add to desk error", new
            {
                item,
                repeat = _cards.Contains(item)
            }));
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
    public void Insert(int index, CardBase item)
    {
        _cards.Insert(index, item);
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
        foreach(CardBase card in cards)
        {
            _cards.Add(card);
        }
    }

    public CardBase PeekWithPhoton(int photonID)
    {
        return _cards.Where(card => card != null && card.photonView.ViewID == photonID).FirstOrDefault();
    }



    //draw a top card
    public CardBase Draw()
    {
        // Check if the list is null or empty
        if(_cards == null || _cards.Count == 0)
        {
            //// Throw an exception or return null
            //throw new InvalidOperationException("The list of cards is empty.");
            return null;
        }
        // Otherwise, proceed as before
        CardBase card = _cards[0];
        _cards.RemoveAt(0);
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

    public List<CardBase> GetAll()
    {
        return _cards;
    }

    #endregion


    [Space(10)]
    [Header("Position Instantiate")]


    public bool finishCardDataLoad;

    public Vector3 offsetInstantiate;

    Vector3 positionTopDeck;
    private void Start()
    {
        finishCardDataLoad = true;

        offsetInstantiate = new Vector3(0f, 0.3f, 0f);
        positionTopDeck = transform.position; //postion will instantite
    }
    /// <summary>
    /// Initial for Desk in match
    /// </summary>
    /// <param name="deckCode"> a string type with Serialize a desk from player's collection</param>
    /// <returns></returns>
    public IEnumerator getCardDataInDeck(string deckCode)
    {
        //get all monster data in game
        List<ICardData> listMonsterDataInGame = GameData.instance.listCardDataInGame;

        //list id card in deck
        print("DeckCode : " + deckCode);
        List<string> listCardID = deckCode.Split('%').ToList();
        print("Desk before: \n" + string.Join(",", listCardID.ToArray()));
        if(listCardID != null)
        {
            listCardID.Shuffle();
        }
        print("Desk after: \n" + string.Join(",", listCardID.ToArray()));

        foreach(string str in listCardID)
        {
            if(!string.IsNullOrEmpty(str))
            {
                string[] arr = str.Split(':');
                string id = arr[0];
                int amount = Int32.Parse(arr[1]);

                for(int i = 0; i < amount; i++)
                {
                    ICardData data = listMonsterDataInGame.First(a => a.Id.Equals(id));
                    if(data != null)
                        cardDatas.Add(data);
                }
            }
        }
        yield return null;
    }
    /// <summary>
    /// Create 3d monster model card and raise photon event for 2 player set up for each card
    /// </summary>
    /// <returns></returns>
    public IEnumerator CreateMonsterCardsInDeckMatch()
    {
        cardDatas.Shuffle();

        foreach(var cardData in cardDatas)
        {
            if(cardData is IMonsterData monsterData)
            {
                var CreateMonster = new CreateCard
                {
                    CardPosition = CardPosition.InDeck,
                    CardTarget = (MonsterData)monsterData,
                    owner = CardOwner.You,
                };
                yield return StartCoroutine(EffectManager.Instance.ExecuteEffectEvent(CreateMonster, new PlayerTarget
                {
                    side = CreateMonster.owner
                }, MatchManager.instance.LocalPlayer));
            }
            else if(cardData is ISpellData)
            {
                var CreateSpell = new CreateCard
                {
                    CardPosition = CardPosition.InDeck,
                    CardTarget = (SpellData)cardData,
                    owner = CardOwner.You,
                };
                yield return StartCoroutine(EffectManager.Instance.ExecuteEffectEvent(CreateSpell, new PlayerTarget
                {
                    side = CreateSpell.owner
                }, MatchManager.instance.LocalPlayer));
            }
            else
            {
                print(this.debug("Not found type of card data"));
            }
            yield return new WaitUntil(() => MatchManager.instance.LocalPlayer.initialCardPlace.onReady());
        }

        yield return null;
    }
    //public IEnumerator genaratorCardInDeck()
    //{
    //    print("genaratorCardInDeck");


    //    //create monster card and instantite
    //    foreach (MonsterData monsterData in cardDatas)
    //    {
    //        MonsterCard monsterCard = PhotonNetwork.Instantiate("MonsterCard", Vector3.zero, Quaternion.identity).GetComponent<MonsterCard>();
    //        monsterCard.setMonsterData(monsterData);
    //        cards.Add(monsterCard);


    //        //monsterCard.transform.parent = this.transform;
    //        //monsterCard.setMonsterData(monsterData);
    //        //monsterCard.loadCardData();
    //        //positionInstantite = new Vector3(positionInstantite.x + offsetInstantiate.x, positionInstantite.y + offsetInstantiate.y, positionInstantite.z + offsetInstantiate.z);
    //    }

    //    //shuffle deck
    //    shuffleDeck();

    //    yield return null;
    //}

    //public void shuffleDeck()
    //{
    //    Debug.Log("shuffleDeck");
    //    for (int i = 0; i < _cards.Count; i++)
    //    {
    //        int randomIndex = UnityEngine.Random.Range(i, _cards.Count);
    //        MonsterCard temp = _cards[i];
    //        Vector3 tempTranform1 = _cards[i].gameObject.transform.localPosition;
    //        Vector3 tempTranform2 = _cards[randomIndex].gameObject.transform.localPosition;

    //        //Debug.Log("1B ,Index " + i + ", Postion: " + cards[i].gameObject.transform.localPosition.y);
    //        //Debug.Log("2B ,Index " + randomIndex + ", Postion: " + cards[randomIndex].gameObject.transform.localPosition.y);

    //        _cards[i] = _cards[randomIndex];
    //        _cards[i].gameObject.transform.localPosition = tempTranform1;

    //        _cards[randomIndex] = temp;
    //        _cards[randomIndex].gameObject.transform.localPosition = tempTranform2;
    //        //Debug.Log("1B " + i + ", Postion: " + cards[i].gameObject.transform.localPosition.y);
    //        //Debug.Log("2B " + randomIndex + ", Postion: " + cards[randomIndex].gameObject.transform.localPosition.y);

    //    }
    //}

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }

    public Vector3 PositionInitialCardInDeck
    {
        private set
        {
            positionTopDeck = value;
        }

        get
        {
            positionTopDeck = new Vector3(positionTopDeck.x + offsetInstantiate.x, positionTopDeck.y + offsetInstantiate.y, positionTopDeck.z + offsetInstantiate.z);
            return positionTopDeck;
        }
    }

    public bool Full
    {
        get
        {
            return _cards.Count == 20;
        }
    }
}
