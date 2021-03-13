using _7DRL_2021.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    //This has high deadlock potential
    class ActionWaitForAction : IActionHasOrigin
    {
        public List<IAction> Actions;
        public bool Done => Actions.Any() && Actions.All(x => x.Done);

        public ICurio Origin { get; set; }
        public ActionSlot Slot;

        public ActionWaitForAction(ICurio curio, ActionSlot slot)
        {
            Origin = curio;
            Slot = slot;
        }

        public void Run()
        {
            var holder = Origin.GetActionHolder(Slot);
            Actions = holder.CurrentActions.ToList();
        }
    }
}
