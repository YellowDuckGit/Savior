using Assets.GameComponent.Card.LogicCard;
using SerializeReferenceEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[SRName("Logic/Effect/Heal")]
public class Heal : AbstractEffect
{
    public int number;

    public override void GainEffect(object register, EffectManager match)
    {
        throw new System.NotImplementedException();
    }

    public override void RevokeEffect(object register, MatchManager match)
    {
        throw new System.NotImplementedException();
    }
}
