using _7DRL_2021.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionClearStab : IActionHasOrigin
    {
        public ICurio Origin { get; set; }

        public bool Done => true;

        public ActionClearStab(ICurio origin)
        {
            Origin = origin;
        }

        public void Run()
        {
            var sword = Origin.GetBehavior<BehaviorSword>();
            if(sword != null)
                sword.StabTargets.Clear();
        }
    }
}
