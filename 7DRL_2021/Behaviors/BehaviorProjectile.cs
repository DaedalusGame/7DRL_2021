using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    class BehaviorProjectile : Behavior, ITickable
    {
        public ICurio Curio;
        public Vector2 Direction;
        public Slider MoveFrame;
        public Slider LifeTime;
        public ICurio Shooter;
        public LerpHelper.Delegate MoveLerp;

        public BehaviorProjectile()
        {
        }

        public BehaviorProjectile(ICurio curio, LerpHelper.Delegate moveLerp)
        {
            Curio = curio;
            MoveLerp = moveLerp;
        }

        public void Fire(ICurio shooter, Vector2 direction, float moveTime, float lifeTime)
        {
            Shooter = shooter;
            Direction = direction;
            MoveFrame = new Slider(moveTime, moveTime);
            LifeTime = new Slider(lifeTime);
            Curio.GetBehavior<BehaviorOrientable>().OrientTo(Util.VectorToAngle(Direction));
        }

        public void Tick(SceneGame scene)
        {
            var tile = Curio.GetMainTile();

            if (MoveFrame.Done)
            {
                MoveFrame = new Slider(MoveFrame.Time - MoveFrame.EndTime, MoveFrame.EndTime);
                var camera = Curio.GetCamera();
                
                var offset = Direction.ToTileOffset();
                var neighbor = tile.GetNeighborOrNull(offset.X, offset.Y);
                if (neighbor == null)
                {
                    //Out of bounds
                    Curio.Destroy();
                }
                else
                {
                    Curio.MoveTo(neighbor, MoveLerp, MoveFrame);
                    camera?.MoveTo(neighbor, LerpHelper.Linear, MoveFrame);
                }
            }

            if (LifeTime.Done)
            {
                //Fizzle
                Curio.Destroy();
            }

            LifeTime += scene.TimeMod;
            MoveFrame += scene.TimeMod;
        }

        public override void Apply()
        {
            Curio.AddBehavior(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorProjectile(curio, MoveLerp), Curio);
        }
    }
}
