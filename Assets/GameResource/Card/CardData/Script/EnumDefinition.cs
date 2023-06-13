using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnumDefinition
{    public enum Rarity
    {
        Normal,
        Elite,
        Legendary,
        Savior
    };

    public enum CardPosition
    {
        InDeck, InHand, InFightField, InSummonField, InGraveyard, All
    }

    public enum CardOwner
    {
        You, Opponent
    }

    public enum CardType
    {
        Monster, Spell
    }

    public enum SpellType
    {
        Fast, Slow
    }
}
