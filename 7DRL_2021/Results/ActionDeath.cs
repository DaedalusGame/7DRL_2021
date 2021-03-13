using _7DRL_2021.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionDeath : IActionHasOrigin, ITickable, ISlider
    {
        public ICurio Origin { get; set; }

        public bool Done => false;

        public float Slide => Frame.Slide;

        public Slider Frame = new Slider(10);
       
        public void Run()
        {
            Origin.MoveTo(Origin.GetMainTile(), LerpHelper.ExponentialIn, this);
        }

        public void Tick(SceneGame scene)
        {
            Frame += 1;
        }
    }
}
