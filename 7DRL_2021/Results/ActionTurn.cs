using _7DRL_2021.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionTurn : IActionHasOrigin, ITickable, ISlider
    {
        public bool Done => Frame.Done;
        public ICurio Origin { get; set; }
        public float TurnAngle;
        private Slider Frame;
        public float Slide => Frame.Slide;

        public ActionTurn(ICurio origin, float turnAngle, int time)
        {
            Origin = origin;
            TurnAngle = turnAngle;
            Frame = new Slider(time);
        }

        public void Run()
        {
            var tile = Origin.GetMainTile();
            var orientable = Origin.GetBehavior<BehaviorOrientable>();

            orientable.TurnTo(orientable.Angle + TurnAngle, LerpHelper.QuadraticIn, this);
        }

        public void Tick(SceneGame scene)
        {
            Frame += scene.TimeMod;
        }
    }
}
