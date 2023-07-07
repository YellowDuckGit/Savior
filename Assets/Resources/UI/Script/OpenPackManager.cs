using Assets.GameComponent.UI.CreateDeck.UI.Script;
using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;

public class OpenPackManager : MonoBehaviour
{
    public Card3DInInventory MonsterCardPrefab;
    public Card3DInInventory SpellCardPrefab;

    public GameObject packContainer;
    public GameObject cardContainer;

    public List<Card3DInInventory> card3DInInventories;
    public MMSequencer sequencer;

    public void StartOpen()
    {
        UIManager.instance.EnableLoadingAPI(true);
        print("GameData.instance.listCardOpenedInPack: " + GameData.instance.listCardOpenedInPack.Count);
        foreach(List<string> itemInPack in GameData.instance.listCardOpenedInPack)
        {
            GameObject prefab = null;
            foreach (string id in itemInPack)
            {
                print(id);
                CardItem cardItem = GameData.instance.listCardItem.SingleOrDefault(a => a.cardData.Id.Equals(id));
                if (cardItem != null)
                {
                    switch (cardItem.cardData.CardType)
                    {
                        case CardType.Monster:
                            prefab = MonsterCardPrefab.gameObject;
                            break;
                        case CardType.Spell:
                            prefab = SpellCardPrefab.gameObject;
                            break;
                    }
                }
                if (prefab != null)
                {
                    Card3DInInventory card = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity).GetComponent<Card3DInInventory>();
                    card.CardItem = cardItem;
                    card.transform.parent = cardContainer.transform;
                    card.transform.rotation = Quaternion.Euler(new Vector3(90f, 0f, 0f));

                    card3DInInventories.Add(card);
                }
            }
        }
        UIManager.instance.EnableLoadingAPI(false);
        UIManager.instance.TurnOnOpenPackScene();
        StartCoroutine(GameData.instance.LoadCardInInventoryUser());
    }

    public void SkipFlip()
    {
        bool allcardFliped = card3DInInventories.Exists(a => a.isFlip == false);
        if(allcardFliped)
        {
            foreach (Card3DInInventory card in card3DInInventories)
            {
                card.flip();
            }
        }
        else
        {
            UIManager.instance.TurnOnStorePacksScene();
            clear();
        }

       

    }

    public void clear()
    {
        if (card3DInInventories.Count > 0)
        {
            print("clear");
            if (card3DInInventories[0] == null)
            {
                card3DInInventories.Clear();
            }
            else
            {
                foreach (Card3DInInventory card in card3DInInventories)
                {
                    Destroy(card.gameObject);
                }
                card3DInInventories.Clear();
            }
        }
     
        GameData.instance.listCardOpenedInPack.Clear();
    }
}
