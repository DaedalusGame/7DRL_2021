using _7DRL_2021.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionWrapper
    {
        public ActionSlot Slot;
        public IAction Action;

        public ActionWrapper(IAction action, ActionSlot slot)
        {
            Slot = slot;
            Action = action;
        }
    }

    interface IAction
    {
        bool Done { get; }

        void Run();
    }

    interface IActionHasOrigin : IAction
    {
        ICurio Origin
        {
            get;
        }
    }

    interface IActionHasTarget : IAction
    {
        ICurio Target
        {
            get;
        }
    }

    interface IActionPaired : IActionHasOrigin, IActionHasTarget
    {
    }
}
