using Assets.GameComponent.Card.LogicCard;
using SerializeReferenceEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GameComponent.Card.Logic.Effect.Destroy
{
    [SRName("Logic/Effect/Destroy")]
    public class DestroyObject : AbstractEffect
    {
        public override bool GainEffect(object register, EffectManager match)
        {
            if (register is MonsterCard monster)
            {
                monster.MoveToGraveyard();
                return true;
            }
            return false;
        }

        public override void RevokeEffect(object register, MatchManager match)
        {
        }
    }
}
