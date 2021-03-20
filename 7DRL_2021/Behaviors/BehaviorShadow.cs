using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _7DRL_2021.Behaviors
{
    class BehaviorShadow : Behavior, ITickable, IDrawable
    {
        static SamplerState SamplerState = new SamplerState()
        {
            AddressU = TextureAddressMode.Wrap,
            AddressV = TextureAddressMode.Clamp,
            Filter = TextureFilter.Point,
        };

        public Curio Curio;
        public LerpVector2 ShadowPosition = new LerpVector2(Vector2.Zero);
        public double DrawOrder => 0;

        public BehaviorShadow()
        {
        }

        public BehaviorShadow(Curio curio)
        {
            Curio = curio;
        }

        public override void Apply()
        {
            Curio.AddBehavior(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            Apply(new BehaviorShadow((Curio)mapper.Map(Curio)), Curio);
        }

        public void Tick(SceneGame scene)
        {
            var tile = Curio.GetMainTile();
            if (tile != null) {
                var pos = tile.GetVisualTarget();
                if (pos != ShadowPosition.End)
                    ShadowPosition.Set(pos, LerpHelper.Quadratic, 10);
            }
            ShadowPosition.Update();
        }

        public void Draw(SceneGame scene, DrawPass pass)
        {
            var tile = Curio.GetMainTile();
            var shadow = SpriteLoader.Instance.AddSprite("content/ring_shadow");
            if (pass == DrawPass.EffectLow)
            {
                scene.FlushSpriteBatch();
                scene.DrawCircle(shadow, SamplerState, Curio.GetVisualTarget(), 20, 0, MathHelper.TwoPi, 12, 0, 1, 0, 1, ColorMatrix.Tint(Color.Black), scene.NonPremultiplied);

            }
            if (pass == DrawPass.EffectLowAdditive)
            {
                scene.SpriteBatch.Draw(scene.Pixel, tile.VisualPosition, new Rectangle(0, 0, 16, 16), new Color(159, 74, 153, 32));
            }
        }

        public void DrawIcon(SceneGame scene, Vector2 pos)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass.EffectLow;
            yield return DrawPass.EffectLowAdditive;
        }

        public bool ShouldDraw(SceneGame scene, Vector2 cameraPosition)
        {
            return Curio.GetMap() == scene.Map;
        }
    }
}
