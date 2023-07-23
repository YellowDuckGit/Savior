using Assets.GameComponent.Card.CardComponents.Script;
using Assets.GameComponent.Card.Logic.TargetObject.Target;
using Assets.GameComponent.Card.LogicCard;
using Assets.GameComponent.Manager.IManager;
using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;


public enum SpellType
{
    Any, Slow, Fast
}
public enum Rarity
{
    Any,
    Normal,
    Elite,
    Epic,
    Legendary,
    Savior
};

public enum CardPosition
{
    Any,
    InDeck,
    InHand,
    InFightField,
    InSummonField,
    InGraveyard,
    InTriggerSpellField
}

public enum CardOwner
{
    Any,
    You,
    Opponent
}

public enum CardType
{
    Any,
    Monster,
    Spell
}

public enum RegionCard
{
    Any,
    FrostClaw,
    ForestBeast,
    Gloaming
}

public abstract class CardBase : MonoBehaviourPun, IPunObservable, ICardBase, INotifyPropertyChanged, ISelectManagerTarget, IAbstractTarget
{
    public object Parents;

    [SerializeField] protected string _id;
    [SerializeField] protected string _name;
    [SerializeField] protected string _description;
    [SerializeField] protected int _cost;
    [SerializeField] protected bool _isSelectAble;
    [SerializeField] protected bool _isSelected;
    [SerializeField] protected bool _forcusable;
    [SerializeField] protected bool _isFocus;
    [SerializeField] protected CardPosition _position;
    [SerializeField] protected CardOwner _cardOwner;
    [SerializeField] protected CardType _cardType;
    [SerializeField] protected CardPlayer _cardPlayer;
    [SerializeField] protected Material _normalAvatar;
    [SerializeField] protected Material _inDeckAvatar;
    [SerializeField] protected Material _inBoardAvatar;
    [SerializeField] protected Sprite _normalAvatar2D;
    [SerializeField] protected Sprite _inDeckAvatar2D;
    [SerializeField] protected Rarity _rarityCard;
    [SerializeField] protected RegionCard _regionCard;
    [SerializeField] protected ICardData BaseCard;
    public abstract string Id
    {
        get; set;
    }
    public abstract string Name
    {
        get; set;
    }
    public abstract string Description
    {
        get; set;
    }
    public abstract int Cost
    {
        get; set;
    }
    public abstract bool IsSelectAble
    {
        get; set;
    }
    public abstract bool IsSelected
    {
        get; set;
    }
    public abstract bool Forcusable
    {
        get; set;
    }
    public abstract bool IsFocus
    {
        get; set;
    }
    public abstract CardPosition Position
    {
        get; set;
    }
    public abstract CardOwner CardOwner
    {
        get; set;
    }
    public abstract CardType CardType
    {
        get; set;
    }
    public abstract CardPlayer CardPlayer
    {
        get; set;
    }
    public abstract Rarity RarityCard
    {
        get; set;
    }
    public abstract RegionCard RegionCard
    {
        get; set;
    }
    public bool IsReady
    {
        get;
        set;
    } = false;
    public abstract Material NormalAvatar
    {
        get;
        set;
    }
    public abstract Material InDeckAvatar
    {
        get;
        set;
    }
    public abstract Material InBoardAvatar
    {
        get;
        set;
    }
    public abstract Sprite NormalAvatar2D
    {
        get;
        set;
    }
    public abstract Sprite InDeckAvatar2D
    {
        get;
        set;
    }

    //public abstract event OnNameChange onNameChange;
    //public abstract event OnCostChange onCostChange;
    //public abstract event OnDescriptionChange onDescriptionChange;
    //public abstract event OnPositionChange onPositionChange;
    //public abstract event OnSelectChange onSelectChange;
    //public abstract event OnAvartarChange onAvartarChange;
    //public abstract event OnFocusChange onFocusChange;

    public List<AbstractEffect> EffectSContain = new();

    public event PropertyChangedEventHandler PropertyChanged;

    public abstract void Play(MatchManager matchManager);
    protected void LeftClickCard()
    {
        this.PostEvent(EventID.OnLeftClickCard, this);
        print(this.ToString());
    }
    protected void RightClickCard()
    {
        this.PostEvent(EventID.OnRightClickCard, this);
    }

    public void Discard()
    {
        if(Parents != null)
        {
            if(Parents is IList list)
            {
                list.Remove(this);
            }
            else if(Parents is IDictionary dictionary)
            {
                dictionary.Remove(this.Id);
            }
            else
            {
                Debug.LogError(this.debug("can not find type of parents"));
            }
        }
        else
        {
            Debug.LogError(this.debug("parents is null"));
        }
        this.gameObject.SetActive(false);
        this.transform.parent = null;
    }

    public abstract void SetupCard();
    public abstract IEnumerator LoadCardFromData();
    public abstract void RegistLocalEvent();

    private void NetworkingClient_EventReceived(EventData obj)
    {
        var args = obj.GetData();

        if((RaiseEvent)obj.Code == RaiseEvent.SET_DATA_CARD_EVENT)
        {
            if(args.photonviewID.Equals(photonView.ViewID))
                StartCoroutine(ExecuteLoadData(args.cardDataId));
        }
    }

    private IEnumerator ExecuteLoadData(string iD_Data)
    {
        yield return StartCoroutine(SetCardBase(iD_Data));
        yield return StartCoroutine(LoadCardFromData());
        IsReady = true;
    }

    private IEnumerator SetCardBase(string iD_Data)
    {
        var cardBase = GameData.instance.listCardDataInGame.First(a => a.Id.Equals(iD_Data));
        if(cardBase is MonsterData monsterData)
        {
            this.BaseCard = monsterData;
        }
        else if(cardBase is SpellData spellData)
        {
            this.BaseCard = spellData;
        }
        else
        {
            Debug.LogError(this.debug("can not find type of card base"));
        }
        yield return null;
    }

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
        return JsonUtility.ToJson(this as ICardBase);
    }

    public abstract override string ToString();

    public void OnMouseDown()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            LeftClickCard();
        }
        else if(Input.GetKeyDown(KeyCode.Mouse1))
        {
            RightClickCard();
        }
    }
    protected void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, e);
    }

    protected void OnPropertyChanged(string propertyName)
    {
        OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}

