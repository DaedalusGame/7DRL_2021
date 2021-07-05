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
        public int SlashStart => Slash.SlashStart;
        public int SlashEnd => Slash.SlashEnd;
        public ActionSwordSlash Slash;

        public bool Done => true;

        public ActionSlashHit(ICurio origin, ICurio target, ActionSwordSlash slash)
        {
            Origin = origin;
            Target = target;
            Slash = slash;
        }

        public void Run()
        {
            //NOOP
        }
    }
}
