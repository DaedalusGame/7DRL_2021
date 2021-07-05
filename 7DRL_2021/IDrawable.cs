using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    interface IDrawable
    {
        double DrawOrder
        {
            get;
        }

        bool ShouldDraw(SceneGame scene, Vector2 cameraPosition);

        IEnumerable<DrawPass> GetDrawPasses();

        void Draw(SceneGame scene, DrawPass pass);

        void DrawIcon(SceneGame scene, Vector2 pos);
    }

    interface IDrawableContainer
    {
        IEnumerable<IDrawable> GetDrawables();
    }

    interface IDrawableCurio : IDrawable
    {
        ICurio DrawCurio
        {
            get;
        }
    }
}
