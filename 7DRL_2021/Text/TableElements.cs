using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    interface IColumnWidth
    {
        bool Strict { get; }
    }

    struct ColumnFixedWidth : IColumnWidth
    {
        public float Width;
        public bool Strict { get; set; }

        public ColumnFixedWidth(float width, bool strict)
        {
            Width = width;
            Strict = strict;
        }
    }

    struct ColumnFractionalWidth : IColumnWidth
    {
        public float Fraction;
        public bool Strict { get; set; }

        public ColumnFractionalWidth(float fraction, bool strict)
        {
            Fraction = fraction;
            Strict = strict;
        }
    }

    struct ColumnWidth
    {
        public float Width;
        public bool Strict;

        public ColumnWidth(float width, bool strict)
        {
            Width = width;
            Strict = strict;
        }
    }

    class ColumnConfigs
    {
        public float Padding = 5;
        List<IColumnWidth> ColumnWidths = new List<IColumnWidth>();

        public ColumnConfigs(IEnumerable<IColumnWidth> columnWidths)
        {
            ColumnWidths = new List<IColumnWidth>(columnWidths);
        }

        public float GetWidth(int columnIndex, float width)
        {
            float trueWidth = width - Padding * (ColumnWidths.Count - 1);
            ColumnWidth[] widths = new ColumnWidth[ColumnWidths.Count];
            for(int i = 0; i < widths.Length; i++)
            {
                if(ColumnWidths[i] is ColumnFixedWidth fixedWidth)
                {
                    widths[i] = new ColumnWidth(fixedWidth.Width, fixedWidth.Strict);
                }
                else if (ColumnWidths[i] is ColumnFractionalWidth fractionalWidth)
                {
                    widths[i] = new ColumnWidth(fractionalWidth.Fraction * (trueWidth - ColumnWidths.OfType<ColumnFixedWidth>().Sum(x => x.Width)), fractionalWidth.Strict);
                }
            }
            float extraWidth = trueWidth - widths.Sum(x => x.Width);
            if(ColumnWidths[columnIndex].Strict)
                return widths[columnIndex].Width;
            else
                return widths[columnIndex].Width + extraWidth / widths.Count(x => !x.Strict);
        }
    }

    class TextTableLine : ITextElement, ITextContainer
    {
        public ColumnConfigs ColumnConfigs;

        public float Width { get; set; } = float.PositiveInfinity; 
        public float Height => Contents.Any() ? Math.Max(0, Contents.Max(x => x.Height)) : 0;

        public float VisualWidth => Width;
        public float VisualHeight => Height;

        public ElementPosition Position { get; private set; }

        public bool IsUnit => true;

        public List<ITextElement> Contents { get; } = new List<ITextElement>();

        public TextTableLine(float width, ColumnConfigs columnConfigs)
        {
            Width = width;
            ColumnConfigs = columnConfigs;
        }

        public float GetColumnWidth(int columnIndex)
        {
            return ColumnConfigs.GetWidth(columnIndex, Width);
        }

        public void Add(ITextElement element)
        {
            if (element is TextTableCell cell)
                Contents.Add(cell);
            else
                throw new ArgumentException();
        }

        public void Draw(ITextContainer parent, Matrix baseTransform, FontRenderer renderer, TextCursorPosition cursorPos)
        {
            float offset = 0;
            float alignOffset = 0;
            renderer.Scene.PushSpriteBatch(transform: Position.Transform * baseTransform);
            renderer.DrawAreaDebug(Vector2.Zero, new Vector2(Width, Height));
            int i = 0;
            foreach (var element in Contents)
            {
                element.Draw(this, baseTransform, renderer, cursorPos + new Vector2(offset + alignOffset, 0));
                element.IncrementPosition(ref cursorPos);
                offset += element.Width + (i < Contents.Count - 1 ? ColumnConfigs.Padding : 0);
                i++;
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

            float offset = 0;
            float alignOffset = 0;
            Position = new ElementPosition(parent.Position.Transform * Matrix.CreateTranslation(position.X + alignOffset, position.Y, 0));
            int i = 0;
            foreach (var element in Contents)
            {
                element.Setup(this, new Vector2(offset, 0));
                offset += element.Width + (i < Contents.Count-1 ? ColumnConfigs.Padding : 0);
                i++;
            }
        }
    }

    class TextTableCell : ITextElement, ITextContainer
    {
        public TextTableLine Line;
        public int ColumnIndex;

        public float Width => Line.GetColumnWidth(ColumnIndex);
        public float Height => Contents.Any() ? Contents.Sum(x => x.Height) : 0;

        public float VisualWidth => Width;
        public float VisualHeight => Height;

        public ElementPosition Position { get; private set; }

        public bool IsUnit => true;

        public List<ITextElement> Contents { get; } = new List<ITextElement>();
        public List<ITextElement> Overflow { get; } = new List<ITextElement>();

        public TextTableCell(TextTableLine line, int columnIndex)
        {
            Line = line;
            ColumnIndex = columnIndex;
        }

        public void Add(ITextElement toAdd)
        {
            var backlog = new LinkedList<ITextElement>();
            var height = 0f;
            backlog.AddFirst(toAdd);
            while (backlog.Any())
            {
                var element = backlog.PopFirst();
                if (element.Width > Width)
                {
                    if (element is ITextSplitable splitable)
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

        public void Draw(ITextContainer parent, Matrix baseTransform, FontRenderer renderer, TextCursorPosition cursorPos)
        {
            renderer.Scene.PushSpriteBatch(transform: Position.Transform * baseTransform);
            renderer.DrawAreaDebug(Vector2.Zero, new Vector2(Width, Height));
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
}
