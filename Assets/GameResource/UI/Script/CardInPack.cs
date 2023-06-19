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
        [SerializeField] private TextMeshProUGUI amount;

        //PackItem packItem;
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
                avatarCard.material = cardItem.cardData.Avatar;
            }
        }
        public int NumberCard
        {
            get { return numberCard; }
            set
            {
                this.numberCard = value;
                amount.text = value + "";
            }
        }
    }
}
