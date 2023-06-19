using SerializeReferenceEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GameComponent.Card.Logic.TargetObject.Select
{
    [SRName("SelectFilter/Target/Player")]
    public class SelectTargetPlayer: AbstractSelectTargetObject
    {
        public enum PlayerTarget
        {
            You,
            Opponent
        }
        public PlayerTarget SelectTarget;
    }
}
