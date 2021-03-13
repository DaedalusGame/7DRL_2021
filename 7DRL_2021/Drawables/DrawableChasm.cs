using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Drawables
{
    class DrawableChasm : Drawable
    {
        public DrawableChasm(string id) : base(id)
        {
        }

        public override void Draw(ICurio curio, SceneGame scene, DrawPass pass)
        {
            //NOOP;
        }

        public override void DrawIcon(ICurio curio, SceneGame scene, Vector2 pos)
        {
            //NOOP;
        }

        public override IEnumerable<DrawPass> GetDrawPasses()
        {
            return Enumerable.Empty<DrawPass>();
        }
    }
}
