using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.GameComponent.UI.CreateDeck.UI.Script
{
    public class CardInPack : MonoBehaviour
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
                avatarCard.material = cardItem.cardData.InDeckAvatar;
            }
        }

        private void OnMouseEnter()
        {
            print("OnMouseEnter");
        }

        private void OnMouseDown()
        {
            print("OnMouseDown");
        }
    }
}
