using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum CardItemState
{
    inDeck, inInventory
}

[Serializable]
public class CardItem: MonoBehaviour
{
  
    public int amount = 0;

    protected ICardData monsterCardData;

    #region Properties
    public ICardData cardData
    {
        get { return monsterCardData; }
        set { monsterCardData = value; }
    }
    
    public int Amount
    {
        get { return amount; }
        private set
        { 
            amount = value; 
        }
    }

    #endregion
}



