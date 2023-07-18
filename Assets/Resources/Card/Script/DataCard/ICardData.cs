using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICardData
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Cost { get; set; }
    public string Description { get; set; }
    public CardType CardType { get; set; }
    public Material Avatar { get; set; }
    public Material NormalAvatar
    {
        get; set;
    }
    public Material InDeckAvatar
    {
        get; set;
    }
    public Material InBoardAvatar
    {
        get; set;
    }
    public Sprite NormalAvatar2D
    {
        get; set;
    }
    public Sprite InDeckAvatar2D
    {
        get; set;
    }


    public Rarity RarityCard { get; set; }
    public RegionCard RegionCard { get; set; }
}
