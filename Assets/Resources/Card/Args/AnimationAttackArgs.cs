using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationAttackArgs
{
    public MonsterCard own;
    public MonsterCard opponnet;

    public AnimationAttackArgs(MonsterCard own, MonsterCard opponnet)
    {
        this.own = own;
        this.opponnet = opponnet;
    }
}
