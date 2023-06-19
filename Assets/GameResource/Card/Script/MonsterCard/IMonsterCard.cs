using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Card
{
    public interface IMonsterCard : IMonsterData
    {
        public MonsterData BaseMonsterData { get; set; }
        public int DefaultHp { get; set; }
    }
}