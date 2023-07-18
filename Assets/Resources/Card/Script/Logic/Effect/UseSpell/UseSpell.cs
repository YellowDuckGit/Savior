using Assets.GameComponent.Card.LogicCard;
using SerializeReferenceEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.GameComponent.Card.Logic.Effect.UseSpell
{
    [SRName("Logic/Effect/UseSpell")]
    public class UseSpell : AbstractEffect
    {
        public ScriptableObject SpellTarget;
        public override bool GainEffect(object register, EffectManager match)
        {
            return true;
          
        }

        public override void RevokeEffect(object register, MatchManager match)
        {
            throw new NotImplementedException();
        }
    }
}
