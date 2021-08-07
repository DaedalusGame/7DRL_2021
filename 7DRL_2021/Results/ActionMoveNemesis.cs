using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionMoveNemesis : IActionPaired, ITickable, ISlider
    {
        static Random Random = new Random();

        public bool Done => Frame.Done;
        public ICurio Origin { get; set; }
        public ICurio Target { get; set; }
        public MapTile Tile;
        private Slider Frame;
        private Slider ShootFrame;
        public float Slide => Frame.Slide;

        public static SoundReference SoundFly = SoundLoader.AddSound("content/sound/fly.wav");

        public ActionMoveNemesis(ICurio origin, ICurio target, MapTile tile, float time)
        {
            Origin = origin;
            Target = target;
            Tile = tile;
            Frame = new Slider(time);
            ShootFrame = new Slider(2);
        }

        public void Run()
        {
            var camera = Origin.GetCamera();
            var nemesis = Origin.GetBehavior<BehaviorNemesis>();
            var neighbor = Tile;
            var orientable = Origin.GetBehavior<BehaviorOrientable>();
            var angle = Util.VectorToAngle(Tile.GetVisualTarget() - Origin.GetVisualTarget());
            if (neighbor != null && !neighbor.IsSolid())
            {
                SoundFly.Play(1, 0, 0);
                nemesis.WingsOpen.Set(1, LerpHelper.ExponentialIn, 30);
                Origin.MoveTo(neighbor, LerpHelper.Linear, this);
                camera?.MoveTo(neighbor, LerpHelper.Linear, this);
                orientable.OrientVisual(angle, LerpHelper.Quadratic, Frame);
            }
        }

        private void EmitWraith()
        {
            var targetTile = Target.GetMainTile().GetNearby(5).ToList().Pick(Random);

            var wraith = new Curio(Template.Wraith);
            wraith.MoveTo(targetTile);
            Behavior.Apply(new BehaviorWraith(wraith, Origin.GetVisualTarget(), Random.NextFloat(10, 30)));
        }

        public void Tick(SceneGame scene)
        {
            Frame += scene.TimeModCurrent;
            ShootFrame += scene.TimeModCurrent;
            if (ShootFrame.Done)
            {
                int radius = 240;
                if (Vector2.DistanceSquared(Origin.GetVisualTarget(), Target.GetVisualTarget()) < radius * radius)
                    EmitWraith();
                ShootFrame.Reset();
            }
            if (Frame.Done)
            {
                var nemesis = Origin.GetBehavior<BehaviorNemesis>();
                nemesis.WingsOpen.Set(0);
            }
        }
    }
}
