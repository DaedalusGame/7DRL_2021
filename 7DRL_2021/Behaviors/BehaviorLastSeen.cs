using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _7DRL_2021.Events;
using Microsoft.Xna.Framework;

namespace _7DRL_2021.Behaviors
{
    class BehaviorLastSeen : Behavior, IDrawable, ITickable
    {
        public ICurio Curio;
        public MapTile LastSeenTile;
        public LerpVector2 LastSeenPos = new LerpVector2(Vector2.Zero);

        public double DrawOrder => 0;

        public BehaviorLastSeen()
        {
        }

        public BehaviorLastSeen(ICurio curio)
        {
            Curio = curio;
        }
        
        public override void Apply()
        {
            Curio.AddBehavior(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorLastSeen(curio), Curio);
        }

        public bool CanSee(Vector2 start, Vector2 end)
        {
            var map = Curio.GetMap();
            return map?.CanSee(start,end) ?? false;
        }

        public bool CanSee(ICurio curio)
        {
            return CanSee(Curio.GetVisualTarget(), curio.GetVisualTarget());
        }

        public void Tick(SceneGame scene)
        {
            var tile = scene.PlayerCurio?.GetMainTile();

            if (tile != null)
            {
                var start = Curio.GetVisualTarget();
                var end = scene.PlayerCurio.GetVisualTarget();
                var maxDistance = 16 * 16;

                if (Vector2.DistanceSquared(start, end) < maxDistance * maxDistance && CanSee(start, end) && LastSeenTile != tile)
                {
                    LastSeenTile = tile;
                    EventBus.PushEvent(new EventSeen(Curio, tile));
                }
            }
            
            LastSeenPos.Update();
        }

        public void Draw(SceneGame scene, DrawPass pass)
        {
            
        }

        public void DrawIcon(SceneGame scene, Vector2 pos)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass.EffectLowAdditive;
        }

        public bool ShouldDraw(SceneGame scene, Vector2 cameraPosition)
        {
            return true;
        }
    }
}
