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
    [SerializeField] private AmountCardDisplay amount;

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
            print(value.cardData.Id + " || value.cardData.Name: " + value.cardData.Name);
            descriptionText.text = value.cardData.Description;
            costText.text = value.cardData.Cost.ToString();
            avatarCard.sprite = value.cardData.NormalAvatar2D;
            amount.SetAmount(numberCard);
            if (cardItem.cardData is MonsterData monsterData)
            {
                atkText.text = monsterData.Attack.ToString();
                hpText.text = monsterData.Hp.ToString();
            }
          
        }
    }


    public int NumberCard
    {
        set
        {
            numberCard = value;
            amount.SetAmount(numberCard);
        }
        get { return numberCard; }
    }
    bool PutInDeck()
    {
        print(this.numberCard);
        if (this.numberCard > 0 && this.numberCard < 4)
        {
            return true;
        }
        else return false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
         

            if (UIManager.instance.PanelCardDetails.transform != this.transform.parent)
            {
                UIManager.instance.LoadCardDetail(cardItem);

            }
        }
        
        else if (eventData.button == PointerEventData.InputButton.Right)
        {

            if (UIManager.instance.isCreateDeck)
            {
                if (PutInDeck())
                {
                    print("Pust Card In DEck");
                    this.PostEvent(EventID.OnPutCardInDeck, this.cardItem.cardData.Id);
                }
            }
        }

        print("Click");
      

    }

    

}
