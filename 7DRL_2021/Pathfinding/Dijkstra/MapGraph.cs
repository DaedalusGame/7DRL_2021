using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Pathfinding
{
    class MapGraph : IWeightedGraph<Point>
    {
        private static readonly Point[] CompassDirs = {
            new Point( 1, 0 ),
            new Point( 0, -1 ),
            new Point( -1, 0 ),
            new Point( 0, 1 ),
            new Point( 1, -1 ),
            new Point( -1, -1 ),
            new Point( -1, 1 ),
            new Point( 1, 1 ),
        };

        public Map Map;
        int[,] CostMap;
        int[,] CurioMap;
        bool[,] BlockMap;
        List<Point> Curios = new List<Point>();
        
        public MapGraph(Map map)
        {
            Map = map;
            CurioMap = new int[Map.Width, Map.Height];
        }

        public int Cost(Point from, Point to)
        {
            return CurioMap[to.X, to.Y] + CostMap[to.X, to.Y] * (Math.Abs(to.X - from.X) + Math.Abs(to.Y - from.Y));
        }

        public IEnumerable<Point> GetNeighbors(Point node)
        {
            return CompassDirs.Select(dir => node + dir).Where(IsWithinMap).Where(pos => !BlockMap[pos.X, pos.Y]);
        }

        private bool IsWithinMap(Point pos)
        {
            return pos.X >= 0 && pos.Y >= 0 && pos.X < Map.Width && pos.Y < Map.Height;
        }

        public void Recalculate()
        {
            CostMap = new int[Map.Width, Map.Height];
            BlockMap = new bool[Map.Width, Map.Height];
            foreach (var tile in Map.EnumerateTiles())
            {
                CostMap[tile.X, tile.Y] = 1;
                if(tile.IsChasm() || tile.IsSolid())
                {
                    BlockMap[tile.X, tile.Y] = true;
                }
            }
        }

        public void RecalculateCurios(IEnumerable<ICurio> except)
        {
            foreach (var previous in Curios)
            {
                CurioMap[previous.X, previous.Y] = 0;
            }
            Curios.Clear();
            foreach (var curio in Map.Curios.Except(except))
            {
                var tile = curio.GetMainTile();
                CurioMap[tile.X, tile.Y] = 300;
                Curios.Add(new Point(tile.X, tile.Y));
            }
        }
    }
}
