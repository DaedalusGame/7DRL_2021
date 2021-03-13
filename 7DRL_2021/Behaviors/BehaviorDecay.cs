using _7DRL_2021.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    class BehaviorDecay : Behavior, ITickable
    {
        public ICurio Curio;
        public Slider Decay;
        public int Score;
        public int Particles;
        public float Radius;

        public BehaviorDecay()
        {
        }

        public BehaviorDecay(ICurio curio, float time, int score, int particles, float radius)
        {
            Curio = curio;
            Decay = new Slider(time);
            Score = score;
            Particles = particles;
            Radius = radius;
        }

        public override void Apply()
        {
            Curio.AddBehavior(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorDecay(curio, Decay.EndTime, Score, Particles, Radius));
        }

        public void Tick(SceneGame scene)
        {
            if (Curio.IsDead())
                Decay += scene.TimeMod;

            if (Decay.Done)
            {
                var actions = new List<ActionWrapper>();
                actions.Add(new ActionCorpseGib(scene.PlayerCurio, Curio, Score, Particles, Radius).InSlot(ActionSlot.Active));
                actions.Apply(Curio);
            }
        }
    }
}
