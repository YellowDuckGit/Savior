using Assets.GameComponent.Card.CardComponents.Script;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class CardInInventory : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI atkText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private Image avatarCard;
    [SerializeField] private TextMeshProUGUI amount;

    CardItem cardItem;

    private int numberCard;

    private void Start()
    {
    }

    public CardItem CardItem
    {
        get { return cardItem; }
        set
        {
            cardItem = value;
            numberCard = cardItem.amount;
            nameText.text = value.cardData.Name;
            descriptionText.text = value.cardData.Description;
            costText.text = value.cardData.Cost.ToString();
            avatarCard.material = value.cardData.NormalAvatar;
            amount.text = "X" + numberCard;
            if (cardItem.cardData is MonsterData monsterData)
            {
                atkText.text = monsterData.Attack.ToString();
                hpText.text = monsterData.Hp.ToString();
            }
            //descriptionText.text = value.cardData.description;
            //costText.text = value.cardData.cost.ToString();
            //atkText.text = value.cardData.atk.ToString();
            //hpText.text = value.cardData.hp.ToString();
            //avatarCard.material = value.cardData.material;
        }
    }


    public int NumberCard
    {
        set
        {
            numberCard = value;
            amount.text = "X" + numberCard;
        }
        get { return numberCard; }
    }
    bool PutInDeck()
    {
        if (this.numberCard > 0 && this.numberCard < 4)
        {
            return true;
        }
        else return false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        print("Click");
        if (UIManager.instance.isCreateDeck)
        {
            if (PutInDeck())
            {
                this.PostEvent(EventID.OnPutCardInDeck, this.cardItem.cardData.Id);
            }
        }
        else if (UIManager.instance.isStoreCards)
        {
            print("show popup card");
            UIManager.instance.ShowPopupCard(this);
        }

    }

}
