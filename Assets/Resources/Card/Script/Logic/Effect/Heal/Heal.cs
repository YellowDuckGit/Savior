using Assets.GameComponent.Card.LogicCard;
using SerializeReferenceEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[SRName("Logic/Effect/Heal")]
public class Heal : AbstractEffect
{
    [UnityEngine.Header("Effect Value")]
    public int number;

    public override bool GainEffect(object register, EffectManager match)
    {
        return true;
    }

    public override void RevokeEffect(object register, MatchManager match)
    {
        throw new System.NotImplementedException();
    }
}
