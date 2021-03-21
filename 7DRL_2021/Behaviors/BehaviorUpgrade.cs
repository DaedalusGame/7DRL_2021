using _7DRL_2021.Events;
using _7DRL_2021.Results;
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

    class BehaviorDestructionWave : BehaviorUpgrade
    {

        public BehaviorDestructionWave() : base()
        {
        }

        public BehaviorDestructionWave(ICurio curio) : base(curio)
        {
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorDestructionWave(curio), Curio);
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
