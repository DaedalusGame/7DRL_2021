using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionGrappleWall : IGrappleAction, ITickable
    {
        public ICurio Origin { get; set; }
        public ICurio Target { get; set; }
        public Vector2 Direction { get; set; }
        public Slider GrappleTime;
        public Slider ReelTime;

        public bool Done => ReelTime.Done;

        static SoundReference SoundSwish = SoundLoader.AddSound("content/sound/swish.wav");
        static SoundReference SoundClack = SoundLoader.AddSound("content/sound/clack.wav");

        public ActionGrappleWall(ICurio origin, ICurio target, Vector2 direction, float grappleTime, float reelTime)
        {
            Origin = origin;
            Target = target;
            Direction = direction;
            GrappleTime = new Slider(grappleTime);
            ReelTime = new Slider(reelTime);
        }

        public void Run()
        {
            var grapple = Origin.GetBehavior<BehaviorGrapplingHook>();
            if(grapple != null)
            {
                SoundSwish.Play(1, 0.5f, 0);
                SoundClack.Play(1, 0, 0);
                grapple.OrientVisual(Util.GetAngleDistance(Origin.GetAngle(), Util.VectorToAngle(Direction)), LerpHelper.ExponentialOut, GrappleTime);
                grapple.Connect(Target.GetVisualTarget);
                grapple.Wave(20, 0, LerpHelper.QuadraticIn, GrappleTime);
            }
        }

        public void Tick(SceneGame scene)
        {
            var orientable = Origin.GetBehavior<BehaviorOrientable>();
            var grapple = Origin.GetBehavior<BehaviorGrapplingHook>();
            bool shouldReel = !GrappleTime.Done;
            GrappleTime += scene.TimeModCurrent;
            if (GrappleTime.Done)
            {
                var tile = Target.GetMainTile();
                var offset = (-Direction).ToTileOffset();
                var neighbor = tile.GetNeighborOrNull(offset.X, offset.Y);

                if (shouldReel)
                {
                    var player = Origin.GetBehavior<BehaviorPlayer>();
                    var camera = Origin.GetCamera();

                    if (neighbor != null && !neighbor.IsSolid())
                    {
                        Origin.MoveTo(neighbor, LerpHelper.CubicOut, ReelTime);
                        camera?.MoveTo(neighbor, LerpHelper.QuadraticOut, ReelTime);
                        grapple.ReelTowards(Target.GetVisualTarget(), ReelTime);
                    }
                    else
                    {
                        grapple.ReelIn(Target.GetVisualTarget(), LerpHelper.QuadraticIn, ReelTime);
                        grapple.OrientTo(-3, LerpHelper.ExponentialOut, ReelTime);
                    }
                    SoundSwish.Play(1, 0, 0);
                }
                bool shouldGrip = !ReelTime.Done;
                ReelTime += scene.TimeModCurrent;
                if(ReelTime.Done && shouldGrip && neighbor != null && !neighbor.IsSolid())
                {
                    Origin.GetFlashHelper()?.AddFlash(ColorMatrix.Flat(Color.White), 5);
                    grapple.GripDirection = offset;
                    orientable.OrientTo(Util.VectorToAngle(offset.ToVector2()));
                    SoundClack.Play(1, -1.0f, 0);
                }
            }
        }
    }
}
