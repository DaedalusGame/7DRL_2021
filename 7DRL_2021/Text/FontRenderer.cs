using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    struct CursorPosition
    {

    }

    class FontParameters
    {
        
    }

    class FontRenderer
    {
        public Scene Scene;
        float ColorHue = 0;

        public FontRenderer(Scene scene)
        {
            Scene = scene;
        }

        public void ResetDebug()
        {
            ColorHue = 0;
        }

        public void DrawChars(IEnumerable<char> chars, float width, float height, TextFormatting format, TextDialog dialog, Matrix transform, TextCursorPosition cursorPos)
        {
            float offset = 0;

            var skew = new Matrix(
                1, 0, 0, 0,
                format.Italic ? -4f / 16 : 0, 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
            );
            var skewCenter = Matrix.CreateTranslation(-width / 2, -height / 2, 0) * skew * Matrix.CreateTranslation(width / 2, height / 2, 0);
            Scene.PushSpriteBatch(transform: skewCenter * transform);

            DrawAreaDebug(Vector2.Zero, new Vector2(width, height));
            foreach (var chr in chars)
            {
                var par = format.GetParams(cursorPos + new Vector2(offset, 0));
                DrawChar(chr, new Vector2(offset, 0), format, dialog.Transform(par, cursorPos + new Vector2(offset, 0)));
                cursorPos.AddCharacters(1);
                offset += format.GetCharWidth(chr) + 1;
            }
            Scene.PopSpriteBatch();
        }

        public void DrawChar(char chr, Vector2 drawpos, TextFormatting format, DialogParams param)
        {
            Texture2D tex = Game.FontSprites[chr / FontUtil.CharsPerPage].Texture;

            int index = chr % FontUtil.CharsPerPage;
            int offset = FontUtil.GetCharOffset(chr);
            int width = FontUtil.GetCharWidth(chr);

            var color = param.Color;
            var border = param.Border;
            var charOffset = param.Offset;
            var charScale = param.Scale;
            var charAngle = param.Angle;
            var bold = format.Bold;
            var scriptOffset = 0;

            //DrawCharLine(drawpos + charOffset + new Vector2(0, 15), width + 1 + (bold ? 1 : 0), color, border);
            //DrawCharLine(drawpos + charOffset + new Vector2(0, 2), width + 1 + (bold ? 1 : 0), color, border);

            var rect = FontUtil.GetCharRect(index);
            var middle = FontUtil.GetCharMiddle(index);

            if (border.A > 0)
            { //Only draw outline if it's actually non-transparent
                Scene.SpriteBatch.Draw(tex, drawpos + charOffset + new Vector2(-offset - 1, scriptOffset + 0) + middle, rect, border, charAngle, middle, charScale, SpriteEffects.None, 0);
                Scene.SpriteBatch.Draw(tex, drawpos + charOffset + new Vector2(-offset, scriptOffset + 1) + middle, rect, border, charAngle, middle, charScale, SpriteEffects.None, 0);
                Scene.SpriteBatch.Draw(tex, drawpos + charOffset + new Vector2(-offset, scriptOffset - 1) + middle, rect, border, charAngle, middle, charScale, SpriteEffects.None, 0);
                if (bold)
                {
                    Scene.SpriteBatch.Draw(tex, drawpos + charOffset + new Vector2(-offset + 2, scriptOffset + 0) + middle, rect, border, charAngle, middle, charScale, SpriteEffects.None, 0);
                    Scene.SpriteBatch.Draw(tex, drawpos + charOffset + new Vector2(-offset + 1, scriptOffset + 1) + middle, rect, border, charAngle, middle, charScale, SpriteEffects.None, 0);
                    Scene.SpriteBatch.Draw(tex, drawpos + charOffset + new Vector2(-offset + 1, scriptOffset - 1) + middle, rect, border, charAngle, middle, charScale, SpriteEffects.None, 0);
                }
                else
                {
                    Scene.SpriteBatch.Draw(tex, drawpos + charOffset + new Vector2(-offset + 1, scriptOffset) + middle, rect, border, charAngle, middle, charScale, SpriteEffects.None, 0);
                }
            }

            Scene.SpriteBatch.Draw(tex, drawpos + charOffset + new Vector2(-offset, scriptOffset) + middle, rect, color, charAngle, middle, charScale, SpriteEffects.None, 0);
            if (bold)
                Scene.SpriteBatch.Draw(tex, drawpos + charOffset + new Vector2(-offset + 1, scriptOffset) + middle, rect, color, charAngle, middle, charScale, SpriteEffects.None, 0);

            //if(charOffset == Vector2.Zero)
            //    DrawCharLine(drawpos + charOffset + new Vector2(-1, 8), width + 2 + (bold ? 1 : 0), color, border);
        }

        public void DrawCharLine(Vector2 pos, int width, Color color, Color border)
        {
            Scene.SpriteBatch.Draw(Scene.Pixel, pos + new Vector2(0, -1), new Rectangle(0, 0, width, 3), border);
            Scene.SpriteBatch.Draw(Scene.Pixel, pos, new Rectangle(0, 0, width, 1), color);
        }

        public void DrawAreaDebug(Vector2 pos, Vector2 size, Color color)
        {
            //Scene.SpriteBatch.Draw(Scene.Pixel, pos, null, color, 0, Vector2.Zero, size, SpriteEffects.None, 0);
        }

        public void DrawAreaDebug(Vector2 pos, Vector2 size)
        {
            DrawAreaDebug(pos, size, new Color(255,255,0,128).RotateHue(ColorHue));
            ColorHue += 0.12f;
        }
    }
}
