using Assets.GameComponent.Card.LogicCard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.GameComponent.Card.Logic.TargetObject.SelfObject
{
    [Serializable]
    public abstract class AbstractTargetSelfObject : AbstractData
    {
        [SerializeReference]
        [SRLogicCard(typeof(AbstractEffect))]
        public List<AbstractEffect> Effect = null;
    }
}
