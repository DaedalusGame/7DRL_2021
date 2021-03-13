using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    class BehaviorBellTower : Behavior, ITickable
    {
        public static GlitchParams Params = new GlitchParams()
        {
            LineSpeed = 0.4f,
            LineShift = 0.002f,
            LineResolution = 0.2f,
            LineVerticalShift = 0.5f,
            LineDrift = 0.77f,
            JumbleSpeed = 6.5f,
            JumbleShift = 0.8f,
            JumbleResolution = 0.5f,
            Jumbleness = 0.8f,
            //Dispersion = 0.02f,
            Shakiness = 8.9f,
            NoiseLevel = 0.1f,
        };

        public ICurio Curio;
        public Slider BellTime;
        public int BellTolls;

        public BehaviorBellTower()
        {
        }

        public BehaviorBellTower(ICurio curio, float time)
        {
            Curio = curio;
            BellTime = new Slider(time);
        }

        public override void Apply()
        {
            Curio.AddBehavior(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorBellTower(curio, BellTime.EndTime));
        }

        public void Tick(SceneGame scene)
        {
            BellTime += scene.TimeMod;
            if(BellTime.Done && BellTolls < 12)
            {
                new ScreenGlitchFlash(scene, slide => Params.WithIntensity(slide), 10);
                new ScreenFlashSimple(scene, ColorMatrix.Tint(Color.Red), LerpHelper.Flick, 10);
                BellTolls += 1;
                BellTime.Time = 0;
                if (BellTolls >= 12)
                {
                    new ScreenBellWraiths(Curio.GetWorld(), this, 60, 300);
                    foreach(var wraithEmitter in Manager.GetBehaviors().OfType<BehaviorWraithEmitter>())
                    {
                        wraithEmitter.Activated = true;
                    }
                }
            }
        }
    }
}
