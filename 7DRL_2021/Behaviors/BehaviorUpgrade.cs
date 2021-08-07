using _7DRL_2021.Events;
using _7DRL_2021.Results;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    abstract class BehaviorUpgrade : Behavior
    {
        public ICurio Curio;

        public BehaviorUpgrade()
        {
        }

        public BehaviorUpgrade(ICurio curio)
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
    }

    class BehaviorDoubleHeal : BehaviorUpgrade
    {

        public BehaviorDoubleHeal() : base()
        {
        }

        public BehaviorDoubleHeal(ICurio curio) : base(curio)
        {
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorDoubleHeal(curio), Curio);
        }

        [EventSubscribe]
        public void OnAction(EventAction e)
        {
            var heartConsumes = e.Actions.GetEffectsBy<ActionConsumeHeart>(Curio);
            foreach (var heartConsume in heartConsumes)
            {
                heartConsume.Heal *= 2;
            }
        }
    }

    class BehaviorSkillDestructionWave : BehaviorUpgrade
    {

        public BehaviorSkillDestructionWave() : base()
        {
        }

        public BehaviorSkillDestructionWave(ICurio curio) : base(curio)
        {
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorSkillDestructionWave(curio), Curio);
        }

        [EventSubscribe]
        public void OnAction(EventAction e)
        {
            var sword = Curio.GetBehavior<BehaviorSword>();
            if (sword == null || !sword.HasHeart)
                return;
            var slashes = e.Actions.GetEffectsBy<ActionSwordSlash>(Curio);
            foreach (var slash in slashes.ToList())
            {
                if(Math.Abs(slash.SlashStart - slash.SlashEnd) == 6)
                {
                    e.Actions.Add(new ActionDestructionWave(slash.Origin, 4, 500).InSlot(ActionSlot.Active));
                }
            }
        }
    }

    class BehaviorSkillButterflyKnives : BehaviorUpgrade
    {

        public BehaviorSkillButterflyKnives() : base()
        {
        }

        public BehaviorSkillButterflyKnives(ICurio curio) : base(curio)
        {
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorSkillButterflyKnives(curio), Curio);
        }

        [EventSubscribe]
        public void OnAction(EventAction e)
        {
            var sword = Curio.GetBehavior<BehaviorSword>();
            if (sword == null)
                return;
            var slashes = e.Actions.GetEffectsBy<ActionSwordSlash>(Curio);
            foreach (var slash in slashes.ToList())
            {
                if (slash.SlashStart == 0 && slash.SlashEnd == -3)
                {
                    e.Actions.Add(new ActionButterflyKnives(slash.Origin, -1, -3).InSlot(ActionSlot.Active));
                }
                if (slash.SlashStart == 0 && slash.SlashEnd == 3)
                {
                    e.Actions.Add(new ActionButterflyKnives(slash.Origin, 1, 3).InSlot(ActionSlot.Active));
                }
            }
        }
    }

    class BehaviorSkillBloodThorn : BehaviorUpgrade
    {

        public BehaviorSkillBloodThorn() : base()
        {
        }

        public BehaviorSkillBloodThorn(ICurio curio) : base(curio)
        {
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorSkillBloodThorn(curio), Curio);
        }

        [EventSubscribe]
        public void OnAction(EventAction e)
        {
            var sword = Curio.GetBehavior<BehaviorSword>();
            if (sword == null || !sword.HasBlood)
                return;
            var slashes = e.Actions.GetEffectsBy<ActionSwordSlash>(Curio);
            foreach (var slash in slashes.ToList())
            {
                e.Actions.Insert(0, new ActionBloodThorn(slash.Origin, slash.SlashStart, slash.SlashEnd).InSlot(ActionSlot.Active));
            }
        }
    }

    class BehaviorSkillVampireBlade : BehaviorUpgrade
    {

        public BehaviorSkillVampireBlade() : base()
        {
        }

        public BehaviorSkillVampireBlade(ICurio curio) : base(curio)
        {
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorSkillVampireBlade(curio), Curio);
        }

        [EventSubscribe]
        public void OnAction(EventAction e)
        {
            var sword = Curio.GetBehavior<BehaviorSword>();
            if (sword == null || !sword.HasBlood)
                return;
            var slashes = e.Actions.GetEffectsBy<ActionConsumeHeartSword>(Curio);
            if(slashes.Any())
            {
                e.Actions.Insert(0, new ActionConsumeBlood(Curio, 1).InSlot(ActionSlot.Active));
                e.Actions.RemoveAll(x => x.Action is ActionConsumeHeartSword);
            }
        }
    }

    class BehaviorSkillBloodfireBlade : BehaviorUpgrade, ITickable
    {
        Random Random = new Random();
        Slider FrameVisual = new Slider(1);
        Slider FrameCharge = new Slider(20);

        bool Enflamed => FrameCharge.Done;

        public BehaviorSkillBloodfireBlade() : base()
        {
        }

        public BehaviorSkillBloodfireBlade(ICurio curio) : base(curio)
        {
        }

        public void Extinguish()
        {
            FrameCharge.Time = 0;
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorSkillBloodfireBlade(curio), Curio);
        }

        public void Tick(SceneGame scene)
        {
            var sword = Curio.GetBehavior<BehaviorSword>();
            var enflamed = Enflamed;

            if (sword != null /*&& sword.HasBlood*/)
                FrameCharge += scene.TimeModCurrent;
            else
                Extinguish();

            if (Enflamed)
            {
                if (FrameVisual.Done)
                {
                    if (sword != null)
                    {
                        var particle = new ExplosionParticle(scene, SpriteLoader.Instance.AddSprite("content/effect_explosion"), sword.GetBlade(8), Random.Next(10, 20))
                        {
                            Angle = Curio.GetVisualAngle() + sword.VisualAngle() + Random.NextFloat(-0.3f, +0.3f),
                            Color = Color.White,
                            DrawPass = DrawPass.EffectAdditive,
                        };
                        particle.Size.Set(Random.NextFloat(0.5f, 1.0f));
                    }
                    FrameVisual.Reset();
                }
                FrameVisual += scene.TimeModCurrent;
            }
        }

        [EventSubscribe]
        public void OnAction(EventAction e)
        {
            var sword = Curio.GetBehavior<BehaviorSword>();
            if (sword == null || !Enflamed)
                return;
            var slashes = e.Actions.GetEffectsBy<ActionSwordSlash>(Curio);
            var stabs = e.Actions.GetEffectsBy<ActionSwordStab>(Curio);

            foreach (var slash in slashes.ToList())
            {
                e.Actions.Insert(0, new ActionFireSlash(Curio, slash).InSlot(ActionSlot.Active));
            }
            
            foreach (var stab in stabs.ToList())
            {
                e.Actions.Insert(0, new ActionFireStab(Curio, stab).InSlot(ActionSlot.Active));
            }
        }

        [EventSubscribe(priority: 50)]
        public void OnSlash(EventAction e)
        {
            var slashHits = e.Actions.GetEffectsBy<ActionSlashHit>(Curio);
            var damages = e.Actions.GetEffectsBy<ActionDamage>(Curio);

            if (!slashHits.Any(x => x.Slash.Modifiers.Any(y => y is ModifierBloodfire)))
                return;

            foreach(var damage in damages)
            {
                damage.Blood = 0;
                damage.Damage += 1;
            }
        }

        [EventSubscribe(priority: 50)]
        public void OnStab(EventAction e)
        {
            var stabHits = e.Actions.GetEffectsBy<ActionStabHit>(Curio);
            var damages = e.Actions.GetEffectsBy<ActionDamage>(Curio);

            if (!stabHits.Any(x => x.Stab.Modifiers.Any(y => y is ModifierBloodfire)))
                return;

            foreach (var stabHit in stabHits.ToList())
            {
                if (e.Actions.Any(x => x.Action is ActionDamage))
                {
                    e.Actions.RemoveAll(x => x.Action is ActionDamage);

                    var actions = new List<ActionWrapper>()
                    {
                        new ActionHeartBurn(stabHit.Origin, stabHit.Target, 1000).InSlot(ActionSlot.Active),
                    };
                    actions.Apply(stabHit.Origin);
                }
            }
        }
    }

    class BehaviorOmicron : BehaviorUpgrade
    {

        public BehaviorOmicron() : base()
        {
        }

        public BehaviorOmicron(ICurio curio) : base(curio)
        {
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorOmicron(curio), Curio);
        }

        [EventSubscribe]
        public void OnAction(EventAction e)
        {
            if (Curio.IsTemplate())
                return;
            e.Actions.RemoveAll(x => x.Action is ActionNemesisRevive);
        }
    }

    class BehaviorDoom : BehaviorUpgrade, ITickable
    {
       
        float ScoreMultiplier;
        ICurio BellTower;

        bool Activated => BellTower != null && !BellTower.Removed;

        public BehaviorDoom() : base()
        {
        }

        public BehaviorDoom(ICurio curio, float scoreMultiplier) : base(curio)
        {
            ScoreMultiplier = scoreMultiplier;
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorDoom(curio, ScoreMultiplier), Curio);
        }

        public void Tick(SceneGame scene)
        {
            if (BellTower == null)
            {
                TryActivate();
            }
        }

        private void TryActivate()
        {
            var bellTower = Manager.GetBehaviors().OfType<BehaviorBellTower>().FirstOrDefault(x => !x.Curio.IsTemplate());
            if (bellTower != null)
            {
                bellTower.BellTime.EndTime *= 0.10f;
                BellTower = bellTower.Curio;
            }
        }

        [EventSubscribe]
        public void OnAction(EventAction e)
        {
            if (!Activated)
                return;
            var heartConsumes = e.Actions.GetEffectsBy<ActionConsumeHeart>(Curio);
            foreach(var heartConsume in heartConsumes)
            {
                heartConsume.Score = (int)(heartConsume.Score * ScoreMultiplier);
            }
        }
    }
}
