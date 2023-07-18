using Assets.GameComponent.Card.Logic.TargetObject;
using Assets.GameComponent.Card.Logic.TargetObject.Target;
using Assets.GameComponent.Card.LogicCard;
using SerializeReferenceEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.GameComponent.Card.Logic.Effect.Store
{
    [SRName("Logic/Get in Stored")]
    public class TempGetStore : AbstractAction
    {
        public List<SelectTargets> selectTargets;


        [Serializable]
        public class SelectTargets
        {
            [SerializeReference]
            [SRLogicCard(typeof(AbstractTarget))]
            public AbstractTarget target = null;

            [SerializeReference]
            [SRLogicCard(typeof(AbstractEffect))]
            public List<AbstractEffect> Effects = null;
        }
    }
}
