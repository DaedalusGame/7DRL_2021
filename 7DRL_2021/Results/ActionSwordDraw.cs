using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionSwordDraw : IActionHasOrigin, ITickable, ISlider
    {
        public bool Done => Frame.Done;
        public ICurio Origin { get; set; }

        int Position;
        private Slider Frame;
        public float Slide => Frame.Slide;

        public ActionSwordDraw(ICurio origin, int position, float time)
        {
            Origin = origin;
            Position = position;
            Frame = new Slider(time);
        }

        public void Run()
        {
            var world = Origin.GetWorld();
            var sword = new BehaviorSword(Origin, 3);
            Behavior.Apply(sword);
            sword.SetScale(1, LerpHelper.QuadraticOut, this);
            var star = new BigStar(world, SpriteLoader.Instance.AddSprite("content/effect_star_big"), sword.GetTip)
            {
                DrawPass = DrawPass.EffectAdditive,
            };
            star.Angle.Set(0, MathHelper.TwoPi, LerpHelper.QuadraticOut, 30);
            star.Scale.Set(0, 0.1f, LerpHelper.QuadraticOut, 30);
            star.ShouldDestroy.Set(true, LerpHelper.Linear, 30);
        }

        public void Tick(SceneGame scene)
        {
            Frame += scene.TimeMod;
        }
    }
}
