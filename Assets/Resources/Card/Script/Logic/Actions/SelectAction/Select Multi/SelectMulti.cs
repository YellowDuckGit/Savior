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

namespace Assets.GameComponent.Card.Logic.Actions.SelectAction.Select_Multi
{
    [SRName("Logic/Select/Select Multiple")]
    public class SelectMulti : AbstractAction
    {
        [SerializeReference]
        [SRLogicCard(typeof(MultiTargetType))]
        public MultiTargetType multiTargetType = null;


        [Serializable]
        public abstract class MultiTargetType
        {
            [SRName("Multi Type/Player")]
            public class MultiTargetPlayer : MultiTargetType
            {
                [SerializeReference]
                [SRLogicCard(typeof(AbstractEffect))]
                public List<AbstractEffect> Effects = null;
            }

            [SRName("Multi Type/Card")]
            public class MultiTargetCard : MultiTargetType
            {
                //[SerializeReference]
                //[SRLogicCard(typeof(CardTarget))]
                public CardTarget target = null;

                [SerializeReference]
                [SRLogicCard(typeof(AbstractEffect))]
                public List<AbstractEffect> Effects = null;
            }
        }

    }
}
