using Assets.GameComponent.Card.Logic.TargetObject.Target.CardTarget;
using Assets.GameComponent.Card.Logic.TargetObject.Target.PlayerTarget;
using Assets.GameComponent.Card.Logic.TargetObject.Target;
using Assets.GameComponent.Card.LogicCard;
using SerializeReferenceEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.GameComponent.Card.Logic.Actions.SelectAction.Select_Self
{
    [SRName("Logic/Select/Self")]

    public class SelectSelf : AbstractAction
    {
        [Header("Target")]
        [SerializeReference]
        [SRLogicCard(typeof(SelfType))]
        public SelfType target = null;

        [Header("Effects")]
        [SerializeReference]
        [SRLogicCard(typeof(AbstractEffect))]
        public List<AbstractEffect> Effects = null;
        [Serializable]
        public abstract class SelfType
        {
            [SRName("Self type/Card")]
            public class SelfCard : SelfType { }
            [SRName("Self type/CardPlayer")]
            public class SelfCardPlayer : SelfType { }

        }
    }
}
