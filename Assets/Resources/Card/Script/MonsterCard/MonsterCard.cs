
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
using System.ComponentModel;
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

    [Space(10)]
    [Header("Stats")]
    [SerializeField] private int _attack;
    [SerializeField] private int _hp;
    [SerializeField] private int _defaultHp;

    [Space(10)]
    [Header("State Card")]

    //public CardPlayer cardPlayer;
    public Hand Hand;
    public Deck Deck;

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
        if(IsCharming)
        {
            if(--charmingCount <= 0)
            {
                print("Revoke IsCharming");
                IsCharming = false;
                charmingCount = 0;
            };
        }

        if(IsTreating)
        {
            if(this.Hp < this.DefaultHp)
            {
                this.Hp += this.Hp;
                if(this.Hp > this.DefaultHp)
                {
                    this.Hp = this.DefaultHp;
                }
            }
        }
        print(this.debug(this.ToString(), new
        {
            IsCharming,
            charmingCount
        }));
    }

    #region Get Set

    [SerializeField] private bool _isTreating;
    public bool IsTreating
    {
        get
        {
            return _isTreating;
        }
        set
        {
            if(value)
            {
                _isTreating = true;
            }
        }
    }



    public override string Id
    {
        get => _id;
        set
        {
            if(!string.IsNullOrEmpty(_id))
                _id = value;
        }
    }
    public override string Name
    {
        get => _name; set
        {
            _name = value;
            OnPropertyChanged(nameof(Name));
            //this.PostEvent(EventID.OnCardUpdate, this);

            //onNameChange?.Invoke(value);
        }
    }
    public override string Description
    {
        get => _description; set
        {
            _description = value;
            OnPropertyChanged(nameof(Description));

            //this.PostEvent(EventID.OnCardUpdate, this);

            //onDescriptionChange?.Invoke(value);

        }
    }
    public override int Cost
    {
        get => _cost; set
        {
            _cost = value;
            OnPropertyChanged(nameof(Cost));

            //this.PostEvent(EventID.OnCardUpdate, this);

            //onCostChange?.Invoke(value);
        }
    }
    public override bool IsSelected
    {
        get => _isSelected; set
        {
            _isSelected = value;
            if(value)
            {
                this.PostEvent(EventID.OnObjectSelected, this);
            }
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
    public override Material Avatar
    {
        get => _avartar; set
        {
            _avartar = value;
            OnPropertyChanged(nameof(Avatar));

            //this.PostEvent(EventID.OnCardUpdate, this);

            //onAvartarChange?.Invoke(value);
        }
    }
    public int Attack
    {
        get
        {
            return this._attack;
        }
        set
        {
            this._attack = value;
            OnPropertyChanged(nameof(Attack));

            //this.PostEvent(EventID.OnCardUpdate, this);

            //onAttackChange?.Invoke(value);
        }

    }
    public int Hp
    {
        get
        {
            return this._hp;
        }
        set
        {
            if(value < this._hp)
            {
                this.PostEvent(EventID.OnCardDamaged, this);
            }
            this._hp = value;
            //onHpChange?.Invoke(value);
            OnPropertyChanged(nameof(Hp));

            //this.PostEvent(EventID.OnCardUpdate, this);
            if(IsCharming)
            {
                IsCharming = false;
            }
            if(isDead())
            {
                MoveToGraveyard();
            }
        }

    }

    public int DefaultHp
    {
        get
        {
            return this._defaultHp;
        }
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
        get
        {
            return _cardOwner;
        }
        set
        {
            _cardOwner = value;
        }
    }
    public override CardType CardType
    {
        get
        {
            return _cardType;
        }
        set
        {
            _cardType = value;
        }
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
    [SerializeField]
    private bool _IsCharming;

    public bool IsCharming
    {
        get
        {
            return _IsCharming;
        }
        set
        {
            if(value /*&& Position == CardPosition.InSummonField*/)
            {
                charmingCount = 2;
                _IsCharming = true;
                //this.PostEvent(EventID.OnGainedAttribute, nameof(IsCharming));
            }
            else
            {
                _IsCharming = false;
                charmingCount = 0;
            }
        }
    }

    public override Rarity RarityCard
    {
        get; set;
    }
    public override RegionCard RegionCard
    {
        get; set;
    }
    [SerializeField]
    private bool _IsDominating;
    public bool IsDominating
    {
        get
        {
            return _IsDominating;
        }
        set
        {
            _IsDominating = value;
        }
    }
    public bool IsBlockAttack
    {
        get; set;
    }
    public bool IsBlockDefend
    {
        get; set;
    }

    #endregion

    #region Raise Event
    //public override event OnNameChange onNameChange;
    //public override event OnCostChange onCostChange;
    //public override event OnDescriptionChange onDescriptionChange;
    //public override event OnPositionChange onPositionChange;
    //public override event OnSelectChange onSelectChange;
    //public override event OnAvartarChange onAvartarChange;
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

    private void RevokeEffectAtEndTurn(MatchManager match)
    {
        this.EffectSContain.Where(eff => eff is IInturnEffect).ToList().ForEach(eff => eff.RevokeEffect(this, match: match));
    }

    #endregion

    #region SetUp Card
    public override IEnumerator LoadCardFromData()
    {
        print($"Start Load Data for {BaseCard.Name}");
        MonsterData source = (MonsterData)BaseCard;

        this.Id = source.Id;
        this.Name = source.Name;
        this.Cost = source.Cost;
        this.Description = source.Description;
        this.CardType = source.CardType;
        this.Avatar = source.Avatar;
        this.RarityCard = source.RarityCard;
        this.RegionCard = source.RegionCard;
        this.Hp = source.Hp;
        this.Attack = source.Attack;

        IEffectAttributes effsource = source;
        IEffectAttributes effDestination = this;

        effDestination.IsCharming = effsource.IsCharming;
        effDestination.IsTreating = effsource.IsTreating;
        effDestination.IsDominating = effsource.IsDominating;
        effDestination.IsBlockAttack = effsource.IsBlockAttack;
        effDestination.IsBlockDefend = effsource.IsBlockDefend;
        //load data form monster data
        //this.Id = _baseMonsterData.Id;
        //this.Name = _baseMonsterData.Name;
        //this.Description = _baseMonsterData.Description;
        //this.Cost = _baseMonsterData.Cost;
        //this.Attack = _baseMonsterData.Attack;
        //this.Hp = _baseMonsterData.Hp;
        this.DefaultHp = source.Hp; //default hp
        //this.Avartar = _baseMonsterData.Avartar;
        //this.IsCharming = _baseMonsterData.IsCharming;
        //this.isTreating = _baseMonsterData.IsTreating;
        if(source.CardEffects != null)
        {
            this.LogicCard = source.CardEffects;
            for(int i = 0; i < this.LogicCard.Length; i++)
            {
                EffectManager.Instance.EffectRegistor(this.LogicCard[i], this);
            }
        }
        yield return null;
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
    //    this.Avartar = _baseMonsterData.Avartar;
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

        print(Hp);
        if(IsDominating && CardPlayer.tokken == EnumDefine.GameTokken.Attack)
        {
            print(this.debug("Attack with IsDominating", new
            {
                card = this.ToString()
            }));
            if(Attack > opponentCard.Hp)
            {
                var offset = Attack - opponentCard.Hp;
                opponentCard.CardPlayer.hp.decrease(offset);

            }
        }
        opponentCard.Hp -= this.Attack;
        print(this.debug($"Attact {this} -> {opponentCard}"));
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

        if(MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Red))
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
        if(_hp <= 0)
        {
            return true;
        }
        else
            return false;
    }
    #endregion

    public void SelectCard()
    {
        print(this.debug(this.ToString(), new
        {
            _isSelectAble
        }));

        if(_isSelectAble)
        {

            if(this.Position == CardPosition.InHand)
            {
                print("Card On hand");

                if(Hand == null)
                    Hand = gameObject.GetComponentInParent<Hand>();
                print(IsSelected ? $"this {Name} have been selected" : $"this {Name} dosen't have been selected");
                if(!IsSelected)
                {
                    print("Card doesn't select yet");
                    var listMonsterCard = Hand.GetAllCardInHand(); //get all monster card in hand
                                                                   //var listUIMonsterCard = listSelect(a => (a.UI)).ToList();//Get UI list

                    var cardSelected = listMonsterCard.SingleOrDefault(a => a.IsSelected == true);//TODO: select card selected-- this true on select 0ne //get card selected

                    if(cardSelected != null) //remove select
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
            else if(Position == CardPosition.InSummonField || Position == CardPosition.InFightField)
            {
                print(this.debug("Select card on field"));

                //check if in pharse atk or defense --> user can select card in summon field to move this in action file
                if(!this.IsSelected)
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
        StringBuilder builder = new StringBuilder();
        builder.Append($"[MONSTER] {Name}{{{Cost}}} {Attack}|{Hp}");
        //IEffectAttributes effectAttributes = this;
        //builder.AppendLine(this.debug("-", new
        //{
        //    IsCharming,
        //    IsTreating,
        //    IsDominating,
        //    IsBlockAttack,
        //    IsBlockDefend
        //}));
        return builder.ToString();
    }

    //public override void LeftClickCard()
    //{
    //    SelectCard();
    //}

    //public override void RightClickCard()
    //{
    //}

    public override void SetupCard()
    {
        if(photonView.IsMine)
        {
            if(MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Red))
            {
                //get player, hand, deck
                GameObject side = GameObject.Find(MatchManager.instance.redSideGameObjectName);
                CardPlayer = side.transform.GetComponentInChildren<CardPlayer>();
                Hand = CardPlayer.GetComponentInChildren<Hand>();
                Deck = CardPlayer.GetComponentInChildren<Deck>();
                CardOwner = CardOwner.You;

                //becom children of deck
                this.transform.parent = CardPlayer.initialCardPlace.transform;

                //add card to list card in deck
                CardPlayer.initialCardPlace.Enqueue(this);
                this.transform.Rotate(new Vector3(180f, 0f, 0f));
            }
            else if(MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Blue))
            {
                //get player, hand, deck
                GameObject side = GameObject.Find(MatchManager.instance.blueSideGameObjectName);
                CardPlayer = side.transform.GetComponentInChildren<CardPlayer>();
                Hand = CardPlayer.GetComponentInChildren<Hand>();
                Deck = CardPlayer.GetComponentInChildren<Deck>();
                CardOwner = CardOwner.You;


                //add card to list card in deck
                CardPlayer.initialCardPlace.Enqueue(this);

                //becom children of deck
                this.transform.parent = CardPlayer.initialCardPlace.transform;
                this.transform.Rotate(new Vector3(180f, 0f, 0f));
            }
        }
        else
        {
            if(MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Red))
            {
                //set parent
                //get player, hand, deck
                GameObject side = GameObject.Find(MatchManager.instance.blueSideGameObjectName);
                CardPlayer = side.transform.GetComponentInChildren<CardPlayer>();
                Hand = CardPlayer.GetComponentInChildren<Hand>();
                Deck = CardPlayer.GetComponentInChildren<Deck>();
                CardOwner = CardOwner.Opponent;

                //becom children of deck
                this.transform.parent = CardPlayer.initialCardPlace.transform;

                //add card to list card in deck
                CardPlayer.initialCardPlace.Enqueue(this);

                //set position
                this.transform.Rotate(new Vector3(180f, 0f, 0f));

            }
            else if(MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Blue))
            {
                //get player, hand, deck
                GameObject side = GameObject.Find(MatchManager.instance.redSideGameObjectName);
                CardPlayer = side.transform.GetComponentInChildren<CardPlayer>();
                Hand = CardPlayer.GetComponentInChildren<Hand>();
                Deck = CardPlayer.GetComponentInChildren<Deck>();
                CardOwner = CardOwner.Opponent;

                //becom children of deck
                this.transform.parent = CardPlayer.initialCardPlace.transform;

                //add card to list card in deck
                CardPlayer.initialCardPlace.Enqueue(this);

                //set position
                this.transform.Rotate(new Vector3(180f, 0f, 0f));

            }
        }
    }


}

