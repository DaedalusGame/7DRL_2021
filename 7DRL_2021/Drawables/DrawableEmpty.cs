using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace _7DRL_2021.Drawables
{
    class DrawableEmpty : IDrawable
    {
        public double DrawOrder => 0;

        public void Draw(SceneGame scene, DrawPass pass)
        {
            //NOOP
        }

        public void DrawIcon(SceneGame scene, Vector2 pos)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DrawPass> GetDrawPasses()
        {
            return Enumerable.Empty<DrawPass>();
        }

        public bool ShouldDraw(SceneGame scene, Vector2 cameraPosition)
        {
            return false;
        }
    }
}
