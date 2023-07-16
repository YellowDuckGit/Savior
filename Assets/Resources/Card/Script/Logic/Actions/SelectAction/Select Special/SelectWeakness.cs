using Assets.GameComponent.Card.LogicCard;
using SerializeReferenceEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.GameComponent.Card.Logic.Actions.SelectAction.Select_Special
{
    [SRName("Logic/Select/Other/Select Monster Weakness")]

    public class SelectWeakness : AbstractAction
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
            }


            /*
             * Region filter
             */
            if(region != RegionCard.Any)
            {
                targets.RemoveAll(card => card.RegionCard.ToString().CompareTo(region.ToString()) != 0);
            }

            var monsters = targets.Select(card => card as MonsterCard).ToList();

            if(registor is MonsterCard regisM)
            {
                monsters.RemoveAll(monster => monster == regisM);
            }

            int weaknessAtk = monsters.Min(x => x.Attack);
            // lấy được list monster có ATK cao nhất
            List<MonsterCard> weaknessMonsters_ATK = monsters.Where(x => x.Attack == weaknessAtk).ToList();

            // nếu có nhiều monster có ATK == nhau or not
            MonsterCard weaknessMonster;
            if(weaknessMonsters_ATK.Count == 1)
            {
                weaknessMonster = weaknessMonsters_ATK[0];
            }
            else
            { //xét đến monster có HP cao nhất
                int lowestHp = monsters.Min(x => x.Hp);
                List<MonsterCard> lowestMonsters_HP = monsters.Where(x => x.Hp == lowestHp).ToList();


                if(lowestMonsters_HP.Count == 1)
                {
                    weaknessMonster = lowestMonsters_HP[0];
                }
                else
                { // nếu có 2 mons có HP bằng nhau, thì lấy mons đầu tiên trong list
                    weaknessMonster = lowestMonsters_HP[0];
                }
            }
            return weaknessMonster;
        }
    }
}
