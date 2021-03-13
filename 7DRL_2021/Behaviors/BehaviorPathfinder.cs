using _7DRL_2021.Pathfinding;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    class BehaviorPathfinder : Behavior, IDrawable, ITickable
    {
        public ICurio Curio;
        public MapGraph Graph;
        public List<Point> Path;
        public bool HasPath => Path != null;
        public Slider Cooldown = new Slider(60);

        public double DrawOrder => 0;

        public BehaviorPathfinder()
        {
        }

        public BehaviorPathfinder(ICurio curio)
        {
            Curio = curio;
        }

        private void Setup()
        {
            var map = Curio.GetMap();
            if(Graph?.Map != map)
            {
                Graph = new MapGraph(map);
                Graph.Recalculate();
            }
            Graph.RecalculateCurios(new[] { Curio });
        }

        public MapTile GetDestination()
        {
            var map = Curio.GetMap();
            if(map != null && Path != null && Path.Any())
            {
                var pos = Path.Last();
                return map.GetTileOrNull(pos.X, pos.Y);
            }
            return null;
        }

        public void FindPath(MapTile destination)
        {
            if (!Cooldown.Done)
                return;
            MapTile source = Curio.GetMainTile();
            if(source != null)
            {
                Setup();
                var path = WeightedPathfinder.Search(Graph, new Point(source.X, source.Y), new Point(destination.X, destination.Y));
                if (path != null)
                    Path = path;
                Cooldown.Time = 0;
            }
        }

        public int GetPathIndex(Point point)
        {
            return Path?.FindIndex(x => point == x) ?? -1;
        }

        public Point? GetNextMove(Point point)
        {
            int index = GetPathIndex(point);
            if (Path == null || index < 0 || index >= Path.Count - 1)
                return null;
            return Path[index + 1] - point;
        }

        public MapTile GetNextTile(Point point)
        {
            var move = GetNextMove(point);
            var tile = Curio.GetMainTile();
            if (tile != null && move != null)
                return tile.GetNeighborOrNull(move.Value.X, move.Value.Y);
            return null;
        }

        public override void Apply()
        {
            Curio.AddBehavior(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorPathfinder(curio));
        }

        public void Tick(SceneGame scene)
        {
            Cooldown += 1;
        }

        public bool ShouldDraw(SceneGame scene)
        {
            return true;
        }

        public IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass.EffectLow;
        }

        public void Draw(SceneGame scene, DrawPass pass)
        {
            /*if(HasPath)
            {
                var sprite = SpriteLoader.Instance.AddSprite("content/heart");
                foreach(var pos in Path)
                {
                    scene.DrawSprite(sprite, 0, new Vector2(pos.X * 16, pos.Y * 16), SpriteEffects.None, 0);
                }
            }*/
        }

        public void DrawIcon(SceneGame scene, Vector2 pos)
        {
            throw new NotImplementedException();
        }
    }
}
