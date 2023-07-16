using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Card
{
    public interface IMonsterCard : IMonsterData
    {
        public int DefaultHp { get; set; }
    }
}