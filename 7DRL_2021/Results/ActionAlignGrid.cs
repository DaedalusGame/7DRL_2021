using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionAlignGrid : IActionHasOrigin, ITickable
    {
        public bool Done => Time.Done;
        public ICurio Origin { get; set; }

        Slider Time;

        public ActionAlignGrid(ICurio origin, float time)
        {
            Origin = origin;
            Time = new Slider(time);
        }

        public void Run()
        {
            Origin.MoveVisual(Origin.GetMainTile(), LerpHelper.CubicIn, Time);
        }

        public void Tick(SceneGame scene)
        {
            Time += 1;
        }
    }
}
