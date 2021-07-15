using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    delegate void TextElementer(TextBuilder textBuilder);

    static class StringFormatUtil
    {
        public static void AppendAsKey(this TextBuilder builder, string str, DialogFormatting dialog = null)
        {
            AppendAsKey(builder, str, Color.White, dialog);
        }

        public static void AppendAsKey(this TextBuilder builder, string str, Color color, DialogFormatting dialog = null)
        {
            TextFormatting formatting = new TextFormatting()
            {
                Bold = true,
                GetParams = (pos) => new DialogParams() {
                    Color = color,
                    Border = Color.Black,
                    Scale = Vector2.One,
                }
            };
            builder.AppendText(str, formatting, dialog);
        }

        public static void AppendStatus(this TextBuilder builder, Symbol symbol, string str, Color color, DialogFormatting dialog = null)
        {
            TextFormatting formatting = new TextFormatting()
            {
                Bold = true,
                GetParams = (pos) => new DialogParams()
                {
                    Color = color,
                    Border = Color.Black,
                    Scale = Vector2.One,
                }
            };
            builder.StartNoBreak();
            builder.AppendSymbol(symbol, formatting, dialog);
            builder.AppendText(str, formatting, dialog);
            builder.EndNoBreak();
        }

        public static void AppendStored(this TextBuilder builder, Symbol symbol, int amount, Color color, DialogFormatting dialog = null)
        {
            AppendStatus(builder, symbol, amount.ToString(), color, dialog);
        }

        public static void AppendGain(this TextBuilder builder, Symbol symbol, int amount, Color color, DialogFormatting dialog = null)
        {
            AppendDescribe(builder, symbol, amount.ToString("+#;-#;0"), color, dialog);
        }

        public static void AppendDescribe(this TextBuilder builder, Symbol symbol, string str, Color color, DialogFormatting dialog = null)
        {
            TextFormatting formatting = new TextFormatting()
            {
                Bold = true,
                GetParams = (pos) => new DialogParams()
                {
                    Color = color,
                    Border = Color.Black,
                    Scale = Vector2.One,
                }
            };
            builder.StartNoBreak();
            builder.AppendText(str, formatting, dialog);
            builder.AppendSymbol(symbol, formatting, dialog);
            builder.EndNoBreak();
        }

        public static void AppendDamage(this TextBuilder builder, Symbol symbol, int damage, Color color, DialogFormatting dialog = null)
        {
            AppendDescribe(builder, symbol, $"{damage:+0;-#}", color, dialog);
        }

        public static void AppendText(this TextBuilder builder, string str, TextFormatting format = null, DialogFormatting dialogFormat = null)
        {
            builder.AppendText(str, format, dialogFormat);
        }

        public static void AppendSymbol(this TextBuilder builder, Symbol symbol, TextFormatting format = null, DialogFormatting dialogFormat = null)
        {
            builder.AppendSymbol(symbol, format, dialogFormat);
        }
    }
}
