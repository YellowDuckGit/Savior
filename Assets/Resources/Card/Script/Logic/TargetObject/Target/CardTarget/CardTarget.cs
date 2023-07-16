using Assets.GameComponent.Card.LogicCard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Assets.GameComponent.Card.Logic.Have.Have;
using UnityEngine;
using SerializeReferenceEditor;
using static Assets.GameComponent.Card.Logic.TargetObject.Target.AbstractTarget.AbstractTargetDataType;
using Unity.VisualScripting;
using static Assets.GameComponent.Card.Logic.TargetObject.Target.AbstractTarget.AbstractTargetDataType.AbstractTargetDataTypeValue;
using System.Drawing.Printing;
using static UnityEngine.Rendering.DebugUI;
using Assets.GameComponent.Card.Logic.Effect;
using System.Security.Policy;

namespace Assets.GameComponent.Card.Logic.TargetObject.Target.CardTarget
{

    [Serializable]
    [SRName("Target type/Card")]
    public class CardTarget : AbstractTarget
    {
        /*
         * COMMON
         * - Card owner
         * - Position
         * - RarityCard
         * - Region card
         * TYPE
         * 1. MonsterTarget
         * - Cost
         * - HP
         * _ Attribute
         * 2. SpellTarget
         * - Cost
         */

        /*
         * COMMON
         */
        [Header("COMMON")]
        public CardOwner owner = CardOwner.You;
        public CardPosition cardPosition = CardPosition.Any;
        public Rarity Rarity = Rarity.Any;
        public RegionCard region = RegionCard.Any;

        /*
         * TYPE
         */
        [Header("CARD TYPE")]
        [SerializeReference]
        [SRLogicCard(typeof(TargetCardType))]
        public TargetCardType CardType = null;

        [HideInInspector]
        public MatchManager match = null;

        public List<CardBase> Execute(MatchManager instance)
        {
            List<CardBase> targets = new();
            CardPlayer player = null;
            if(owner == CardOwner.You)
            {
                player = instance.LocalPlayer;
            }
            else if(owner == CardOwner.Opponent)
            {
                player = instance.OpponentPlayer;
            }
            else
            {
                Debug.Log(this.debug("not found player selected", new
                {
                    owner,
                    cardPosition
                }));
                return targets;
            }

            /*
             * position filter
             */
            switch(cardPosition)
            {
                case CardPosition.Any:
                    break;
                case CardPosition.InDeck:
                    targets.AddRange(player.deck.GetAll());
                    break;
                case CardPosition.InHand:
                    targets.AddRange(player.hand.GetAllCardInHand());
                    break;
                case CardPosition.InFightField:
                    var listcardInFight = player.fightZones.Where(zone => zone.isFilled() && zone.monsterCard != null).Select(zone => zone.monsterCard as CardBase).ToList();
                    targets.AddRange(listcardInFight);
                    break;
                case CardPosition.InSummonField:
                    var listcardInSummon = player.summonZones.Where(zone => zone.isFilled() && zone.GetMonsterCard() != null).Select(zone => zone.GetMonsterCard() as CardBase).ToList();
                    targets.AddRange(listcardInSummon);
                    break;
                case CardPosition.InGraveyard:
                    break;
                case CardPosition.InTriggerSpellField:
                    break;
                default:
                    Debug.Log(this.debug("not found position selected", new
                    {
                        owner,
                        cardPosition
                    }));
                    return targets;
            }
            /*
             * Rarity filter
             */
            if(Rarity != Rarity.Any)
            {
                targets.RemoveAll(card => card.RarityCard.ToString().CompareTo(Rarity.ToString()) != 0);
            }


            /*
             * Region filter
             */
            if(region != RegionCard.Any)
            {
                targets.RemoveAll(card => card.RegionCard.ToString().CompareTo(region.ToString()) != 0);
            }
            if(CardType != null)
            {
                if(targets.Count != 0)
                {
                    CardType.Execute(targets);
                }
                else
                {
                    Debug.Log(this.debug("Empty target"));
                }

            }
            else
            {
                Debug.Log(this.debug("CardType being null"));
            }

            return targets;

        }

        //public override bool isMatch(object target)
        //{
        //    throw new NotImplementedException();
        //}


        /* 
         * Initial
         */
        [Serializable]
        public abstract class TargetCardType
        {
            public abstract void Execute(List<CardBase> targets);

            [SRName("Type/Monster")]
            public class TargetCardMonster : TargetCardType
            {
                [Header("OPTION")]
                [SerializeReference]
                [SRLogicCard(typeof(TargetCardMonsterOption))]
                public List<TargetCardMonsterOption> monsterOptions = null;

                public override void Execute(List<CardBase> targets)
                {
                    /*
                     * remove all card not monster
                     */

                    targets.RemoveAll(card => card is MonsterCard == false);


                    /*
                     * Execute option filter
                     */

                    foreach(var option in monsterOptions)
                    {
                        option.Execute(targets);
                    }
                }

                [Serializable]
                public abstract class TargetCardMonsterOption
                {
                    public abstract void Execute(List<CardBase> targets);

                    /*
                    * Cost
                    * HP
                    * Attribute
                    */
                    [SRName("Monster Option/Cost")]
                    public class TargetCardMonsterOptCost : TargetCardMonsterOption
                    {
                        [SerializeReference]
                        [SRLogicCard(typeof(AbstractTargetDataTypeValue))]
                        public AbstractTargetDataTypeValue value = null;

                        public override void Execute(List<CardBase> targets)
                        {
                            if(value != null)
                            {
                                value.Execute(targets, (MonsterCard card) => card.Cost);
                            }
                        }
                    }
                    [SRName("Monster Option/HP")]
                    public class TargetCardMonsterOptHp : TargetCardMonsterOption
                    {
                        [SerializeReference]
                        [SRLogicCard(typeof(AbstractTargetDataTypeValue))]
                        public AbstractTargetDataTypeValue value = null;

                        public override void Execute(List<CardBase> targets)
                        {
                            if(value != null)
                            {
                                value.Execute(targets, (MonsterCard card) => card.Hp);
                            }
                        }
                    }
                    [SRName("Monster Option/Attribute")]
                    public class TargetCardMonsterOptAttribute : TargetCardMonsterOption
                    {
                        public AbstractTargetDataTypeAttribute attributes = null;

                        public override void Execute(List<CardBase> targets)
                        {
                            if(attributes != null)
                            {
                                attributes.Execute<MonsterCard, CardBase>(targets);
                            }
                        }
                    }
                }
            }
            [SRName("Type/Spell")]
            public class TargetCardSpell : TargetCardType
            {
                [Header("OPTION")]
                [SerializeReference]
                [SRLogicCard(typeof(TargetCardSpellOption))]
                public List<TargetCardSpellOption> spellOption = null;

                public override void Execute(List<CardBase> targets)
                {
                    /*
                    * remove all card not monster
                    */

                    targets.RemoveAll(card => card is SpellCard == false);


                    /*
                     * Execute option filter
                     */

                    foreach(var option in spellOption)
                    {
                        option.Execute(targets);
                    }
                }

                [Serializable]
                public abstract class TargetCardSpellOption
                {

                    public abstract void Execute(List<CardBase> targets);
                    /*
                     * Cost
                     */
                    [SRName("Spell Option/Cost")]
                    public class TargetCardSpellOptCost : TargetCardSpellOption
                    {
                        [SerializeReference]
                        [SRLogicCard(typeof(AbstractTargetDataTypeValue))]
                        public AbstractTargetDataTypeValue value = null;

                        public override void Execute(List<CardBase> targets)
                        {
                            if(value != null)
                            {
                                value.Execute(targets, (SpellCard card) => card.Cost);
                            }
                        }
                    }


                }
            }
        }
    }
}
