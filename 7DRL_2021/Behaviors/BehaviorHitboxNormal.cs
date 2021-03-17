using _7DRL_2021.Events;
using _7DRL_2021.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    class BehaviorHitboxNormal : Behavior
    {
        public ICurio Curio;

        public BehaviorHitboxNormal()
        {
        }

        public BehaviorHitboxNormal(ICurio curio)
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
            Apply(new BehaviorHitboxNormal(curio));
        }

        [EventSubscribe]
        public void OnAction(EventAction e)
        {
            var hits = e.Actions.GetEffectsTargetting<IWeaponHit>(Curio).ToList();
            var alive = Curio.GetBehavior<BehaviorAlive>();

            foreach(var hit in hits)
            {
                if (hit is ActionEnemyHit)
                    continue;

                if(hit is ActionStabHit stab)
                {
                    if (alive.Armor <= 0)
                    {
                        e.Actions.Add(new ActionDamage(hit.Origin, hit.Target, 1, 50, SoundLoader.AddSound("content/sound/stab.wav")).InSlot(ActionSlot.Active));
                        e.Actions.Add(new ActionStabStuck(hit.Origin, hit.Target).InSlot(ActionSlot.Active));
                    }
                    else
                    {
                        e.Actions.Add(new ActionParrySword(hit.Origin, hit.Target).InSlot(ActionSlot.Active));
                    }
                }
                else
                {
                    if (alive.Armor <= 0)
                    {
                        e.Actions.Add(new ActionDamage(hit.Origin, hit.Target, 1, 50, SoundLoader.AddSound("content/sound/hit.wav")).InSlot(ActionSlot.Active));
                    }
                    else
                    {
                        e.Actions.Add(new ActionDestroyArmor(hit.Origin, hit.Target, 1, 50).InSlot(ActionSlot.Active));
                    }
                }
            }
        }
    }
}
