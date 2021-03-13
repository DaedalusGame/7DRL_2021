using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    class Map
    {
        public SceneGame World;
        public int Width;
        public int Height;
        public MapTile[,] Tiles;
        public IEnumerable<Curio> Curios => Manager.GetCurios(this).OfType<Curio>();


        public Map(SceneGame world, int width, int height)
        {
            World = world;
            SetSize(width, height);
        }

        private void SetSize(int width, int height, int offsetx = 0, int offsety = 0)
        {
            var newTiles = new MapTile[width, height];

            MapTile getTile(int x, int y)
            {
                if (Tiles != null && InBounds(x, y))
                    return Tiles[x, y];
                else
                    return null;
            }


            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    MapTile tile = getTile(x - offsetx, y - offsety);
                    if (tile != null)
                    {
                        newTiles[x, y] = tile;
                        tile.X = x;
                        tile.Y = y; //I guess this might work for now.
                    }
                    else
                        newTiles[x, y] = new MapTile(this, x, y);

                }
            }

            Width = width;
            Height = height;
            Tiles = newTiles;
        }

        public MapTile GetTile(int x, int y)
        {
            if (InMap(x, y))
                return Tiles[x, y];
            else
                return Tiles[0, 0];
        }

        public MapTile GetTileOrNull(int x, int y)
        {
            if (InMap(x, y))
                return Tiles[x, y];
            else
                return null;
        }

        public bool CanSee(Vector2 start, Vector2 end)
        {
            for (float i = 0; i < Vector2.Distance(start / 16, end / 16); i += 0.5f)
            {
                var pos = Util.ToTile(start / 16 + Vector2.Normalize(end - start) * i);
                var tile = GetTileOrNull(pos.X, pos.Y);
                if (tile == null || tile.IsSolid())
                    return false;
            }
            return true;
        }

        public IEnumerable<MapTile> GetNearby(int x, int y, int radius)
        {
            for (int dx = MathHelper.Clamp(x - radius, 0, Width - 1); dx <= MathHelper.Clamp(x + radius, 0, Width - 1); dx++)
            {
                for (int dy = MathHelper.Clamp(y - radius, 0, Height - 1); dy <= MathHelper.Clamp(y + radius, 0, Height - 1); dy++)
                {
                    yield return GetTile(dx, dy);
                }
            }
        }

        public IEnumerable<MapTile> GetNearby(Rectangle rectangle, int radius)
        {
            for (int dx = MathHelper.Clamp(rectangle.Left - radius, 0, Width - 1); dx <= MathHelper.Clamp(rectangle.Right - 1 + radius, 0, Width - 1); dx++)
            {
                for (int dy = MathHelper.Clamp(rectangle.Top - radius, 0, Height - 1); dy <= MathHelper.Clamp(rectangle.Bottom - 1 + radius, 0, Height - 1); dy++)
                {
                    yield return GetTile(dx, dy);
                }
            }
        }

        public IEnumerable<MapTile> EnumerateTiles()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    yield return GetTile(x, y);
                }
            }
        }

        public bool InMap(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Width && y < Height;
        }

        public bool InBounds(int x, int y)
        {
            return x >= 1 && y >= 1 && x < Width - 1 && y < Height - 1;
        }
    }
}
