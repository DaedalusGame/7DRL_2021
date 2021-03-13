using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionSlashHit : IWeaponHit
    {
        public ICurio Origin { get; set; }
        public ICurio Target { get; set; }
        public int SlashStart, SlashEnd;

        public bool Done => true;

        public ActionSlashHit(ICurio origin, ICurio target, int slashStart, int slashEnd)
        {
            Origin = origin;
            Target = target;
            SlashStart = slashStart;
            SlashEnd = slashEnd;
        }

        public void Run()
        {
            //NOOP
        }
    }
}
