using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Drawables
{
    class DrawableTile : Drawable
    {
        SpriteReference Sprite;
        public SpriteReference Icon;
        DrawPass Pass;

        SimpleNoise Noise = new SimpleNoise(0);

        public DrawableTile(string id, SpriteReference sprite, DrawPass pass) : base(id)
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
            var tile = curio.GetMainTile();
            var offset = curio.GetOffset();
            /*scene.PushSpriteBatch(shader: scene.Shader, shaderSetup: (matrix, projection) =>
            {
                scene.SetupColorMatrix(curio.GetColor() * ColorMatrix.Identity, matrix, projection);
            });*/
            var floor = Sprite;
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
        }
    }
}
