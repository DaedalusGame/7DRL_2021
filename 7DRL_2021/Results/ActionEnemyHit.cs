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

        public SoundReference HitSound;

        public ActionEnemyHit(ICurio origin, ICurio target, SoundReference hitSound)
        {
            Origin = origin;
            Target = target;
            HitSound = hitSound;
        }

        public void Run()
        {
            //NOOP
        }
    }
}
