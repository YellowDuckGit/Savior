using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EventGameHandler
{
    #region Card
    #region Base Card
    public delegate void OnNameChange(string name);
    public delegate void OnCostChange(int cost);
    public delegate void OnDescriptionChange(string description);
    public delegate void OnPositionChange(CardPosition position);
    public delegate void OnSelectChange(bool value);
    public delegate void OnavatarChange(Material material);
    public delegate void OnFocusChange(bool value);
    #endregion

    #region Monster Card
    public delegate void OnAttactChange(int value);
    public delegate void OnPlay();
    public delegate bool OnSummon(SummonZone summonField, SummonArgs args);//check
    public delegate void OnSummoned();
    public delegate void OnHpChange(int value);
    public delegate void OnMoveToGraveyard();
    public delegate void OnMoveToFightZone(FightZone fightZone);
    public delegate void OnMoveToSummonZone(FightZone fightZone, SummonZone summonZone);
    #endregion
    #endregion

    #region Match
    public delegate void OnPlayCard(CardBase card, PlayArgs args);
    public delegate void OnSummonCard(SummonZone zone, SummonArgs args);
    public class SummonArgs
    {
        public MonsterCard card;
        public SummonZone summonZone;
    }
    #endregion
    public class PlayArgs
    {
        public CardBase card;
        public FightZone fightZone;
        public SummonZone summonZone;
    }
}

