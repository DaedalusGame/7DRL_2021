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
    abstract class MenuNew
    {
        public MenuRoot Root => MenuRoot.Current;

        public double DrawPriority;
        public double UpdatePriority;
        public double InputPriority;
        public bool BlocksInput;
        public Func<bool> AllowInput = () => true;

        public int Ticks;
        public bool ShouldClose;
        public bool Closing;
        public abstract FontRenderer FontRenderer { get; }

        protected bool InputBlocked;
        public abstract IEnumerable<IMenuArea> MenuAreas
        {
            get;
        }

        public void Close()
        {
            ShouldClose = true;
        }

        public virtual void Update(Scene scene)
        {
            Ticks++;
        }

        public virtual void HandleInput(Scene scene)
        {
            //NOOP
        }

        public virtual void PreDraw(Scene scene)
        {
            //NOOP
        }

        public abstract void Draw(Scene scene);
    }

    abstract class MenuRoot
    {
        public static MenuRoot Current { get; private set; }

        List<MenuNew> Menus = new List<MenuNew>();

        protected MenuRoot()
        {
            //TODO: Cleanup previous root
            Current = this; //Global state, cry about it
        }

        public virtual void Update(Scene scene)
        {
            foreach (var menu in Menus.OrderBy(x => x.UpdatePriority).ToList())
                menu.Update(scene);
            Menus.RemoveAll(x => x.ShouldClose);
        }

        public virtual void HandleInput(Scene scene)
        {
            foreach (var menu in Menus.OrderBy(x => x.InputPriority))
            {
                menu.HandleInput(scene);
                if (menu.BlocksInput)
                {
                    break;
                }
            }
        }

        public virtual void Draw(Scene scene)
        {
            foreach (var menu in Menus.OrderBy(x => x.DrawPriority))
                menu.Draw(scene);
        }

        public virtual void PreDraw(Scene scene)
        {
            foreach (var menu in Menus)
                menu.PreDraw(scene);
        }

        public IMenuArea GetMouseOver(int mouseX, int mouseY)
        {
            return GetMouseOver(Menus.SelectMany(x => x.MenuAreas), mouseX, mouseY);
        }

        public IMenuArea GetMouseOver(IEnumerable<IMenuArea> areas, int mouseX, int mouseY)
        {
            areas = areas.OrderByDescending(x => x.Priority);
            return areas.FirstOrDefault(x => x.IsWithin(x.Anchor.Transform(new Vector2(mouseX, mouseY))));
        }

        public void OpenMenu(MenuNew menu)
        {
            Menus.Add(menu);
        }
    }

    abstract class MenuWindow : MenuNew, IMenuAnchor, IMenuArea
    {
        public Vector2 Position;
        public Vector2 Size => new Vector2(Width, Height);
        public int Width { get; set; }
        public int Height { get; set; }

        public float HorizontalAlignment = 0.5f;
        public float VerticalAlignment = 0.5f;

        public LerpFloat OpenLerp = new LerpFloat(0);

        public SpriteReference ContentSprite;
        public SpriteReference LabelSprite;
        protected TextBuilder Label;
        bool LabelDirty = true;

        protected Vector2 WindowSize => MathHelper.Lerp(0.5f, 1.0f, OpenLerp.Value) * Size;
        protected Vector2 WindowOffset => new Vector2(HorizontalAlignment, VerticalAlignment) * WindowSize;
        protected Vector2 WindowPosition => Position - WindowOffset;

        public IMenuAnchor Anchor => this;
        public ITooltipProvider Tooltip { get; set; }
        public double Priority => DrawPriority;

        protected MenuWindow() : base()
        {
            WindowOpen();
        }

        public void WindowOpen()
        {
            OpenLerp.Set(1, LerpHelper.QuadraticOut, 10);
        }

        public void WindowClose()
        {
            OpenLerp.Set(0, LerpHelper.QuadraticIn, 10);
            Closing = true;
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);

            OpenLerp.Update();

            if (Closing && OpenLerp.Value <= 0)
            {
                Close();
            } 

            if (LabelDirty)
            {
                InitLabel();
                LabelDirty = false;
            }

            Label?.Update();
        }

        private void InitLabel()
        {
            //TODO: init label
        }

        protected void DrawWindow(Scene scene)
        {
            var openCoeff = OpenLerp.Value;

            if (openCoeff <= 0)
                return;

            //int offsetX = (int)(Width * HorizontalAlignment);
            //int offsetY = (int)(Height * VerticalAlignment);
            //var rectInterior = new Rectangle((int)Position.X - offsetX, (int)Position.Y - offsetY, Width, Height);
            //float openResize = MathHelper.Lerp(-0.5f, 0.0f, openCoeff);
            //rectInterior.Inflate(rectInterior.Width * openResize, rectInterior.Height * openResize);
            float openResize = MathHelper.Lerp(0.5f, 1.0f, openCoeff);
            int width = (int)(openResize * Width);
            int height = (int)(openResize * Height);
            int offsetX = (int)(width * HorizontalAlignment);
            int offsetY = (int)(height * VerticalAlignment);
            var rectInterior = new Rectangle((int)Position.X - offsetX, (int)Position.Y - offsetY, width, height);

            var minWidth = rectInterior.Width * 2 / 3;

            scene.DrawUI(ContentSprite, rectInterior, Color.White);
            if (Label != null && !Label.IsEmpty() && openCoeff >= 1)
            {
                var labelWidth = (int)MathHelper.Clamp(Label.GetContentWidth() + 8, minWidth, rectInterior.Width);
                Rectangle rectExterior = new Rectangle(rectInterior.X + (rectInterior.Width - labelWidth) / 2, rectInterior.Y - 20, labelWidth, 16);
                scene.DrawUI(LabelSprite, rectExterior, Color.White);
                Label.Draw(new Vector2(rectExterior.X, rectExterior.Y), FontRenderer);
            }
        }

        public Vector2 Transform(Vector2 pos)
        {
            return pos - WindowPosition;
        }

        public bool IsWithin(Vector2 mousePos)
        {
            var rectInterior = new Rectangle(Point.Zero, WindowSize.ToPoint());
            return rectInterior.Contains(mousePos);
        }

        public void AddTooltip(TextBuilder text)
        {
            //NOOP
        }
    }

    class SubMenuHandlerNew<T> : SubMenuHandlerNew where T : MenuNew
    {
        public T Menu;
        protected override MenuNew InternalMenu => Menu;

        public void Open(T menu)
        {
            if (IsOpen)
                throw new Exception();
            Menu = menu;
            Menu.DrawPriority = DrawPriority;
            Menu.UpdatePriority = UpdatePriority;
            Menu.InputPriority = InputPriority;
            Menu.BlocksInput = BlocksInput;
            Menu.AllowInput = AllowInput;
            MenuRoot.Current.OpenMenu(Menu);
        }
    }

    abstract class SubMenuHandlerNew
    {
        protected abstract MenuNew InternalMenu
        {
            get;
        }

        private MenuRoot Root;

        public bool IsExist => InternalMenu != null && !InternalMenu.ShouldClose;
        public bool IsOpen => InternalMenu != null && !InternalMenu.Closing;

        public double DrawPriority;
        public double UpdatePriority;
        public double InputPriority;
        public bool BlocksInput;
        public Func<bool> AllowInput = () => true;

        public void Close()
        {
            InternalMenu?.Close();
        }
    }

    class MenuTextboxNew : MenuWindow
    {
        public override FontRenderer FontRenderer => Scene.FontRenderer;
        public override IEnumerable<IMenuArea> MenuAreas => Content.MenuAreas.Append(this);

        Scene Scene;
        MenuContentText Content;

        public MenuTextboxNew(Scene scene, Action<TextBuilder> textGenerator)
        {
            Scene = scene;
            Content = new MenuContentTextStatic(scene.FontRenderer, this, textGenerator);
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            Content.Update();
        }

        public override void PreDraw(Scene scene)
        {
            base.PreDraw(scene);
            Content.PreDraw(Width, Height);
        }

        public override void Draw(Scene scene)
        {
            DrawWindow(scene);
            if (OpenLerp.Value >= 1)
                Content.Draw(WindowPosition);
        }
    }

    class TitleUINew : MenuRoot
    {
        public SceneTitle Scene;
        public SubMenuHandlerNew<MenuTitleWindowNew> TitleMenu = new SubMenuHandlerNew<MenuTitleWindowNew>() { BlocksInput = true, InputPriority = 10, DrawPriority = 0 };
        public SubMenuHandlerNew<MenuNew> SubMenu = new SubMenuHandlerNew<MenuNew>() { BlocksInput = true, DrawPriority = 10 };
        public SubMenuHandlerNew<TooltipUINew> Tooltip = new SubMenuHandlerNew<TooltipUINew>() { DrawPriority = 9999 };

        public TitleUINew(SceneTitle scene)
        {
            Scene = scene;
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);

            if (Scene.TitleSM.CurrentState == TitleState.Finish && !TitleMenu.IsOpen)
            {
                TitleMenu.Open(new MenuTitleWindowNew(this)
                {
                    Position = new Vector2(Scene.Viewport.Width / 2, Scene.Viewport.Height * 3 / 4),
                    Width = 256,
                    Height = 160,
                    LabelSprite = SpriteLoader.Instance.AddSprite("content/ui_box"),
                    ContentSprite = SpriteLoader.Instance.AddSprite("content/ui_gab"),
                });
                Tooltip.Open(new TooltipUINew(Scene));
            }
        }
    }

    class MenuTitleWindowNew : MenuActNew
    {
        public MenuTitleWindowNew(TitleUINew ui) : base(ui.Scene)
        {
            var formatName = new TextFormatting()
            {
                Bold = true,
            };

            Add(new ActAction((builder) => {
                builder.StartLine(LineAlignment.Center);
                builder.AppendText("New Game", formatName);
                builder.NewLine();
                builder.AppendText("Starts a new game", FormatDescription);
                builder.EndLine();
            }, () =>
            {
                ui.Scene.NewGame();
            }));
            Add(new ActAction((builder) => {
                builder.StartLine(LineAlignment.Center);
                builder.AppendText("Options", formatName);
                builder.NewLine();
                builder.AppendText("Change game settings", FormatDescription);
                builder.EndLine();
            }, () =>
            {
                ui.SubMenu.Open(new MenuOptionsWindowNew(ui.Scene) {
                    Position = new Vector2(ui.Scene.Viewport.Width * 1 / 2, ui.Scene.Viewport.Height * 2 / 3),
                    Width = 256,
                    Height = 320,
                    LabelSprite = SpriteLoader.Instance.AddSprite("content/ui_box"),
                    ContentSprite = SpriteLoader.Instance.AddSprite("content/ui_box"),
                });
            }));
            //TODO
            /*Add(new ActAction((builder) => {
                builder.StartLine(LineAlignment.Center);
                builder.AppendText("Statistics", formatName);
                builder.NewLine();
                builder.AppendText("View statistics", FormatDescription);
                builder.EndLine();
            }, () =>
            {
                ui.SubMenu.Open(new StatMenu(ui.Scene));
            }));*/
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
            Add(new ActAction((builder) => {
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

    class MenuOptionsWindowNew : MenuActNew
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

        public MenuOptionsWindowNew(Scene scene) : base(scene)
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

            AddDefault(new ActAction((builder) => {
                builder.StartLine(LineAlignment.Left);
                builder.AppendText("Cancel", formatName);
                builder.NewLine();
                builder.AppendText("Return to previous menu", FormatDescription);
                builder.EndLine();
            }, () =>
            {
                scene.OptionsFile.Flush();
                WindowClose();
            }));
        }
    }

    class MenuActNew : MenuWindow
    {
        public TextFormatting FormatDescription = new TextFormatting()
        {
            GetParams = (pos) => new DialogParams()
            {
                Scale = Vector2.One,
                Color = Color.DarkGray,
                Border = Color.Black,
            }
        };

        public override FontRenderer FontRenderer => Scene.FontRenderer;
        public override IEnumerable<IMenuArea> MenuAreas => Content.MenuAreas.Append(this);

        Scene Scene;
        MenuContentAct Content;

        public MenuActNew(Scene scene)
        {
            Scene = scene;
            Content = new MenuContentAct(scene.FontRenderer, this);
        }

        public void Add(ActAction action)
        {
            Content.Add(action);
        }

        public void AddDefault(ActAction action)
        {
            Content.AddDefault(action);
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            Content.Update();
        }

        public override void HandleInput(Scene scene)
        {
            base.HandleInput(scene);

            if (InputBlocked || Closing)
                return;

            if (scene.InputState.IsKeyPressed(Keys.Enter))
                Content.Select();
            if (scene.InputState.IsKeyPressed(Keys.Escape))
                Content.SelectDefault();
            if (scene.InputState.IsKeyPressed(Keys.W, 15, 5))
                Content.MoveSelection(-1);
            if (scene.InputState.IsKeyPressed(Keys.S, 15, 5))
                Content.MoveSelection(+1);
            HandleSlideInput(scene);
            Content.UpdateScroll();
        }

        public void HandleSlideInput(Scene scene)
        {
            bool left = scene.InputState.IsKeyPressed(Keys.A, 15, 5);
            bool right = scene.InputState.IsKeyPressed(Keys.D, 15, 5);

            //Either key is released or both are down at the same time, set counter to 0;
            if (scene.InputState.IsKeyReleased(Keys.A) || scene.InputState.IsKeyReleased(Keys.D) || (scene.InputState.IsKeyDown(Keys.A) && scene.InputState.IsKeyDown(Keys.D)))
            {
                Content.SlideReset();
            }
            else if (left)
            {
                Content.Slide(-1);
            }
            else if (right)
            {
                Content.Slide(+1);
            }
        }

        public override void PreDraw(Scene scene)
        {
            base.PreDraw(scene);
            Content.PreDraw(Width, Height);
        }

        public override void Draw(Scene scene)
        {
            DrawWindow(scene);
            if(OpenLerp.Value >= 1)
                Content.Draw(WindowPosition);
        }
    }

    class TooltipUINew : MenuNew
    {
        public override FontRenderer FontRenderer => Scene.FontRenderer;
        public override IEnumerable<IMenuArea> MenuAreas => Enumerable.Empty<IMenuArea>();

        SubMenuHandlerNew<MenuTooltipWindow> TooltipWindow = new SubMenuHandlerNew<MenuTooltipWindow>() { DrawPriority = 9999 };

        Scene Scene;

        public TooltipUINew(Scene scene)
        {
            Scene = scene;
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);

            if (!scene.TooltipCursor.HasTooltip)
            {
                if (TooltipWindow.IsOpen)
                    TooltipWindow.Menu.WindowClose();
            }
            else
            {
                if (!TooltipWindow.IsOpen)
                    TooltipWindow.Open(new MenuTooltipWindow(Scene)
                    {
                        LabelSprite = SpriteLoader.Instance.AddSprite("content/ui_box"),
                        ContentSprite = SpriteLoader.Instance.AddSprite("content/ui_box"),
                    });
                TooltipWindow.Menu.SetTooltip(Scene.TooltipCursor);
            }
        }

        public override void Draw(Scene scene)
        {
            var mousePos = new Vector2(scene.InputState.MouseX, scene.InputState.MouseY);
            var mouseCursor = SpriteLoader.Instance.AddSprite("content/ui_mouse_cursor");
            scene.DrawSprite(mouseCursor, 0, mousePos, SpriteEffects.None, 0);
        }
    }

    class MenuTooltipWindow : MenuWindow
    {
        public override FontRenderer FontRenderer => Scene.FontRenderer;
        public override IEnumerable<IMenuArea> MenuAreas => Enumerable.Empty<IMenuArea>();

        Scene Scene;
        MenuContentTooltip Content;

        public MenuTooltipWindow(Scene scene)
        {
            Scene = scene;
            Content = new MenuContentTooltip(FontRenderer, this);
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            Content.Update();
            Position = new Vector2(scene.InputState.MouseX + 8, scene.InputState.MouseY + 8);
            Width = (int)Content.ContentWidth;
            Height = (int)Content.ContentHeight;
            HorizontalAlignment = VerticalAlignment = 0;
        }

        public override void PreDraw(Scene scene)
        {
            base.PreDraw(scene);
            Content.PreDraw(scene.Viewport.Width, scene.Viewport.Height);
        }

        public override void Draw(Scene scene)
        {
            DrawWindow(scene);
            if (OpenLerp.Value >= 1)
                Content.Draw(WindowPosition);
        }

        public void SetTooltip(ITooltipCursor tooltipCursor)
        {
            Content.SetTooltip(tooltipCursor);
        }
    }

    abstract class MenuContent
    {
        public IMenuAnchor Anchor;
        public RenderTarget2D RenderTarget;

        protected MenuContent(IMenuAnchor anchor)
        {
            Anchor = anchor;
        }

        public abstract void Update();

        public abstract void PreDraw(int width, int height);

        public abstract void Draw(Vector2 position);
    }

    abstract class MenuContentText : MenuContent
    {
        protected FontRenderer FontRenderer;
        protected TextBuilder Text;
        private bool Dirty;

        protected float Width;
        protected float Height;

        public IEnumerable<IMenuArea> MenuAreas => Text?.MenuAreas ?? Enumerable.Empty<IMenuArea>();

        protected MenuContentText(FontRenderer fontRenderer, IMenuAnchor anchor) : base(anchor)
        {
            FontRenderer = fontRenderer;
        }

        protected abstract void Init();

        public override void Update()
        {
            Text?.Update();
        }

        public void SetDirty()
        {
            Dirty = true;
        }

        public override void PreDraw(int width, int height)
        {
            Width = width;
            Height = height;
            if (Dirty)
            {
                Init();
                Dirty = false;
            }

            var scene = FontRenderer.Scene;
            var projection = Matrix.CreateOrthographicOffCenter(0, width, height, 0, 0, -1);
            if (width > 0 && height > 0)
            {
                Util.SetupRenderTarget(scene, ref RenderTarget, width, height);
                scene.SetRenderTarget(RenderTarget);
                scene.PushSpriteBatch(blendState: scene.NonPremultiplied, samplerState: SamplerState.PointWrap, projection: projection);
                scene.GraphicsDevice.Clear(Color.TransparentBlack);
                Text.Draw(new Vector2(0, 0), FontRenderer);
                scene.PopSpriteBatch();
            }
        }

        public override void Draw(Vector2 position)
        {
            if(RenderTarget != null)
                FontRenderer.Scene.SpriteBatch.Draw(RenderTarget, position, Color.White);
        }
    }

    class MenuContentTextStatic : MenuContentText
    {
        Action<TextBuilder> TextGenerator;

        public MenuContentTextStatic(FontRenderer fontRenderer, IMenuAnchor anchor, Action<TextBuilder> textGenerator) : base(fontRenderer, anchor)
        {
            TextGenerator = textGenerator;
        }

        protected override void Init()
        {
            Text = new TextBuilder(Width, float.MaxValue);
            TextGenerator(Text);
            Text.EndContainer();
            Text.Finish();
        }
    }

    class MenuContentAct : MenuContentText
    {
        

        List<ActAction> Actions = new List<ActAction>();
        Dictionary<int, MenuAreaText> SelectionAreas = new Dictionary<int, MenuAreaText>();

        public int Selection;
        public int SelectionCount => Actions.Count;
        public int DefaultSelection = -1;
        int SlideCount;
        public LerpFloat Scroll = new LerpFloat(0);

        public MenuContentAct(FontRenderer fontRenderer, IMenuAnchor anchor) : base(fontRenderer, anchor)
        {
        }

        protected override void Init()
        {
            var cursor = SpriteLoader.Instance.AddSprite("content/cursor");
            Text = new TextBuilder(Width, float.MaxValue);
            int index = 0;
            foreach (var action in Actions)
            {
                Text.StartTableRow(Width, new ColumnConfigs(new IColumnWidth[] {
                    new ColumnFixedWidth(16, true),
                    new ColumnFixedWidth(0, false),
                    new ColumnFixedWidth(16, true),
                })
                { Padding = 0 });
                Text.StartTableCell();
                Text.AppendElement(new TextElementCursor(cursor, 16, 16, () => IsSelected(action)));
                Text.EndTableCell();
                Text.StartTableCell();
                Text.StartMenuArea(0.1, new TooltipProviderFunction((text =>
                {
                    text.StartLine(LineAlignment.Left);
                    text.AppendText("Test and stuff");
                    text.EndLine();
                })), Anchor);
                action.Text(Text);
                Text.EndMenuArea();
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
        }

        private bool IsSelected(ActAction action)
        {
            return Actions[Selection] == action;
        }

        public void Add(ActAction action)
        {
            Actions.Add(action);
            SetDirty();
        }

        public void AddDefault(ActAction action)
        {
            DefaultSelection = Actions.Count;
            Add(action);
        }

        public void MoveSelection(int move)
        {
            Selection += move;
            Selection = SelectionCount <= 0 ? 0 : (Selection + SelectionCount) % SelectionCount;
        }

        public void Select()
        {
            if(Selection < SelectionCount)
                Select(Selection);
        }

        public void SelectDefault()
        {
            if(DefaultSelection >= 0)
                Select(DefaultSelection);
        }

        public void Select(int selection)
        {
            if (Actions[selection].Enabled())
                Actions[selection].Action();
        }

        public void SlideReset()
        {
            SlideCount = 0;
        }

        public void Slide(int slide)
        {
            SlideCount += slide;
            if (Actions[Selection].Enabled())
                Actions[Selection].Slide(SlideCount);
        }

        public override void Update()
        {
            base.Update();

            Scroll.Update();
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
    }

    class MenuContentTooltip : MenuContentText
    {
        ITooltipCursor TooltipCursor;

        public float ContentWidth => Text?.GetContentWidth() ?? 0;
        public float ContentHeight => Text?.GetContentHeight() ?? 0;

        public MenuContentTooltip(FontRenderer fontRenderer, IMenuAnchor anchor) : base(fontRenderer, anchor)
        {
        }

        public void SetTooltip(ITooltipCursor tooltipCursor)
        {
            TooltipCursor = tooltipCursor;
            SetDirty();
        }

        protected override void Init()
        {
            if(!TooltipCursor.Invalid)
            {
                Text = new TextBuilder(Width, float.MaxValue);
                TooltipCursor.GenerateTooltip(Text);
                Text.EndContainer();
                Text.Finish();
            }
        }
    }
}
