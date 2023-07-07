using Assets.GameComponent.Card.CardComponents.Script;
using Assets.GameComponent.Card.LogicCard;
using ExitGames.Client.Photon;
using JetBrains.Annotations;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using static EventGameHandler;


public enum SpellType
{
    Slow, Fast
}
public enum Rarity
{
    Normal,
    Elite,
    Epic,
    Legendary,
};

public enum CardPosition
{
    InDeck, InHand, InFightField, InSummonField, InGraveyard, All,
    InTriggerSpellField
}

public enum CardOwner
{
    You, Opponent
}

public enum CardType
{
    Monster, Spell
}

public enum RegionCard
{
    FrostClaw,
    ForestBeast,
    Gloaming
}

public abstract class CardBase : MonoBehaviourPun, IPunObservable, ICardBase
{
    protected string _id;
    protected string _name;
    protected string _description;
    protected int _cost;
    protected bool _isSelectAble;
    protected bool _isSelected;
    protected bool _forcusable;
    protected bool _isFocus;
    protected CardPosition _position;
    protected CardOwner _cardOwner;
    protected CardType _cardType;
    protected CardPlayer _cardPlayer;
    protected Material _normalavatar;
    protected Material _inDeckavatar;
    protected Material _inBoardavatar;
    protected Sprite _normalavatar2d;

    protected Rarity _rarityCard;
    protected RegionCard _regionCard;

    public abstract string Id { get; set; }
    public abstract string Name { get; set; }
    public abstract string Description { get; set; }
    public abstract int Cost { get; set; }
    public abstract bool IsSelectAble { get; set; }
    public abstract bool IsSelected { get; set; }
    public abstract bool Forcusable { get; set; }
    public abstract bool IsFocus { get; set; }
    public abstract CardPosition Position { get; set; }
    public abstract CardOwner CardOwner { get; set; }
    public abstract CardType CardType { get; set; }
    public abstract CardPlayer CardPlayer { get; set; }
    public abstract Material NormalAvatar { get; set; }
    public abstract Material InDeckAvatar { get; set; }
    public abstract Material InBoardAvatar { get; set; }
    public abstract Rarity RarityCard { get; set; }
    public abstract RegionCard RegionCard { get; set; }
    public Sprite NormalAvatar2D { get; set; }
    public Sprite InDeckAvatar2D { get; set; }

    //public abstract event OnNameChange onNameChange;
    //public abstract event OnCostChange onCostChange;
    //public abstract event OnDescriptionChange onDescriptionChange;
    //public abstract event OnPositionChange onPositionChange;
    //public abstract event OnSelectChange onSelectChange;
    //public abstract event OnavatarChange onavatarChange;
    //public abstract event OnFocusChange onFocusChange;

    public List<AbstractEffect> EffectSContain = new();

    public abstract void Play(MatchManager matchManager);
    public abstract void OnClick();
    public abstract void SetupCard();
    public abstract void LoadCardFromData();
    public abstract void RegistLocalEvent();

    public abstract void NetworkingClient_EventReceived(EventData eventData);
    private void Awake()
    {
        SetupCard();
    }

    private void Start()
    {
        RegistLocalEvent();
    }

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
    }

    public string toJson()
    {
        return "";
    }

    public override string ToString()
    {
        return $"{Name}";
    }

    public void OnMouseDown()
    {
        OnClick();
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) { }
}

