using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    public enum Alignment
    {
        Left,
        Center,
        Right,
    }

    public delegate Color TextColorFunction(int dialogIndex);
    public delegate Vector2 TextOffsetFunction(int dialogIndex);

    struct CharInfo
    {
        public int Offset;
        public int Width;
        public bool Predefined;

        public CharInfo(int offset, int width, bool predefined)
        {
            Offset = offset;
            Width = width;
            Predefined = predefined;
        }
    }

    static class FontUtil
    {
        public class Gibberish
        {
            public Dictionary<int, List<char>> ByWidth;
            public Func<char, bool> CharSelection;

            public Gibberish(Func<char, bool> charSelection)
            {
                CharSelection = charSelection;
            }

            public char GetSimilar(char chr)
            {
                if (ByWidth == null)
                    ByWidth = Enumerable.Range(0, CharInfo.Length).Where(x => CharSelection((char)x)).GroupBy(x => CharInfo[x].Width).ToDictionary(x => x.Key, x => x.Select(y => (char)y).ToList());
                int width = GetCharWidth(chr);
                if (ByWidth.ContainsKey(width))
                    return ByWidth[width].Pick(Random);
                return chr;
            }
        }

        public const int CharWidth = 16;
        public const int CharHeight = 16;
        public const int CharsPerRow = 32;
        public const int CharsPerColumn = 32;
        public const int CharsPerPage = CharsPerColumn * CharsPerRow;

        public static CharInfo[] CharInfo = new CharInfo[65536];
        public static Gibberish GibberishStandard = new Gibberish(x => x > ' ' && x <= '~');
        public static Gibberish GibberishAlpha = new Gibberish(x => (x > ' ' && x <= '~') || (x > 160 && x <= 832));
        public static Gibberish GibberishQuery = new Gibberish(x => (x > 5120 && x <= 5760 - 128));
        public static Gibberish GibberishAlquimy = new Gibberish(x => (x > 40960 && x <= 40960 + 1024 + 128) || (x > 40960 + 1024 + 256 && x <= 40960 + 1024 + 256 + 256));
        public static Gibberish GibberishRune = new Gibberish(x => (x >= 6144 + 32 && x <= 6144 + 120 - 1) || (x >= 6144 + 128 && x <= 6144 + 128 + 32 + 10));
        public static Random Random = new Random();

        public static char GetSimilarChar(char chr, Gibberish gibberish)
        {
            if (Random.Next(2) > 0)
                return chr;
            return gibberish.GetSimilar(chr);
        }

        public static Rectangle GetCharRect(int index)
        {
            int x = index % CharsPerRow;
            int y = index / CharsPerRow;

            return new Rectangle(x * CharWidth, y * CharHeight, CharWidth, CharHeight);
        }

        public static Vector2 GetCharMiddle(int index)
        {
            int x = CharInfo[index].Offset;
            int width = CharInfo[index].Width;

            return new Vector2(x + width / 2, CharHeight / 2);
        }

        public static int GetCharWidth(char chr)
        {
            return CharInfo[chr].Width;
        }

        public static int GetCharOffset(char chr)
        {
            return CharInfo[chr].Offset;
        }

        public static void RegisterChar(Color[] blah, int width, int height, char chr, int index)
        {
            Rectangle rect = GetCharRect(index);

            int left = rect.Width - 1;
            int right = 0;
            bool empty = true;

            for (int x = rect.Left; x < rect.Right; x++)
                for (int y = rect.Top; y < rect.Bottom; y++)
                {
                    if (blah[y * width + x].A > 0)
                    {
                        left = Math.Min(left, x - rect.Left);
                        right = Math.Max(right, x - rect.Left);
                        empty = false;
                    }
                }

            if (!CharInfo[chr].Predefined)
                CharInfo[chr] = new CharInfo(left, empty ? 0 : right - left + 1, false);
        }

        private static string CachedString;
        private static int CachedWidth;
        private static int CachedHeight;
        private static List<object> CachedTokens;

        private static int GetWordLength(string str)
        {
            int width = 0;
            foreach (var chr in str)
            {
                width += GetCharWidth(chr);
            }
            return width;
        }
    }
}
