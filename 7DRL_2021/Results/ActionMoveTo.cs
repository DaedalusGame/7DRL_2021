using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionMoveTo : IActionHasOrigin, ITickable, ISlider
    {
        public bool Done => Frame.Done;
        public ICurio Origin { get; set; }
        public MapTile Tile;
        private Slider Frame;
        public float Slide => Frame.Slide;

        public ActionMoveTo(ICurio origin, MapTile tile, float time)
        {
            Origin = origin;
            Tile = tile;
            Frame = new Slider(time);
        }

        public void Run()
        {
            var camera = Origin.GetCamera();
            var neighbor = Tile;
            if (neighbor != null && !neighbor.IsSolid())
            {
                Origin.MoveTo(neighbor, LerpHelper.Linear, this);
                camera?.MoveTo(neighbor, LerpHelper.Linear, this);
            }
        }

        public void Tick(SceneGame scene)
        {
            Frame += scene.TimeMod;
        }
    }
}
