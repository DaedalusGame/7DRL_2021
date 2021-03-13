using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionEnemyHit : IWeaponHit
    {
        public ICurio Origin { get; set; }
        public ICurio Target { get; set; }

        public bool Done => true;

        public ActionEnemyHit(ICurio origin, ICurio target)
        {
            Origin = origin;
            Target = target;
        }

        public void Run()
        {
            //NOOP
        }
    }
}
