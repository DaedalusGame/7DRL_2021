using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionGripDash : IActionHasOrigin, ITickable
    {
        static Random Random = new Random();

        public ICurio Origin { get; set; }
        public Vector2 Direction;
        public Slider Frame;
        public bool Done => Frame.Done;

        static SoundReference SoundSwish = SoundLoader.AddSound("content/sound/swish.wav");
        static SoundReference SoundKick = SoundLoader.AddSound("content/sound/clack.wav");

        public ActionGripDash(ICurio origin, Vector2 direction, float time)
        {
            Origin = origin;
            Direction = direction;
            Frame = new Slider(time);
        }

        public void Run()
        {
            var player = Origin.GetBehavior<BehaviorPlayer>();
            var grapple = Origin.GetBehavior<BehaviorGrapplingHook>();
            var orientable = Origin.GetBehavior<BehaviorOrientable>();

            grapple.GripDirection = null;
            orientable.OrientTo(Util.VectorToAngle(Direction));
            if (player != null)
            {
                player.Momentum.Direction = Direction;
                player.Momentum.Amount = Math.Min(player.Momentum.Amount, 32);
            }
            SoundKick.Play(1, Random.NextFloat(-0.5f, +0.5f), 0);
            SoundSwish.Play(1, Random.NextFloat(-1.0f, -0.5f), 0);
        }

        public void Tick(SceneGame scene)
        {
            Frame += scene.TimeModCurrent;
        }
    }
}
