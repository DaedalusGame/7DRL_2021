using _7DRL_2021.Behaviors;
using _7DRL_2021.Drawables;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    class MapTile : ICurio, IDrawable, ITooltipProvider
    {
        public Guid GlobalID
        {
            get;
            set;
        }
        public bool Removed
        {
            get;
            set;
        }

        public Map Map;
        public int X, Y;

        public Vector2 VisualPosition => new Vector2(X * 16, Y * 16);
        public Vector2 VisualTarget => VisualPosition + new Vector2(8, 8);
        public IEnumerable<Curio> Contents => this.GetBehaviors<BehaviorOnTile>().Select(x => x.Curio);

        public IEnumerable<Behavior> Behaviors => this.GetBehaviors();

        public double DrawOrder => 0;

        public MapTile(Map map, int x, int y, Template template = null) : base()
        {
            Map = map;
            X = x;
            Y = y;
            this.Setup(template);
        }

        public MapTile GetNeighbor(int dx, int dy)
        {
            return Map.GetTile(X + dx, Y + dy);
        }

        public MapTile GetNeighborOrNull(int dx, int dy)
        {
            return Map.GetTileOrNull(X + dx, Y + dy);
        }

        public IEnumerable<MapTile> GetAdjacentNeighbors()
        {
            return new[] { GetNeighborOrNull(1, 0), GetNeighborOrNull(0, 1), GetNeighborOrNull(-1, 0), GetNeighborOrNull(0, -1) }.Where(x => x != null);
        }

        public IEnumerable<MapTile> GetAllNeighbors()
        {
            return new[] { GetNeighborOrNull(1, 0), GetNeighborOrNull(0, 1), GetNeighborOrNull(-1, 0), GetNeighborOrNull(0, -1), GetNeighborOrNull(1, 1), GetNeighborOrNull(-1, 1), GetNeighborOrNull(-1, -1), GetNeighborOrNull(1, -1) }.Where(x => x != null);
        }

        public IEnumerable<MapTile> GetNearby(int radius)
        {
            return Map.GetNearby(X, Y, radius);
        }

        public IEnumerable<MapTile> GetNearby(Rectangle rectangle, int radius)
        {
            return Map.GetNearby(rectangle, radius);
        }

        public void AddPrimary(Curio curio)
        {
            Add(curio);
        }

        public void Add(Curio curio)
        {
            Behavior.Apply(new BehaviorOnTile(this, curio));
        }

        public override string ToString()
        {
            return $"({X},{Y})";
        }

        public void AddTooltip(TextBuilder tooltip)
        {
            //NOOP
        }

        public bool ShouldDraw(SceneGame scene, Vector2 cameraPosition)
        {
            return Map == scene.Map;
        }

        public IEnumerable<DrawPass> GetDrawPasses()
        {
            return Drawable.Floor.GetDrawPasses();
        }

        public void DrawIcon(SceneGame scene, Vector2 pos)
        {
            Drawable.Floor.DrawIcon(this, scene, pos);
        }

        public void Draw(SceneGame scene, DrawPass pass)
        {
            Drawable.Floor.Draw(this, scene, pass);
        }
    }
}
