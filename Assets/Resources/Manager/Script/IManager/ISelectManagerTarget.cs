using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GameComponent.Manager.IManager
{
    public interface ISelectManagerTarget
    {
        public bool IsSelectAble { get; set; }
        public bool IsSelected { get; set; }
    }
}
