using _7DRL_2021.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionKeepMoving : IActionHasOrigin, ITickable
    {
        public bool Done => true;

        public ICurio Origin
        {
            get;
            set;
        }
        public int Decelerate;
        public bool Stab;

        public ActionKeepMoving(ICurio origin, int decelerate = -1, bool stab = false)
        {
            Origin = origin;
            Stab = stab;
            Decelerate = decelerate;
        }

        public void Run()
        {
            TryMove();
        }

        public void Tick(SceneGame scene)
        {
            TryMove();
        }

        private void TryMove()
        {
            var passive = Origin.GetActionHolder(ActionSlot.Passive);
            var player = Origin.GetBehavior<BehaviorPlayer>();
            if (player != null && passive.Done)
            {
                var actions = new List<ActionWrapper>();
                if (Decelerate < 0)
                    actions.Add(new ActionChangeMomentum(Origin, Decelerate).InSlot(ActionSlot.Passive));
                player.AddDefaultMove(actions);
                if (Stab)
                    player.AddDefaultStab(actions);
                actions.Apply(Origin);
            }
        }
    }
}
