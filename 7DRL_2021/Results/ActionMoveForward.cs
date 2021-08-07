using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionMoveForward : IActionHasOrigin, ITickable, ISlider
    {
        public bool Done => Frame.Done;
        public ICurio Origin { get; set; }
        public Vector2 Direction;
        private Slider Frame;
        public float Slide => Frame.Slide;

        public ActionMoveForward(ICurio origin, Vector2 direction, float time)
        {
            Origin = origin;
            Direction = direction;
            Frame = new Slider(time);
        }

        public void Run()
        {
            var camera = Origin.GetCamera();
            var tile = Origin.GetMainTile();
            var offset = Direction.ToTileOffset();
            var neighbor = tile.GetNeighborOrNull(offset.X, offset.Y);
            if (neighbor != null && !neighbor.IsSolid())
            {
                Origin.MoveTo(neighbor, LerpHelper.Linear, this);
                camera?.MoveTo(neighbor, LerpHelper.Linear, this);
            }
        }

        public void Tick(SceneGame scene)
        {
            foreach(var moveTickable in Origin.GetBehaviors<IMoveTickable>())
            {
                moveTickable.MoveTick(Direction);
            }
            Frame += scene.TimeModCurrent;
        }
    }
}
