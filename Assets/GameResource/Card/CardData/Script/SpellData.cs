using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnumDefinition;

[System.Serializable]
[CreateAssetMenu(fileName = "SC", menuName = "Scriptable Objects/SpellCard", order = 2)]
public class SpellData : ScriptableObject, CardData
{
    [field: SerializeField]
    public string id { get; set; } = "null";

    [field: SerializeField]
    public string name { get; set; }
    [field: SerializeField]
    public string description { get; set; }

    [field: SerializeField]
    public int cost { get; set; }

    public SpellType spellType;

    public Material material;

    public static int idCounter;

    public Rarity rarityCard;

    //TODO: fix it
    //[SerializeReference]
    //[SRLogicCard(typeof(AbstractCondition))]
    //public AbstractCondition[] CardEffect;
}
