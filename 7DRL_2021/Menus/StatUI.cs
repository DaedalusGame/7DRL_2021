using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Menus
{
    class StatMenu : Menu
    {
        public abstract class Tab
        {
            protected StatMenu StatMenu;
            public TextBuilder Text;
            public bool Dirty = true;
            public List<ITextElement> Lines = new List<ITextElement>();

            protected virtual bool CanInit => true;

            public Tab(StatMenu menu)
            {
                StatMenu = menu;
            }

            public void Update()
            {
                if (Dirty && CanInit)
                {
                    Init();
                    Dirty = false;
                }

                Text?.Update();
            }

            public abstract void Init();
        }

        public class RunTab : Tab
        {
            public HighscoreRunFile File;
            public RunStats Stats => File.Score;
            public AsyncCheck Loading;

            protected override bool CanInit => Loading.Done;

            public RunTab(StatMenu menu, HighscoreRunFile file, AsyncCheck loading) : base(menu)
            {
                File = file;
                Loading = loading;
            }

            public override void Init()
            {
                Text = new TextBuilder(StatMenu.Width, float.PositiveInfinity);
                AddLine((left) =>
                {
                    left.StartLine(LineAlignment.Left);
                    left.AppendText("Level");
                    left.EndLine();
                }, (right) =>
                {
                    right.StartLine(LineAlignment.Right);
                    right.AppendText(Stats.Level.ToString());
                    right.EndLine();
                });
                AddLine((left) =>
                {
                    left.StartLine(LineAlignment.Left);
                    left.AppendText("Score");
                    left.EndLine();
                }, (right) =>
                {
                    right.StartLine(LineAlignment.Right);
                    right.AppendText(Stats.Score.ToString());
                    right.EndLine();
                });
                AddLine((left) =>
                {
                    left.StartLine(LineAlignment.Left);
                    left.AppendText("Cards");
                    left.EndLine();
                }, (right) =>
                {
                    right.StartLine(LineAlignment.Right);
                    foreach(var card in Stats.Cards)
                    {
                        right.AppendElement(new TextElementIcon(card, right.DefaultFormat, right.DefaultDialog));
                    }
                    right.EndLine();
                });
                AddLine((left) =>
                {
                    left.StartLine(LineAlignment.Left);
                    left.AppendText("Kills");
                    left.EndLine();
                }, (right) =>
                {
                    right.StartLine(LineAlignment.Right);
                    right.AppendText(Stats.Kills.ToString());
                    right.EndLine();
                });
                AddLine((left) =>
                {
                    left.StartLine(LineAlignment.Left);
                    left.AppendText("Gibs");
                    left.EndLine();
                }, (right) =>
                {
                    right.StartLine(LineAlignment.Right);
                    right.AppendText(Stats.Gibs.ToString());
                    right.EndLine();
                });
                AddLine((left) =>
                {
                    left.StartLine(LineAlignment.Left);
                    left.AppendText("Splats Collected");
                    left.EndLine();
                }, (right) =>
                {
                    right.StartLine(LineAlignment.Right);
                    right.AppendText(Stats.Splats.ToString());
                    right.EndLine();
                });
                AddLine((left) =>
                {
                    left.StartLine(LineAlignment.Left);
                    left.AppendText("Hearts Ripped");
                    left.EndLine();
                }, (right) =>
                {
                    right.StartLine(LineAlignment.Right);
                    right.AppendText(Stats.HeartsRipped.ToString());
                    right.EndLine();
                });
                AddLine((left) =>
                {
                    left.StartLine(LineAlignment.Left);
                    left.AppendText("Hearts Eaten");
                    left.EndLine();
                }, (right) =>
                {
                    right.StartLine(LineAlignment.Right);
                    right.AppendText(Stats.HeartsEaten.ToString());
                    right.EndLine();
                });
                AddLine((left) =>
                {
                    left.StartLine(LineAlignment.Left);
                    left.AppendText("Rats Hunted");
                    left.EndLine();
                }, (right) =>
                {
                    right.StartLine(LineAlignment.Right);
                    right.AppendText(Stats.RatsHunted.ToString());
                    right.EndLine();
                });
                AddLine((left) =>
                {
                    left.StartLine(LineAlignment.Left);
                    left.AppendText("Cards Crushed");
                    left.EndLine();
                }, (right) =>
                {
                    right.StartLine(LineAlignment.Right);
                    right.AppendText(Stats.CardsCrushed.ToString());
                    right.EndLine();
                });
                Text.EndContainer();
                Text.Finish();
            }

            private void AddLine(Action<TextBuilder> leftText, Action<TextBuilder> rightText)
            {
                Text.StartTableRow(StatMenu.Width, new ColumnConfigs(new IColumnWidth[] {
                    new ColumnFixedWidth(16, true),
                    new ColumnFractionalWidth(0.5f, false),
                    new ColumnFractionalWidth(0.5f, false),
                     new ColumnFixedWidth(16, true),
                }));
                Text.StartTableCell();
                Text.EndTableCell();
                Text.StartTableCell();
                leftText(Text);
                Text.EndTableCell();
                Text.StartTableCell();
                rightText(Text);
                Text.EndTableCell();
                Lines.Add(Text.EndTableRow());
            }
        }

        Scene Scene;
        LabelledUI UI;
        public Vector2 Position;  
        List<Tab> Tabs = new List<Tab>();

        Tab LeftTab;
        Tab RightTab;
        LerpFloat SwitchLerp = new LerpFloat(0);
        public int Selection;
        public int TabSelection;
        public LerpFloat Scroll = new LerpFloat(0);

        public override FontRenderer FontRenderer => Scene.FontRenderer;

        protected override IEnumerable<IMenuArea> MenuAreas => Enumerable.Empty<IMenuArea>();

        public StatMenu(Scene scene)
        {
            Scene = scene;
            Width = scene.Viewport.Width / 3;
            Height = scene.Viewport.Height / 2;
            Position = new Vector2(scene.Viewport.Width / 2, scene.Viewport.Height / 2);

            UI = new LabelledUI(SpriteLoader.Instance.AddSprite("content/ui_box"), SpriteLoader.Instance.AddSprite("content/ui_box"), null, () => new Point(Width, Height));

            foreach(var file in GetRunScores())
            {
                Tabs.Add(new RunTab(this, file, file.ReloadAsync()));
            }

            LeftTab = Tabs.First();

            var formatName = new TextFormatting()
            {
                Bold = true,
            };
        }

        private IEnumerable<HighscoreRunFile> GetRunScores()
        {
            DirectoryInfo dir = new DirectoryInfo("stats/runs");
            return dir.GetFiles("*.json").Select(file => new HighscoreRunFile(file.FullName, new RunStats()));
        }

        public override void HandleInput(Scene scene)
        {
            base.HandleInput(scene);

            if (InputBlocked)
                return;

            Tab tab = GetCurrentTab();

            if (scene.InputState.IsKeyPressed(Keys.W, 15, 5))
                Selection--;
            if (scene.InputState.IsKeyPressed(Keys.S, 15, 5))
                Selection++;
            if (scene.InputState.IsKeyPressed(Keys.A, 15, 5))
            {
                TabSelection = Tabs.Count <= 0 ? 0 : (TabSelection + Tabs.Count - 1) % Tabs.Count;
                LeftTab = Tabs[TabSelection];
                RightTab = tab;
                SwitchLerp.Set(1, 0, LerpHelper.Linear, 10);
            }
            if (scene.InputState.IsKeyPressed(Keys.D, 15, 5))
            {
                TabSelection = Tabs.Count <= 0 ? 0 : (TabSelection + Tabs.Count + 1) % Tabs.Count;
                LeftTab = tab;
                RightTab = Tabs[TabSelection];
                SwitchLerp.Set(0, 1, LerpHelper.Linear, 10);
            }

            Selection = tab.Lines.Count <= 0 ? 0 : (Selection + tab.Lines.Count) % tab.Lines.Count;
            UpdateScroll(tab);
        }

        private Tab GetCurrentTab()
        {
            if (SwitchLerp.End < 0.5f)
                return LeftTab;
            else
                return RightTab;
        }

        public void UpdateScroll(Tab tab)
        {
            if (tab.Lines.Empty())
                return;
            var currentTop = tab.Lines[Selection].GetTop();
            var currentBottom = tab.Lines[Selection].GetBottom();
            if (currentTop < Scroll)
            {
                Scroll.Set(currentTop, LerpHelper.Linear, 5);
            }
            if (currentBottom > Scroll + Height)
            {
                Scroll.Set(currentBottom - Height, LerpHelper.Linear, 5);
            }
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);

            LeftTab?.Update();
            RightTab?.Update();

            SwitchLerp.Update();
            Scroll.Update();
        }

        public override void PreDraw(Scene scene)
        {
            base.PreDraw(scene);
            scene.PushSpriteBatch(blendState: scene.NonPremultiplied, samplerState: SamplerState.PointWrap, projection: Projection);
            scene.GraphicsDevice.Clear(Color.TransparentBlack);
            if (LeftTab != null && LeftTab.Text != null)
                LeftTab.Text.Draw(new Vector2(-Width * SwitchLerp.Value, -Scroll), FontRenderer);
            if (RightTab != null && RightTab.Text != null)
                RightTab.Text.Draw(new Vector2(Width * (1 - SwitchLerp.Value), -Scroll), FontRenderer);
            scene.PopSpriteBatch();
        }

        public override void Draw(Scene scene)
        {
            int x = (int)Position.X - Width / 2;
            int y = (int)Position.Y - Height / 2;
            float openCoeff = Math.Min(Ticks / 7f, 1f);
            UI.Draw(FontRenderer, x, y, openCoeff);
            if (openCoeff >= 1)
            {
                scene.SpriteBatch.Draw(RenderTarget, new Rectangle(x, y, RenderTarget.Width, RenderTarget.Height), RenderTarget.Bounds, Color.White);
            }
        }
    }
}
