using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    struct TextPosition
    {
        public Vector2 Position;
        public Matrix Transform;

        public TextPosition(Vector2 position, Matrix transform)
        {
            Position = position;
            Transform = transform;
        }
    }

    class TextFormatting
    {
        public bool Bold;
        public bool Italic;
        public GetDialogParams GetParams = DefaultParams;

        public int GetCharWidth(char chr)
        {
            return FontUtil.GetCharWidth(chr) + (Bold ? 1 : 0);
        }

        public static DialogParams DefaultParams(TextCursorPosition position) => new DialogParams()
        {
            Color = Color.White,
            Border = Color.Black,
            Offset = Vector2.Zero,
            Scale = Vector2.One,
            Angle = 0,
        };
    }

    abstract class DialogFormatting
    {
        List<ITextDialogable> AllElements;
        protected List<ITextDialogable> Elements = new List<ITextDialogable>();

        public void Setup(List<ITextDialogable> allElements)
        {
            AllElements = allElements;
            SetupDialog();
        }

        public void Show()
        {
            foreach(var element in Elements)
            {
                element.Dialog.Show();
            }
        }

        public void Hide()
        {
            foreach (var element in Elements)
            {
                element.Dialog.Hide();
            }
        }

        public void Highlight()
        {
            foreach (var element in Elements)
            {
                element.Dialog.Highlight();
            }
        }

        protected abstract void SetupDialog();

        public void Add(ITextDialogable element)
        {
            Elements.Add(element);
        }

        protected ITextDialogable GetNext(ITextDialogable element)
        {
            int index = AllElements.IndexOf(element);
            return AllElements.ElementAtOrDefault(index + 1);
        }

        protected ITextDialogable GetPrevious(ITextDialogable element)
        {
            int index = AllElements.IndexOf(element);
            return AllElements.ElementAtOrDefault(index - 1);
        }
    }

    class DialogFormattingStandard : DialogFormatting
    {
        int TextSpeed;

        public DialogFormattingStandard(int textSpeed)
        {
            TextSpeed = textSpeed;
        }

        protected override void SetupDialog()
        {
            foreach(var element in Elements)
            {
                element.Dialog.ShowPrevious = GetPrevious(element)?.Dialog;
                element.Dialog.TransformShow = Transform;
                element.Dialog.ShowSlider = new DialogSliderCharacter(element is TextElementSpace ? 1 : TextSpeed, 20, element.Characters);
            }
        }

        private static DialogParams Transform(DialogParams param, TextCursorPosition pos, float slide)
        {
            if (slide <= 0)
            {
                param.Color = Color.Transparent;
                param.Border = Color.Transparent;
            }
            else
            {
                float angle = Util.RandomNoise(pos.GlobalCharacter) * MathHelper.TwoPi;
                param.Offset = Vector2.Lerp(Util.AngleToVector(angle) * 20, Vector2.Zero, MathHelper.Clamp(slide * 2,0,1));
                param.Scale = Vector2.Lerp(new Vector2(3, 3), Vector2.One, MathHelper.Clamp(slide * 2, 0, 1));
                param.Border = Color.Lerp(Color.Orange.RotateHue(pos.GlobalCharacter / 20f), Color.Black, slide);
            }
            return param;
        }
    }

    class DialogFormattingInstant : DialogFormatting
    {
        protected override void SetupDialog()
        {
            foreach (var element in Elements)
            {
                ITextDialogable previous = GetPrevious(element);
                element.Dialog.ShowPrevious = previous?.Dialog;
                element.Dialog.TransformShow = Transform;
                element.Dialog.ShowSlider = new DialogSliderTime(20, 0);
            }
        }

        private static DialogParams Transform(DialogParams param, TextCursorPosition pos, float slide)
        {
            if (slide <= 0)
            {
                param.Color = Color.Transparent;
                param.Border = Color.Transparent;
            }
            param.Scale = Vector2.Lerp(new Vector2(3, 3), Vector2.One, slide);
            return param;
        }
    }

    class DialogFormattingIdentity : DialogFormatting
    {
        protected override void SetupDialog()
        {
            //NOOP
        }
    }

    delegate DialogParams GetDialogParams(TextCursorPosition position);

    delegate DialogParams TransformDialogParams(DialogParams param, TextCursorPosition position, float slide);

    struct DialogParams
    {
        public Color Color;
        public Color Border;
        public Vector2 Offset;
        public Vector2 Scale;
        public float Angle;

        public DialogParams(DialogParams param)
        {
            Color = param.Color;
            Border = param.Border;
            Offset = param.Offset;
            Scale = param.Scale;
            Angle = param.Angle;
        }

        public void Decompose(out Color color, out Color border, out Vector2 offset, out Vector2 scale, out float angle)
        {
            color = Color;
            border = Border;
            offset = Offset;
            scale = Scale;
            angle = Angle;
        }
    }

    class TextDialog
    {
        public bool ShowFlag;
        public bool HideFlag;
        public bool HighlightFlag;

        public bool ShowDone => ShowSlider.Done && (ShowPrevious?.ShowDone ?? true);
        public bool HideDone => HideSlider.Done && (HidePrevious?.HideDone ?? true);
        public bool HighlightDone => HighlightSlider.Done && (HighlightPrevious?.HighlightDone ?? true);
        public TextDialog ShowPrevious;
        public TextDialog HidePrevious;
        public TextDialog HighlightPrevious;
        public IDialogSlider ShowSlider = DialogSliderConstant.Full;
        public IDialogSlider HideSlider = DialogSliderConstant.Empty;
        public IDialogSlider HighlightSlider = DialogSliderConstant.Empty;

        public TransformDialogParams TransformShow = TransformIdentity;
        public TransformDialogParams TransformHide = TransformIdentity;
        public TransformDialogParams TransformHighlight = TransformIdentity;

        public void Show()
        {
            ShowSlider.Reset();
            ShowFlag = true;
        }

        public void Highlight()
        {
            HighlightSlider.Reset();
            HighlightFlag = true;
        }

        public void Hide()
        {
            HideSlider.Reset();
            HideFlag = true;
        }

        public void Reset()
        {
            ShowSlider.Reset();
            HideSlider.Reset();
            HighlightSlider.Reset();
            ShowFlag = false;
            HideFlag = false;
            HighlightFlag = false;
        }

        public void Update()
        {
            if (ShowFlag && (ShowPrevious?.ShowDone ?? true))
                ShowSlider.Update();
            if (HideFlag && (HidePrevious?.HideDone ?? true))
                HideSlider.Update();
            if (HighlightFlag && (HighlightPrevious?.HighlightDone ?? true))
                HighlightSlider.Update();
        }

        public DialogParams Transform(DialogParams par, TextCursorPosition pos)
        {
            par = TransformShow(par, pos, ShowSlider.GetSlide(pos));
            par = TransformHighlight(par, pos, HighlightSlider.GetSlide(pos));
            par = TransformHide(par, pos, HideSlider.GetSlide(pos));
            return par;
        }

        public static DialogParams TransformIdentity(DialogParams param, TextCursorPosition pos, float slide) => param;
    }

    interface IDialogSlider
    {
        bool Done { get; }
        
        void Update();

        void Reset();

        float GetSlide(TextCursorPosition pos);
    }

    class DialogSliderConstant : IDialogSlider
    {
        public static readonly DialogSliderConstant Empty = new DialogSliderConstant(0);
        public static readonly DialogSliderConstant Full = new DialogSliderConstant(1);

        float Slide;
        public bool Done => true;

        private DialogSliderConstant(float slide)
        {
            Slide = slide;
        }

        public void Update()
        {
            //NOOP
        }

        public void Reset()
        {
            //NOOP
        }

        public float GetSlide(TextCursorPosition pos)
        {
            return Slide;
        }
    }

    class DialogSliderCharacter : IDialogSlider
    {
        Slider Interval;
        float Lag;
        int CharacterCurrent;
        int CharacterCount;

        public bool Done => CharacterCurrent >= CharacterCount;

        public DialogSliderCharacter(int interval, int lag, int characters)
        {
            Interval = new Slider(interval);
            Lag = lag;
            CharacterCount = characters;
        }

        public void Update()
        {
            Interval += 1;
            if(Interval.Done)
            {
                CharacterCurrent++;
                Interval.Time = 0;
            }
        }

        public void Reset()
        {
            Interval.Time = 0;
            CharacterCurrent = 0;
        }

        public float GetSlide(TextCursorPosition pos)
        {
            float n = (CharacterCurrent + Interval.Slide) * Interval.EndTime;
            return MathHelper.Clamp((n - pos.LocalCharacter * Interval.EndTime) / Lag, 0, 1);
        }
    }

    class DialogSliderTime : IDialogSlider
    {
        Slider SliderLag;
        Slider Slider;

        public bool Done => Slider.Done;

        public DialogSliderTime(int time) : this(time, time)
        {
        }

        public DialogSliderTime(int timeLag, int time)
        {
            SliderLag = new Slider(timeLag);
            Slider = new Slider(time);
        }        

        public void Update()
        {
            SliderLag += 1;
            Slider += 1;
        }

        public void Reset()
        {
            SliderLag.Time = 0;
            Slider.Time = 0;
        }

        public float GetSlide(TextCursorPosition pos)
        {
            return SliderLag.Slide;
        }
    }

    struct TextCursorPosition
    {
        public int GlobalCharacter;
        public int LocalCharacter;
        public int Element;
        public Vector2 Position;
       
        public TextCursorPosition(int globalCharacter, int localCharacter, int element, Vector2 position)
        {
            GlobalCharacter = globalCharacter;
            LocalCharacter = localCharacter;
            Element = element;
            Position = position;
        }

        public void AddCharacters(int n)
        {
            GlobalCharacter += n;
            LocalCharacter += n;
        }

        public void IncrementElement()
        {
            LocalCharacter = 0;
            Element++;
        }

        public static TextCursorPosition operator +(TextCursorPosition pos, Vector2 offset)
        {
            return new TextCursorPosition(pos.GlobalCharacter, pos.LocalCharacter, pos.Element, pos.Position + offset);
        }
    }

    struct ElementPosition
    {
        public Matrix Transform;

        public ElementPosition(Matrix transform)
        {
            Transform = transform;
        }
    }

    interface ITextElementHandler
    {
        void Add(ITextElement element);
    }

    interface ITextElement
    {
        float Width { get; }
        float Height { get; }

        ElementPosition Position { get; }

        /// <summary>
        /// True iff this element is considered an atomic unit.
        /// </summary>
        bool IsUnit { get; }

        void Setup(ITextContainer parent, Vector2 position);

        void Draw(ITextContainer parent, Matrix baseTransform, FontRenderer renderer, TextCursorPosition cursorPos);

        void IncrementPosition(ref TextCursorPosition cursorPos);
    }

    interface ITextContainer : ITextElement
    {
        List<ITextElement> Contents { get; }

        void Add(ITextElement element);
    }

    interface ITextDialogable : ITextElement
    {
        TextDialog Dialog { get; }

        DialogFormatting DialogFormat { get; }

        int Characters { get; }
    }

    interface ITextAlignable : ITextElement
    {
        LineAlignment Alignment { get; }
    }

    interface ITextSplitable
    {
        IEnumerable<ITextElement> Split(float width);
    }

    class TextElementWord : ITextElement, ITextSplitable, ITextDialogable
    {
        public float Width => GetWidth();
        public float Height => 16; //TODO: line height formatting

        public ElementPosition Position { get; private set; }
        public TextDialog Dialog { get; private set; } = new TextDialog();

        public bool IsUnit => Text.Length == 1;

        public int Characters => Text.Length;

        public string Text;
        public TextFormatting Format { get; private set; }
        public DialogFormatting DialogFormat { get; private set; }

        public TextElementWord(string text, TextFormatting format, DialogFormatting dialogFormat)
        {
            Text = text;
            Format = format;
            DialogFormat = dialogFormat;
        }

        private float GetWidth()
        {
            if (Text.Length <= 0)
                return 0;
            int seperator = (Text.Length) * 1;
            return Text.Sum(Format.GetCharWidth) + seperator;
        }

        public IEnumerable<ITextElement> Split(float width)
        {
            StringBuilder a = new StringBuilder();
            StringBuilder b = new StringBuilder();
            float aWidth = 0;

            foreach(char element in Text)
            {
                float elementWidth = Format.GetCharWidth(element);
                if (aWidth + elementWidth + 1 > width)
                {
                    b.Append(element);
                }
                else
                {
                    a.Append(element);
                }
                aWidth += elementWidth + 1;
            }

            string aString = a.ToString().Trim();
            string bString = b.ToString().Trim();
            yield return new TextElementWord(aString, Format, DialogFormat);
            if (!string.IsNullOrEmpty(bString))
            {
                yield return new TextElementWord(bString, Format, DialogFormat);
            }
        }

        public override string ToString()
        {
            return Text;
        }

        public void Setup(ITextContainer parent, Vector2 position)
        {
            Position = new ElementPosition(parent.Position.Transform * Matrix.CreateTranslation(position.X, position.Y, 0));
        }

        public void Draw(ITextContainer parent, Matrix baseTransform, FontRenderer renderer, TextCursorPosition cursorPos)
        {
            renderer.DrawChars(Text, Width, Height, Format, Dialog, Position.Transform * baseTransform, cursorPos);
        }

        public void IncrementPosition(ref TextCursorPosition cursorPos)
        {
            cursorPos.AddCharacters(Text.Length);
            cursorPos.IncrementElement();
        }
    }

    class TextElementSpace : ITextElement, ITextDialogable
    {
        public float Width
        {
            get;
            set;
        }
        public float Height => 0;

        public ElementPosition Position { get; private set; }
        public TextDialog Dialog { get; private set; } = new TextDialog();
        public int Characters => 1;

        public bool IsUnit => true;

        public TextFormatting Format { get; private set; }
        public DialogFormatting DialogFormat { get; private set; }

        public TextElementSpace(float width, TextFormatting format, DialogFormatting dialogFormat)
        {
            Width = width;
            Format = format;
            DialogFormat = dialogFormat;
        }

        public override string ToString()
        {
            return " ";
        }

        public void Setup(ITextContainer parent, Vector2 position)
        {
            Position = parent.Position;
        }

        public void Draw(ITextContainer parent, Matrix baseTransform, FontRenderer renderer, TextCursorPosition cursorPos)
        {
            var charOffset = Vector2.Zero;
            var color = Color.White;
            var border = Color.Black; //TODO: formatting
            //renderer.DrawCharLine(charOffset + new Vector2(0, 15), (int)Width, color, border);
            //renderer.DrawCharLine(charOffset + new Vector2(0, 8), (int)Width, color, border);
        }

        public void IncrementPosition(ref TextCursorPosition cursorPos)
        {
            cursorPos.AddCharacters(1);
            cursorPos.IncrementElement();
        }
    }

    class TextElementSymbol : ITextElement, ITextDialogable
    {
        public float Width => 16;
        public float Height => 16;

        public ElementPosition Position { get; private set; }
        public TextDialog Dialog { get; private set; } = new TextDialog();
        public int Characters => 1;

        public bool IsUnit => true;

        public Symbol Symbol;
        public TextFormatting Format { get; private set; }
        public DialogFormatting DialogFormat { get; private set; }

        public TextElementSymbol(Symbol symbol, TextFormatting format, DialogFormatting dialogFormat)
        {
            Symbol = symbol;
            Format = format;
            DialogFormat = dialogFormat;
        }

        public void Setup(ITextContainer parent, Vector2 position)
        {
            Position = new ElementPosition(parent.Position.Transform * Matrix.CreateTranslation(position.X, position.Y, 0));
        }

        public void Draw(ITextContainer parent, Matrix baseTransform, FontRenderer renderer, TextCursorPosition cursorPos)
        {
            var par = Format.GetParams(cursorPos);
            par = Dialog.Transform(par, cursorPos);
            Color color = par.Color;
            Vector2 offset = par.Offset;
            Vector2 scale = par.Scale;
            float angle = par.Angle;
            renderer.Scene.PushSpriteBatch(transform: Matrix.CreateTranslation(-8, -8, 0) * Matrix.CreateScale(scale.X, scale.Y, 0) * Matrix.CreateRotationZ(angle) * Matrix.CreateTranslation(8, 8, 0) * Position.Transform * baseTransform);
            if(color.A > 0)
                Symbol.DrawIcon(renderer.Scene, offset, par, 0);
            renderer.Scene.PopSpriteBatch();
        }

        public void IncrementPosition(ref TextCursorPosition cursorPos)
        {
            cursorPos.AddCharacters(1);
            cursorPos.IncrementElement();
        }
    }

    class TextElementNoBreak : ITextElement, ITextContainer
    {
        public float Width => Contents.Sum(x => x.Width);
        public float Height => Contents.Any() ? Math.Max(MinHeight, Contents.Max(x => x.Height)) : MinHeight;
        public float MinHeight = 8; //TODO: Line height

        public ElementPosition Position { get; private set; }

        public bool IsUnit => true;

        public List<ITextElement> Contents { get; } = new List<ITextElement>();

        public void Add(ITextElement element)
        {
            Contents.Add(element);
        }


        public void Setup(ITextContainer parent, Vector2 position)
        {
            float offset = 0;

            Position = new ElementPosition(parent.Position.Transform * Matrix.CreateTranslation(position.X, position.Y, 0));
            foreach (var element in Contents)
            {
                element.Setup(this, new Vector2(offset, 0));
                offset += element.Width;
            }
        }

        public void Draw(ITextContainer parent, Matrix baseTransform, FontRenderer renderer, TextCursorPosition cursorPos)
        {
            float offset = 0;

            renderer.Scene.PushSpriteBatch(transform: Position.Transform * baseTransform);
            renderer.DrawAreaDebug(Vector2.Zero, new Vector2(Width, Height));
            foreach (var element in Contents)
            {
                element.Draw(this, baseTransform, renderer, cursorPos + new Vector2(offset, 0));
                element.IncrementPosition(ref cursorPos);
                offset += element.Width;
            }
            renderer.Scene.PopSpriteBatch();
        }

        public void IncrementPosition(ref TextCursorPosition cursorPos)
        {
            foreach(var element in Contents)
            {
                element.IncrementPosition(ref cursorPos);
            }
        }

        public override string ToString()
        {
            return $"<nobr>{string.Join("", Contents)}</nobr>";
        }
    }

    enum LineAlignment
    {
        Left,
        Center,
        Right,
    }

    class TextElementLine : ITextElement, ITextContainer, ITextAlignable, ITextSplitable
    {
        public float Width => Contents.Sum(x => x.Width);
        public float Height => Contents.Any() ? Math.Max(MinHeight,Contents.Max(x => x.Height)) : MinHeight;
        public float MinHeight = 8; //TODO: Line height
        public LineAlignment Alignment { get; set; }
        public ElementPosition Position { get; private set; }

        public bool IsUnit => false;

        public List<ITextElement> Contents { get; } = new List<ITextElement>();

        public bool Splitable => true;

        public TextElementLine(LineAlignment alignment)
        {
            Alignment = alignment;
        }

        public void Add(ITextElement element)
        {
            Contents.Add(element);
        }

        public IEnumerable<ITextElement> Split(float width)
        {
            LinkedList<ITextElement> backlog = new LinkedList<ITextElement>(Contents);
            TextElementLine a = new TextElementLine(Alignment);
            TextElementLine b = new TextElementLine(Alignment);
            float aWidth = 0;
            while(backlog.Any())
            {
                var element = backlog.PopFirst();
                float elementWidth = element.Width;
                
                if (b.Contents.Any())
                {
                    b.Add(element);
                }
                else if (aWidth + elementWidth > width)
                {
                    if(elementWidth > width && aWidth < width) //Special case, word would never fit!
                    {
                        if(element is ITextSplitable splitable && !element.IsUnit) //Only non-units can be split
                        {
                            var splitResult = splitable.Split(width - aWidth).ToList();
                            if (splitResult.First().Width <= 0)
                                aWidth = width;
                            foreach (var split in splitResult.Reverse<ITextElement>())
                            {
                                if (split.Width > 0)
                                    backlog.AddFirst(split);
                            }
                        }
                        continue;
                    }
                    else
                    {
                        b.Add(element);
                    }
                }
                else
                {
                    a.Add(element);    
                }
                aWidth += elementWidth;
            }
            a.Trim();
            b.Trim();
            yield return a;
            if (b.Contents.Any())
                yield return b;
        }

        public void Trim()
        {
            while (Contents.Any() && Contents.First() is TextElementSpace)
                Contents.RemoveAt(0);
            while (Contents.Any() && Contents.Last() is TextElementSpace)
                Contents.RemoveAt(Contents.Count-1);
        }

        public override string ToString()
        {
            return string.Join("", Contents);
        }

        public void Setup(ITextContainer parent, Vector2 position)
        {
            float offset = 0;
            float alignOffset = GetAlignmentOffset(parent);
            Position = new ElementPosition(parent.Position.Transform * Matrix.CreateTranslation(position.X + alignOffset, position.Y, 0));
            foreach (var element in Contents)
            {
                element.Setup(this, new Vector2(offset, 0));
                offset += element.Width;
            }
        }

        public void Draw(ITextContainer parent, Matrix baseTransform, FontRenderer renderer, TextCursorPosition cursorPos)
        {
            float offset = 0;
            float alignOffset = GetAlignmentOffset(parent);
            renderer.Scene.PushSpriteBatch(transform: Position.Transform * baseTransform);
            renderer.DrawAreaDebug(Vector2.Zero, new Vector2(Width, Height));
            foreach (var element in Contents)
            {
                element.Draw(this, baseTransform, renderer, cursorPos + new Vector2(offset + alignOffset, 0));
                element.IncrementPosition(ref cursorPos);
                offset += element.Width;
            }
            renderer.Scene.PopSpriteBatch();
        }

        public void IncrementPosition(ref TextCursorPosition cursorPos)
        {
            foreach(var element in Contents)
            {
                element.IncrementPosition(ref cursorPos);
            }
        }

        private float GetAlignmentOffset(ITextContainer parent)
        {
            float xOffset = 0;
            float width = parent.Width;
            if (width >= float.PositiveInfinity)
                width = 0;
            switch (Alignment)
            {
                case (LineAlignment.Left):
                    xOffset = 0;
                    break;
                case (LineAlignment.Center):
                    xOffset = (width - Width) / 2;
                    break;
                case (LineAlignment.Right):
                    xOffset = width - Width;
                    break;
            }

            return xOffset;
        }
    }

    /// <summary>
    /// Vertical flow container
    /// </summary>
    class TextElementContainer : ITextElement, ITextContainer
    {
        public float Width { get; set; } = float.PositiveInfinity;
        public float Height { get; set; } = float.PositiveInfinity;

        public bool IsUnit => false;

        public TextElementContainer(float width, float height)
        {
            Width = width;
            Height = height;
        }

        public List<ITextElement> Contents { get; } = new List<ITextElement>();
        public List<ITextElement> Overflow { get; } = new List<ITextElement>();

        public ElementPosition Position { get; set; }

        public void Add(ITextElement toAdd)
        {
            var backlog = new LinkedList<ITextElement>();
            var height = 0f;
            backlog.AddFirst(toAdd);
            while(backlog.Any())
            {
                var element = backlog.PopFirst();
                if(Contents.Sum(x => x.Height) + element.Height > Height)
                {
                    Overflow.Add(element);
                }
                else if(element.Width > Width)
                {
                    if(element is ITextSplitable splitable)
                    {
                        var splitResult = splitable.Split(Width).ToList();
                        if (splitResult.First().Width <= 0)
                            continue;
                        foreach (var result in splitResult.Reverse<ITextElement>())
                        {
                            if (result.Width > 0)
                                backlog.AddFirst(result);
                        }
                    }
                }
                else
                {
                    Contents.Add(element);
                    height += element.Height;
                }
            }
        }

        public override string ToString()
        {
            return string.Join("", Contents);
        }

        public void Draw(ITextContainer parent, Matrix baseTransform, FontRenderer renderer, TextCursorPosition cursorPos)
        {
            renderer.Scene.PushSpriteBatch(transform: Position.Transform * baseTransform);
            renderer.DrawAreaDebug(Vector2.Zero, new Vector2(Width, Height), Color.Black);
            float yOffset = 0;
            foreach (var element in Contents)
            {
                element.Draw(this, baseTransform, renderer, cursorPos + new Vector2(0, yOffset));
                element.IncrementPosition(ref cursorPos);
                yOffset += element.Height;
            }
            renderer.Scene.PopSpriteBatch();
        }

        public void IncrementPosition(ref TextCursorPosition cursorPos)
        {
            foreach (var element in Contents)
            {
                element.IncrementPosition(ref cursorPos);
            }
        }

        public void Setup(ITextContainer parent, Vector2 position)
        {
            Matrix matrix = parent?.Position.Transform ?? Matrix.Identity;
            Position = new ElementPosition(matrix * Matrix.CreateTranslation(position.X, position.Y, 0));
            float yOffset = 0;
            foreach (var element in Contents)
            {
                element.Setup(this, new Vector2(0, yOffset));
                yOffset += element.Height;
            }
        }
    }

    class TextBuilder
    {
        ITextContainer Root;
        Stack<ITextContainer> ContainerStack = new Stack<ITextContainer>();
        ITextContainer CurrentContainer => ContainerStack.Peek();

        Action<ITextElement> Handlers;
        public List<MenuAreaText> MenuAreas = new List<MenuAreaText>();
        MenuAreaText CurrentMenuArea;
        public TextPosition TextPosition;

        public TextFormatting DefaultFormat;
        public DialogFormatting DefaultDialog;

        

        public TextBuilder(float width, float height, TextFormatting defaultFormat = null, DialogFormatting defaultDialog = null)
        {
            Root = new TextElementContainer(width, height);
            ContainerStack.Push(Root);
            DefaultFormat = defaultFormat ?? new TextFormatting();
            DefaultDialog = defaultDialog ?? new DialogFormattingIdentity();
        }

        public ITextElement GetCurrent()
        {
            return ContainerStack.Peek();
        }

        public float GetWidth()
        {
            return GetCurrent().Width;
        }

        public void StartContainer(float width, float height)
        {
            var container = new TextElementContainer(width, height);
            ContainerStack.Push(container);
        }

        public TextElementContainer EndContainer()
        {
            TextElementContainer container = (TextElementContainer)ContainerStack.Pop();
            if(ContainerStack.Any())
                ContainerStack.Peek().Add(container);
            return container;
        }

        public void StartLine(LineAlignment alignment)
        {
            var line = new TextElementLine(alignment);
            ContainerStack.Push(line);
        }

        public TextElementLine EndLine()
        {
            TextElementLine line = (TextElementLine)ContainerStack.Pop();
            ContainerStack.Peek().Add(line);
            return line;
        }

        public void StartNoBreak()
        {
            var line = new TextElementNoBreak();
            ContainerStack.Push(line);
        }

        public TextElementNoBreak EndNoBreak()
        {
            TextElementNoBreak noBreak = (TextElementNoBreak)ContainerStack.Pop();
            ContainerStack.Peek().Add(noBreak);
            return noBreak;
        }

        public void StartHandler(ITextElementHandler handler)
        {
            Handlers += handler.Add;
        }

        public void EndHandler(ITextElementHandler handler)
        {
            Handlers -= handler.Add;
        }

        public void StartMenuArea(double priority, ITooltipProvider tooltipProvider = null)
        {
            if (CurrentMenuArea != null)
                throw new Exception();

            CurrentMenuArea = new MenuAreaText(this, priority, tooltipProvider);

            StartHandler(CurrentMenuArea);
        }

        public MenuAreaText EndMenuArea()
        {
            if (CurrentMenuArea == null)
                throw new Exception();

            var menuArea = CurrentMenuArea;
            CurrentMenuArea = null;

            EndHandler(menuArea);
            MenuAreas.Add(menuArea);
            return menuArea;
        }

        public void AppendFormat(string format, params object[] parameters)
        {
            StringBuilder stringBuilder = new StringBuilder();
            Stack<char> cellar = new Stack<char>();

            void pushString()
            {
                AppendText(stringBuilder.ToString());
                stringBuilder.Clear();
            }

            for(int i = 0; i < format.Length; i++)
            {
                var c = format[i];
                if (c == '{')
                {
                    if (cellar.Empty())
                        cellar.Push(c);
                    else if (cellar.Peek() == c)
                    {
                        cellar.Pop();
                        stringBuilder.Append(c);
                    }
                    else
                        throw new FormatException();
                }
                else if (c == '}')
                {
                    if (cellar.Empty())
                        cellar.Push(c);
                    else if (cellar.Peek() == c) //TODO: WILL NEVER FIRE, NEED TO DELAY TOKEN CREATION UNTIL WE'RE SURE WE DIDN'T MAKE A FORMAT ERROR
                    {
                        if(cellar.Count > 1)
                            throw new FormatException();
                        cellar.Pop();
                        stringBuilder.Append(c);
                    }
                    else
                    {
                        var token = new string(cellar.Reverse().ToArray(), 1, cellar.Count - 1);
                        var index = int.Parse(token);
                        var parameter = parameters[index];
                        if (parameter is string str)
                            stringBuilder.Append(str);
                        else if(parameter is ITextElement element)
                        {
                            pushString();
                            AppendElement(element);
                        }
                        else if(parameter is Action<TextBuilder> action)
                        {
                            pushString();
                            action(this);
                        }
                        else if (parameter is TextElementer textElementer)
                        {
                            pushString();
                            textElementer(this);
                        }
                        else
                        {
                            throw new FormatException();
                        }
                        cellar.Clear();
                    }
                }
                else if(c >= '0' && c <= '9' && cellar.Count > 0)
                {
                    cellar.Push(c);
                }
                else
                {
                    stringBuilder.Append(c);
                }
            }

            if(stringBuilder.Length > 0)
                pushString();
        }

        public void AppendText(string str, TextFormatting format = null, DialogFormatting dialogFormat = null)
        {
            format = format ?? DefaultFormat;
            dialogFormat = dialogFormat ?? DefaultDialog;

            StringBuilder builder = new StringBuilder();

            void pushString()
            {
                if (builder.Length > 0)
                    AppendElement(new TextElementWord(builder.ToString(), format, dialogFormat));
                builder.Clear();
            }

            for (int i = 0; i < str.Length; i++)
            {
                char chr = str[i];

                switch (chr)
                {
                    case (' '):
                        pushString();
                        AppendElement(new TextElementSpace(4, format, dialogFormat));
                        break;
                    case ('\n'):
                        pushString();
                        NewLine();
                        break;
                    default:
                        builder.Append(chr);
                        break;
                }
            }

            pushString();
        }

        public void AppendSymbol(Symbol symbol, TextFormatting format = null, DialogFormatting dialogFormat = null)
        {
            format = format ?? DefaultFormat;
            dialogFormat = dialogFormat ?? DefaultDialog;

            AppendElement(new TextElementSymbol(symbol, format, dialogFormat));
        }

        public void AppendSpace(int width, TextFormatting format = null, DialogFormatting dialogFormat = null)
        {
            format = format ?? DefaultFormat;
            dialogFormat = dialogFormat ?? DefaultDialog;

            AppendElement(new TextElementSpace(width, format, dialogFormat));
        }

        public void AppendElement(ITextElement element)
        {
            Handlers?.Invoke(element);
            CurrentContainer.Add(element);
        }

        public void NewLine()
        {
            var line = EndLine();
            StartLine(line.Alignment);
        }

        public void NewLine(LineAlignment alignment)
        {
            EndLine();
            StartLine(alignment);
        }

        public void StartTableRow(float width, ColumnConfigs columnConfigs)
        {
            var container = new TextTableLine(width, columnConfigs);
            ContainerStack.Push(container);
        }

        public TextTableLine EndTableRow()
        {
            TextTableLine row = (TextTableLine)ContainerStack.Pop();
            if (ContainerStack.Any())
                ContainerStack.Peek().Add(row);
            return row;
        }

        public void StartTableCell()
        {
            TextTableLine row = (TextTableLine)ContainerStack.Peek();
            var cell = new TextTableCell(row, row.Contents.Count);
            ContainerStack.Push(cell);
        }

        public TextTableCell EndTableCell()
        {
            TextTableCell cell = (TextTableCell)ContainerStack.Pop();
            if (ContainerStack.Any())
                ContainerStack.Peek().Add(cell);
            return cell;
        }

        public void Update()
        {
            foreach (var e in EnumerateElements().OfType<ITextDialogable>())
            {
                e.Dialog.Update();
            }
        }

        public void Draw(Vector2 position, FontRenderer renderer, Matrix? transform = null)
        {
            SetPosition(position, transform);
            renderer.ResetDebug();
            Root.Draw(null, TextPosition.Transform * Matrix.CreateTranslation(TextPosition.Position.X, TextPosition.Position.Y, 0), renderer, new TextCursorPosition());
        }

        public void Draw(TextPosition position, FontRenderer renderer)
        {
            Draw(position.Position, renderer, position.Transform);
        }
        
        public void SetPosition(Vector2 position, Matrix? transform = null)
        {
            TextPosition = new TextPosition(position, transform ?? Matrix.Identity);
        }

        public void Finish()
        {
            Root.Setup(null, Vector2.Zero);

            var dialogElements = EnumerateElements().OfType<ITextDialogable>().ToList();
            foreach (var element in dialogElements)
            {
                element.DialogFormat.Add(element);
            }
            
            foreach (var dialogFormat in dialogElements.Select(x => x.DialogFormat).Distinct())
            {
                dialogFormat.Setup(dialogElements);
            }
        }

        public IEnumerable<ITextElement> EnumerateElements()
        {
            Stack<ITextElement> elements = new Stack<ITextElement>();
            elements.Push(Root);

            while (elements.Count > 0)
            {
                var element = elements.Pop();
                if (element is ITextContainer container)
                {
                    foreach (var e in container.Contents.Reverse<ITextElement>())
                        elements.Push(e);
                }
                yield return element;
            }
        }

        public float GetContentHeight()
        {
            if (Root.Contents.Empty())
                return 0;
            return Root.Contents.Sum(x => x.Height);
        }

        public float GetContentWidth()
        {
            if (Root.Contents.Empty())
                return 0;
            return Root.Contents.Max(x => x.Width);
        }


        public bool IsEmpty()
        {
            return Root.Contents.Empty();
        }
    }
}
