using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Menus
{
    class ActionSliderSound : ActActionNew
    {
        public ActionSliderSound(Action<TextBuilder> text, Action action, Func<bool> enabled = null) : base(text, action, enabled)
        {
        }

        public override void Slide(int n)
        {
            float increment = 0.01f * Math.Sign(n);
            if (Math.Abs(n) > 10)
                increment = 0.1f * Math.Sign(n);
            SoundLoader.SoundMasterVolume = MathHelper.Clamp(SoundLoader.SoundMasterVolume + increment, 0f, 1f);
            //TODO: Play sound
        }
    }

    class OptionsMenu : MenuActNew
    {
        public OptionsMenu(TitleUI ui) : base(ui.Scene, null, new Vector2(ui.Scene.Viewport.Width / 2, ui.Scene.Viewport.Height * 2 / 3), SpriteLoader.Instance.AddSprite("content/ui_box"), SpriteLoader.Instance.AddSprite("content/ui_box"), 256, 16 * 20)
        {
            var formatName = new TextFormatting()
            {
                Bold = true,
            };

            Add(new ActActionNew((builder) => {
                builder.StartLine(LineAlignment.Left);
                builder.AppendText("Master Volume", formatName);
                builder.NewLine();
                builder.EndLine();
            }, () =>
            {

            }));
            Add(new ActActionNew((builder) => {
                builder.StartLine(LineAlignment.Left);
                builder.AppendText("Music Volume", formatName);
                builder.NewLine();
                builder.EndLine();
            }, () =>
            {

            }));
            Add(new ActionSliderSound((builder) => {
                builder.StartLine(LineAlignment.Left);
                builder.AppendText("Sound Volume", formatName);
                builder.NewLine();
                builder.AppendElement(new TextElementDynamic(() => SoundLoader.SoundMasterVolume.ToString("##0%"), 32, builder.DefaultFormat, builder.DefaultDialog)
                {
                    Alignment = LineAlignment.Right
                });
                builder.EndLine();
            }, () =>
            {

            }));

            AddDefault(new ActActionNew((builder) => {
                builder.StartLine(LineAlignment.Left);
                builder.AppendText("Cancel", formatName);
                builder.NewLine();
                builder.AppendText("Return to previous menu", FormatDescription);
                builder.EndLine();
            }, () =>
            {
                ui.SubMenu.Close();
            }));
        }
    }

}
