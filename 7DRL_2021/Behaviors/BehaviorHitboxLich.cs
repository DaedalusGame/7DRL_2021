using _7DRL_2021.Events;
using _7DRL_2021.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    class BehaviorHitboxLich : Behavior
    {
        public ICurio Curio;

        public BehaviorHitboxLich()
        {
        }

        public BehaviorHitboxLich(ICurio curio)
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
            Apply(new BehaviorHitboxLich(curio));
        }

        [EventSubscribe]
        public void OnAction(EventAction e)
        {
            var hits = e.Actions.GetEffectsTargetting<IWeaponHit>(Curio).ToList();
            var alive = Curio.GetBehavior<BehaviorAlive>();

            foreach (var hit in hits)
            {
                if (hit is ActionEnemyHit)
                    continue;

                if (hit is ActionStabHit stab)
                {
                    e.Actions.Add(new ActionHitVisual(hit.Origin, hit.Target, SoundLoader.AddSound("content/sound/stab.wav")).InSlot(ActionSlot.Active));
                    e.Actions.Add(new ActionStabStuck(hit.Origin, hit.Target).InSlot(ActionSlot.Active));
                }
                else
                {
                    //Crickets
                }
            }
        }
    }
}
