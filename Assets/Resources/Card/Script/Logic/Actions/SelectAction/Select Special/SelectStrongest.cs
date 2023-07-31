using Assets.GameComponent.Card.Logic.TargetObject.Target;
using Assets.GameComponent.Card.Logic.TargetObject.Target.CardTarget;
using Assets.GameComponent.Card.LogicCard;
using SerializeReferenceEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.GameComponent.Card.Logic.TargetObject.Select
{
    [SRName("Logic/Select/Other/Select Monster Strongest")]
    public class SelectStrongest : AbstractAction
    {
        /*
         * COMMON
         */
        [Header("COMMON")]
        public CardOwner owner;
        public CardPosition cardPosition;
        public Rarity Rarity;
        public RegionCard region;

        [SerializeReference]
        [SRLogicCard(typeof(AbstractEffect))]
        public List<AbstractEffect> Effects = null;

        public MonsterCard Execute(object registor, MatchManager instance)
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
                Debug.Log(this.debug("not found player selected"));
                return null;
            }

            /*
             * position filter
             */
            switch(cardPosition)
            {
                case CardPosition.Any:

                    break;
                case CardPosition.InDeck:
                    targets = player.deck.GetAll();
                    break;
                case CardPosition.InHand:
                    targets = player.hand.GetAllCardInHand();
                    break;
                case CardPosition.InFightField:
                    targets = player.fightZones.Where(zone => zone.monsterCard != null).Select(zone => zone.monsterCard as CardBase).ToList();
                    break;
                case CardPosition.InSummonField:
                    targets = player.summonZones.Where(zone => zone.GetMonsterCard() != null).Select(zone => zone.GetMonsterCard() as CardBase).ToList();
                    break;
                case CardPosition.InGraveyard:
                    break;
                case CardPosition.InTriggerSpellField:
                    break;
                default:
                    break;
            }
            /*
             * Rarity filter
             */
            if(Rarity != Rarity.Any)
            {
                targets.RemoveAll(card => card.RarityCard.ToString().CompareTo(Rarity.ToString()) != 0);
                if(targets.Count == 0)
                {
                    return null;
                }
                else if(targets.Count == 1)
                {
                    return targets[0] as MonsterCard;
                }
            }


            /*
             * Region filter
             */
            if(region != RegionCard.Any)
            {
                targets.RemoveAll(card => card.RegionCard.ToString().CompareTo(region.ToString()) != 0);
                if(targets.Count == 0)
                {
                    return null;
                }
                else if(targets.Count == 1)
                {
                    return targets[0] as MonsterCard;
                }//else continue
            }

            var monsters = targets.Select(card => card as MonsterCard).ToList();

            if(registor is MonsterCard regisM)
            {
                monsters.RemoveAll(monster => monster == regisM);
                if(monsters.Count == 0)
                {
                    return null;
                }
                else if(monsters.Count == 1)
                {
                    return monsters[0];
                }//else continue
            }

            int strongestAttack = monsters.Max(x => x.Attack);
            // lấy được list monster có ATK cao nhất
            List<MonsterCard> strongestATK = monsters.Where(x => x.Attack == strongestAttack).ToList();

            // nếu có nhiều monster có ATK == nhau or not
            MonsterCard strongestMonster;
            if(strongestATK.Count == 1)
            {
                strongestMonster = strongestATK[0];
            }
            else
            { //xét đến monster có HP cao nhất
                int strongestHp = monsters.Max(x => x.Hp);
                List<MonsterCard> strongestHP = monsters.Where(x => x.Hp == strongestHp).ToList();


                if(strongestHP.Count == 1)
                {
                    strongestMonster = strongestHP[0];
                }
                else
                { // nếu có 2 mons có HP bằng nhau, thì lấy mons đầu tiên trong list
                    strongestMonster = strongestHP[0];
                }
            }
            return strongestMonster;
        }
    }
}
