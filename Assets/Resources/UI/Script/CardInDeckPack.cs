using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardInDeckPack : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Image avatarCard;
    [SerializeField] private TextMeshProUGUI amount;

    CardItem cardItem;

    int numberCard = 0;

    public CardItem CardItem
    {
        get { return cardItem; }
        set
        {
            cardItem = value;
            nameText.text = cardItem.cardData.Name;
            costText.text = cardItem.cardData.Cost.ToString();
            avatarCard.material = cardItem.cardData.InDeckAvatar;
        }
    }

    public int NumberCard
    {
        get { return numberCard; }
        set 
        { 
            this.numberCard = value;
            amount.text = "X"+value;
        }
    }

    public bool CanRemoveCard()
    {
        if (cardItem.amount > 0)
        {
            return true;
        }
        else return false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        print("Click");
        if(CanRemoveCard())
        {
            this.PostEvent(EventID.OnRemoveCardInDeck, this.cardItem.cardData.Id);
        }
    }

    public string ToJson() {
        return "";
    }
}
