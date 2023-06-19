using SerializeReferenceEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.GameComponent.Card.LogicCard.ListLogic.Effect
{
    [SRName("Logic/Effect/BuffStats")]
    public class BuffStats : AbstractEffect, IInturnEffect
    {
        public int number;
        public bool InTurn;

        public override void GainEffect(object register, EffectManager match)
        {
            MonoBehaviour.print(match.debug("GainEffect Buffstart", new
            {
                register
            }));
            if (register is MonsterCard monster)
            {
                MonoBehaviour.print(match.debug("GainEffect Buffstart", new
                {
                    register
                }));
                monster.Attack += number;
                monster.Hp += number;
                monster.EffectSContain.Add(this);

            }
        }

        public override void RevokeEffect(object register, MatchManager match)
        {
            if (InTurn)
            {
                MonoBehaviour.print(match.debug("RevokeEffect Buffstart", new
                {
                    register
                }));
                if (register is MonsterCard monster)
                {
                    monster.Attack -= number;
                    monster.Hp -= number;
                    monster.EffectSContain.Remove(this);
                    MonoBehaviour.print(match.debug("RevokeEffect Buffstart", new
                    {
                        register
                    }));
                }
            }
        }


        //public bool inTurn;
    }
}
