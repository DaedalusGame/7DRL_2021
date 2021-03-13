using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionCollision : IActionHasOrigin, ITickable, ISlider
    {
        public bool Done => Frame.Done;
        public ICurio Origin { get; set; }
        public ICurio Collision;
        private Slider Frame;
        public float Slide => Frame.Slide;

        public ActionCollision(ICurio origin, ICurio collision, int time)
        {
            Origin = origin;
            Collision = collision;
            Frame = new Slider(time);
        }

        public void Run()
        {
            var world = Origin.GetWorld();
            Origin.GetFlashHelper()?.AddFlash(ColorMatrix.Flat(Color.White), 20);
            Origin.GetShakeHelper()?.AddShakeRandom(3, LerpHelper.QuadraticOut, 30);
            var alive = Origin.GetBehavior<BehaviorAlive>();
            var damage = 1;
            if (Collision.IsSpiky())
                damage = 2;
            if (alive != null)
            {
                alive.TakeDamage(damage);
            }
        }

        public void Tick(SceneGame scene)
        {
            Frame += scene.TimeMod;
        }
    }
}
