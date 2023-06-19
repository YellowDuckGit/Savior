using Assets.GameComponent.Card.Logic.Effect;
using Assets.GameComponent.Card.LogicCard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.GameComponent.Card.CardComponents.Script
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "MC", menuName = "Scriptable Objects/SpellCard", order = 2)]
    public class SpellData : ScriptableObject, ISpellData
    {
        [field: SerializeField]
        public string Id { get; set; } = "null";

        [field: SerializeField]
        public string Name { get; set; }

        [field: SerializeField]
        public int Cost { get; set; }
        [field: SerializeField]
        public SpellType SpellType { get; set; }

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


        [SerializeReference]
        [SRLogicCard(typeof(AbstractCondition))]
        public AbstractCondition[] CardEffect;
    }
}
