using _7DRL_2021.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionSwordSheathe : IActionHasOrigin, ITickable, ISlider
    {
        public bool Done => Frame.Done;
        public ICurio Origin { get; set; }

        private Slider Frame;
        public float Slide => Frame.Slide;

        public ActionSwordSheathe(ICurio origin, float time)
        {
            Origin = origin;
            Frame = new Slider(time);
        }

        public void Run()
        {
            var sword = Origin.GetBehavior<BehaviorSword>();
            sword.SetScale(0, LerpHelper.QuadraticIn, this);
        }

        public void Tick(SceneGame scene)
        {
            Frame += scene.TimeModCurrent;

            if(Frame.Done)
            {
                var actions = new List<ActionWrapper>();
                actions.Add(new ActionConsumeHeartSword(Origin, 1000, 1).InSlot(ActionSlot.Active));
                actions.Apply(Origin);
                Origin.RemoveBehaviors(x => x is BehaviorSword);
            }
        }
    }
}
