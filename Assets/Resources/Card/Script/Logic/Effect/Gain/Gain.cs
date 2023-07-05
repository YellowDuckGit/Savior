using Assets.GameComponent.Card.LogicCard;
using SerializeReferenceEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GameComponent.Card.Logic.Effect.Gain
{
    [SRName("Logic/Effect/Gain")]
    internal class Gain : AbstractEffect, IEffectAttributes
    {
        public bool _IsCharming;
        public bool IsCharming { get; set; }

        public bool _IsTreating;
        public bool IsTreating { get; set; }

        public override void GainEffect(object register, EffectManager match)
        {
            if (_IsCharming)
            {
                if (register is MonsterCard monster)
                {
                    monster.IsCharming = true;
                }
            }

            if(_IsTreating)
            {
                if(register is MonsterCard monster)
                {
                    monster.IsTreating = true;
                    this.debug("monster is treating: " + monster.IsTreating);
                }
            }
        }

        public override void RevokeEffect(object register, MatchManager match)
        {
            throw new NotImplementedException();
        }
    }
}
