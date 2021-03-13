using _7DRL_2021.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    class BehaviorMovable : Behavior
    {
        public Curio Curio;
        public Func<Vector2> VisualPosition = () => Vector2.Zero;

        public MapTile Tile => Curio.GetMainTile();
        public Mask Mask;

        public BehaviorMovable()
        {
        }

        public BehaviorMovable(Curio curio, Mask mask)
        {
            Curio = curio;
            Mask = mask;
        }

        IEnumerable<Point> Zero = new Point[] { Point.Zero };

        private void SetMask(MapTile primary)
        {
            primary.AddPrimary(Curio);
            foreach (var point in Mask.Except(Zero))
            {
                MapTile neighbor = primary.GetNeighbor(point.X, point.Y);
                if (neighbor != null)
                    neighbor.Add(Curio);
            }
        }

        private void UnsetMask()
        {
            foreach (var effect in Curio.GetBehaviors<BehaviorOnTile>())
                effect.Remove();
        }

        public void Resize()
        {
            MapTile tile = Tile;
            UnsetMask();
            SetMask(tile);
        }

        public void MoveVisual(Vector2 newPosition, LerpHelper.Delegate lerp, ISlider slider)
        {
            var oldPosition = VisualPosition();
            VisualPosition = () =>
            {
                float slide = MathHelper.Clamp(slider.Slide, 0, 1);
                return Vector2.Lerp(oldPosition, newPosition, (float)lerp(0, 1, slide));
            };
        }

        public void MoveVisual(MapTile tile, LerpHelper.Delegate lerp, ISlider slider)
        {
            MoveVisual(new Vector2(tile.X, tile.Y) * 16, lerp, slider);
        }

        public void TeleportVisual(Vector2 newPosition)
        {
            VisualPosition = () => newPosition;
        }

        public void TeleportVisual(MapTile tile)
        {
            TeleportVisual(new Vector2(tile.X, tile.Y) * 16);
        }

        public void MoveTo(MapTile tile)
        {
            MoveTo(tile, null, null);
        }

        public void MoveTo(MapTile tile, LerpHelper.Delegate lerp, ISlider slider)
        {
            MapTile previousTile = Tile;
            EventBus.PushEvent(new EventMove(Curio, previousTile, tile));
            UnsetMask();
            if (tile != null)
            {
                SetMask(tile);
                if (slider == null)
                    TeleportVisual(tile);
                else
                    MoveVisual(tile, lerp, slider);
            }
            EventBus.PushEvent(new EventMove.Finish(Curio, previousTile, tile));
        }

        public override void Apply()
        {
            Curio.AddBehavior(this);
            var pos = Curio.GetBehavior<BehaviorOnTile>();
            if (pos != null)
            {
                TeleportVisual(pos.MapTile);
            }
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = (Curio)mapper.Map(Curio);
            Apply(new BehaviorMovable(curio, Mask), Curio);
        }
    }
}
