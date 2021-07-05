using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    class BehaviorWraithEmitter : Behavior, ITickable
    {
        static Random Random = new Random();

        public ICurio Curio;
        public Slider Frame;
        public bool Activated = false;

        static SoundReference SoundEmit = SoundLoader.AddSound("content/sound/laugh.wav");

        public BehaviorWraithEmitter()
        {
        }

        public BehaviorWraithEmitter(ICurio curio)
        {
            Curio = curio;
            Frame = new Slider(Random.NextFloat(20, 40));
        }

        public override void Apply()
        {
            Curio.AddBehavior(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorWraithEmitter(curio), Curio);
        }

        private void EmitWraith(ICurio target)
        {
            var emitTile = Curio.GetMainTile();
            var targetTile = target.GetMainTile().GetNearby(5).ToList().Pick(Random);

            var wraith = new Curio(Template.Wraith);
            wraith.MoveTo(targetTile);
            Behavior.Apply(new BehaviorWraith(wraith, emitTile.VisualTarget, Random.NextFloat(30, 70)));
            SoundEmit.Play(1f, Random.NextFloat(-1, +1), Random.NextFloat(-1, +1));
        }

        public void Tick(SceneGame scene)
        {
            if (!Activated)
                return;
            Frame += scene.TimeMod;
            if(Frame.Done)
            {
                float radius = 240;
                if(scene.PlayerCurio.IsAlive() && Vector2.DistanceSquared(Curio.GetVisualTarget(), scene.PlayerCurio.GetVisualTarget()) < radius * radius)
                    EmitWraith(scene.PlayerCurio);
                Frame.Reset();
                Frame.EndTime = Random.NextFloat(40, 80);
            }
        }
    }
}
