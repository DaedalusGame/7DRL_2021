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

        public SceneLoading(Game game) : base(game)
        {
        }

        public override void Draw(GameTime gameTime)
        {
            PushSpriteBatch();
            DrawText("Restarting Game...", new Vector2(Viewport.Width / 2, Viewport.Height / 2), Alignment.Center, new TextParameters().SetBold(true).SetColor(Color.White, Color.Black));
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
