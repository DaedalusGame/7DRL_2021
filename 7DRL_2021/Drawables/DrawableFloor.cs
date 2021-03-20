using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Drawables
{
    class DrawableFloor : Drawable
    {
        SpriteReference Sprite;
        public SpriteReference Icon;
        public Color Color;
        DrawPass Pass;

        SimpleNoise Noise = new SimpleNoise(0);
        SimpleNoise NoiseX = new SimpleNoise(1);
        SimpleNoise NoiseY = new SimpleNoise(2);

        public DrawableFloor(string id, SpriteReference sprite, Color color, DrawPass pass) : base(id)
        {
            Sprite = sprite;
            Icon = sprite;
            Color = color;
            Pass = pass;
        }

        private Color GetColor(int x, int y)
        {
            return Color;
        }

        public override void Draw(ICurio curio, SceneGame scene, DrawPass pass)
        {
            var tile = curio.GetMainTile();
            //var offset = curio.GetOffset();

            if(pass != Pass)
            {
                //return;
                var wall = SpriteLoader.Instance.AddSprite("content/terrain_chasm_wall");

                var noise = Noise.GetValue(tile.X, tile.Y);
                int ox = 0;
                int oy = 0;
                Color color = GetColor(tile.X, tile.Y);
                switch(pass)
                {
                    case DrawPass.Chasm1:
                        ox = NoiseX.GetValue(tile.X, tile.Y);
                        oy = NoiseY.GetValue(tile.X, tile.Y);
                        color = Color.Lerp(color, Color.Black, 0.75f);
                        break;
                    case DrawPass.Chasm2:
                        ox = NoiseX.GetValue(tile.X + 1000, tile.Y);
                        oy = NoiseY.GetValue(tile.X, tile.Y);
                        color = Color.Lerp(color, Color.Black, 0.50f);
                        break;
                    case DrawPass.Chasm3:
                        ox = NoiseX.GetValue(tile.X + 2000, tile.Y);
                        oy = NoiseY.GetValue(tile.X, tile.Y);
                        color = Color.Lerp(color, Color.Black, 0.25f);
                        break;
                }
                scene.SpriteBatch.Draw(wall.Texture, new Rectangle(tile.X * 16, tile.Y * 16, 16, 16), new Rectangle(tile.X * 16 + ox, tile.Y * 16 + oy, 16, 16), color);
                return;
            }

            /*scene.PushSpriteBatch(shader: scene.Shader, shaderSetup: (matrix, projection) =>
            {
                scene.SetupColorMatrix(curio.GetColor() * ColorMatrix.Identity, matrix, projection);
            });*/
            var floor = Sprite;
            var debris = SpriteLoader.Instance.AddSprite("content/terrain_debris");
            if (Noise.GetValue(tile.X, tile.Y) % 100 < 3)
                floor = debris;
            scene.SpriteBatch.Draw(floor.Texture, new Rectangle(tile.X * 16, tile.Y * 16, 16, 16), new Rectangle(tile.X * 16, tile.Y * 16, 16, 16), GetColor(tile.X, tile.Y));
            //scene.PopSpriteBatch();
        }

        public override void DrawIcon(ICurio curio, SceneGame scene, Vector2 pos)
        {
            var tile = curio.GetMainTile();
            /*scene.PushSpriteBatch(shader: scene.Shader, shaderSetup: (matrix, projection) =>
            {
                scene.SetupColorMatrix(curio.GetColor() * ColorMatrix.Identity, matrix, projection);
            });*/
            scene.DrawSprite(Icon, 0, pos - Icon.Middle, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, GetColor(tile.X, tile.Y), 0);
            //scene.PopSpriteBatch();
        }

        public override IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return Pass;
            //yield return DrawPass.Chasm1;
            //yield return DrawPass.Chasm2;
            //yield return DrawPass.Chasm3;
        }
    }
}
