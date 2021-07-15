using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    class SceneLoading : Scene
    {
        int Ticks = 0;
        TextBuilder LoadingText;

        public SceneLoading(Game game) : base(game)
        {
            LoadingText = new TextBuilder(float.PositiveInfinity, float.PositiveInfinity);
            LoadingText.StartLine(LineAlignment.Center);
            LoadingText.AppendText("Restarting Game...");
            LoadingText.EndLine();
            LoadingText.EndContainer();
            LoadingText.Finish();
        }

        public override void Draw(GameTime gameTime)
        {
            PushSpriteBatch();
            LoadingText.Draw(new Vector2(Viewport.Width / 2, Viewport.Height / 2), FontRenderer);
            PopSpriteBatch();
        }

        public override void Update(GameTime gameTime)
        {
            Ticks++;
            if (Ticks > 40)
            {
                Game.Scene = new SceneGame(Game);
            }
        }
    }
}
