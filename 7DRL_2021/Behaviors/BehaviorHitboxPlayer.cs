using _7DRL_2021.Events;
using _7DRL_2021.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    class BehaviorHitboxPlayer : Behavior
    {
        public ICurio Curio;

        public BehaviorHitboxPlayer()
        {
        }

        public BehaviorHitboxPlayer(ICurio curio)
        {
            Curio = curio;
        }

        public override void Apply()
        {
            Curio.AddBehavior(this);
            EventBus.Register(this);
        }

        public override void Remove()
        {
            base.Remove();
            EventBus.Unregister(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorHitboxPlayer(curio));
        }

        [EventSubscribe]
        public void OnAction(EventAction e)
        {
            var hits = e.Actions.GetEffectsTargetting<ActionEnemyHit>(Curio).ToList();
            var alive = Curio.GetBehavior<BehaviorAlive>();

            foreach (var hit in hits)
            {
                if (alive.Armor <= 0)
                {
                    e.Actions.Add(new ActionPlayerDamage(hit.Origin, hit.Target, 1).InSlot(ActionSlot.Active));
                }
                else
                {
                    e.Actions.Add(new ActionDestroyArmor(hit.Origin, hit.Target, 1, 0).InSlot(ActionSlot.Active));
                }
            }
        }
    }
}
