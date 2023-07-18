using Assets.GameComponent.Card.CardComponents.Script;
using Assets.GameComponent.Card.Logic.Effect;
using Assets.GameComponent.Card.LogicCard;
using Card;
using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

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

    public override string Id
    {
        get
        {
            return _id;
        }
        set
        {
            this._id = value;
        }
    }
    public override string Name
    {
        get
        {
            return _name;
        }
        set
        {
            this._name = value;
            OnPropertyChanged(nameof(Name));
        }
    }
    public override string Description
    {
        get
        {
            return _description;
        }
        set
        {
            this._description = value;
            OnPropertyChanged(nameof(Description));
        }
    }
    public override int Cost
    {
        get
        {
            return _cost;
        }
        set
        {
            this._cost = value;
            OnPropertyChanged(nameof(Cost));
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
            this._isSelectAble = value;
        }
    }
    public override bool IsSelected
    {
        get
        {
            return _isSelected;
        }
        set
        {
            this._isSelected = value;
        }
    }
    public override bool Forcusable
    {
        get
        {
            return _forcusable;
        }
        set
        {
            this._forcusable = value;
        }
    }
    public override bool IsFocus
    {
        get
        {
            return _isFocus;
        }
        set
        {
            this._isFocus = value;
        }
    }
    public override CardPosition Position
    {
        get
        {
            return _position;
        }
        set
        {
            this._position = value;
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
            this._cardOwner = value;
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
            this._cardType = value;
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
            this._cardPlayer = value;
        }
    }
 
    public SpellData BaseSpellData
    {
        get
        {
            return _baseSpellData;
        }
        set
        {
            this._baseSpellData = value;
        }
    }
    public override Rarity RarityCard
    {
        get
        {
            return _rarityCard;
        }
        set
        {
            this._rarityCard = value;
        }
    }
    public override RegionCard RegionCard
    {
        get
        {
            return _regionCard;
        }
        set
        {
            this._regionCard = value;
        }
    }
    public SpellType SpellType
    {
        get
        {
            return _spellType;
        }
        set
        {
            this._spellType = value;
        }
    }

    public override Material NormalAvatar
    {
        get
        {
            return _normalAvatar;
        }
        set
        {
            this._normalAvatar = value;
        }
    }
    public override Material InDeckAvatar
    {
        get
        {
            return _inDeckAvatar;
        }
        set
        {
            this._inDeckAvatar = value;
        }
    }
    public override Material InBoardAvatar
    {
        get
        {
            return _inBoardAvatar;
        }
        set
        {
            this._inBoardAvatar = value;
        }
    }
    public override Sprite NormalAvatar2D
    {
        get
        {
            return _normalAvatar2D;
        }
        set
        {
            this._normalAvatar2D = value;
        }
    }
    public override Sprite InDeckAvatar2D
    {
        get
        {
            return _inDeckAvatar2D;
        }
        set
        {
            this._inDeckAvatar2D = value;
        }
    }

    public override void Play(MatchManager matchManager)
    {
    }

    public override void SetupCard()
    {
        if(photonView.IsMine)
        {
            if(MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Red))
            {
                //get player, hand, deck
                GameObject side = GameObject.Find(MatchManager.instance.redSideGameObjectName);
                CardPlayer = side.transform.GetComponentInChildren<CardPlayer>();
                hand = CardPlayer.GetComponentInChildren<Hand>();
                deck = CardPlayer.GetComponentInChildren<Deck>();


                //becom children of deck
                this.transform.parent = deck.transform;

                //add card to list card in deck
                CardPlayer.initialCardPlace.Enqueue(this);

                //set position
                this.transform.position = deck.PositionInitialCardInDeck;
                this.transform.Rotate(new Vector3(180f, 0f, 0f));



            }
            else if(MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Blue))
            {
                //get player, hand, deck
                GameObject side = GameObject.Find(MatchManager.instance.blueSideGameObjectName);
                CardPlayer = side.transform.GetComponentInChildren<CardPlayer>();
                hand = CardPlayer.GetComponentInChildren<Hand>();
                deck = CardPlayer.GetComponentInChildren<Deck>();


                //add card to list card in deck
                CardPlayer.initialCardPlace.Enqueue(this);

                //becom children of deck
                this.transform.parent = deck.transform;

                //set position
                this.transform.position = deck.PositionInitialCardInDeck;
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
                hand = CardPlayer.GetComponentInChildren<Hand>();
                deck = CardPlayer.GetComponentInChildren<Deck>();

                //becom children of deck
                this.transform.parent = deck.transform;

                //add card to list card in deck
                CardPlayer.initialCardPlace.Enqueue(this);

                //set position
                this.transform.position = deck.PositionInitialCardInDeck;
                this.transform.Rotate(new Vector3(180f, 0f, 0f));

            }
            else if(MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Blue))
            {
                //get player, hand, deck
                GameObject side = GameObject.Find(MatchManager.instance.redSideGameObjectName);
                CardPlayer = side.transform.GetComponentInChildren<CardPlayer>();
                hand = CardPlayer.GetComponentInChildren<Hand>();
                deck = CardPlayer.GetComponentInChildren<Deck>();

                //becom children of deck
                this.transform.parent = deck.transform;

                //add card to list card in deck
                CardPlayer.initialCardPlace.Enqueue(this);

                //set position
                this.transform.position = deck.PositionInitialCardInDeck;
                this.transform.Rotate(new Vector3(180f, 0f, 0f));

            }
        }
    }

    public override void RegistLocalEvent()
    {
    }



    public override IEnumerator LoadCardFromData()
    {
        print($"Start Load Data for {BaseCard.Name}");
        //load data form monster data
        SpellData source = (SpellData)BaseCard;
        Id = source.Id;
        Name = source.Name;
        Cost = source.Cost;
        Description = source.Description;
        CardType = source.CardType;
        NormalAvatar = source.NormalAvatar;
        NormalAvatar2D = source.NormalAvatar2D;
        InDeckAvatar = source.InDeckAvatar;
        InDeckAvatar2D = source.InDeckAvatar2D;
        InBoardAvatar = source.InBoardAvatar;
        NormalAvatar = source.NormalAvatar;

        RarityCard = source.RarityCard;
        RegionCard = source.RegionCard;

        if(source.CardEffect != null)
        {
            this.LogicCard = source.CardEffect;
            for(int i = 0; i < this.LogicCard.Length; i++)
            {
                EffectManager.Instance.EffectRegistor(this.LogicCard[i], this);
            }
        }
        print("End Load Data");
        yield return null;
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

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"[SPELL] {Name}{{{Cost}}} - {Description}");
        return builder.ToString();
    }
}
