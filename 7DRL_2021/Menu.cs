using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    class LabelledUI
    {
        protected SpriteReference ContentSprite;
        protected SpriteReference LabelSprite;
        protected TextBuilder Label;
        protected Func<Point> Size;

        public LabelledUI(SpriteReference labelSprite, SpriteReference contentSprite, Action<TextBuilder> label, Func<Point> size)
        {
            ContentSprite = contentSprite;
            LabelSprite = labelSprite;
            Label = new TextBuilder(float.PositiveInfinity, 16);
            Label.StartLine(LineAlignment.Center);
            label(Label);
            Label.EndLine();
            Label.EndContainer();
            Label.Finish();
            Size = size;
        }

        public virtual void Update()
        {
            Label.Update();
        }

        public virtual void Draw(FontRenderer fontRenderer, int x, int y, float openCoeff)
        {
            if (openCoeff <= 0)
                return;
            
            var rectInterior = new Rectangle(new Point(x, y), Size());
            float openResize = MathHelper.Lerp(-0.5f, 0.0f, openCoeff);
            rectInterior.Inflate(rectInterior.Width * openResize, rectInterior.Height * openResize);

            var minWidth = rectInterior.Width * 2 / 3;

            fontRenderer.Scene.DrawUI(ContentSprite, rectInterior, Color.White);
            if (!Label.IsEmpty() && openCoeff >= 1)
            {
                var width = (int)MathHelper.Clamp(Label.GetContentWidth() + 8, minWidth, rectInterior.Width);
                Rectangle rectExterior = new Rectangle(rectInterior.X + (rectInterior.Width - width) / 2, rectInterior.Y - 20, width, 16);
                fontRenderer.Scene.DrawUI(LabelSprite, rectExterior, Color.White);
                Label.Draw(new Vector2(rectExterior.X, rectExterior.Y), fontRenderer);
            }
        }
    }

    class LabelledUIText : LabelledUI
    {
        TextBuilder Content;

        public LabelledUIText(SpriteReference labelSprite, SpriteReference contentSprite, Action<TextBuilder> label, Action<TextBuilder> content) : base(labelSprite, contentSprite, label, null)
        {
            Content = new TextBuilder(float.PositiveInfinity, 16);
            label(Content);
            Content.EndContainer();
            Content.Finish();
            Size = GetSize;
        }

        private Point GetSize()
        {
            return new Point((int)Content.GetContentWidth(), (int)Content.GetContentHeight());
        }

        public override void Update()
        {
            base.Update();
            Content.Update();
        }

        public override void Draw(FontRenderer fontRenderer, int x, int y, float openCoeff)
        {
            base.Draw(fontRenderer, x, y, openCoeff);
            Content.Draw(new Vector2(x, y), fontRenderer);
        }
    }

    abstract class Menu
    {
        public int Ticks;
        public virtual bool ShouldClose
        {
            get;
            set;
        }
        public RenderTarget2D RenderTarget;
        public virtual int Width
        {
            get;
            set;
        }
        public virtual int Height
        {
            get;
            set;
        }
        public Matrix Projection;
        public abstract FontRenderer FontRenderer { get; }

        protected bool InputBlocked;
        protected virtual IEnumerable<SubMenuHandler> SubMenuHandlers => Enumerable.Empty<SubMenuHandler>();
        protected abstract IEnumerable<IMenuArea> MenuAreas
        {
            get;
        }

        public IMenuArea GetMouseOver(int mouseX, int mouseY)
        {
            var areas = GetAllMenus().SelectMany(x => x.MenuAreas).OrderBy(x => x.Priority);
            return areas.FirstOrDefault(x => x.IsWithin(mouseX, mouseY));
        }

        public void Close()
        {
            ShouldClose = true;
        }

        public virtual void Update(Scene scene)
        {
            Ticks++;
            foreach (var handler in SubMenuHandlers.OrderBy(x => x.UpdatePriority))
                handler.Update(scene);
        }

        public virtual void HandleInput(Scene scene)
        {
            InputBlocked = false;
            foreach (var handler in SubMenuHandlers.OrderBy(x => x.InputPriority))
            {
                handler.HandleInput(scene);
                if (handler.IsOpen && handler.BlocksInput)
                {
                    InputBlocked = true;
                    break;
                }
            }
        }

        public IEnumerable<Menu> GetAllMenus()
        {
            return new[] { this }.Concat(SubMenuHandlers.Where(x => x.IsOpen).SelectMany(x => x.InternalMenu.GetAllMenus())).Where(x => x != null);
        }

        /*protected void DrawLabelledUI(Scene scene, SpriteReference contentSprite, SpriteReference labelSprite, Rectangle rectInterior, string label)
        {
            var minWidth = rectInterior.Width * 2 / 3;

            scene.DrawUI(contentSprite, rectInterior, Color.White);
            if (!string.IsNullOrWhiteSpace(label))
            {
                var parameters = new TextParameters().SetColor(Color.White, Color.Black).SetBold(true).SetConstraints(rectInterior.Width, 16);
                var width = MathHelper.Clamp(FontUtil.GetStringWidth(label, parameters) + 8, minWidth, rectInterior.Width);
                Rectangle rectExterior = new Rectangle(rectInterior.X + (rectInterior.Width - width) / 2, rectInterior.Y - 20, width, 16);
                scene.DrawUI(labelSprite, rectExterior, Color.White);
                scene.DrawText(label, new Vector2(rectInterior.X, rectExterior.Y), Alignment.Center, new TextParameters().SetColor(Color.White, Color.Black).SetBold(true).SetConstraints(rectInterior.Width, rectExterior.Height));
            }
        }*/

        public virtual void PreDraw(Scene scene)
        {
            Projection = Matrix.CreateOrthographicOffCenter(0, Width, Height, 0, 0, -1);
            if (Width > 0 && Height > 0)
            {
                Util.SetupRenderTarget(scene, ref RenderTarget, Width, Height);
                scene.SetRenderTarget(RenderTarget);
            }
            foreach (var handler in SubMenuHandlers)
                handler.PreDraw(scene);
        }

        public abstract void Draw(Scene scene);
    }

    interface IMenuArea
    {
        double Priority
        {
            get;
        }

        void AddTooltip(TextBuilder text);
        
        bool IsWithin(int x, int y);
    }

    class MenuAreaText : IMenuArea, ITextElementHandler
    {
        TextBuilder Text;
        List<ITextElement> Elements = new List<ITextElement>();
        public double Priority
        {
            get;
            set;
        }
        ITooltipProvider TooltipProvider;

        public MenuAreaText(TextBuilder text, double priority, ITooltipProvider tooltipProvider)
        {
            Text = text;
            Priority = priority;
            TooltipProvider = tooltipProvider;
        }

        public void Add(ITextElement element)
        {
            Elements.Add(element);
        }

        private IEnumerable<ITextElement> GetElements()
        {
            return Elements;
        }

        public bool IsWithin(int x, int y)
        {
            var textPos = Text.TextPosition;
            foreach(var element in GetElements())
            {
                var transformFinal = textPos.Transform * Matrix.CreateTranslation(textPos.Position.X, textPos.Position.Y, 0) * element.Position.Transform;
                var localPos = Vector2.Transform(new Vector2(x,y), Matrix.Invert(transformFinal));
                if(localPos.X >= 0 && localPos.Y >= 0 && localPos.X < element.Width && localPos.Y < element.Height)
                    return true;
            }
            return false;
        }

        public void AddTooltip(TextBuilder text)
        {
            TooltipProvider?.AddTooltip(text);
        }
    }

    class MenuAreaBasic : IMenuArea
    {
        Menu Menu;
        Func<Vector2> Position;

        public MenuAreaBasic(Menu menu, Func<Vector2> position, double priority)
        {
            Menu = menu;
            Position = position;
            Priority = priority;
        }

        public double Priority
        {
            get;
            set;
        }

        public void AddTooltip(TextBuilder text)
        {
            //NOOP
        }

        public bool IsWithin(int x, int y)
        {
            return new Rectangle((int)Position().X - Menu.Width / 2, (int)Position().Y - Menu.Height / 2, Menu.Width, Menu.Height).Contains(x, y);
        }
    }

    class SubMenuHandler<T> : SubMenuHandler where T : Menu
    {
        public T Menu;
        public override Menu InternalMenu => Menu;

        public void Open(T menu)
        {
            Menu = menu;
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            if (Menu != null && Menu.ShouldClose)
                Menu = null;
        }
    }

    abstract class SubMenuHandler
    {
        public abstract Menu InternalMenu
        {
            get;
        }

        public bool IsOpen => InternalMenu != null;
        public double UpdatePriority;
        public double InputPriority;
        public bool BlocksInput;
        public Func<bool> AllowInput = () => true;

        public void Close()
        {
            InternalMenu?.Close();
        }

        public void HandleInput(Scene scene)
        {
            if (AllowInput())
                InternalMenu?.HandleInput(scene);
        }

        public virtual void Update(Scene scene)
        {
            InternalMenu?.Update(scene);
        }

        public void PreDraw(Scene scene)
        {
            InternalMenu?.PreDraw(scene);
        }

        public void Draw(Scene scene)
        {
            InternalMenu?.Draw(scene);
        }
    }
}
