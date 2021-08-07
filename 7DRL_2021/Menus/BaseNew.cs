using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    class ActActionNew
    {
        public Action<TextBuilder> Text;
        public Action Action = () => { };
        public Func<bool> Enabled = () => true;

        public ActActionNew(Action<TextBuilder> text, Action action, Func<bool> enabled = null)
        {
            Text = text;
            if (action != null)
                Action = action;
            if (enabled != null)
                Enabled = enabled;
        }

        public virtual void Slide(int n)
        {
        }
    }

    class MenuActNew : Menu
    {
        public override FontRenderer FontRenderer => Scene.FontRenderer;
        Scene Scene;
        LabelledUI UI;
        TextBuilder Text;
        public Vector2 Position;
        List<ActActionNew> Actions = new List<ActActionNew>();
        Dictionary<int, MenuAreaText> SelectionAreas = new Dictionary<int, MenuAreaText>(); 

        public int Selection;
        public int SelectionCount => Actions.Count;
        public LerpFloat Scroll = new LerpFloat(0);

        int SlideCount;

        public int DefaultSelection = -1;

        bool Dirty = true;

        protected override IEnumerable<IMenuArea> MenuAreas => Text.MenuAreas;

        public TextFormatting FormatDescription = new TextFormatting()
        {
            GetParams = (pos) => new DialogParams()
            {
                Scale = Vector2.One,
                Color = Color.DarkGray,
                Border = Color.Black,
            }
        };

        public MenuActNew(Scene scene, Action<TextBuilder> name, Vector2 position, SpriteReference label, SpriteReference ui, int width, int height)
        {
            Scene = scene;
            Position = position;
            Width = width;
            Height = height;

            UI = new LabelledUI(label, ui, name, () => new Point(Width, Height));

            Init();
        }

        private void Init()
        {
            var cursor = SpriteLoader.Instance.AddSprite("content/cursor");
            Text = new TextBuilder(Width, float.MaxValue);
            int index = 0;
            foreach(var action in Actions)
            {
                Text.StartTableRow(Width, new ColumnConfigs(new IColumnWidth[] {
                    new ColumnFixedWidth(16, true),
                    new ColumnFixedWidth(0, false),
                    new ColumnFixedWidth(16, true),
                }) { Padding = 0 });
                Text.StartTableCell();
                Text.AppendElement(new TextElementCursor(cursor, 16, 16, () => IsSelected(action)));
                Text.EndTableCell();
                Text.StartTableCell();
                action.Text(Text);
                Text.EndTableCell();
                Text.StartTableCell();

                Text.EndTableCell();
                var row = Text.EndTableRow();
                var selectionArea = new MenuAreaText(Text, 0, null);
                selectionArea.Add(row);
                SelectionAreas.Add(index, selectionArea);
                index++;
            }
            Text.EndContainer();
            Text.Finish();
            Dirty = false;
        }

        private bool IsSelected(ActActionNew action)
        {
            return Actions[Selection] == action;
        }

        public void Add(ActActionNew action)
        {
            Actions.Add(action);
            Dirty = true;
        }

        public void AddDefault(ActActionNew action)
        {
            DefaultSelection = Actions.Count;
            Add(action);
        }

        public void Select(int selection)
        {
            if (Actions[selection].Enabled())
                Actions[selection].Action();
        }

        public void Slide(int selection, int slide)
        {
            if (Actions[selection].Enabled())
                Actions[selection].Slide(slide);
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);

            Scroll.Update();

            if (Dirty)
            {
                Init();
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
            HandleSlide(scene);
            Selection = SelectionCount <= 0 ? 0 : (Selection + SelectionCount) % SelectionCount;
            UpdateScroll();
            //Scroll = MathHelper.Clamp(Scroll, Math.Max(0, Selection * LineHeight - Height + LineHeight), Math.Min(Selection * LineHeight, SelectionCount * LineHeight - Height));

        }

        public void HandleSlide(Scene scene)
        {
            bool left = scene.InputState.IsKeyPressed(Keys.A, 15, 5);
            bool right = scene.InputState.IsKeyPressed(Keys.D, 15, 5);

            //Either key is released or both are down at the same time, set counter to 0;
            if (scene.InputState.IsKeyReleased(Keys.A) || scene.InputState.IsKeyReleased(Keys.D) || (scene.InputState.IsKeyDown(Keys.A) && scene.InputState.IsKeyDown(Keys.D)))
            {
                SlideCount = 0;
            }
            else if (left)
            {
                SlideCount--;
                Slide(Selection, SlideCount);
            }
            else if (right)
            {
                SlideCount++;
                Slide(Selection, SlideCount);
            }
        }

        public void UpdateScroll()
        {
            if (SelectionAreas.Empty())
                return;
            var currentTop = SelectionAreas[Selection].GetTop();
            var currentBottom = SelectionAreas[Selection].GetBottom();
            if (currentTop < Scroll)
            {
                Scroll.Set(currentTop, LerpHelper.Linear, 5);
            }
            if (currentBottom > Scroll + Height)
            {
                Scroll.Set(currentBottom - Height, LerpHelper.Linear, 5);
            }
        }

        public override void PreDraw(Scene scene)
        {
            base.PreDraw(scene);
            scene.PushSpriteBatch(blendState: scene.NonPremultiplied, samplerState: SamplerState.PointWrap, projection: Projection);
            scene.GraphicsDevice.Clear(Color.TransparentBlack);
            Text.Draw(new Vector2(0, -Scroll), FontRenderer);
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
