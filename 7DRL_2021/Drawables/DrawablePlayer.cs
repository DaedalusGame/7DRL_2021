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
    class DrawablePlayer : Drawable
    {
        public DrawablePlayer(string id) : base(id)
        {
        }

        public override void Draw(ICurio curio, SceneGame scene, DrawPass pass)
        {
            var spriteHelmetForward = SpriteLoader.Instance.AddSprite("content/player_helmet_5");
            var spriteHelmetBack = SpriteLoader.Instance.AddSprite("content/player_helmet_4");

            var spriteBodyForward = SpriteLoader.Instance.AddSprite("content/player_forward");
            var spriteBodyBack = SpriteLoader.Instance.AddSprite("content/player_back");

            var spriteSword = SpriteLoader.Instance.AddSprite("content/player_sword");
            var spriteGrip = SpriteLoader.Instance.AddSprite("content/player_grip");
            var spriteSwordBlood = SpriteLoader.Instance.AddSprite("content/player_sword_bloody");
            var spriteSwordHeart = SpriteLoader.Instance.AddSprite("content/player_sword_heart");
            var spriteWings = SpriteLoader.Instance.AddSprite("content/player_wings");

            var sword = curio.GetBehavior<BehaviorSword>();
            var grapple = curio.GetBehavior<BehaviorGrapplingHook>();
            var player = curio.GetBehavior<BehaviorPlayer>();
            var alive = curio.GetBehavior<BehaviorAlive>();

            if (alive.Armor > 0)
            {
                spriteBodyForward = SpriteLoader.Instance.AddSprite("content/player_forward_armor");
                spriteBodyBack = SpriteLoader.Instance.AddSprite("content/player_back_armor");
            }

            var world = curio.GetWorld();
            var center = curio.GetVisualPosition() + new Vector2(8,8);
            var offset = curio.GetOffset();
            var color = curio.GetColor();
            var angleBody = curio.GetVisualAngle();
            scene.PushSpriteBatch(shader: scene.Shader, shaderSetup: (transform, projection) =>
            {
                scene.SetupColorMatrix(color);
            });
            if (grapple != null && grapple.ShouldRender)
            {
                var angleGrapple = angleBody + grapple.VisualAngle();
                scene.DrawSpriteExt(spriteGrip, 0, center + offset - spriteGrip.Middle, spriteGrip.Middle, angleGrapple, new Vector2(1), SpriteEffects.None, Color.White, 0);
            }
            if (sword != null)
            {
                var angleSword = angleBody + sword.VisualAngle();
                if (sword.HasBlood)
                    spriteSword = spriteSwordBlood;
                scene.DrawSpriteExt(spriteSword, 0, center + offset - spriteSword.Middle, spriteSword.Middle, angleSword, new Vector2(sword.VisualScale()), SpriteEffects.None, Color.White, 0);
                if(sword.HasHeart)
                    scene.DrawSpriteExt(spriteSwordHeart, 0, center + offset - spriteSwordHeart.Middle, spriteSwordHeart.Middle, angleSword, new Vector2(sword.VisualScale()), SpriteEffects.None, Color.White, 0);
            }
            SpriteReference spriteHelmet;
            SpriteReference spriteBody;
            float headPos = MathHelper.Lerp(-4, 4, player.ForwardBack);
            if (player.ForwardBack > 0.5f)
            {
                spriteHelmet = spriteHelmetForward;
                spriteBody = spriteBodyForward;
            }
            else
            {
                spriteHelmet = spriteHelmetBack;
                spriteBody = spriteBodyBack;
            }
            scene.DrawSpriteExt(spriteBody, 0, center + offset - spriteBody.Middle, spriteBody.Middle, angleBody, new Vector2(1), SpriteEffects.None, Color.White, 0);
            scene.DrawSpriteExt(spriteHelmet, (int)(player.HairFrame / 4), center + offset + Util.AngleToVector(angleBody) * headPos - spriteHelmet.Middle, spriteHelmet.Middle, angleBody, new Vector2(1), SpriteEffects.None, Color.White, 0);
            //scene.PushSpriteBatch(blendState: BlendState.Additive);
            //scene.DrawSpriteExt(spriteWings, 0, center + offset - spriteWings.Middle, spriteWings.Middle, angleBody, new Vector2(1), SpriteEffects.None, new Color(200, 192, 255), 0);
            //scene.PopSpriteBatch();
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
