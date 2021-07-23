using _7DRL_2021.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    class SceneTitle : Scene
    {
        public TitleUI Menu;

        public override float TimeMod => 1;

        public SceneTitle(Game game) : base(game)
        {
            Menu = new TitleUI(this);
        }

        private void DrawTextures()
        {
            Menu.PreDraw(this);
        }

        public override void Draw(GameTime gameTime)
        {
            DrawTextures();

            SetRenderTarget(null);

            PushSpriteBatch(blendState: NonPremultiplied, samplerState: SamplerState.PointWrap, projection: Projection);

            Menu.Draw(this);

            PopSpriteBatch();
        }

        public override void Update(GameTime gameTime)
        {
            Menu.Update(this);
            Menu.HandleInput(this);

            MenuCursor = Menu.GetMouseOver(InputState.MouseX, InputState.MouseY);
        }

        public void NewGame()
        {
            Game.Scene = new SceneGame(Game);
        }
    }
}
