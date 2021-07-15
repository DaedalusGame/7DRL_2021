using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    class TextElementCounter : ITextElement, ITextDialogable
    {
        public float Width => GetBackgroundWidth();
        public float Height => 16; //TODO: line height formatting

        public ElementPosition Position { get; private set; }
        public TextDialog Dialog { get; private set; } = new TextDialog();

        public bool IsUnit => true;

        public int Characters => BackgroundText.Length;

        public Func<int> Value;
        public int Digits;
        public string BackgroundText => new string('0', Digits);
        public string ForegroundText => MathHelper.Clamp(Value(), 0, (float)Math.Pow(10, Digits)-1).ToString();
        public TextFormatting FormatFront { get; private set; }
        public TextFormatting FormatBack { get; private set; }
        public DialogFormatting DialogFormat { get; private set; }

        public TextElementCounter(Func<int> value, int digits, TextFormatting formatFront, TextFormatting formatBack, DialogFormatting dialogFormat)
        {
            Value = value;
            Digits = digits;
            FormatFront = formatFront;
            FormatBack = formatBack;
            DialogFormat = dialogFormat;
        }

        private float GetBackgroundWidth()
        {
            if (BackgroundText.Length <= 0)
                return 0;
            int seperator = (BackgroundText.Length) * 1;
            return BackgroundText.Sum(FormatBack.GetCharWidth) + seperator;
        }

        private float GetForegroundWidth()
        {
            if (ForegroundText.Length <= 0)
                return 0;
            int seperator = (ForegroundText.Length) * 1;
            return ForegroundText.Sum(FormatFront.GetCharWidth) + seperator;
        }

        public override string ToString()
        {
            return ForegroundText;
        }

        public void Setup(ITextContainer parent, Vector2 position)
        {
            Position = new ElementPosition(parent.Position.Transform * Matrix.CreateTranslation(position.X, position.Y, 0));
        }

        public void Draw(ITextContainer parent, Matrix baseTransform, FontRenderer renderer, TextCursorPosition cursorPos)
        {
            renderer.DrawChars(BackgroundText, Width, Height, FormatBack, Dialog, Position.Transform * baseTransform, cursorPos);
            var offset = GetBackgroundWidth() - GetForegroundWidth();
            renderer.DrawChars(ForegroundText, Width, Height, FormatFront, Dialog, Matrix.CreateTranslation(offset,0,0) * Position.Transform * baseTransform, cursorPos);
        }

        public void IncrementPosition(ref TextCursorPosition cursorPos)
        {
            cursorPos.AddCharacters(BackgroundText.Length);
            cursorPos.IncrementElement();
        }
    }

    class TextElementDynamic : ITextElement, ITextDialogable
    {
        public float Width { get; set; }
        public float Height => 16; //TODO: line height formatting

        public ElementPosition Position { get; private set; }
        public TextDialog Dialog { get; private set; } = new TextDialog();

        public bool IsUnit => true;

        public int Characters => Text.Length;
        public string Text => Value();

        public Func<string> Value;
        public TextFormatting Format { get; private set; }
        public DialogFormatting DialogFormat { get; private set; }

        public TextElementDynamic(Func<string> value, float width, TextFormatting format, DialogFormatting dialogFormat)
        {
            Value = value;
            Width = width;
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
}
