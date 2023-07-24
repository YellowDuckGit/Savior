using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EventID
{
    None,
    OnDragCardToPackDeck,
    OnPutCardInDeck,
    OnRemoveCardInDeck,
    OnChangeDeckName,
    OnChangeNumberCardInDeck,
    OnMoveCardToFightZone,
    OnMoveCardToSummonZone,
    OnStartTurn,
    ////Match 
    //OnBeginMatch,
    //OnMiddleMatch,
    //OnEndMatch,

    ////Round
    //OnBeginRound,
    //OnMiddleRound,
    //OnEndRound,

    /// <summary>
    /// card
    /// </summary>
    OnSummonMonster,
    AfterSummon,
    OnObjectSelected,
    EndMatch,
    EndAttackAndDefensePhase,
    OnEndTurn,
    OnEndRound,
    OnStartRound,
    OnUIClickCard,
    OnEnterCard,
    OnExitCard,
    OnCardUpdate,
    OnMoveToFightZone,
    OnMoveToSummonZone,
    OnMoveToGraveyard,
    OnMoveCardInTriggerSpell,
    OnLeftClickCard,
    OnRightClickCard,
    OnCardDamaged,
    OnStartMatch,
    OnEndMatch,
    OnCardAttack,
    OnRightClickHoverCard,
}
