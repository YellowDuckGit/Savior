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
    OnCardSelected,
    EndMatch,
    EndAttackAndDefensePhase,
    OnEndTurn,
    OnEndRound,
    OnStartRound,
    OnClickCard,
    OnEnterCard,
    OnExitCard,
    OnCardUpdate,
    OnMoveToFightZone,
    OnMoveToSummonZone,
    OnMoveToGraveyard,
    OnMoveCardInTriggerSpell,
}
