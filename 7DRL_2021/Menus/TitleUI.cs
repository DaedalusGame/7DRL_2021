using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Menus
{
    class TitleUI : Menu
    {
        public SceneTitle Scene;
        SubMenuHandler<TitleMenu> TitleMenu = new SubMenuHandler<TitleMenu>() { BlocksInput = true, InputPriority = 10 };
        public SubMenuHandler<Menu> SubMenu = new SubMenuHandler<Menu>() { BlocksInput = true };

        protected override IEnumerable<IMenuArea> MenuAreas => Enumerable.Empty<IMenuArea>();
        protected override IEnumerable<SubMenuHandler> SubMenuHandlers => new SubMenuHandler[] { TitleMenu, SubMenu };

        public override FontRenderer FontRenderer => Scene.FontRenderer;

        public TitleUI(SceneTitle scene)
        {
            Scene = scene;
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
           
            if (Scene.TitleSM.CurrentState == TitleState.Finish && !TitleMenu.IsOpen)
                TitleMenu.Open(new TitleMenu(this));
        }

        public override void Draw(Scene scene)
        {
            TitleMenu.Draw(scene);

            SubMenu.Draw(scene);
        }
    }

    class TitleMenu : MenuActNew
    {
        public TitleMenu(TitleUI ui) : base(ui.Scene, null, new Vector2(ui.Scene.Viewport.Width / 2, ui.Scene.Viewport.Height * 3 / 4), SpriteLoader.Instance.AddSprite("content/ui_box"), SpriteLoader.Instance.AddSprite("content/ui_gab"), 256, 16 * 10)
        {
            var formatName = new TextFormatting() {
                Bold = true,
            };

            Add(new ActActionNew((builder) => {
                builder.StartLine(LineAlignment.Center);
                builder.AppendText("New Game", formatName);
                builder.NewLine();
                builder.AppendText("Starts a new game", FormatDescription);
                builder.EndLine();
            }, () =>
            {
                ui.Scene.NewGame();
            }));
            Add(new ActActionNew((builder) => {
                builder.StartLine(LineAlignment.Center);
                builder.AppendText("Options", formatName);
                builder.NewLine();
                builder.AppendText("Change game settings", FormatDescription);
                builder.EndLine();
            }, () =>
            {
                ui.SubMenu.Open(new OptionsMenu(ui.Scene));
            }));
            Add(new ActActionNew((builder) => {
                builder.StartLine(LineAlignment.Center);
                builder.AppendText("Statistics", formatName);
                builder.NewLine();
                builder.AppendText("View statistics", FormatDescription);
                builder.EndLine();
            }, () =>
            {
                ui.SubMenu.Open(new StatMenu(ui.Scene));
            }));
            /*Add(new ActActionNew((builder) => {
                builder.StartLine(LineAlignment.Center);
                builder.AppendText("Discord", formatName);
                builder.NewLine();
                builder.AppendText("Join our Discord", FormatDescription);
                builder.EndLine();
            }, () =>
            {
                Process.Start("https://discord.com/invite/J4bn3FG");
            }));
            Add(new ActActionNew((builder) => {
                builder.StartLine(LineAlignment.Center);
                builder.AppendText("Github", formatName);
                builder.NewLine();
                builder.AppendText("Report an issue", FormatDescription);
                builder.EndLine();
            }, () =>
            {
                Process.Start("https://github.com/DaedalusGame/7DRL_2021");
            }));*/
            Add(new ActActionNew((builder) => {
                builder.StartLine(LineAlignment.Center);
                builder.AppendText("Quit", formatName);
                builder.NewLine();
                builder.AppendText("Exits to desktop", FormatDescription);
                builder.EndLine();
            }, () =>
            {
                ui.Scene.Quit();
            }));
        }
    }
}
