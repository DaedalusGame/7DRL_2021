using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
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

        public TitleUI(SceneTitle scene)
        {
            Scene = scene;

            TitleMenu.Open(new TitleMenu(this));
        }

        public override void Draw(Scene scene)
        {
            if (TitleMenu != null)
            {
                TitleMenu.Draw(scene);
            }

            if (SubMenu != null)
            {
                SubMenu.Draw(scene);
            }
        }
    }

    class TitleMenu : MenuTextSelection
    {
        public TitleMenu(TitleUI ui) : base(String.Empty, new Vector2(ui.Scene.Viewport.Width / 2, ui.Scene.Viewport.Height / 2), 256, 8)
        {
            Add(new ActAction("New Game", "", () =>
            {
                ui.Scene.NewGame();
            }));
        }
    }
}
