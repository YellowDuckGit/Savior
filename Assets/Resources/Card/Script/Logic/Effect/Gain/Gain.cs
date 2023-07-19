using Assets.GameComponent.Card.Logic.Effect.Store;
using Assets.GameComponent.Card.Logic.TargetObject.Select;
using Assets.GameComponent.Card.LogicCard;
using Assets.GameComponent.Card.LogicCard.ListLogic.Effect;
using JetBrains.Annotations;
using SerializeReferenceEditor;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor.Rendering;
using UnityEngine;
using static Assets.GameComponent.Card.Logic.Effect.Gain.Gain.GainData;
using static Assets.GameComponent.Card.Logic.Effect.Gain.Gain.GainData.GainMonsterStats.GainMonsterStatsData;
using static UnityEngine.Rendering.DebugUI;

namespace Assets.GameComponent.Card.Logic.Effect.Gain
{
    [SRName("Logic/Effect/Gain")]
    public class Gain : AbstractEffect, IInturnEffect
    {

        [SerializeReference]
        [SRLogicCard(typeof(GainData))]
        public GainData _Gain = null;

        public bool InTurn;

        [Serializable]
        public abstract class GainData : AbstractData
        {
            [SRName("Gain/Attribute")]
            public class GainMonsterAttribute : GainData, IEffectAttributes
            {
                public bool _IsCharming;
                public bool IsCharming
                {
                    get; set;
                }

                public bool _IsTreating;
                public bool IsTreating
                {
                    get; set;
                }

                public bool _IsDominating;
                public bool IsDominating
                {
                    get; set;
                }

                public bool _IsBlockAttack;
                public bool IsBlockAttack
                {
                    get; set;
                }

                public bool _IsBlockDefend;
                public bool IsBlockDefend
                {
                    get; set;
                }
            }
            [SRName("Gain/Stats")]
            public class GainMonsterStats : GainData
            {
                [SerializeReference]
                [SRLogicCard(typeof(GainMonsterStatsData))]
                public GainMonsterStatsData _Stats = null;

                [Serializable]
                public abstract class GainMonsterStatsData
                {
                    [SRName("Stats/HP")]
                    public class GainMonsterStatsHP : GainMonsterStatsData
                    {
                        [SerializeReference]
                        [SRLogicCard(typeof(GainMonsterStatsValue))]
                        public GainMonsterStatsValue _HP = null;

                        public override void Execute(object registor, MonsterCard monster)
                        {
                            _HP.Execute(registor, this, monster);
                        }
                    }
                    [SRName("Stats/Attack")]
                    public class GainMonsterStatsAttack : GainMonsterStatsData
                    {
                        [SerializeReference]
                        [SRLogicCard(typeof(GainMonsterStatsValue))]
                        public GainMonsterStatsValue _ATTACK = null;

                        public override void Execute(object registor, MonsterCard monster)
                        {
                            _ATTACK.Execute(registor, this, monster);
                        }
                    }
                    [Serializable]
                    public abstract class GainMonsterStatsValue
                    {

                        public abstract void Execute(object register, GainMonsterStatsData data, MonsterCard target);
                        [SRName("Type/Number")]
                        public class GainMonsterStatsValueNumber : GainMonsterStatsValue
                        {
                            public int value;

                            public override void Execute(object register, GainMonsterStatsData data, MonsterCard target)
                            {
                                if(data is GainMonsterStatsHP gainHP)
                                {
                                    target.Hp = value;
                                }
                                else if(data is GainMonsterStatsAttack gainAttack)
                                {
                                    target.Attack = value;
                                }
                            }
                        }
                        [SRName("Type/Stored")]
                        public class GainMonsterStatsValueStore : GainMonsterStatsValue
                        {
                            public bool RemoveAfterUse = true;
                            public int value
                            {
                                get; private set;
                            }


                            public override void Execute(object register, GainMonsterStatsData data, MonsterCard target)
                            {
                                if(register is MonsterCard monsterCard)
                                {
                                    if(TempStore.isContaint(monsterCard.photonView.ViewID))
                                    {
                                        MonsterCard storeObject = TempStore.GetFromStore(monsterCard.photonView.ViewID) as MonsterCard;
                                        if(data is GainMonsterStatsHP gainHP)
                                        {
                                            value = storeObject.Hp;
                                        }
                                        else if(data is GainMonsterStatsAttack gainAttack)
                                        {
                                            value = storeObject.Attack;
                                        }
                                    }

                                }

                            }
                        }

                    }

                    public abstract void Execute(object registor, MonsterCard monster);
                }

                public void Execute(object registor, MonsterCard monster)
                {
                    if(_Stats != null)
                    {
                        _Stats.Execute(registor, monster);
                    }
                }
            }
        }



        public override bool GainEffect(object register, EffectManager match)
        {
            if(_Gain is GainMonsterAttribute gainAttr)
            {
                if(gainAttr._IsCharming)
                {
                    if(register is MonsterCard monster)
                    {
                        monster.IsCharming = true;
                        monster.EffectSContain.Add(this);
                        return true;
                    }

                    return false;
                }

                if(gainAttr._IsTreating)
                {
                    if(register is MonsterCard monster)
                    {
                        monster.IsTreating = true;
                        this.debug("monster is treating: " + monster.IsTreating);
                        monster.EffectSContain.Add(this);
                        return true;
                    }

                    return false;
                }
                if(gainAttr._IsDominating)
                {
                    if(register is MonsterCard monster)
                    {
                        monster.IsDominating = true;
                        monster.EffectSContain.Add(this);
                        return true;
                    }
                    return false;
                }
                if(gainAttr._IsBlockAttack)
                {
                    if(register is MonsterCard monster)
                    {
                        monster.IsBlockAttack = true;
                        monster.EffectSContain.Add(this);
                        return true;
                    }
                    return false;
                }
                if(gainAttr._IsBlockDefend)
                {
                    if(register is MonsterCard monster)
                    {
                        monster.IsBlockDefend = true;
                        monster.EffectSContain.Add(this);
                        return true;
                    }
                    return false;
                }
            }
            else if(_Gain is GainMonsterStats gainStarts)
            {
                if(register is MonsterCard monster)
                {
                    gainStarts.Execute(register, monster);
                }
            }
            return false;

        }

        public override void RevokeEffect(object register, MatchManager match)
        {
            if(InTurn)
            {
                if(_Gain is GainMonsterAttribute gainAttr)
                {
                    if(gainAttr._IsCharming)
                    {
                        if(register is MonsterCard monster)
                        {
                            monster.EffectSContain.Remove(this);
                            monster.IsCharming = false;

                        }

                    }

                    if(gainAttr._IsTreating)
                    {
                        if(register is MonsterCard monster)
                        {
                            monster.EffectSContain.Remove(this);
                            monster.IsTreating = false;
                            this.debug("monster is treating: " + monster.IsTreating);

                        }


                    }
                    if(gainAttr._IsDominating)
                    {
                        if(register is MonsterCard monster)
                        {
                            monster.EffectSContain.Remove(this);
                            monster.IsDominating = false;

                        }

                    }
                    if(gainAttr._IsBlockAttack)
                    {
                        if(register is MonsterCard monster)
                        {
                            monster.EffectSContain.Remove(this);
                            monster.IsBlockAttack = false;

                        }

                    }
                    if(gainAttr._IsBlockDefend)
                    {
                        if(register is MonsterCard monster)
                        {
                            monster.EffectSContain.Remove(this);
                            monster.IsBlockDefend = false;

                        }

                    }
                }
                else if(_Gain is GainMonsterStats gainStarts)
                {
                    if(register is MonsterCard monster)
                    {
                        gainStarts.Execute(register, monster);
                    }
                }

            }

        }
    }
}
