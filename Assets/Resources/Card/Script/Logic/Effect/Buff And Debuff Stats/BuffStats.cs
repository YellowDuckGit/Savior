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
        public int Attack, Hp;
        public bool InTurn;

        public override bool GainEffect(object register, EffectManager match)
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
                monster.Attack += Attack;
                monster.Hp += Hp;
                monster.EffectSContain.Add(this);
                return true;
            }
            return false;
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
                    monster.Attack -= Attack;
                    monster.Hp -= Hp;
                    monster.EffectSContain.Remove(this);
                    MonoBehaviour.print(match.debug("RevokeEffect Buffstart", new
                    {
                        register
                    }));
                }
            }
        }
    }
}
