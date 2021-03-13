using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _7DRL_2021.Drawables
{
    class DrawableLich : Drawable
    {
        public DrawableLich(string id) : base(id)
        {
        }

        public override void Draw(ICurio curio, SceneGame scene, DrawPass pass)
        {
            var spriteHead = SpriteLoader.Instance.AddSprite("content/lich_head");
            var spriteBody = SpriteLoader.Instance.AddSprite("content/lich_body");
            var spriteHeart = SpriteLoader.Instance.AddSprite("content/lich_heart");
            var spriteWeapon = SpriteLoader.Instance.AddSprite("content/lich_weapon");

            var center = curio.GetVisualPosition() + new Vector2(8, 8);
            var offset = curio.GetOffset();
            var color = curio.GetColor();
            var angleBody = curio.GetVisualAngle();
            var lich = curio.GetBehavior<BehaviorLich>();
            scene.PushSpriteBatch(shader: scene.Shader, shaderSetup: (transform, projection) =>
            {
                scene.SetupColorMatrix(color);
            });

            if (pass == DrawPass.EffectLow)
            {
                scene.DrawSpriteExt(spriteBody, 0, center - spriteBody.Middle, spriteBody.Middle, scene.Frame * 0.1f, new Vector2(1), SpriteEffects.None, Color.Red, 0);
                scene.DrawSpriteExt(spriteBody, 0, center - spriteBody.Middle, spriteBody.Middle, scene.Frame * 0.2f, new Vector2(0.8f), SpriteEffects.None, Color.DarkRed, 0);

                var weaponMiddle = new Vector2(spriteWeapon.Width / 2, spriteWeapon.Height * 2 / 3);
                var weaponOffset = Util.AngleToVector(lich.SwordAngle);
                scene.DrawSpriteExt(spriteWeapon, 0, center + Util.AngleToVector(angleBody) + weaponOffset * 8 - weaponMiddle, weaponMiddle, lich.SwordAngle, new Vector2(lich.SwordScale), SpriteEffects.None, Color.White, 0);

                scene.DrawSpriteExt(spriteBody, 0, center - spriteBody.Middle, spriteBody.Middle, scene.Frame * 0.3f, new Vector2(0.6f), SpriteEffects.None, Color.Black, 0);
            }
            if (pass == DrawPass.Creature)
            {
                if(!curio.IsHeartless())
                    scene.DrawSpriteExt(spriteHeart, 0, center + offset - spriteHeart.Middle, spriteHeart.Middle, 0, new Vector2(1), SpriteEffects.None, Color.White, 0);
            }
            if (pass == DrawPass.Effect)
            {
                var headOffset = Util.AngleToVector(angleBody) * -8;
                scene.DrawSpriteExt(spriteHead, 0, center + headOffset + offset - spriteHead.Middle, spriteHead.Middle, angleBody, new Vector2(1), SpriteEffects.None, Color.White, 0);
            }

            scene.PopSpriteBatch();
        }

        public override void DrawIcon(ICurio curio, SceneGame scene, Vector2 pos)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass.EffectLow;
            yield return DrawPass.Creature;
            yield return DrawPass.Effect;
        }
    }
}
