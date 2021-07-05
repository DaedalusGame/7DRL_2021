using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionFireStab : IActionHasOrigin
    {
        static Random Random = new Random();

        public bool Done => Stab.Done;
        public ICurio Origin { get; set; }

        public ActionSwordStab Stab;

        public ActionFireStab(ICurio origin, ActionSwordStab stab)
        {
            Origin = origin;
            Stab = stab;
        }

        public void Run()
        {
            Stab.Modifiers.Add(new ModifierBloodfire());
        }
    }
}
