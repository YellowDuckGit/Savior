using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RaiseEvent : byte
{
    DRAW_CARD_EVENT,
    SET_DATA_CARD_EVENT,
    MoveCardInTriggerSpell,
    SKIP_TURN,
    SWITCH_TURN,
    MOVE_FIGHTZONE,
    MOVE_SUMMONZONE,
    ATTACK,
    DEFENSE,
    UPDATE_DATA_MONSTER_EVENT,
    EFFECT_EXCUTE,
    EFFECT_UPDATE_STATUS,
    SET_DATA_SPELL_EVENT
}