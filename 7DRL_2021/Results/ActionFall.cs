using _7DRL_2021.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionFall : IActionHasOrigin, ITickable, ISlider
    {
        public bool Done => false;
        public ICurio Origin { get; set; }
        private Slider Frame;
        public float Slide => Frame.Slide;

        static SoundReference SoundFall = SoundLoader.AddSound("content/sound/fall.wav");

        public ActionFall(ICurio origin, int time)
        {
            Origin = origin;
            Frame = new Slider(time);
        }

        public void Run()
        {
            var world = Origin.GetWorld();
            Origin.GetBehavior<BehaviorDrawable>()?.SetHidden("chasm", true);
            new SmokeParticle(world, SpriteLoader.Instance.AddSprite("content/effect_fall"), Origin.GetVisualTarget(), (int)Frame.EndTime)
            {
                SizeLerp = LerpHelper.ExponentialOut,
                DissipateTime = 1,
                DrawPass = DrawPass.Chasm3,
            };
            SoundFall.Play(1, 0, 0);
        }

        public void Tick(SceneGame scene)
        {
            Frame += scene.TimeMod;
            if (Frame.Done)
            {
                var alive = Origin.GetBehavior<BehaviorAlive>();
                alive.SetDamage(alive.HP);
                Origin.MoveTo(null);
            }
        }
    }
}
