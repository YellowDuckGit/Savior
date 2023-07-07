using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.GameComponent.UI.CreateDeck.UI.Script
{
    public class CardInPack : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Image avatarCard;

        //PackItem packItem;
        CardItem cardItem;

        public CardItem CardItem
        {
            get { return cardItem; }
            set
            {
                cardItem = value;
                nameText.text = cardItem.cardData.Name.ToString();
                costText.text = cardItem.cardData.Cost.ToString();
                avatarCard.sprite = cardItem.cardData.InDeckAvatar2D;
            }
        }


        //public void OnPointerClick(PointerEventData eventData)
        //{
        //    if (eventData.button == PointerEventData.InputButton.Right)
        //    {
        //        if (UIManager.instance.PanelCardDetails.transform != this.transform.parent)
        //            UIManager.instance.LoadCardDetail(cardItem);
        //    }
        //    print("Click");
        //}

        public void OnPointerClick(PointerEventData eventData)
        {
            //if (eventData.button == PointerEventData.InputButton.Right)
            //{
            //}
            print("Click");
            if (UIManager.instance.PanelCardDetails.transform != this.transform.parent)
                UIManager.instance.LoadCardDetail(cardItem);
        }
    }
}
