using Photon.Realtime;
using SerializeReferenceEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GameComponent.Card.Logic.TargetObject.Target.AnyTarget
{
    [SRName("Target type/Any")]
    public class AnyTarget : AbstractTarget
    {
        public List<object> Execute(MatchManager instance)
        {
            List<object> result = new();
            var redPlayer = instance.redPlayer;
            var bluePlayer = instance.bluePlayer;

            //var redHand = redPlayer.hand.GetAllCardInHand();
            //var blueHand = bluePlayer.hand.GetAllCardInHand();

            var redFight = redPlayer.fightZones.Where(zone => zone.monsterCard != null).Select(zone => zone.monsterCard as CardBase).ToList();
            var blueFight = bluePlayer.fightZones.Where(zone => zone.monsterCard != null).Select(zone => zone.monsterCard as CardBase).ToList();

            var redSm = redPlayer.summonZones.Where(zone => zone.GetMonsterCard() != null).Select(zone => zone.GetMonsterCard() as CardBase).ToList();
            var blueSm = bluePlayer.summonZones.Where(zone => zone.GetMonsterCard() != null).Select(zone => zone.GetMonsterCard() as CardBase).ToList();

            result.Add(redPlayer);
            result.Add(bluePlayer);

            //result.AddRange(redHand);
            //result.AddRange(blueHand);

            result.AddRange(redFight);
            result.AddRange(blueFight);

            result.AddRange(redSm);
            result.AddRange(blueSm);

            return result;
        }
    }
}
