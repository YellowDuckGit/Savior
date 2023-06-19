using Assets.GameComponent.Card.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.GameComponent.Card.LogicCard
{
    [Serializable]
    public abstract class AbstractEffect : AbstractData
    {
        public abstract void RevokeEffect(object register, MatchManager match);
        public abstract void GainEffect(object register, EffectManager match);
    }
}
