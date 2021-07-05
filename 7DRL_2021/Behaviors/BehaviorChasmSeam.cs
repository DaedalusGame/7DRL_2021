using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace _7DRL_2021.Behaviors
{
    class BehaviorChasmSeam : Behavior, IDrawable
    {
        static SimpleNoise Noise = new SimpleNoise(0);
        static SimpleNoise NoiseX = new SimpleNoise(1);
        static SimpleNoise NoiseY = new SimpleNoise(2);

        public ICurio Curio;
        public Color Color;

        public BehaviorChasmSeam()
        {
        }

        public BehaviorChasmSeam(ICurio curio, Color color)
        {
            Curio = curio;
            Color = color;
        }

        public double DrawOrder => 0;

        public override void Apply()
        {
            Curio.AddBehavior(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorChasm(curio), Curio);
        }

        public void Draw(SceneGame scene, DrawPass pass)
        {
            var tile = Curio.GetMainTile();
            var wall = SpriteLoader.Instance.AddSprite("content/terrain_chasm_wall");

            var noise = Noise.GetValue(tile.X, tile.Y);
            int depth = (noise % 3) + 1;
            int ox = 0;
            int oy = 0;
            Color color = Color;
            switch (pass)
            {
                case DrawPass.Chasm1:
                    if (depth < 3) return;
                    ox = NoiseX.GetValue(tile.X, tile.Y);
                    oy = NoiseY.GetValue(tile.X, tile.Y);
                    color = Color.Lerp(color, Color.Black, 0.75f);
                    break;
                case DrawPass.Chasm2:
                    if (depth < 2) return;
                    ox = NoiseX.GetValue(tile.X + 1000, tile.Y);
                    oy = NoiseY.GetValue(tile.X, tile.Y);
                    color = Color.Lerp(color, Color.Black, 0.50f);
                    break;
                case DrawPass.Chasm3:
                    if (depth < 1) return;
                    ox = NoiseX.GetValue(tile.X + 2000, tile.Y);
                    oy = NoiseY.GetValue(tile.X, tile.Y);
                    color = Color.Lerp(color, Color.Black, 0.25f);
                    break;
            }
            scene.SpriteBatch.Draw(wall.Texture, new Rectangle(tile.X * 16, tile.Y * 16, 16, 16), new Rectangle(tile.X * 16 + ox, tile.Y * 16 + oy, 16, 16), color);
        }

        public void DrawIcon(SceneGame scene, Vector2 pos)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass.Chasm1;
            yield return DrawPass.Chasm2;
            yield return DrawPass.Chasm3;
        }

        public bool ShouldDraw(SceneGame scene, Vector2 cameraPosition)
        {
            var tile = Curio.GetMainTile();
            return tile.GetMap() == scene.Map && scene.IsWithinCamera(cameraPosition, tile.VisualTarget);
        }
    }
}
