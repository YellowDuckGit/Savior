
using Assets.GameComponent.Card.Logic.Effect;
using Assets.GameComponent.Card.LogicCard;
using Card;
using Photon.Realtime;
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using static EnumDefine;


[System.Serializable]
[CreateAssetMenu(fileName = "MC", menuName = "Scriptable Objects/MonsterCard", order = 1)]
public class MonsterData : ScriptableObject, IMonsterData, IEffectAttributes
{
    [field: SerializeField]
    public string Id { get; set; } = "null";

    [field: SerializeField]
    public string Name { get; set; }

    [field: SerializeField]
    public int Cost { get; set; }

    [field: SerializeField]
    public int Attack { get; set; }

    [field: SerializeField]
    public int Hp { get; set; }

    [field: SerializeField]
    public string Description { get; set; }

    [field: SerializeField]
    public CardType CardType { get; set; }

    [field: SerializeField]
    public Material Avatar { get; set; }

    [field: SerializeField]
    public Rarity RarityCard { get; set; }

    [field: SerializeField]
    public RegionCard RegionCard { get; set; }

    [field: SerializeField]
    public bool IsCharming { get; set; }

    [field: SerializeField]
    public bool IsTreating { get; set; }

    [SerializeReference]
    [SRLogicCard(typeof(AbstractCondition))]
    public AbstractCondition[] CardEffect;
}
