using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    enum Facing
    {
        North,
        East,
        South,
        West,
    }

    class SimpleNoise
    {
        public int Seed;
        byte[] Alpha = new byte[256];
        byte[] Beta = new byte[256];

        public SimpleNoise(int seed)
        {
            Seed = seed;
            Random noiseRandom = new Random(Seed);
            noiseRandom.NextBytes(Alpha);
            noiseRandom.NextBytes(Beta);
        }

        public int GetValue(int x, int y)
        {
            int xa = Util.PositiveMod(x, 16);
            int ya = Util.PositiveMod(y, 16);
            int xb = Util.PositiveMod(Util.FloorDiv(x, 16), 16);
            int yb = Util.PositiveMod(Util.FloorDiv(y, 16), 16);

            return Alpha[xa + ya * 16] + Beta[xb + yb * 16];
        }
    }

    class MultiDict<TKey, TValue>
    {
        private Dictionary<TKey, List<TValue>> Data = new Dictionary<TKey, List<TValue>>();

        public IEnumerable<TValue> this[TKey key]
        {
            get
            {
                return Data[key];
            }
        }

        public void Add(TKey key, TValue value)
        {
            List<TValue> list;
            if (Data.TryGetValue(key, out list))
                list.Add(value);
            else
                Data.Add(key, new List<TValue>() { value });
        }

        public IEnumerable<TValue> GetOrEmpty(TKey key)
        {
            List<TValue> list;
            if (Data.TryGetValue(key, out list))
                return list;
            else
                return Enumerable.Empty<TValue>();
        }
    }

    static class Util
    {
        public static void SetupRenderTarget(this GraphicsDevice graphicsDevice, ref RenderTarget2D renderTarget, int width, int height)
        {
            if (renderTarget == null || renderTarget.IsContentLost)
                renderTarget = new RenderTarget2D(graphicsDevice, width, height);
        }

        public static bool IsStepTowards(Point target, Point last, Point current)
        {
            return GetDistanceSquared(current, target) <= GetDistanceSquared(last, target);
        }

        public static Vector2 Scuff(this Vector2 vec, float dist)
        {
            float scale = dist / Math.Max(Math.Abs(vec.X), Math.Abs(vec.Y));
            return vec * scale;
        }

        public static int GetDistanceSquared(Point a, Point b)
        {
            int dx = a.X - b.X;
            int dy = a.Y - b.Y;

            return dx * dx + dy * dy;
        }

        public static int PositiveMod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }

        public static float PositiveMod(float x, float m)
        {
            float r = x % m;
            return r < 0 ? r + m : r;
        }

        public static int FloorDiv(int x, int m)
        {
            if (((x < 0) ^ (m < 0)) && (x % m != 0))
            {
                return (x / m - 1);
            }
            else
            {
                return (x / m);
            }
        }

        public static float RandomNoise(int n)
        {
            Random random = new Random(n);
            return random.NextFloat();
        }

        public static float ReverseClamp(float n, float lower, float upper)
        {
            return MathHelper.Clamp(ReverseLerp(n, lower, upper), 0, 1);
        }

        public static float ReverseLerp(float n, float lower, float upper)
        {
            return (n - lower) / (upper - lower);
        }

        public static double ReverseLerp(double n, double lower, double upper)
        {
            return (n - lower) / (upper - lower);
        }

        public static T WithMin<T, V>(this IEnumerable<T> enumerable, Func<T, V> selector) where V : IComparable
        {
            return enumerable.Aggregate((i1, i2) => selector(i1).CompareTo(selector(i2)) < 0 ? i1 : i2);
        }

        public static T WithMax<T, V>(this IEnumerable<T> enumerable, Func<T, V> selector) where V : IComparable
        {
            return enumerable.Aggregate((i1, i2) => selector(i1).CompareTo(selector(i2)) > 0 ? i1 : i2);
        }

        public static IEnumerable<T> NonNull<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.Where(x => x != null);
        }

        public static V IfNonEmpty<T, V>(this IEnumerable<T> enumerable, Func<IEnumerable<T>, V> transform, V defaultValue)
        {
            if (enumerable.Any())
                return transform(enumerable);
            else
                return defaultValue;
        }

        public static bool Empty<T>(this IEnumerable<T> enumerable)
        {
            return !enumerable.Any();
        }

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultValue)
        {
            TValue value;
            if (dict.TryGetValue(key, out value))
                return value;
            else
                return defaultValue;
        }

        public static ILookup<TKey, TElement> ToMultiLookup<TSource, TKey, TElement>(this IEnumerable<TSource> enumerable, Func<TSource, IEnumerable<TKey>> keySelector, Func<TSource, TElement> valueSelector)
        {
            return enumerable.SelectMany(obj => keySelector(obj).Select(pass => Tuple.Create(valueSelector(obj), pass))).ToLookup(obj => obj.Item2, obj => obj.Item1);
        }

        public static ILookup<TKey, TElement> ToMultiLookup<TKey, TElement>(this IEnumerable<TElement> enumerable, Func<TElement, IEnumerable<TKey>> keySelector)
        {
            return enumerable.SelectMany(obj => keySelector(obj).Select(pass => Tuple.Create(obj, pass))).ToLookup(obj => obj.Item2, obj => obj.Item1);
        }

        public static IEnumerable<IGrouping<TKey, TElement>> MultiGroupBy<TSource, TKey, TElement>(this IEnumerable<TSource> enumerable, Func<TSource, IEnumerable<TKey>> keySelector, Func<TSource, TElement> valueSelector)
        {
            return enumerable.SelectMany(obj => keySelector(obj).Select(pass => Tuple.Create(valueSelector(obj), pass))).GroupBy(obj => obj.Item2, obj => obj.Item1);
        }

        public static IEnumerable<IGrouping<TKey, TElement>> MultiGroupBy<TKey, TElement>(this IEnumerable<TElement> enumerable, Func<TElement, IEnumerable<TKey>> keySelector)
        {
            return enumerable.SelectMany(obj => keySelector(obj).Select(pass => Tuple.Create(obj, pass))).GroupBy(obj => obj.Item2, obj => obj.Item1);
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable)
        {
            return new HashSet<T>(enumerable);
        }

        public static Color RotateHue(this Color color, double amount)
        {
            double U = Math.Cos(amount * Math.PI * 2);
            double W = Math.Sin(amount * Math.PI * 2);

            double r = (.299 + .701 * U + .168 * W) * color.R
                        + (.587 - .587 * U + .330 * W) * color.G
                        + (.114 - .114 * U - .497 * W) * color.B;
            double g = (.299 - .299 * U - .328 * W) * color.R
                        + (.587 + .413 * U + .035 * W) * color.G
                        + (.114 - .114 * U + .292 * W) * color.B;
            double b = (.299 - .3 * U + 1.25 * W) * color.R
                        + (.587 - .588 * U - 1.05 * W) * color.G
                        + (.114 + .886 * U - .203 * W) * color.B;

            return new Color((int)r, (int)g, (int)b, color.A);
        }

        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.R, color.G, color.B, (int)(color.A * alpha));
        }

        public static string EnglishJoin(string seperator, string finalSeperator, IEnumerable<string> values)
        {
            values = values.ToList();
            var first = values.Take(values.Count() - 1);
            var last = values.Last();
            if (!first.Any())
                return last;
            else
                return $"{String.Join(seperator, first)}{finalSeperator}{last}";
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> toShuffle, Random random)
        {
            List<T> shuffled = new List<T>();
            foreach (T value in toShuffle)
            {
                shuffled.Insert(random.Next(shuffled.Count + 1), value);
            }
            return shuffled;
        }

        private static T PickInternal<T>(IList<T> enumerable, Random random, bool remove)
        {
            int select = random.Next(enumerable.Count());
            T pick = enumerable[select];
            if (remove)
                enumerable.RemoveAt(select);
            return pick;
        }

        public static T Pick<T>(this IList<T> enumerable, Random random)
        {
            return PickInternal(enumerable, random, false);
        }

        public static T PickBest<T>(this IList<T> enumerable, Func<T, IComparable> keySelector, Random random)
        {
            var pick = enumerable.Shuffle(random).OrderByDescending(keySelector).First();
            return pick;
        }

        public static T PickAndRemove<T>(this IList<T> enumerable, Random random)
        {
            return PickInternal(enumerable, random, true);
        }

        public static T PickAndRemoveBest<T>(this IList<T> enumerable, Func<T, IComparable> keySelector, Random random)
        {
            var pick = enumerable.Shuffle(random).OrderByDescending(keySelector).First();
            enumerable.Remove(pick);
            return pick;
        }

        public static T PickWeighted<T>(this IList<T> enumerable, Func<T, double> weight, Random random)
        {
            double totalWeight = enumerable.Sum(item => weight(item));
            double selection = random.NextDouble() * totalWeight;
            foreach (var item in enumerable)
            {
                selection -= weight(item);
                if (selection <= 0)
                    return item;
            }
            return enumerable.First();
        }

        public static float NextFloat(this Random random)
        {
            return (float)random.NextDouble();
        }

        public static float NextFloat(this Random random, float lower, float upper)
        {
            return lower + random.NextFloat() * (upper - lower);
        }


        public static float NextAngle(this Random random)
        {
            return random.NextFloat() * MathHelper.TwoPi;
        }

        public static Vector2 Mirror(this Vector2 vector, SpriteEffects mirror)
        {
            if (mirror.HasFlag(SpriteEffects.FlipHorizontally))
                vector.X *= -1;
            if (mirror.HasFlag(SpriteEffects.FlipVertically))
                vector.Y *= -1;

            return vector;
        }

        public static Vector2 TurnRight(this Vector2 vec)
        {
            return new Vector2(vec.Y, -vec.X);
        }

        public static Vector2 TurnLeft(this Vector2 vec)
        {
            return new Vector2(-vec.Y, vec.X);
        }

        public static Point Mirror(this Point offset)
        {
            return new Point(-offset.X, -offset.Y);
        }

        public static Point ToOffset(this Facing facing)
        {
            switch (facing)
            {
                case (Facing.North):
                    return new Point(0, -1);
                case (Facing.East):
                    return new Point(1, 0);
                case (Facing.South):
                    return new Point(0, 1);
                case (Facing.West):
                    return new Point(-1, 0);
                default:
                    return Point.Zero;
            }
        }

        public static Point ToLateral(this Facing facing)
        {
            switch (facing)
            {
                case (Facing.North):
                    return new Point(1, 0);
                case (Facing.East):
                    return new Point(0, 1);
                case (Facing.South):
                    return new Point(-1, 0);
                case (Facing.West):
                    return new Point(0, -1);
                default:
                    return Point.Zero;
            }
        }

        public static Facing TurnLeft(this Facing facing)
        {
            switch (facing)
            {
                case (Facing.North):
                    return Facing.West;
                case (Facing.East):
                    return Facing.North;
                case (Facing.South):
                    return Facing.East;
                case (Facing.West):
                    return Facing.South;
                default:
                    throw new Exception();
            }
        }

        public static Facing TurnRight(this Facing facing)
        {
            return facing.TurnLeft().Mirror();
        }

        public static Facing Turn(this Facing facing, int n)
        {
            n = n % 4;
            for (int i = n; i > 0; i--)
                facing = facing.TurnRight();
            for (int i = n; i < 0; i++)
                facing = facing.TurnLeft();
            return facing;
        }

        public static Facing Mirror(this Facing facing)
        {
            switch (facing)
            {
                case (Facing.North):
                    return Facing.South;
                case (Facing.East):
                    return Facing.West;
                case (Facing.South):
                    return Facing.North;
                case (Facing.West):
                    return Facing.East;
                default:
                    throw new Exception();
            }
        }

        public static float GetAngleDistance(float a0, float a1)
        {
            var max = Math.PI * 2;
            var da = (a1 - a0) % max;
            return (float)(2 * da % max - da);
        }

        public static float AngleLerp(float a0, float a1, float t)
        {
            return a0 + GetAngleDistance(a0, a1) * t;
        }

        public static Vector2 AngleToVector(float angle)
        {
            return new Vector2((float)Math.Sin(angle), (float)-Math.Cos(angle));
        }

        public static float VectorToAngle(Vector2 vector)
        {
            return (float)Math.Atan2(vector.X, -vector.Y);
        }

        public static Vector2 RotateVector(Vector2 vector, float addAngle)
        {
            var length = vector.Length();
            var angle = VectorToAngle(vector) + addAngle;
            return AngleToVector(angle) * length;
        }

        public static float ToAngle(this Facing facing)
        {
            return VectorToAngle(facing.ToOffset().ToVector2());
        }

        public static int GetSectionDelta(int aMin, int aMax, int bMin, int bMax)
        {
            if (aMax < bMin)
                return Math.Abs(aMax - bMin);
            if (bMax < aMin)
                return -Math.Abs(bMax - aMin);
            return 0;
        }

        public static int GetDeltaX(Rectangle a, Rectangle b)
        {
            return GetSectionDelta(a.Left, a.Right - 1, b.Left, b.Right - 1);
        }

        public static int GetDeltaY(Rectangle a, Rectangle b)
        {
            return GetSectionDelta(a.Top, a.Bottom - 1, b.Top, b.Bottom - 1);
        }

        public static int GetTaxicabDistance(Rectangle a, Rectangle b)
        {
            return Math.Max(Math.Abs(GetDeltaX(a, b)), Math.Abs(GetDeltaY(a, b)));
        }

        public static void SetupRenderTarget(this Scene scene, ref RenderTarget2D renderTarget, int width, int height)
        {
            if (renderTarget == null || renderTarget.IsContentLost || renderTarget.Width != width || renderTarget.Height != height)
            {
                if (renderTarget != null && !renderTarget.IsDisposed)
                    renderTarget.Dispose();
                renderTarget = new RenderTarget2D(scene.GraphicsDevice, width, height);
            }
        }

        public static void DrawPass(this ILookup<DrawPass, IDrawable> drawPasses, SceneGame scene, DrawPass pass)
        {
            foreach (var obj in drawPasses[pass].OrderBy(x => x.DrawOrder))
            {
                obj.Draw(scene, pass);
            }
        }

        public static float PointToAngle(Point point)
        {
            return VectorToAngle(point.ToVector2());
        }

        public static Point ToTileOffset(this Vector2 vector)
        {
            return new Point((int)Math.Floor(vector.X + 0.5f), (int)Math.Floor(vector.Y + 0.5f));
        }

        public static float GetClosestAngle(this IEnumerable<float> angles, float angle)
        {
            return angles.WithMin(x => Math.Abs(GetAngleDistance(x, angle)));
        }

        public static T GetClosestAngle<T>(this IDictionary<float,T> angles, float angle)
        {
            return angles.WithMin(x => Math.Abs(GetAngleDistance(x.Key, angle))).Value;
        }

        public static Point ToTile(this Vector2 vector)
        {
            return new Point((int)Math.Floor(vector.X), (int)Math.Floor(vector.Y));
        }

        public static T PopFirst<T>(this LinkedList<T> list)
        {
            var t = list.First();
            list.RemoveFirst();
            return t;
        }

        public static T PopLast<T>(this LinkedList<T> list)
        {
            var t = list.Last();
            list.RemoveLast();
            return t;
        }

        public static float GetTop(this ITextElement element)
        {
            return GetAllLocalPoints(element).Min(pos => pos.Y);
        }

        public static float GetBottom(this ITextElement element)
        {
            return GetAllLocalPoints(element).Max(pos => pos.Y);
        }

        public static float GetTop(this IEnumerable<ITextElement> elements)
        {
            return elements.SelectMany(GetAllLocalPoints).Min(pos => pos.Y);
        }

        public static float GetBottom(this IEnumerable<ITextElement> elements)
        {
            return elements.SelectMany(GetAllLocalPoints).Max(pos => pos.Y);
        }

        private static IEnumerable<Vector2> GetAllLocalPoints(ITextElement element)
        {
            var transformFinal = element.Position.Transform;

            var a = Vector2.Transform(new Vector2(0, 0), transformFinal);
            var b = Vector2.Transform(new Vector2(element.Width, 0), transformFinal);
            var c = Vector2.Transform(new Vector2(0, element.Height), transformFinal);
            var d = Vector2.Transform(new Vector2(element.Width, element.Height), transformFinal);

            return new[] { a, b, c, d };
        }
    }
}
