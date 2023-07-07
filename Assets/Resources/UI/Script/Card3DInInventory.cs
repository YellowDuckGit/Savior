using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Card3DInInventory : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI atkText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private MeshRenderer avatarCard;
    [SerializeField] private MeshRenderer highlight;
    [SerializeField] private MMFeedbacks feedbacks;

    [SerializeField] private Material highlightNormal;
    [SerializeField] private Material highlightElite;
    [SerializeField] private Material highlightRace;
    [SerializeField] private Material highlightEpic;
    [SerializeField] private Material highlightLegendary;
    CardItem cardItem;

    public bool isFlip = false;

    private void Start()
    {
    }

    public CardItem CardItem
    {
        get { return cardItem; }
        set
        {
            cardItem = value;
            nameText.text = value.cardData.Name;
            descriptionText.text = value.cardData.Description;
            costText.text = value.cardData.Cost.ToString();
            avatarCard.material = value.cardData.NormalAvatar;
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

            switch (value.cardData.RarityCard)
            {
                case Rarity.Normal:
                    highlight.material = highlightNormal;
                    break;
                case Rarity.Elite:
                    highlight.material = highlightElite;
                    break;
                case Rarity.Epic:
                    highlight.material = highlightEpic;
                    break;
                case Rarity.Legendary:
                    highlight.material = highlightLegendary;
                    break;

            }
        }
    }

    public void OnMouseDown()
    {
        flip();

    }

    public void flip()
    {
        if (!isFlip)
        {
            isFlip = true;
            feedbacks.PlayFeedbacks();
        }
    }

}
