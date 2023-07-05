
using Assets.GameComponent.Card.CardComponents.Script;
using Assets.GameComponent.Card.CardComponents.Script.UI;
using Assets.GameComponent.Card.Logic.Effect;
using Assets.GameComponent.Card.LogicCard;
using Assets.GameComponent.Card.LogicCard.ListLogic.Effect;
using Card;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using static EventGameHandler;

public class MonsterCard : CardBase, IMonsterCard, IEffectAttributes
{
    [Header("Reference Display")]
    private MonsterData _baseMonsterData;

    [Space(10)]
    [Header("Stats")]
    private int _attack;
    private int _hp;
    private int _defaultHp;

    [Space(10)]
    [Header("State Card")]

    //public CardPlayer cardPlayer;
    public Hand hand;
    public Deck deck;

    [SerializeField]
    public AbstractCondition[] LogicCard;


    //public void Awake()
    //{
    //    //Register();
    //    SetupMonsterCard();
    //}

    //public void Start()
    //{
    //    this.RegisterListener(EventID.OnStartRound, (match) => StartRound(match as MatchManager));
    //}

    public override void RegistLocalEvent()
    {
        this.RegisterListener(EventID.OnStartRound, (match) => StartRound(match as MatchManager));
        this.RegisterListener(EventID.OnEndRound, (param) => RevokeEffectAtEndTurn(param as MatchManager));
    }

    private void StartRound(MatchManager matchManager)
    {
        print("start round");
        if (IsCharming)
        {
            if (--charmingCount <= 0)
            {
                print("Revoke IsCharming");
                IsCharming = false;
                charmingCount = 0;
            };
        }

        if (IsTreating)
        {
            if (this.Hp < this.DefaultHp)
            {
                this.Hp += this.Hp;
                if (this.Hp > this.DefaultHp)
                {
                    this.Hp = this.DefaultHp;
                }
            }
        }
        print(this.debug(this.ToString(), new { IsCharming, charmingCount }));
    }

    #region Get Set

    private bool _isTreating;
    public bool IsTreating
    {
        get
        {
            return _isTreating;
        }
        set
        {
            if (value)
            {
                _isTreating = true;
            }
        }
    }

    public MonsterData BaseMonsterData
    {
        get { return this._baseMonsterData; }
        set { this._baseMonsterData = value; }
    }

    public override string Id
    {
        get => _id;
        set
        {
            if (!string.IsNullOrEmpty(_id))
                _id = value;
        }
    }
    public override string Name
    {
        get => _name; set
        {
            _name = value;
            this.PostEvent(EventID.OnCardUpdate, this);

            //onNameChange?.Invoke(value);
        }
    }
    public override string Description
    {
        get => _description; set
        {
            _description = value;
            this.PostEvent(EventID.OnCardUpdate, this);

            //onDescriptionChange?.Invoke(value);

        }
    }
    public override int Cost
    {
        get => _cost; set
        {
            _cost = value;
            this.PostEvent(EventID.OnCardUpdate, this);

            //onCostChange?.Invoke(value);
        }
    }
    public override bool IsSelected
    {
        get => _isSelected; set
        {
            _isSelected = value;
            this.PostEvent(EventID.OnCardSelected, this);
            //onSelectChange?.Invoke(value);
        }
    }
    public override CardPosition Position
    {
        get => _position; set
        {
            _position = value;
            //onPositionChange?.Invoke(value);
        }
    }
    public override Material NormalAvatar
    {
        get => _normalavatar; set
        {
            _normalavatar = value;
            this.PostEvent(EventID.OnCardUpdate, this);

            //onavatarChange?.Invoke(value);
        }
    }

    public override Material InBoardAvatar
    {
        get => _inBoardavatar; set
        {
            _inBoardavatar = value;
            this.PostEvent(EventID.OnCardUpdate, this);

            //onavatarChange?.Invoke(value);
        }
    }

    public override Material InDeckAvatar
    {
        get => _inDeckavatar; set
        {
            _inDeckavatar = value;
            this.PostEvent(EventID.OnCardUpdate, this);

            //onavatarChange?.Invoke(value);
        }
    }
    public int Attack
    {
        get { return this._attack; }
        set
        {
            this._attack = value;
            this.PostEvent(EventID.OnCardUpdate, this);

            //onAttackChange?.Invoke(value);
        }
    }
    public int Hp
    {
        get { return this._hp; }
        set
        {
            this._hp = value;
            //onHpChange?.Invoke(value);
            this.PostEvent(EventID.OnCardUpdate, this);
            if (IsCharming)
            {
                IsCharming = false;
            }
            if (isDead())
            {
                MoveToGraveyard();
            }
        }

    }

    public int DefaultHp
    {
        get { return this._defaultHp; }
        set
        {
            this._defaultHp = value;
        }
    }
    public override bool Forcusable
    {
        get => _forcusable; set
        {
            _forcusable = value;
        }
    }
    public override bool IsFocus
    {
        get =>/* _forcusable &&*/ _isFocus; set
        {
            _isFocus = value;
            //onFocusChange?.Invoke(value);
        }
    }
    public override CardOwner CardOwner
    {


        get { return _cardOwner; }
        set { _cardOwner = value; }
    }
    public override CardType CardType
    {
        get { return _cardType; }
        set { _cardType = value; }
    }
    public override bool IsSelectAble
    {
        get
        {
            return _isSelectAble;
        }
        set
        {
            _isSelectAble = value;
        }
    }

    public override CardPlayer CardPlayer
    {
        get
        {
            return _cardPlayer;
        }
        set
        {
            _cardPlayer = value;
        }
    }
    public int charmingCount = 0;
    private bool _IsCharming;
    public bool IsCharming
    {
        get { return _IsCharming; }
        set
        {
            if (value && Position == CardPosition.InSummonField)
            {
                charmingCount = 2;
                _IsCharming = value;
            }
            else
            {
                _IsCharming = false;
                charmingCount = 0;
            }
        }
    }

    public override Rarity RarityCard { get; set; }
    public override RegionCard RegionCard { get; set; }


    #endregion

    #region Raise Event
    //public override event OnNameChange onNameChange;
    //public override event OnCostChange onCostChange;
    //public override event OnDescriptionChange onDescriptionChange;
    //public override event OnPositionChange onPositionChange;
    //public override event OnSelectChange onSelectChange;
    //public override event OnavatarChange onavatarChange;
    //public override event OnFocusChange onFocusChange;
    //public event OnAttactChange onAttackChange;
    //public event OnPlay onPlay;
    //public event OnSummon onSummon;
    //public event OnHpChange onHpChange;
    //public event OnMoveToGraveyard onMoveToGraveyard;
    //public event OnMoveToFightZone onMoveToFightZone;
    //public event OnMoveToSummonZone onMoveToSummonZone;
    //public event OnSummonCard onSummonCard;
    //public event OnSummoned onSummonedCard;


    //private void OnEnable()
    //{
    //    PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
    //}


    //private void OnDisable()
    //{
    //    PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
    //}

    public override void NetworkingClient_EventReceived(EventData obj)
    {
        var args = obj.GetData();
        if ((RaiseEvent)obj.Code == RaiseEvent.SET_DATA_CARD_EVENT)
        {
            if (args.photonviewID.Equals(photonView.ViewID))
            {
                BaseMonsterData = (MonsterData)GameData.instance.listCardDataInGame.First(a => a.Id.Equals(args.cardDataId));
                LoadCardFromData();
            }
        }
        if ((RaiseEvent)obj.Code == RaiseEvent.UPDATE_DATA_MONSTER_EVENT)
        {
            if (args.photonviewID.Equals(photonView.ViewID))
            {
                var card = args.card as MonsterCard;
                this.Hp = card.Hp;
                this.Attack = card.Attack;
            }
        }
    }


    private void RevokeEffectAtEndTurn(MatchManager match)
    {
        this.EffectSContain.Where(eff => eff is IInturnEffect).ToList().ForEach(eff => eff.RevokeEffect(this, match: match));
    }

    #endregion

    #region SetUp Card
    public override void LoadCardFromData()
    {
        print($"Start Load Data for {_baseMonsterData.Name}");
        IMonsterData source = _baseMonsterData;

        this.Id = source.Id;
        this.Name = source.Name;
        this.Cost = source.Cost;
        this.Description = source.Description;
        this.CardType = source.CardType;
        this.NormalAvatar = source.NormalAvatar;
        this.InDeckAvatar = source.InDeckAvatar;
        this.InBoardAvatar = source.InBoardAvatar;
        this.RarityCard = source.RarityCard;
        this.RegionCard = source.RegionCard;
        this.Hp = source.Hp;
        this.Attack = source.Attack;

        IEffectAttributes effsource = _baseMonsterData;
        IEffectAttributes effDestination = this;

        effDestination.IsCharming = effsource.IsCharming;
        effDestination.IsTreating = effsource.IsTreating;

        //load data form monster data
        //this.Id = _baseMonsterData.Id;
        //this.Name = _baseMonsterData.Name;
        //this.Description = _baseMonsterData.Description;
        //this.Cost = _baseMonsterData.Cost;
        //this.Attack = _baseMonsterData.Attack;
        //this.Hp = _baseMonsterData.Hp;
        this.DefaultHp = _baseMonsterData.Hp; //default hp
        //this.avatar = _baseMonsterData.avatar;
        //this.IsCharming = _baseMonsterData.IsCharming;
        //this.isTreating = _baseMonsterData.IsTreating;
        if (_baseMonsterData.CardEffect != null)
        {
            this.LogicCard = _baseMonsterData.CardEffect;
            for (int i = 0; i < this.LogicCard.Length; i++)
            {
                EffectManager.Instance.EffectRegistor(this.LogicCard[i], this);
            }
        }
        print("End Load Data");
    }

    //public void updateCard()
    //{
    //    print($"Start Update Data for {_baseMonsterData.name}");
    //    //load data form monster data
    //    this.Id = _baseMonsterData.Id;
    //    this.Name = _baseMonsterData.name;
    //    this.Description = _baseMonsterData.Description;
    //    this.Cost = _baseMonsterData.Cost;
    //    this.Attack = _baseMonsterData.Attack;
    //    this.Hp = _baseMonsterData.Hp;
    //    this.avatar = _baseMonsterData.avatar;
    //    this.isTreating = _baseMonsterData.IsTreating;
    //    if (_baseMonsterData.CardEffect != null)
    //    {
    //        this.LogicCard = _baseMonsterData.CardEffect;
    //        for (int i = 0; i < this.LogicCard.Length; i++)
    //        {
    //            EffectManager.Instance.EffectRegistor(this.LogicCard[i], this);
    //        }
    //    }
    //    print("End Update Data");
    //}

    //public void ChangeProperties(int attack, int hp)
    //{
    //    Attack = attack;
    //    Hp = hp;
    //}

    //private void SetupMonsterCard()
    //{
    //    if (photonView.IsMine)
    //    {
    //        if (MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Red))
    //        {
    //            //get player, hand, deck
    //            GameObject side = GameObject.Find(MatchManager.instance.redSideGameObjectName);
    //            CardPlayer = side.transform.GetComponentInChildren<CardPlayer>();
    //            hand = CardPlayer.GetComponentInChildren<Hand>();
    //            deck = CardPlayer.GetComponentInChildren<Deck>();


    //            //becom children of deck
    //            this.transform.parent = deck.transform;

    //            //add card to list card in deck
    //            deck.Add(this);

    //            //set position
    //            this.transform.position = deck.PositionInitialCardInDeck;
    //            this.transform.Rotate(new Vector3(180f, 0f, 0f));



    //        }
    //        else if (MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Blue))
    //        {
    //            //get player, hand, deck
    //            GameObject side = GameObject.Find(MatchManager.instance.blueSideGameObjectName);
    //            CardPlayer = side.transform.GetComponentInChildren<CardPlayer>();
    //            hand = CardPlayer.GetComponentInChildren<Hand>();
    //            deck = CardPlayer.GetComponentInChildren<Deck>();


    //            //add card to list card in deck
    //            deck.Add(this);

    //            //becom children of deck
    //            this.transform.parent = deck.transform;

    //            //set position
    //            this.transform.position = deck.PositionInitialCardInDeck;
    //            this.transform.Rotate(new Vector3(180f, 0f, 0f));

    //        }
    //    }
    //    else
    //    {
    //        if (MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Red))
    //        {
    //            //set parent
    //            //get player, hand, deck
    //            GameObject side = GameObject.Find(MatchManager.instance.blueSideGameObjectName);
    //            CardPlayer = side.transform.GetComponentInChildren<CardPlayer>();
    //            hand = CardPlayer.GetComponentInChildren<Hand>();
    //            deck = CardPlayer.GetComponentInChildren<Deck>();

    //            //becom children of deck
    //            this.transform.parent = deck.transform;

    //            //add card to list card in deck
    //            deck.Add(this);

    //            //set position
    //            this.transform.position = deck.PositionInitialCardInDeck;
    //            this.transform.Rotate(new Vector3(180f, 0f, 0f));

    //        }
    //        else if (MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Blue))
    //        {
    //            //get player, hand, deck
    //            GameObject side = GameObject.Find(MatchManager.instance.redSideGameObjectName);
    //            CardPlayer = side.transform.GetComponentInChildren<CardPlayer>();
    //            hand = CardPlayer.GetComponentInChildren<Hand>();
    //            deck = CardPlayer.GetComponentInChildren<Deck>();

    //            //becom children of deck
    //            this.transform.parent = deck.transform;

    //            //add card to list card in deck
    //            deck.Add(this);

    //            //set position
    //            this.transform.position = deck.PositionInitialCardInDeck;
    //            this.transform.Rotate(new Vector3(180f, 0f, 0f));

    //        }
    //    }

    //}
    #endregion

    #region Actions

    public void attack(MonsterCard opponentCard)
    {
        opponentCard.Hp -= this.Attack;
        print(Hp);
    }

    public override void Play(MatchManager matchManager)
    {
        //onPlay?.Invoke();
    }

    public void TriggerSummoned()
    {
        print($"{this.ToString()} summoned");
        //onSummonedCard?.Invoke();
    }
    //click --> call funct --> switch cam --> wait click file --> click filed --> summon
    public void Summon(SummonZone summonField)
    {
        IsSelected = false;
        //onSummon?.Invoke(summonField, new ); // process in MatchManager

        //change card from hand to summon zone 

        //pay for cost
        //CardPlayer.mana.Number -= Cost; // process in MatchManager

        //reset skip turn
        MatchManager.instance.ResetSkipTurn();
    }

    public void MoveToFightZone(FightZone fightZone)
    {
        IsSelected = false;
        this.PostEvent(EventID.OnMoveToFightZone, new MoveToFightZoneArgs
        {
            sender = this,
            fightZone = fightZone
        });
        //onMoveToFightZone?.Invoke(fightZone);
    }

    public void MoveToSummonZone(FightZone fightZone, SummonZone summonZone)
    {
        print("MonsterCard>>MoveToSummonZone");
        IsSelected = false;
        this.PostEvent(EventID.OnMoveToSummonZone, new MoveToSummonZoneArgs
        {
            sender = this,
            fightZone = fightZone,
            summonZone = summonZone
        });
        //onMoveToSummonZone?.Invoke(fightZone, summonZone);
    }

    public void MoveToGraveyard()
    {
        this.PostEvent(EventID.OnMoveToGraveyard, new MoveToGraveyardArgs
        {
            sender = this
        });
        //onMoveToGraveyard?.Invoke();
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
    #endregion

    #region Funtional
    public bool isCanSummon(int manaPlayer)
    {
        return manaPlayer >= this.Cost;
    } // should check in game manager

    public bool isDead()
    {
        if (_hp <= 0)
        {
            return true;
        }
        else return false;
    }
    #endregion

    public void SelectCard()
    {
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

    public override string ToString()
    {
        return $"{Name}{{{Cost}}} {Attack}|{Hp}";
    }

    public override void OnClick()
    {
        print(this.debug(this.ToString()));
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            print("left Click card");
            //select 
            //IsSelected = true; 
            SelectCard();
            //if (Forcusable)
            IsFocus = true;
        }
        else if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            print(this.ToString());
        }
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


}

