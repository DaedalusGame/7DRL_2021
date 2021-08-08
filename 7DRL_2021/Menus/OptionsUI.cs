using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Menus
{
    abstract class ActionSlider : ActActionNew
    {
        protected abstract float Slider { get; set; }

        public ActionSlider(Action<TextBuilder> text, Action action = null, Func<bool> enabled = null) : base(text, action, enabled)
        {
        }

        public override void Slide(int n)
        {
            float increment = 0.01f * Math.Sign(n);
            if (Math.Abs(n) > 10)
                increment = 0.1f * Math.Sign(n);
            Slider = MathHelper.Clamp(Slider + increment, 0f, 1f);
        }
    }

    class OptionsMenu : MenuActNew
    {
        class SoundSlider : ActionSlider
        {
            protected override float Slider { get => SoundLoader.SoundMasterVolume; set => SoundLoader.SoundMasterVolume = value; }

            public SoundSlider(Action<TextBuilder> text, Action action = null, Func<bool> enabled = null) : base(text, action, enabled)
            {
            }

            public override void Slide(int n)
            {
                base.Slide(n);
                //TODO: play sound
            }
        }

        class MusicSlider : ActionSlider
        {
            protected override float Slider { get => SoundLoader.MusicMasterVolume; set => SoundLoader.MusicMasterVolume = value; }

            public MusicSlider(Action<TextBuilder> text, Action action = null, Func<bool> enabled = null) : base(text, action, enabled)
            {
            }
        }

        class MasterSlider : ActionSlider
        {
            protected override float Slider { get => SoundLoader.MasterVolume; set => SoundLoader.MasterVolume = value; }

            public MasterSlider(Action<TextBuilder> text, Action action = null, Func<bool> enabled = null) : base(text, action, enabled)
            {
            }

            public override void Slide(int n)
            {
                base.Slide(n);
                //TODO: play sound
            }
        }

        public OptionsMenu(TitleUI ui) : base(ui.Scene, null, new Vector2(ui.Scene.Viewport.Width / 2, ui.Scene.Viewport.Height * 2 / 3), SpriteLoader.Instance.AddSprite("content/ui_box"), SpriteLoader.Instance.AddSprite("content/ui_box"), 256, 16 * 20)
        {
            var formatName = new TextFormatting()
            {
                Bold = true,
            };

            Add(new MasterSlider((builder) => {
                builder.StartLine(LineAlignment.Left);
                builder.AppendText("Master Volume", formatName);
                builder.NewLine();
                builder.AppendElement(new TextElementDynamic(() => SoundLoader.MasterVolume.ToString("##0%"), 32, builder.DefaultFormat, builder.DefaultDialog)
                {
                    Alignment = LineAlignment.Right
                });
                builder.AppendSpace(8);
                builder.AppendElement(new TextElementBar(30, () => SoundLoader.MasterVolume));
                builder.EndLine();
            }));
            Add(new MusicSlider((builder) => {
                builder.StartLine(LineAlignment.Left);
                builder.AppendText("Music Volume", formatName);
                builder.NewLine();
                builder.AppendElement(new TextElementDynamic(() => SoundLoader.MusicMasterVolume.ToString("##0%"), 32, builder.DefaultFormat, builder.DefaultDialog)
                {
                    Alignment = LineAlignment.Right
                });
                builder.AppendSpace(8);
                builder.AppendElement(new TextElementBar(30, () => SoundLoader.MusicMasterVolume));
                builder.EndLine();
            }));
            Add(new SoundSlider((builder) => {
                builder.StartLine(LineAlignment.Left);
                builder.AppendText("Sound Volume", formatName);
                builder.NewLine();
                builder.AppendElement(new TextElementDynamic(() => SoundLoader.SoundMasterVolume.ToString("##0%"), 32, builder.DefaultFormat, builder.DefaultDialog)
                {
                    Alignment = LineAlignment.Right
                });
                builder.AppendSpace(8);
                builder.AppendElement(new TextElementBar(30, () => SoundLoader.SoundMasterVolume));
                builder.EndLine();
            }));

            AddDefault(new ActActionNew((builder) => {
                builder.StartLine(LineAlignment.Left);
                builder.AppendText("Cancel", formatName);
                builder.NewLine();
                builder.AppendText("Return to previous menu", FormatDescription);
                builder.EndLine();
            }, () =>
            {
                Close(ui);
            }));
        }

        private static void Close(TitleUI ui)
        {
            ui.Scene.OptionsFile.Flush();
            ui.SubMenu.Close();
        }
    }

}
