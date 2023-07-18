using Assets.GameComponent.Card.Logic.TargetObject.Target;
using Assets.GameComponent.Card.Logic.TargetObject.Target.AnyTarget;
using Assets.GameComponent.Card.Logic.TargetObject.Target.CardTarget;
using Assets.GameComponent.Card.Logic.TargetObject.Target.PlayerTarget;
using Assets.GameComponent.Card.LogicCard;
using SerializeReferenceEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.GameComponent.Card.Logic.TargetObject
{
    [SRName("Logic/Select/Select Target")]
    public class SelectTarget : AbstractAction
    {
        public List<SelectTargets> selectTargets;


        [Serializable]
        public class SelectTargets
        {
            [SerializeReference]
            [SRLogicCard(typeof(CardTarget), typeof(PlayerTarget), typeof(AnyTarget))]
            public AbstractTarget target = null;

            [SerializeReference]
            [SRLogicCard(typeof(AbstractEffect))]
            public List<AbstractEffect> Effects = null;
        }
    }
}
