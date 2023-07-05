using Assets.GameComponent.Card.CardComponents.Script;
using Assets.GameComponent.Card.Logic.Effect;
using Assets.GameComponent.Card.LogicCard;
using Card;
using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpellCard : CardBase, ISpellCard
{
    [Header("Reference Display")]
    private SpellData _baseSpellData;
    private SpellType _spellType;

    [Space(10)]
    [Header("State Card")]

    //public CardPlayer cardPlayer;
    public Hand hand;
    public Deck deck;

    [SerializeField]
    public AbstractCondition[] LogicCard;

    public override string Id { get { return _id; } set { this._id = value; } }
    public override string Name { get { return _name; } set { this._name = value; this.PostEvent(EventID.OnCardUpdate, this); } }
    public override string Description { get { return _description; } set { this._description = value; this.PostEvent(EventID.OnCardUpdate, this); } }
    public override int Cost { get { return _cost; } set { this._cost = value; this.PostEvent(EventID.OnCardUpdate, this); } }
    public override bool IsSelectAble { get { return _isSelectAble; } set { this._isSelectAble = value; } }
    public override bool IsSelected { get { return _isSelected; } set { this._isSelected = value; } }
    public override bool Forcusable { get { return _forcusable; } set { this._forcusable = value; } }
    public override bool IsFocus { get { return _isFocus; } set { this._isFocus = value; } }
    public override CardPosition Position { get { return _position; } set { this._position = value; } }
    public override CardOwner CardOwner { get { return _cardOwner; } set { this._cardOwner = value; } }
    public override CardType CardType { get { return _cardType; } set { this._cardType = value; } }
    public override CardPlayer CardPlayer { get { return _cardPlayer; } set { this._cardPlayer = value; } }
    public override Material NormalAvatar { get { return _normalavatar; } set { this._normalavatar = value; this.PostEvent(EventID.OnCardUpdate, this); } }
    public override Material InDeckAvatar { get { return _normalavatar; } set { this._normalavatar = value; this.PostEvent(EventID.OnCardUpdate, this); } }
    public override Material InBoardAvatar { get { return _normalavatar; } set { this._normalavatar = value; this.PostEvent(EventID.OnCardUpdate, this); } }
    public SpellData BaseSpellData { get { return _baseSpellData; } set { this._baseSpellData = value; } }
    public override Rarity RarityCard { get { return _rarityCard; } set { this._rarityCard = value; } }
    public override RegionCard RegionCard { get { return _regionCard; } set { this._regionCard = value; } }
    public SpellType SpellType { get { return _spellType; } set { this._spellType = value; } }


    public override void OnClick()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            print("left Click card");
            print(this.debug(this.ToString(), new { _isSelectAble }));

            if (_isSelectAble)
            {

                if (this.Position == CardPosition.InHand)
                {
                    print("Card On hand");

                    if (hand == null) hand = gameObject.GetComponentInParent<Hand>();
                    print(IsSelected ? $"this {Name} have been selected" : $"this {Name} dosen't have been selected");
                    if (!IsSelected)
                    {
                        print("Card doesn't select yet");
                        var listMonsterCard = hand.GetAllCardInHand(); //get all monster card in hand
                                                                       //var listUIMonsterCard = listSelect(a => (a.UI)).ToList();//Get UI list

                        var cardSelected = listMonsterCard.SingleOrDefault(a => a.IsSelected == true);//TODO: select card selected-- this true on select 0ne //get card selected

                        if (cardSelected != null) //remove select
                        {
                            cardSelected.IsSelected = false;
                            print($"Card {cardSelected.Name} be unselect");
                        }
                        this.IsSelected = true;
                    }
                    //else //bam lai mot lan nua thi no van focus nhung khong select la sai
                    //{
                    //    this.IsSelected = false;
                    //    print($"Card {Name} have been unselected");
                    //}
                }
                else if (Position == CardPosition.InSummonField || Position == CardPosition.InFightField)
                {
                    print(this.debug("Select card on field"));

                    //check if in pharse atk or defense --> user can select card in summon field to move this in action file
                    if (!this.IsSelected)
                    {
                        this.IsSelected = true; //co the select nhieu monstercard trong 2 field nay 
                    }
                    else
                    {
                        this.IsSelected = false;
                    }
                }
            }
            print($"{this.Name} card can't be select");
        }
        else if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            print(this.ToString());
        }
    }

    public override void Play(MatchManager matchManager)
    {
    }

    public override void SetupCard()
    {
        if (photonView.IsMine)
        {
            if (MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Red))
            {
                //get player, hand, deck
                GameObject side = GameObject.Find(MatchManager.instance.redSideGameObjectName);
                CardPlayer = side.transform.GetComponentInChildren<CardPlayer>();
                hand = CardPlayer.GetComponentInChildren<Hand>();
                deck = CardPlayer.GetComponentInChildren<Deck>();


                //becom children of deck
                this.transform.parent = deck.transform;

                //add card to list card in deck
                deck.Add(this);

                //set position
                this.transform.position = deck.PositionInitialCardInDeck;
                this.transform.Rotate(new Vector3(180f, 0f, 0f));



            }
            else if (MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Blue))
            {
                //get player, hand, deck
                GameObject side = GameObject.Find(MatchManager.instance.blueSideGameObjectName);
                CardPlayer = side.transform.GetComponentInChildren<CardPlayer>();
                hand = CardPlayer.GetComponentInChildren<Hand>();
                deck = CardPlayer.GetComponentInChildren<Deck>();


                //add card to list card in deck
                deck.Add(this);

                //becom children of deck
                this.transform.parent = deck.transform;

                //set position
                this.transform.position = deck.PositionInitialCardInDeck;
                this.transform.Rotate(new Vector3(180f, 0f, 0f));

            }
        }
        else
        {
            if (MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Red))
            {
                //set parent
                //get player, hand, deck
                GameObject side = GameObject.Find(MatchManager.instance.blueSideGameObjectName);
                CardPlayer = side.transform.GetComponentInChildren<CardPlayer>();
                hand = CardPlayer.GetComponentInChildren<Hand>();
                deck = CardPlayer.GetComponentInChildren<Deck>();

                //becom children of deck
                this.transform.parent = deck.transform;

                //add card to list card in deck
                deck.Add(this);

                //set position
                this.transform.position = deck.PositionInitialCardInDeck;
                this.transform.Rotate(new Vector3(180f, 0f, 0f));

            }
            else if (MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Blue))
            {
                //get player, hand, deck
                GameObject side = GameObject.Find(MatchManager.instance.redSideGameObjectName);
                CardPlayer = side.transform.GetComponentInChildren<CardPlayer>();
                hand = CardPlayer.GetComponentInChildren<Hand>();
                deck = CardPlayer.GetComponentInChildren<Deck>();

                //becom children of deck
                this.transform.parent = deck.transform;

                //add card to list card in deck
                deck.Add(this);

                //set position
                this.transform.position = deck.PositionInitialCardInDeck;
                this.transform.Rotate(new Vector3(180f, 0f, 0f));

            }
        }
    }

    public override void RegistLocalEvent()
    {
    }

    public override void NetworkingClient_EventReceived(EventData obj)
    {
        var args = obj.GetData();
        if ((RaiseEvent)obj.Code == RaiseEvent.SET_DATA_CARD_EVENT)
        {
            if (args.photonviewID.Equals(photonView.ViewID))
            {
                BaseSpellData = (SpellData)GameData.instance.listCardDataInGame.First(a => a.Id.Equals(args.cardDataId));
                LoadCardFromData();
            }
        }
    }

    public override void LoadCardFromData()
    {
        print($"Start Load Data for {_baseSpellData.Name}");
        //load data form monster data
        ISpellData source = _baseSpellData;
        ISpellData destination = this;

        destination.Id = source.Id;
        destination.Name = source.Name;
        destination.Cost = source.Cost;
        destination.Description = source.Description;
        destination.CardType = source.CardType;
        destination.NormalAvatar = source.NormalAvatar;
        destination.InDeckAvatar = source.InDeckAvatar;
        destination.InBoardAvatar = source.InBoardAvatar;
        destination.RarityCard = source.RarityCard;
        destination.RegionCard = source.RegionCard;
        //this.Id = _baseSpellData.Id;
        //this.Name = _baseSpellData.name;
        //this.Description = _baseSpellData.Description;
        //this.Cost = _baseSpellData.Cost;
        //this.avatar = _baseSpellData.avatar;
        if (_baseSpellData.CardEffect != null)
        {
            this.LogicCard = _baseSpellData.CardEffect;
            for (int i = 0; i < this.LogicCard.Length; i++)
            {
                EffectManager.Instance.EffectRegistor(this.LogicCard[i], this);
            }
        }
        print("End Load Data");
    }
    public void RemoveCardFormParentPresent()
    {
        this.transform.parent = null;
        this.gameObject.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
    }
    public void MoveCardIntoNewParent(Transform parentTransform)
    {
        this.transform.parent = parentTransform;
        this.gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);

        if (MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Red))
            transform.Rotate(0f, 180f, 0f);
    }
}
