using Assets.GameComponent.Card.Logic;
using Assets.GameComponent.Card.Logic.TargetObject;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets.GameComponent.Card.LogicCard
{
    [Serializable]
    public abstract class AbstractCondition : AbstractData
    {
        [HideInInspector]
        public CardBase Card;

        [SerializeReference]
        [SRLogicCard(typeof(AbstractTargetObject))]
        public List<AbstractData> Action;

    }
}
