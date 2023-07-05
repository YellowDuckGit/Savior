using Assets.GameComponent.Card.LogicCard;
using Assets.GameComponent.Card.LogicCard.ListLogic.Effect;
using SerializeReferenceEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[SRName("Logic/Effect/DameTo")]
public class Dame : AbstractEffect
{
    public int number;

    public bool inturn { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }


    public override void GainEffect(object register, EffectManager match)
    {
        throw new System.NotImplementedException();
    }

    public override void RevokeEffect(object register, MatchManager match)
    {
        throw new System.NotImplementedException();
    }
}
