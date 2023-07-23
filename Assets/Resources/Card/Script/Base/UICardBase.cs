using Assets.GameComponent.Card.CardComponents.Script.UI;
using EPOOutline;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Video;
using static UnityEngine.Rendering.DebugUI;

public abstract class UICardBase<T> : MonoBehaviour, IUICardBase where T : CardBase
{
    #region UI

    /*For debug*/
    [SerializeField] protected TextMeshProUGUI _name;
    /*For debug*/
    [SerializeField] protected TextMeshProUGUI _cost;
    /*For debug*/
    [SerializeField] protected TextMeshProUGUI _description;
    /*For debug*/
    [SerializeField] protected MeshRenderer _avatar;
    /*For debug*/
    [SerializeField] protected T _cardTarget;
    protected Outlinable _outline;

    public abstract CardAnimationController Controller
    {
        get; set;
    }
    public abstract TextMeshProUGUI UIName
    {
        get; set;
    }
    public abstract TextMeshProUGUI UICost
    {
        get; set;
    }
    public abstract TextMeshProUGUI UIDescription
    {
        get; set;
    }
    public abstract MeshRenderer UIAvatar
    {
        get; set;
    }
    public abstract Outlinable UIOutline
    {
        get; set;
    }

    public abstract T CardTarget
    {
        get; set;
    }

    private void Awake()
    {
        print(this.debug($"UI for {CardTarget} Awake"));
        GetCardComponents();
        //this.RegisterListener(EventID.OnCardUpdate, (CardTarget) => OnCardUpdate(CardTarget as T));
        RegisLocalListener();
    }

    private void RegistInterActionEvent()
    {
        this.RegisterListener(EventID.OnUIClickCard, (cardTarget) => OnClickOnCard(cardTarget as UICardBase<T>));
        this.RegisterListener(EventID.OnEnterCard, (cardTarget) => OnEnterCard(cardTarget as UICardBase<T>));
        this.RegisterListener(EventID.OnExitCard, (cardTarget) => OnExitCard(cardTarget as UICardBase<T>));
    }
    private void RevokeInterActionEvent()
    {
        this.RemoveListener(EventID.OnUIClickCard, (cardTarget) => OnClickOnCard(cardTarget as UICardBase<T>));
        this.RemoveListener(EventID.OnEnterCard, (cardTarget) => OnEnterCard(cardTarget as UICardBase<T>));
        this.RemoveListener(EventID.OnExitCard, (cardTarget) => OnExitCard(cardTarget as UICardBase<T>));
    }

    private void Start()
    {
        //print(this.debug($"UI for {CardTarget} Start"));
        //this.RegisterListener(EventID.OnClickCard, (cardTarget) => OnClickOnCard(cardTarget as UICardBase<T>));
        //this.RegisterListener(EventID.OnEnterCard, (cardTarget) => OnEnterCard(cardTarget as UICardBase<T>));
        //this.RegisterListener(EventID.OnExitCard, (cardTarget) => OnExitCard(cardTarget as UICardBase<T>));
    }

    public abstract void RegisLocalListener();

    public abstract void GetCardComponents();
    public abstract void OnCardUpdate(object sender, PropertyChangedEventArgs e);

    //public abstract void updateName(string Name);/* => this.Name.text = name;*/
    //public abstract void updateDescription(string Description);/* => this.Description.text = Description;*/
    //public abstract void updateCost(int Cost);/* => this.Cost.text = Cost.ToString();*/
    //public abstract void updateAvatar(Material Avatar);/* => this.Avatar.material = Avatar;*/
    #endregion
    #region Logic

    private bool _isSelected;
    private bool _isEnter;
    private bool _isForcus;
    private bool _interactable;

    public bool IsSelected
    {
        get
        {
            return _isSelected;
        }
        set
        {
            _isSelected = value;
        }
    }

    public bool IsEnter
    {
        get
        {
            return _isEnter;
        }
        set
        {
            if (value)
            {
                EnterCard();
            }
            else
            {
                UnEnterCard();
            }
            _isEnter = value;
        }
    }

    public bool IsForcus
    {
        get
        {
            return _isForcus;
        }
        set
        {
            if(value)
            {
                FocusCard();
            }
            else
            {
                UnFocusCard();
            }
            _isForcus = value;
        }
    }

    public bool Interactable
    {
        get => _interactable; set
        {
            _interactable = value;

            if(_interactable)
            {
                RegistInterActionEvent();
            }
            else
            {
                RevokeInterActionEvent();
            }
        }
    }

    public Vector3 OriginPostion;
    public Vector3 OriginRotation;


    //public MonsterCard card;
    //private CardBase _card;


    #region Set Get


    #endregion
    private void OnEnable()
    {
        Interactable = true;
    }
    public void OnDisable()
    {
        Interactable = false;
    }
    public void OnMouseEnter()
    {
        this.PostEvent(EventID.OnEnterCard, this);
    }

    private void OnMouseExit()
    {
        this.PostEvent(EventID.OnExitCard, this);
    }

    private void OnMouseDown()
    {
        this.PostEvent(EventID.OnUIClickCard, this);
    }

    private void OnClickOnCard(UICardBase<T> cardTarget)
    {
        if(cardTarget == this)
        {
            this.IsForcus = true;
        }
        else
        {
            if(this.IsForcus)
            {
                this.IsForcus = false;
            }
        }
    }
    private void OnEnterCard(UICardBase<T> cardUITarget)
    {
        if(cardUITarget == this)
        {
            if(this.CardTarget.Position == CardPosition.InHand)
            {
                if(!IsEnter)
                {
                    IsEnter = true;
                }
            }
        }
        else
        {
            if(this.CardTarget.Position == CardPosition.InHand)
            {
                if(IsEnter)
                {
                    IsEnter = false;
                }
            }
        }
    }

    private void OnExitCard(UICardBase<T> cardTarget)
    {
        if(cardTarget == this)
        {
            if(this.CardTarget.Position == CardPosition.InHand)
            {
                if(IsEnter)
                {
                    IsEnter = false;
                }
            }
        }

    }

    #region Animation 
    public void EnterCard()
    {
        if((this.CardTarget.Position == CardPosition.InHand))
            Controller.PlayHover();
    }

    public void UnEnterCard()
    {
        if ((this.CardTarget.Position == CardPosition.InHand))
            Controller.PlayUnHover();
    }

    public bool FocusCard()
    {
        print($"FocusCard card {_cardTarget.Name}, ID: {_cardTarget.photonView.ViewID}");
        if(this._outline != null)
        {
            this._isForcus = true;
            this._outline.enabled = true;
        }
        return this.IsForcus;
    }
    public bool UnFocusCard()
    {
        print($"UnFocusCard card {_cardTarget.Name}");
        if(this._outline != null)
        {
            this._isForcus = false;
            if(!_cardTarget.IsSelected)
                this._outline.enabled = false;
        }
        return this.IsForcus;
    }

    Color originalColor;
    public bool SelectCard()
    {
        print($"select card {_cardTarget.Name}");
        if(this._outline != null)
        {
            this._isSelected = true;
            //_card.IsSelected = true;
            Renderer rend = this._outline.GetComponent<Renderer>();
            originalColor = rend.material.color;
            rend.material.color = new Color(0.5f, 1, 1);
            this._outline.enabled = true;
        }
        return this._outline.isActiveAndEnabled;
    }

    public bool UnSelectCard()
    {
        print($"unselect card {_cardTarget.Name}");
        if(this._outline != null)
        {
            this._isSelected = false;
            //_card.IsSelected = false;
            Renderer rend = this._outline.GetComponent<Renderer>();
            rend.material.color = originalColor;
            this._outline.enabled = false;
        }
        return this._outline.isActiveAndEnabled;
    }

    #endregion
    #endregion
}

