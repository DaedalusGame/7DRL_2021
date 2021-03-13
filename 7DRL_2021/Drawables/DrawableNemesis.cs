using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static _7DRL_2021.Behaviors.BehaviorNemesis;

namespace _7DRL_2021.Drawables
{
    class DrawableNemesis : Drawable
    {
        public DrawableNemesis(string id) : base(id)
        {
        }

        public override void Draw(ICurio curio, SceneGame scene, DrawPass pass)
        {
            var spriteHelmet = SpriteLoader.Instance.AddSprite("content/nemesis_helmet");
            var spriteBodyForward = SpriteLoader.Instance.AddSprite("content/nemesis_forward");
            var spriteBodyBack = SpriteLoader.Instance.AddSprite("content/nemesis_back");
            var spriteWings = SpriteLoader.Instance.AddSprite("content/nemesis_wings");
            var spriteParry = SpriteLoader.Instance.AddSprite("content/nemesis_parry");

            var alive = curio.GetBehavior<BehaviorAlive>();
            var nemesis = curio.GetBehavior<BehaviorNemesis>();

            if (alive.Armor > 0)
            {
                spriteBodyForward = SpriteLoader.Instance.AddSprite("content/nemesis_forward_armor");
                spriteBodyBack = SpriteLoader.Instance.AddSprite("content/nemesis_back_armor");
            }

            var center = curio.GetVisualPosition() + new Vector2(8,8);
            var offset = curio.GetOffset();
            var color = curio.GetColor();
            var angleBody = curio.GetVisualAngle();
            scene.PushSpriteBatch(shader: scene.Shader, shaderSetup: (transform, projection) =>
            {
                scene.SetupColorMatrix(color);
            });
            SpriteReference spriteBody;
            float headPos = MathHelper.Lerp(-4, 4, nemesis.ForwardBack);
            if (nemesis.ForwardBack > 0.5f)
            {
                spriteBody = spriteBodyForward;
            }
            else
            {
                spriteBody = spriteBodyBack;
            }
            scene.DrawSpriteExt(spriteBody, 0, center + offset - spriteBody.Middle, spriteBody.Middle, angleBody, new Vector2(1), SpriteEffects.None, Color.White, 0);
            scene.DrawSpriteExt(spriteHelmet, 0, center + offset + Util.AngleToVector(angleBody) * headPos - spriteHelmet.Middle, spriteHelmet.Middle, angleBody, new Vector2(1), SpriteEffects.None, Color.White, 0);
            if(nemesis.State == NemesisState.Parry)
                scene.DrawSpriteExt(spriteParry, 0, center + offset - spriteParry.Middle, spriteParry.Middle, angleBody, new Vector2(1), SpriteEffects.None, Color.White, 0);
            if (nemesis.ForwardBack > 0.5f && nemesis.WingsOpen > 0f)
            {
                Color wingColor = Color.Lerp(Color.Red, Color.Black, nemesis.WingsOpen);
                scene.DrawSpriteExt(spriteWings, 0, center + offset - spriteWings.Middle, spriteWings.Middle, angleBody, new Vector2(1), SpriteEffects.None, wingColor, 0);
            }

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
