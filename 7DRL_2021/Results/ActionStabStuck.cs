using _7DRL_2021.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionStabStuck : IActionPaired
    {
        public ICurio Origin { get; set; }
        public ICurio Target { get; set; }

        public bool Done => true;

        public ActionStabStuck(ICurio origin, ICurio target)
        {
            Origin = origin;
            Target = target;
        }

        public void Run()
        {
            var sword = Origin.GetBehavior<BehaviorSword>();
            if (sword != null)
            {
                sword.StabTargets.Add(Target);
                sword.HasBlood = true;
            }
        }
    }
}
