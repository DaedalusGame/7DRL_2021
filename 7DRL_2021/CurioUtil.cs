using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    static class CurioUtil
    {
        public static MapTile GetMainTile(this ICurio curio)
        {
            if (curio is MapTile tile)
                return tile;
            var tiles = Manager.GetBehaviors<BehaviorOnTile>(curio);
            if (tiles.Any())
                return tiles.First().MapTile;
            return null;
        }

        public static IEnumerable<MapTile> GetTiles(this ICurio curio)
        {
            if (curio is MapTile tile)
                return new[] { tile };
            var tiles = Manager.GetBehaviors<BehaviorOnTile>(curio);
            return tiles.Select(x => x.MapTile);
        }

        public static float GetAngle(this ICurio curio)
        {
            var orientable = curio.GetBehavior<BehaviorOrientable>();
            if (orientable != null)
                return orientable.Angle;
            return 0;
        }

        public static bool IsSolid(this ICurio curio)
        {
            return curio.HasBehaviors<BehaviorSolid>();
        }

        public static bool IsChasm(this ICurio curio)
        {
            return curio.HasBehaviors<BehaviorChasm>();
        }

        public static bool IsSpiky(this ICurio curio)
        {
            return curio.HasBehaviors<BehaviorSpiky>();
        }

        public static bool IsHeartless(this ICurio curio)
        {
            return curio.HasBehaviors<BehaviorHeartless>();
        }

        public static bool IsGrappleTarget(this ICurio curio)
        {
            return curio.HasBehaviors<IGrappleTarget>();
        }

        public static ICurio GetGrappleTarget(this MapTile tile)
        {
            var x = tile.Contents.FirstOrDefault(IsGrappleTarget);
            if (x != null)
                return x;
            if (tile.IsGrappleTarget())
                return tile;
            return null;
        }

        public static IEnumerable<IDrawable> GetDrawables()
        {
            var behaviors = Manager.GetBehaviors().OfType<IDrawable>();
            return behaviors;
        }

        public static IEnumerable<IDrawable> GetDrawables(this ICurio curio)
        {
            var behaviors = Manager.GetBehaviors(curio).OfType<IDrawable>();
            if (behaviors.Any())
                return behaviors;
            if (curio is IDrawable drawable)
                return new[] { drawable };
            return Enumerable.Empty<IDrawable>();
        }
        
        public static IEnumerable<IPreDrawable> GetPreDrawables(this ICurio curio)
        {
            var behaviors = Manager.GetBehaviors(curio).OfType<IPreDrawable>();
            if (behaviors.Any())
                return behaviors;
            return Enumerable.Empty<IPreDrawable>();
        }

        public static void DrawIcon(this ICurio curio, SceneGame scene, Vector2 pos)
        {
            var behavior = curio.GetBehavior<BehaviorDrawable>();
            if (behavior != null)
                behavior.DrawIcon(scene, pos);
            else if (curio is IDrawable drawable)
                drawable.DrawIcon(scene, pos);
        }

        public static Vector2 GetVisualPosition(this ICurio curio)
        {
            var movable = curio.GetBehavior<BehaviorMovable>();
            if (movable != null)
                return movable.VisualPosition();
            MapTile tile = curio.GetMainTile();
            return tile?.VisualPosition ?? Vector2.Zero;
        }

        public static Vector2 GetVisualTarget(this ICurio curio)
        {
            var movable = curio.GetBehavior<BehaviorMovable>();
            if (movable != null)
                return movable.VisualPosition() + new Vector2(8, 8);
            MapTile tile = curio.GetMainTile();
            return tile?.VisualTarget ?? Vector2.Zero;
        }

        public static float GetVisualAngle(this ICurio curio)
        {
            var orientable = curio.GetBehavior<BehaviorOrientable>();
            if (orientable != null)
                return orientable.VisualAngle();
            return 0;
        }

        public static FlashHelper GetFlashHelper(this ICurio curio)
        {
            var drawable = curio.GetBehavior<BehaviorDrawable>();
            if (drawable != null)
                return drawable.FlashHelper;
            return null;
        }

        public static ShakeHelper GetShakeHelper(this ICurio curio)
        {
            var drawable = curio.GetBehavior<BehaviorDrawable>();
            if (drawable != null)
                return drawable.ShakeHelper;
            return null;
        }

        public static Mask GetMask(this ICurio curio)
        {
            var movable = curio.GetBehavior<BehaviorMovable>();
            if (movable != null)
            {
                return movable.Mask;
            }
            return null;
        }

        public static bool MoveTo(this ICurio curio, MapTile tile)
        {
            return MoveTo(curio, tile, null, null);
        }

        public static bool MoveTo(this ICurio curio, MapTile tile, LerpHelper.Delegate lerp, ISlider slider)
        {
            var movable = curio.GetBehavior<BehaviorMovable>();
            if (movable != null)
            {
                movable.MoveTo(tile, lerp, slider);
                return true;
            }
            return false;
        }

        public static bool MoveVisual(this ICurio curio, Vector2 newPosition, LerpHelper.Delegate lerp, ISlider slider)
        {
            var movable = curio.GetBehavior<BehaviorMovable>();
            if (movable != null)
            {
                movable.MoveVisual(newPosition, lerp, slider);
                return true;
            }
            return false;
        }

        public static bool TeleportVisual(this ICurio curio, Vector2 newPosition)
        {
            var movable = curio.GetBehavior<BehaviorMovable>();
            if (movable != null)
            {
                movable.TeleportVisual(newPosition);
                return true;
            }
            return false;
        }

        public static ColorMatrix GetColor(this ICurio curio)
        {
            return curio.GetBehaviors().OfType<IColored>().OrderBy(x => x.GetColorPriority()).Aggregate(ColorMatrix.Identity, (x, y) => x * y.GetColor());
        }

        public static Vector2 GetOffset(this ICurio curio)
        {
            return curio.GetBehaviors().OfType<IOffseted>().OrderBy(x => x.GetOffsetPriority()).Aggregate(Vector2.Zero, (x, y) => x + y.GetOffset());
        }

        public static SceneGame GetWorld(this ICurio curio)
        {
            MapTile tile = curio.GetMainTile();
            if (tile != null)
                return tile.Map.World;
            return null; //TODO: some curios might not be tilebound
        }

        public static BehaviorActionHolder GetActionHolder(this ICurio curio, ActionSlot type)
        {
            return curio.GetBehaviors<BehaviorActionHolder>().FirstOrDefault(x => x.Type == type);
        }

        public static bool IsAlive(this ICurio curio)
        {
            var alive = curio.GetBehavior<BehaviorAlive>();
            if (alive != null)
                return !alive.CurrentDead;
            return false;
        }

        public static bool IsDead(this ICurio curio)
        {
            var alive = curio.GetBehavior<BehaviorAlive>();
            if (alive != null)
                return alive.CurrentDead;
            return false;
        }

        public static bool IsDeadOrDestroyed(this ICurio curio)
        {
            return curio.Removed || curio.IsDead();
        }

        public static ICurio GetCamera(this ICurio curio)
        {
            var cameraFollow = curio.GetBehavior<BehaviorFollowCamera>();
            if (cameraFollow != null)
                return cameraFollow.Camera;
            return null;
        }
    }
}
