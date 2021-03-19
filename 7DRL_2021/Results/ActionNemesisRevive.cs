using _7DRL_2021.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionNemesisRevive : IActionHasOrigin, ITickable
    {
        static GlitchParams Params = new GlitchParams()
        {
            LineSpeed = 0.28f,
            LineShift = 0.04f,
            LineResolution = 2.1f,
            LineVerticalShift = 0.45f,
            LineDrift = 0.26f,
            JumbleSpeed = 0.43f,
            JumbleShift = 0.68f,
            JumbleResolution = 0.95f,
            Jumbleness = 0.62f,
            NoiseLevel = 1.0f,
            Shakiness = 2.8f,
        };

        public ICurio Origin { get; set; }

        public Slider Frame;
        public bool Done => Frame.Done;

        public static SoundReference SoundRevive = SoundLoader.AddSound("content/sound/revive.wav");

        public ActionNemesisRevive(ICurio origin, float time)
        {
            Origin = origin;
            Frame = new Slider(time);
        }

        public void Run()
        {
            var world = Origin.GetWorld();
            SoundRevive.Play(1, 0, 0);
            new ScreenGlitchFlash(world, slide => Params.WithIntensity((1- slide) * 0.1f), (int)Frame.EndTime);
            new TimeFade(world, 0.1f, LerpHelper.ExponentialOut, (int)Frame.EndTime);
        }

        public void Tick(SceneGame scene)
        {
            Frame += 1;
            if (Frame.Done)
            {
                var alive = Origin.GetBehavior<BehaviorAlive>();
                alive.SetDamage(0);
            } 
        }
    }
}
