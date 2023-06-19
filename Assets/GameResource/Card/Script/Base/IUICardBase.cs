using System;
using TMPro;
using UnityEngine;

public interface IUICardBase
{

    public TextMeshProUGUI UIName { get; set; }
    public TextMeshProUGUI UICost { get; set; }
    public TextMeshProUGUI UIDescription { get; set; }
    //public CardType CardType { get; set; }
    public MeshRenderer UIAvatar { get; set; }

    public GameObject UIOutline { get; set; }
    //public Rarity RarityCard { get; set; }
    //public RegionCard RegionCard { get; set; }

}