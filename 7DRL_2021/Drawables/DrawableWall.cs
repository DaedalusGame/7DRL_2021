using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Drawables
{
    class DrawableWallSpiked : Drawable
    {
        SpriteReference Sprite;
        public SpriteReference Icon;
        DrawPass Pass;

        SimpleNoise Noise = new SimpleNoise(0);

        public DrawableWallSpiked(string id, SpriteReference sprite, DrawPass pass) : base(id)
        {
            Sprite = sprite;
            Icon = sprite;
            Pass = pass;
        }

        private Color GetColor(int x, int y)
        {
            Color color;
            color = new Color(124, 88, 114);
            return color;
        }

        public override void Draw(ICurio curio, SceneGame scene, DrawPass pass)
        {
            var spikes = SpriteLoader.Instance.AddSprite("content/terrain_spikes");
            var tile = curio.GetMainTile();
            var offset = curio.GetOffset();

            if (pass == DrawPass.WallBottom)
                scene.DrawSprite(spikes, 0, new Vector2(tile.X * 16 + 8, tile.Y * 16 + 8) - spikes.Middle, SpriteEffects.None, 0);
            if(pass == DrawPass.WallTop)
                scene.SpriteBatch.Draw(Sprite.Texture, new Rectangle(tile.X * 16, tile.Y * 16, 16, 16), new Rectangle(tile.X * 16, tile.Y * 16, 16, 16), GetColor(tile.X, tile.Y));
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
            yield return DrawPass.WallBottom;
            yield return DrawPass.WallTop;
        }
    }
}
