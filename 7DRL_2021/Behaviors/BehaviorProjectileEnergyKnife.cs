using _7DRL_2021.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    class BehaviorProjectileEnergyKnife : Behavior, ITickable
    {
        public ICurio Curio;
        public BehaviorProjectile Projectile => Curio.GetBehavior<BehaviorProjectile>();

        public BehaviorProjectileEnergyKnife()
        {
        }

        public BehaviorProjectileEnergyKnife(ICurio curio)
        {
            Curio = curio;
        }

        public void Tick(SceneGame scene)
        {
            var tile = Curio.GetMainTile();

            if (tile != null)
            {
                if (tile.IsSolid())
                {
                    Curio.Destroy();
                }

                var targets = tile.Contents.Where(x => x.IsAlive() && x != Projectile.Shooter);
                if (targets.Any())
                {
                    var target = targets.First();
                    var actions = new List<ActionWrapper>();
                    actions.Add(new ActionEnergyKnifeHit(Curio, target).InSlot(ActionSlot.Active));
                    actions.Apply(target);
                    Curio.Destroy();
                }
            }
        }

        public override void Apply()
        {
            Curio.AddBehavior(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorProjectileEnergyKnife(curio), Curio);
        }
    }
}
