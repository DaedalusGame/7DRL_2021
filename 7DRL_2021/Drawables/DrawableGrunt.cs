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
    class DrawableGrunt : Drawable
    {
        SpriteReference SpriteBody;
        SpriteReference SpriteWeapon;

        public DrawableGrunt(string id, SpriteReference body, SpriteReference weapon) : base(id)
        {
            SpriteBody = body;
            SpriteWeapon = weapon;
        }

        public override void Draw(ICurio curio, SceneGame scene, DrawPass pass)
        {
            var body = SpriteBody;
            var alive = curio.GetBehavior<BehaviorAlive>();
            if(alive.Armor > 0)
            {
                body = SpriteLoader.Instance.AddSprite($"{SpriteBody.FileName}_armor");
            }
            var center = curio.GetVisualPosition() + new Vector2(8, 8);
            var offset = curio.GetOffset();
            var color = curio.GetColor();
            var angleBody = curio.GetVisualAngle();
            scene.PushSpriteBatch(shader: scene.Shader, shaderSetup: (transform, projection) =>
            {
                scene.SetupColorMatrix(color);
            });
            scene.DrawSpriteExt(body, 0, center + offset - SpriteBody.Middle, SpriteBody.Middle, angleBody, new Vector2(1), SpriteEffects.None, Color.White, 0);
            DrawDagger(curio, scene, pass);
            DrawMace(curio, scene, pass);
            scene.PopSpriteBatch();
        }

        private void DrawDagger(ICurio curio, SceneGame scene, DrawPass pass)
        {
            var center = curio.GetVisualPosition() + new Vector2(8, 8);
            var offset = curio.GetOffset();
            var angleBody = curio.GetVisualAngle();
            var dagger = curio.GetBehavior<BehaviorDagger>();
            if (dagger != null && !dagger.Upswing.Done)
            {
                var weaponAngle = (float)LerpHelper.QuadraticOut(0, 1, dagger.Upswing.Slide);
                var weaponScale = (float)LerpHelper.QuadraticOut(0.5, 1.0, dagger.Upswing.Slide);
                var weaponStartPos = Util.AngleToVector(angleBody + MathHelper.PiOver4 * 2) * 8;
                var weaponEndPos = Util.AngleToVector(angleBody + MathHelper.PiOver4 * 3) * 10;
                var weaponPos = center + offset + Vector2.Lerp(weaponStartPos, weaponEndPos, (float)LerpHelper.QuadraticOut(0, 1, dagger.Upswing.Slide));
                scene.DrawSpriteExt(SpriteWeapon, 0, weaponPos - SpriteWeapon.Middle, SpriteWeapon.Middle, angleBody + weaponAngle, new Vector2(weaponScale), SpriteEffects.None, Color.White, 0);
            }
        }

        private void DrawMace(ICurio curio, SceneGame scene, DrawPass pass)
        {
            var center = curio.GetVisualPosition() + new Vector2(8, 8);
            var offset = curio.GetOffset();
            var angleBody = curio.GetVisualAngle();
            var mace = curio.GetBehavior<BehaviorMace>();
            if (mace != null && (!mace.Upswing.Done || !mace.MaceReturn.Done))
            {
                var weaponAngle = (float)LerpHelper.QuadraticOut(0, 1, mace.Upswing.Slide);
                var weaponScale = (float)LerpHelper.QuadraticOut(0.5, 1.0, mace.Upswing.Slide);
                var weaponStartPos = Util.AngleToVector(angleBody + MathHelper.PiOver4 * 2) * 8;
                var weaponEndPos = Util.AngleToVector(angleBody + MathHelper.PiOver4 * 3) * 10;
                var weaponPos = center + offset + Vector2.Lerp(weaponStartPos, weaponEndPos, (float)LerpHelper.QuadraticOut(0, 1, mace.Upswing.Slide));
                scene.DrawSpriteExt(SpriteWeapon, 0, weaponPos - SpriteWeapon.Middle, SpriteWeapon.Middle, angleBody + weaponAngle, new Vector2(weaponScale), SpriteEffects.None, Color.White, 0);
                if(!mace.Upswing.Done)
                {
                    mace.DrawMace(scene, weaponPos, Util.AngleToVector(mace.UpswingAngle) * (float)LerpHelper.QuadraticOut(0, 12, mace.MaceReturn.Slide), 2);   
                }
                if(!mace.MaceReturn.Done)
                {
                    var maceOffset = mace.MacePosition - weaponPos;
                    mace.DrawMace(scene, weaponPos, Vector2.Lerp(maceOffset, Vector2.Zero, (float)LerpHelper.QuadraticOut(0,1,mace.MaceReturn.Slide)), 8);
                }
            }
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
