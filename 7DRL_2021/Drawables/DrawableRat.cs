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
    class DrawableRat : Drawable
    {
        public DrawableRat(string id) : base(id)
        {
        }

        public override void Draw(ICurio curio, SceneGame scene, DrawPass pass)
        {
            var spriteIdle = SpriteLoader.Instance.AddSprite("content/rat_idle");
            var spriteMove = SpriteLoader.Instance.AddSprite("content/rat_move");
            var center = curio.GetVisualPosition() + new Vector2(8, 8);
            var offset = curio.GetOffset();
            var color = curio.GetColor();
            var angleBody = curio.GetVisualAngle();
            var pathfinder = curio.GetBehavior<BehaviorPathfinder>();
            scene.PushSpriteBatch(shader: scene.Shader, shaderSetup: (transform, projection) =>
            {
                scene.SetupColorMatrix(color);
            });
            var spriteBody = pathfinder.HasPath ? spriteMove : spriteIdle;
            scene.DrawSpriteExt(spriteBody, 0, center + offset - spriteBody.Middle, spriteBody.Middle, angleBody, new Vector2(1), SpriteEffects.None, Color.White, 0);
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
