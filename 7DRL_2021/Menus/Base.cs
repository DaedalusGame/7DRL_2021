using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Menus
{
    class NameInput : Menu
    {
        Scene Scene;
        string OldString;
        public string NewString;
        public bool HasResult => OldString != NewString.Trim();

        public Vector2 Position;
        private MenuAreaBasic MenuAreaBasic;
        protected override IEnumerable<IMenuArea> MenuAreas => new[] { MenuAreaBasic };

        LabelledUI UI;
        TextBuilder Text;

        public override FontRenderer FontRenderer => Scene.FontRenderer;

        public NameInput(Scene scene, string name, string text, Vector2 position, int width, string oldString)
        {
            Scene = scene;
            OldString = oldString;
            NewString = oldString;
            Position = position;
            Width = width;
            MenuAreaBasic = new MenuAreaBasic(this, () => Position, 10);
            SpriteReference textbox = SpriteLoader.Instance.AddSprite("content/ui_box");
            var formatting = new TextFormatting()
            {
                Bold = false,
            };
            var dialogInstant = new DialogFormattingInstant();
            UI = new LabelledUI(textbox, textbox, textBuilder =>
            {
                textBuilder.AppendText(name, formatting, dialogInstant);
            }, () => new Point(Width, Height));

            Text = new TextBuilder(Width - 16 - 16, Height);
            Text.StartLine(LineAlignment.Left);
            Text.AppendText(text, formatting, dialogInstant);
            Text.EndLine();
            Text.StartLine(LineAlignment.Left);
            //TODO: Text Input
            Text.EndLine();
            Text.EndContainer();
            Text.Finish();
        }

        public override int Height
        {
            get
            {
                return 16 * 3;
            }
            set
            {
                //NOOP
            }
        }

        public override void HandleInput(Scene scene)
        {
            scene.InputState.AddText(ref NewString);
            if (scene.InputState.IsKeyPressed(Keys.Enter))
                Close();
            if (scene.InputState.IsKeyPressed(Keys.Escape))
            {
                NewString = OldString;
                Close();
            }
            base.HandleInput(scene);
        }

        public override void PreDraw(Scene scene)
        {
            base.PreDraw(scene);
            scene.PushSpriteBatch(blendState: scene.NonPremultiplied, samplerState: SamplerState.PointWrap, projection: Projection);
            scene.GraphicsDevice.Clear(Color.TransparentBlack);
            Text.Draw(new Vector2(0, 0), FontRenderer);
            //scene.DrawText(Text, new Vector2(8, 4), Alignment.Left, new TextParameters().SetColor(Color.White, Color.Black).SetConstraints(Width - 16 - 16, int.MaxValue));
            //scene.DrawText($"{NewString}{Game.FormatBlinkingCursor(Ticks, 40)}", new Vector2(8, 4 + 16), Alignment.Center, new TextParameters().SetColor(Color.White, Color.Black).SetConstraints(Width - 16 - 16, int.MaxValue));
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

    class InfoBox : Menu
    {
        Scene Scene;
        public Vector2 Position;
        public int Scroll;

        public SpriteReference TextBoxSprite = SpriteLoader.Instance.AddSprite("content/ui_box");
        public SpriteReference LabelSprite = SpriteLoader.Instance.AddSprite("content/ui_box");

        MenuAreaBasic MenuAreaBasic;
        LabelledUI UI;
        TextBuilder Text;

        protected override IEnumerable<IMenuArea> MenuAreas => new IMenuArea[] { MenuAreaBasic };
        public override FontRenderer FontRenderer => Scene.FontRenderer;

        public InfoBox(Scene scene, Action<TextBuilder> name, Action<TextBuilder> text, Vector2 position, int width, int height)
        {
            Scene = scene;
            Position = position;
            Width = width;
            Height = height;
            MenuAreaBasic = new MenuAreaBasic(this, () => Position, 10);

            SpriteReference textbox = SpriteLoader.Instance.AddSprite("content/ui_box");
            var formatting = new TextFormatting()
            {
                Bold = false,
            };
            var dialogInstant = new DialogFormattingInstant();
            UI = new LabelledUI(textbox, textbox, name, () => new Point(Width, Height));

            Text = new TextBuilder(Width, Height);
            text(Text);
            Text.EndContainer();
            Text.Finish();
        }

        public override void HandleInput(Scene scene)
        {
            int textHeight = (int)Text.GetContentHeight();
            if (scene.InputState.IsKeyPressed(Keys.Enter))
                Close();
            if (scene.InputState.IsKeyPressed(Keys.Escape))
                Close();
            if (scene.InputState.IsKeyPressed(Keys.W, 10, 1))
                Scroll -= 3;
            if (scene.InputState.IsKeyPressed(Keys.S, 10, 1))
                Scroll += 3;
            if (scene.InputState.IsMouseWheelUp())
                Scroll -= 6;
            if (scene.InputState.IsMouseWheelDown())
                Scroll += 6;
            Scroll = MathHelper.Clamp(Scroll, 0, textHeight - Height + 8);
            base.HandleInput(scene);
        }

        public override void PreDraw(Scene scene)
        {
            base.PreDraw(scene);
            scene.PushSpriteBatch(blendState: scene.NonPremultiplied, samplerState: SamplerState.PointWrap, projection: Projection);
            scene.GraphicsDevice.Clear(Color.TransparentBlack);
            Text.Draw(new Vector2(8, 4 - Scroll), FontRenderer);
            //scene.DrawText(Text(), new Vector2(8, 4 - Scroll), Alignment.Left, new TextParameters().SetColor(TextColor, BorderColor).SetConstraints(Width - 16 - 16, int.MaxValue));
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
                //scene.DrawText(Text(), new Vector2(x+8, y+4), Alignment.Left, new TextParameters().SetColor(Color.White,Color.Black).SetConstraints(Width - 16 - 16, Height-8));
            }
        }
    }

    class ControlInfo : Menu
    {
        public class ControlTag
        {
            SpriteReference Sprite = SpriteLoader.Instance.AddSprite("content/ui_tag");
            public TextBuilder Text;

            public int Width => (int)Text.GetContentWidth() + 24;
            public int Height => (int)Text.GetContentHeight();

            public ControlTag(Action<TextBuilder> text)
            {
                TextFormatting format = new TextFormatting()
                {
                    Bold = false,
                    GetParams = pos => new DialogParams()
                    {
                        Color = Color.Black,
                        Border = Color.Transparent,
                        Scale = Vector2.One,
                    }
                };
                Text = new TextBuilder(160, float.PositiveInfinity, defaultFormat: format);
                Text.StartLine(LineAlignment.Left);
                text(Text);
                Text.EndLine();
                Text.EndContainer();
                Text.Finish();
            }

            public void Draw(FontRenderer fontRenderer, int x, int y)
            {
                var uiRect = new Rectangle(x, y, Width, Height);
                fontRenderer.Scene.DrawUI(Sprite, uiRect, Color.White);
                Text.Draw(new Vector2(x + 4, y), fontRenderer);
            }
        }

        Scene Scene;
        public override FontRenderer FontRenderer => Scene.FontRenderer;
        List<ControlTag> Tags = new List<ControlTag>();
        protected override IEnumerable<IMenuArea> MenuAreas => Tags.SelectMany(x => x.Text.MenuAreas);

        public ControlInfo(Scene scene, IEnumerable<ControlTag> strings)
        {
            Scene = scene;
            Tags.AddRange(strings);
        }

        public override void Draw(Scene scene)
        {
            var ui = SpriteLoader.Instance.AddSprite("content/ui_tag");
            var x = 8;
            var y = 64;
            foreach (var tag in Tags)
            {
                tag.Draw(FontRenderer, x, y);

                y += tag.Height + 8;
            }
        }
    }

    class CardAct : Menu
    {
        public Scene Scene;

        public CardAction[] Actions;
        public CardArea[] CardAreas;
        public LabelledUI[] CardUIs;
        public override int Width
        {
            get
            {
                return Scene.Viewport.Width * 8 / 10;
            }
            set
            {
                //NOOP;
            }
        }
        public override int Height
        {
            get
            {
                return Scene.Viewport.Height / 6;
            }
            set
            {
                //NOOP;
            }
        }
        public int CardWidth = 128;
        public int CardHeight => Height + 40;

        protected override IEnumerable<IMenuArea> MenuAreas => CardAreas;
        public override FontRenderer FontRenderer => Scene.FontRenderer;

        public CardAct(Scene scene, CardAction[] actions)
        {
            Scene = scene;
            Actions = actions;
            CardAreas = new CardArea[Actions.Length];
            var ui = SpriteLoader.Instance.AddSprite("content/ui_box");
            for (int i = 0; i < Actions.Length; i++)
            {
                CardAction action = Actions[i];
                //if(action != null)
                //    action.Frame.Time = i * -10; //TODO: Make this work
                CardAreas[i] = new CardArea(this, i, 15 + i * 5);
                CardUIs[i] = new LabelledUIText(ui, ui, labelBuilder =>
                {
                    labelBuilder.AppendText(action.Name);
                }, contentBuilder =>
                {
                    contentBuilder.StartLine(LineAlignment.Center);
                    contentBuilder.AppendText(action.Name);
                    contentBuilder.EndLine();
                });
            }
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            foreach (var action in Actions)
                action?.Update();
        }

        public override void HandleInput(Scene scene)
        {
            base.HandleInput(scene);

            var state = scene.InputState;

            /*for (int i = 0; i < Actions.Length; i++)
            {
                CardAction action = Actions[i];
                if (action != null)
                    action.Hovered = action.Enabled() && scene.MenuCursor is CardArea area && i == area.Index && area.Menu == this;
            }

            if (state.IsMousePressed(MouseButton.Left) && scene.MenuCursor is CardArea card && card.Menu == this)
            {
                CardAction action = Actions[card.Index];
                if (action != null && action.Enabled())
                    action.Action();
            }*/
        }

        public virtual Rectangle GetSelectRectangle(int index)
        {
            int offset = (Scene.Viewport.Width - Width) / 2;
            int widthInterior = Width - CardWidth;
            int segmentWidth = widthInterior / (Actions.Length - 1);
            int x = offset + index * segmentWidth;
            int y = Scene.Viewport.Height - Height;

            return new Rectangle(x, y, CardWidth, Height);
        }

        public override void Draw(Scene scene)
        {
            var ui = SpriteLoader.Instance.AddSprite("content/ui_box");

            int offset = (scene.Viewport.Width - Width) / 2;
            int widthInterior = Width - CardWidth;
            int segmentWidth = widthInterior / (Actions.Length - 1);

            for (int i = 0; i < Actions.Length; i++)
            {
                var action = Actions[i];
                if (action == null)
                    continue;
                int height = Height;
                if (!action.Enabled())
                    height = height * 4 / 5;
                height += (int)MathHelper.Lerp(0, 30, (float)LerpHelper.QuadraticOut(0, 1, action.HoverFrame.Slide));
                int x = offset + i * segmentWidth;
                int y = scene.Viewport.Height - (int)MathHelper.Lerp(0, height, (float)LerpHelper.ElasticOut(0, 1, action.Frame.Slide));
                CardUIs[i].Draw(FontRenderer, x, y, 1);
                //DrawLabelledUI(scene, ui, ui, new Rectangle(x, y, CardWidth, height + 40), action.Name);
                //scene.DrawText(action.Description, new Vector2(x, y), Alignment.Center, new TextParameters().SetColor(Color.White, Color.Black).SetConstraints(CardWidth, height));
            }
        }
    }

    class CardArea : IMenuArea
    {
        public CardAct Menu;
        public int Index;
        public double Priority
        {
            get;
            set;
        }
        public IMenuAnchor MouseTransform { get; set; }
        public ITooltipProvider Tooltip { get; set; }

        public CardArea(CardAct menu, int index, double priority)
        {
            Menu = menu;
            Index = index;
            Priority = priority;
        }

        public void AddTooltip(TextBuilder text)
        {
            //NOOP
        }

        public bool IsWithin(Vector2 mousePos)
        {
            return Menu.GetSelectRectangle(Index).Contains(mousePos);
        }
    }

    class CardAction
    {
        public virtual string Name
        {
            get;
            set;
        }
        public virtual string Description
        {
            get;
            set;
        }

        public Action Action = () => { };
        public Func<bool> Enabled = () => true;
        public Slider Frame;
        public bool Hovered;
        public Slider HoverFrame = new Slider(10);

        public CardAction(string name, string description, int appearTime, Action action, Func<bool> enabled = null)
        {
            Name = name;
            Description = description;
            Action = action;
            if (action != null)
                Action = action;
            if (enabled != null)
                Enabled = enabled;
            Frame = new Slider(appearTime);
        }

        public void Update()
        {
            Frame += 1;
            if (Hovered)
                HoverFrame += 1;
            else
                HoverFrame -= 1;
        }
    }
    /*
    abstract class MenuAct : Menu
    {
        class SelectionArea : IMenuArea
        {
            public MenuAct Menu;
            public double Priority
            {
                get;
                set;
            }
            public int Index;

            public SelectionArea(MenuAct menu, int index, double priority)
            {
                Menu = menu;
                Priority = priority;
                Index = index;
            }

            public void GenerateTooltip(TextBuilder text)
            {
                //NOOP
            }

            public bool IsWithin(int x, int y)
            {
                int x1 = (int)Menu.Position.X - Menu.Width / 2;
                int y1 = (int)Menu.Position.Y - Menu.Height / 2;

                int top = Index * Menu.LineHeight - Menu.Scroll;

                return Menu.MenuAreaBasic.IsWithin(x, y) && new Rectangle(x1, y1 + top, Menu.Width, Menu.LineHeight).Contains(x, y);
            }
        }

        public string Name;

        public abstract int SelectionCount
        {
            get;
        }
        public int Selection;
        public int Scroll;
        public int ScrollHeight;

        public int DefaultSelection = -1;

        public Vector2 Position;
        private MenuAreaBasic MenuAreaBasic;
        private List<SelectionArea> SelectionAreas = new List<SelectionArea>();

        protected override IEnumerable<IMenuArea> MenuAreas => new IMenuArea[] { MenuAreaBasic }.Concat(SelectionAreas);

        public override int Height
        {
            get { return ScrollHeight * LineHeight; }
            set { }
        }
        public virtual int LineHeight => 16;

        public MenuAct(string name, Vector2 position, int width, int scrollHeight)
        {
            Name = name;
            Position = position;
            Width = width;
            ScrollHeight = scrollHeight;
            MenuAreaBasic = new MenuAreaBasic(this, () => Position, 10);
        }

        public abstract void Select(int selection);

        public override void Update(Scene scene)
        {
            base.Update(scene);
            if (SelectionAreas.Count != SelectionCount)
            {
                SelectionAreas.RemoveAll(x => x.Index >= SelectionCount);
                for (int i = SelectionAreas.Count; i < SelectionCount; i++)
                {
                    SelectionAreas.Add(new SelectionArea(this, i, 15));
                }
            }
        }

        public override void HandleInput(Scene scene)
        {
            base.HandleInput(scene);

            if (InputBlocked)
                return;

            if (scene.InputState.IsKeyPressed(Keys.Enter) && Selection < SelectionCount)
                Select(Selection);
            if (scene.InputState.IsKeyPressed(Keys.Escape) && DefaultSelection >= 0)
                Select(DefaultSelection);
            if (scene.InputState.IsKeyPressed(Keys.W, 15, 5))
                Selection--;
            if (scene.InputState.IsKeyPressed(Keys.S, 15, 5))
                Selection++;
            Selection = SelectionCount <= 0 ? 0 : (Selection + SelectionCount) % SelectionCount;
            Scroll = MathHelper.Clamp(Scroll, Math.Max(0, Selection * LineHeight - Height + LineHeight), Math.Min(Selection * LineHeight, SelectionCount * LineHeight - Height));

        }

        public override void PreDraw(Scene scene)
        {
            base.PreDraw(scene);

            scene.PushSpriteBatch(blendState: scene.NonPremultiplied, samplerState: SamplerState.PointWrap, projection: Projection);
            scene.GraphicsDevice.Clear(Color.TransparentBlack);
            for (int i = 0; i < SelectionCount; i++)
            {
                DrawLine(scene, new Vector2(0, i * LineHeight - Scroll), i);
            }
            scene.PopSpriteBatch();
        }

        public override void Draw(Scene scene)
        {
            SpriteReference textbox = SpriteLoader.Instance.AddSprite("content/ui_box");
            int x = (int)Position.X - Width / 2;
            int y = (int)Position.Y - Height / 2;
            float openCoeff = Math.Min(Ticks / 7f, 1f);
            float openResize = MathHelper.Lerp(-0.5f, 0.0f, openCoeff);
            Rectangle rect = new Rectangle(x, y, Width, Height);
            rect.Inflate(rect.Width * openResize, rect.Height * openResize);
            //if (openCoeff > 0)
            //    DrawLabelledUI(scene, textbox, textbox, rect, openCoeff >= 1 ? Name : string.Empty);
            if (openCoeff >= 1)
                scene.SpriteBatch.Draw(RenderTarget, rect, RenderTarget.Bounds, Color.White);
        }

        public abstract void DrawLine(Scene scene, Vector2 linePos, int e);
    }
    */
    /*class ActAction
    {
        public virtual string Name
        {
            get;
            set;
        }
        public virtual string Description
        {
            get;
            set;
        }
        public Action Action = () => { };
        public Func<bool> Enabled = () => true;

        public ActAction(string name, string description, Action action, Func<bool> enabled = null)
        {
            Name = name;
            Description = description;
            Action = action;
            if (action != null)
                Action = action;
            if (enabled != null)
                Enabled = enabled;
        }
    }*/

    /*class MenuTextSelection : MenuAct
    {
        Scene Scene;
        List<ActAction> Actions = new List<ActAction>();

        public override int SelectionCount => Actions.Count;
        public Alignment Alignment = Alignment.Left;

        public override FontRenderer FontRenderer => Scene.FontRenderer;

        public MenuTextSelection(Scene scene, string name, Vector2 position, int width, int scrollHeight) : base(name, position, width, scrollHeight)
        {
            Scene = scene;
        }

        public void Add(ActAction action)
        {
            Actions.Add(action);
        }

        public void AddDefault(ActAction action)
        {
            DefaultSelection = Actions.Count;
            Add(action);
        }

        public override void Select(int selection)
        {
            if (Actions[selection].Enabled())
                Actions[selection].Action();
        }

        public override void DrawLine(Scene scene, Vector2 linePos, int e)
        {
            ActAction action = Actions[e];
            SpriteReference cursor = SpriteLoader.Instance.AddSprite("content/cursor");
            if (Selection == e)
                scene.SpriteBatch.Draw(cursor.Texture, linePos + new Vector2(0, LineHeight / 2 - cursor.Height / 2), cursor.GetFrameRect(0), Color.White);
            Color color = Color.White;
            if (!action.Enabled())
                color = Color.Gray;
            //scene.DrawText(action.Name, linePos + new Vector2(16, 0), Alignment, new TextParameters().SetConstraints(Width - 32, 16).SetBold(true).SetColor(color, Color.Black));
        }
    }*/
}
