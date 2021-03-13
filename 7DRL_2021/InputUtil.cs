using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    static class InputUtil
    {
        public static Dictionary<Keys, Point> NumpadKeys = new Dictionary<Keys, Point>()
        {
            { Keys.NumPad1, new Point(-1, +1) },
            { Keys.NumPad2, new Point(0, +1) },
            { Keys.NumPad3, new Point(+1, +1) },
            { Keys.NumPad4, new Point(-1, 0) },
            //Numpad5 is no direction
            { Keys.NumPad6, new Point(+1, 0) },
            { Keys.NumPad7, new Point(-1, -1) },
            { Keys.NumPad8, new Point(0, -1) },
            { Keys.NumPad9, new Point(+1, -1) },
        };
        public static Dictionary<Keys, Point> WASDKeys = new Dictionary<Keys, Point>()
        {
            { Keys.W, new Point(0, -1) },
            { Keys.A, new Point(-1, 0) },
            { Keys.S, new Point(0, +1) },
            { Keys.D, new Point(+1, 0) },
        };
        public static Dictionary<Keys, Point> ArrowKeys = new Dictionary<Keys, Point>()
        {
            { Keys.Up, new Point(0, -1) },
            { Keys.Left, new Point(-1, 0) },
            { Keys.Down, new Point(0, +1) },
            { Keys.Right, new Point(+1, 0) },
        };
        public static Dictionary<Keys, int> NumberKeys = new Dictionary<Keys, int>()
        {
            { Keys.D0, 0 },
            { Keys.D1, 1 },
            { Keys.D2, 2 },
            { Keys.D3, 3 },
            { Keys.D4, 4 },
            { Keys.D5, 5 },
            { Keys.D6, 6 },
            { Keys.D7, 7 },
            { Keys.D8, 8 },
            { Keys.D9, 9 },
        };

        public static Point? GetDirectionNumpadDown(InputTwinState state)
        {
            var result = NumpadKeys.Where(x => state.IsKeyDown(x.Key)).Select(x => x.Value);
            if (result.Count() == 1)
                return result.Single();
            return null;
        }

        public static Point? GetDirectionNumpadPressed(InputTwinState state, int repeatStart, int repeatStep)
        {
            var result = NumpadKeys.Where(x => state.IsKeyPressed(x.Key, repeatStart, repeatStep)).Select(x => x.Value);
            if (result.Count() == 1)
                return result.Single();
            return null;
        }

        public static Point? GetDirectionDown(this IDictionary<Keys, Point> map, InputTwinState state)
        {
            var shift = state.IsKeyDown(Keys.LeftShift);
            var result = map.Where(x => state.IsKeyDown(x.Key)).Select(x => x.Value);
            if (result.Count() < 2 && shift)
                return null;
            var direction = result.Aggregate(Point.Zero, (a, b) => a + b);
            if (direction == Point.Zero)
                return null;
            return direction;
        }

        public static Point? GetDirectionPressed(this IDictionary<Keys, Point> map, InputTwinState state, int repeatStart, int repeatStep)
        {
            if (map.Any(x => state.IsKeyPressed(x.Key, repeatStart, repeatStep)))
                return GetDirectionDown(map, state);
            return null;
        }

        public static int? GetNumberDown(this IDictionary<Keys, int> map, InputTwinState state)
        {
            var result = map.Where(x => state.IsKeyDown(x.Key)).Select(x => x.Value);
            if (result.Count() == 1)
                return result.Single();
            return null;
        }

        public static int? GetNumberPressed(this IDictionary<Keys, int> map, InputTwinState state)
        {
            var result = map.Where(x => state.IsKeyPressed(x.Key)).Select(x => x.Value);
            if (result.Count() == 1)
                return result.Single();
            return null;
        }

        public static int? GetNumberPressed(this IDictionary<Keys, int> map, InputTwinState state, int repeatStart, int repeatStep)
        {
            var result = map.Where(x => state.IsKeyPressed(x.Key, repeatStart, repeatStep)).Select(x => x.Value);
            if (result.Count() == 1)
                return result.Single();
            return null;
        }
    }
}
