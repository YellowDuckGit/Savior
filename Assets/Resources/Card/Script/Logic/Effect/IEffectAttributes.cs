using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GameComponent.Card.Logic.Effect
{
    public interface IEffectAttributes
    {
        public bool IsCharming { get; set; }
        public bool IsTreating { get; set; }
        public bool IsDominating { get; set; }
        public bool IsBlockAttack { get; set; }
        public bool IsBlockDefend { get; set; }
    }
}
