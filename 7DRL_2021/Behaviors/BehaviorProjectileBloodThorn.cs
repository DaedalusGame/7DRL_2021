using _7DRL_2021.Events;
using _7DRL_2021.Results;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    class BehaviorProjectileBloodThorn : Behavior, ITickable
    {
        public ICurio Curio;
        public BehaviorProjectile Projectile => Curio.GetBehavior<BehaviorProjectile>();

        public BehaviorProjectileBloodThorn()
        {
        }

        public BehaviorProjectileBloodThorn(ICurio curio)
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
            }
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
            Apply(new BehaviorProjectileBloodThorn(curio), Curio);
        }

        [EventSubscribe]
        public void OnMove(EventMove.Finish e)
        {
            if (e.Curio != Curio)
                return;

            var orientable = Curio.GetBehavior<BehaviorOrientable>();

            var trail = new Curio(Template.BloodThornTrail);
            trail.MoveTo(e.Source);
            trail.GetBehavior<BehaviorTrailBloodThorn>().Shooter = Projectile.Shooter;
            new TrailParticle(Curio.GetWorld(), SpriteLoader.Instance.AddSprite("content/bloodthorn"), Curio.GetVisualTarget(), 30)
            {
                Angle = orientable.Angle,
                FadeLerp = LerpHelper.QuadraticIn,
            };
        }
    }

    class BehaviorTrailBloodThorn : Behavior, ITickable
    {
        public ICurio Curio;
        public ICurio Shooter;
        public Slider Lifetime = new Slider(20);

        public IFrameCollection AlreadyHit = new IFrameCollection();

        public BehaviorTrailBloodThorn()
        {
        }

        public BehaviorTrailBloodThorn(ICurio curio)
        {
            Curio = curio;
        }

        public void Tick(SceneGame scene)
        {
            var tile = Curio.GetMainTile();

            if (tile != null)
            {
                foreach(var target in tile.Contents.Where(x => x != Shooter && x.IsAlive()))
                {
                    if (!AlreadyHit.IsInvincible(target, 1))
                    {
                        var actions = new List<ActionWrapper>();
                        actions.Add(new ActionEnergyKnifeHit(Curio, target).InSlot(ActionSlot.Active));
                        actions.Apply(target);
                        AlreadyHit.Add(target);
                    }
                }
            }

            AlreadyHit.Tick(scene.TimeModCurrent);

            if (Lifetime.Done)
                Curio.Destroy();
            Lifetime += scene.TimeModCurrent;
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
            Apply(new BehaviorTrailBloodThorn(curio), Curio);
        }
    }
}
