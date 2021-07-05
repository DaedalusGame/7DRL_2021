using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Drawables
{
    class DrawableProjectile : Drawable
    {
        SpriteReference Sprite;

        public DrawableProjectile(string id, SpriteReference sprite) : base(id)
        {
            Sprite = sprite;
        }

        public override void Draw(ICurio curio, SceneGame scene, DrawPass pass)
        {
            var center = curio.GetVisualPosition() + new Vector2(8, 8);
            var offset = curio.GetOffset();
            var color = curio.GetColor();
            var angleBody = curio.GetVisualAngle();
            scene.PushSpriteBatch(shader: scene.Shader, shaderSetup: (transform, projection) =>
            {
                scene.SetupColorMatrix(color);
            });
            scene.DrawSpriteExt(Sprite, 0, center + offset - Sprite.Middle, Sprite.Middle, angleBody, new Vector2(1), SpriteEffects.None, Color.White, 0);
            scene.PopSpriteBatch();
        }

        public override void DrawIcon(ICurio curio, SceneGame scene, Vector2 pos)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass.Creature;
        }
    }
}
