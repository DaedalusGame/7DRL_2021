using _7DRL_2021.Results;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    class BehaviorGruntGeneric : Behavior, ITickable
    {
        public ICurio Curio;

        public float MoveTime;
        public float TurnTime;

        Random Random = new Random();

        public BehaviorGruntGeneric()
        {
        }

        public BehaviorGruntGeneric(ICurio curio, float moveTime, float turnTime)
        {
            Curio = curio;
            MoveTime = moveTime;
            TurnTime = turnTime;
        }

        public override void Apply()
        {
            Curio.AddBehavior(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorGruntGeneric(curio, MoveTime, TurnTime), Curio);
        }

        public void Tick(SceneGame scene)
        {
            var active = Curio.GetActionHolder(ActionSlot.Active);
            var lastSeen = Curio.GetBehavior<BehaviorLastSeen>();

            if (active?.Done ?? true && Curio.IsAlive())
            {
                bool specialAction = PerformSpecialAction();
                
                if (!specialAction && lastSeen.LastSeenTile != null)
                {
                    PerformMovement(lastSeen);
                }
            }
        }

        bool PerformSpecialAction()
        {
            var world = Curio.GetWorld();
            var orientable = Curio.GetBehavior<BehaviorOrientable>();
            var delta = world.PlayerCurio.GetVisualTarget() - Curio.GetVisualTarget();
            var distance = delta.Length();

            var attacks = Curio.GetBehaviors<IGruntAttack>().Shuffle(Random).OrderByDescending(x => x.Priority);

            foreach (var attack in attacks)
            {
                if (attack.CanUse())
                {
                    attack.Perform();
                    return true;
                }
            }

            return false;
        }

        bool PerformMovement(BehaviorLastSeen lastSeen)
        {
            var pathfinder = Curio.GetBehavior<BehaviorPathfinder>();
            var orientable = Curio.GetBehavior<BehaviorOrientable>();
            var tile = Curio.GetMainTile();

            var nextMove = pathfinder.GetNextMove(new Point(tile.X, tile.Y));
            if (lastSeen.LastSeenTile != pathfinder.GetDestination() || !nextMove.HasValue)
                pathfinder.FindPath(lastSeen.LastSeenTile);
            if (nextMove.HasValue)
            {
                var angleCurrent = orientable.Angle;
                var angleTarget = Util.PointToAngle(nextMove.Value);
                var angleDelta = MathHelper.WrapAngle(angleTarget - angleCurrent);

                var movement = new List<ActionWrapper>();
                if (Math.Abs(angleDelta) < 0.0001)
                {
                    movement.Add(new ActionMoveForward(Curio, nextMove.Value.ToVector2(), MoveTime).InSlot(ActionSlot.Active));
                }
                else if (angleDelta < 0)
                {
                    movement.Add(new ActionTurn(Curio, -MathHelper.PiOver4, TurnTime).InSlot(ActionSlot.Active));
                }
                else if (angleDelta > 0)
                {
                    movement.Add(new ActionTurn(Curio, MathHelper.PiOver4, TurnTime).InSlot(ActionSlot.Active));
                }
                movement.Apply(Curio);
                return true;
            }

            return false;
        }
    }

    interface IGruntAttack
    {
        float Priority { get; }

        bool CanUse();

        void Perform();
    }

    abstract class BehaviorGruntAttack : Behavior, IGruntAttack
    {
        public ICurio Curio;
        public float Priority { get; set; }

        public BehaviorGruntAttack()
        {
        }

        public BehaviorGruntAttack(ICurio curio, float priority)
        {
            Curio = curio;
            Priority = priority;
        }

        public override void Apply()
        {
            Curio.AddBehavior(this);
        }

        public abstract bool CanUse();

        public abstract void Perform();
    }

    class BehaviorKnifeAttack : BehaviorGruntAttack
    {
        public float TurnTime;
        public float UpSwingTime;
        public float DownSwingTime;

        public BehaviorKnifeAttack() : base()
        {
        }

        public BehaviorKnifeAttack(ICurio curio, float priority, float turnTime, float upSwingTime, float downSwingTime) : base(curio, priority)
        {
            TurnTime = turnTime;
            UpSwingTime = upSwingTime;
            DownSwingTime = downSwingTime;
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorKnifeAttack(curio, Priority, TurnTime, UpSwingTime, DownSwingTime), Curio);
        }

        public override bool CanUse()
        {
            var world = Curio.GetWorld();
            var delta = world.PlayerCurio.GetVisualTarget() - Curio.GetVisualTarget();
            var distance = delta.Length();

            return distance < 24f;
        }

        public override void Perform()
        {
            var world = Curio.GetWorld();
            var delta = world.PlayerCurio.GetVisualTarget() - Curio.GetVisualTarget();
            var orientable = Curio.GetBehavior<BehaviorOrientable>();
            var angleCurrent = orientable.Angle;
            var angleTarget = Enumerable.Range(0, 8).Select(x => MathHelper.PiOver4 * x).GetClosestAngle(Util.VectorToAngle(delta));
            var angleDelta = MathHelper.WrapAngle(angleTarget - angleCurrent);
            var attack = new List<ActionWrapper>()
            {
                new ActionDaggerAttack(Curio, Util.ToTileOffset(Util.AngleToVector(angleTarget)), UpSwingTime, DownSwingTime).InSlot(ActionSlot.Active),
                new ActionTurn(Curio, angleDelta, TurnTime).InSlot(ActionSlot.Active),
            };
            attack.Apply(Curio);
        }
    }

    class BehaviorMaceAttack : BehaviorGruntAttack
    {
        public float TurnTime;
        public float UpSwingTime;
        public float DownSwingTime;

        public BehaviorMaceAttack() : base()
        {
        }

        public BehaviorMaceAttack(ICurio curio, float priority, float turnTime, float upSwingTime, float downSwingTime) : base(curio, priority)
        {
            TurnTime = turnTime;
            UpSwingTime = upSwingTime;
            DownSwingTime = downSwingTime;
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorMaceAttack(curio, Priority, TurnTime, UpSwingTime, DownSwingTime), Curio);
        }

        public override bool CanUse()
        {
            var world = Curio.GetWorld();
            var tile = Curio.GetMainTile();
            var map = tile.Map;
            var mace = Curio.GetBehavior<BehaviorMace>();

            return mace != null && mace.IsInArea(world.PlayerCurio) && map.CanSee(Curio.GetVisualTarget(), world.PlayerCurio.GetVisualTarget());
        }

        public override void Perform()
        {
            var world = Curio.GetWorld();
            var delta = world.PlayerCurio.GetVisualTarget() - Curio.GetVisualTarget();
            var orientable = Curio.GetBehavior<BehaviorOrientable>();
            var angleCurrent = orientable.Angle;
            var angleTarget = Enumerable.Range(0, 8).Select(x => MathHelper.PiOver4 * x).GetClosestAngle(Util.VectorToAngle(delta));
            var angleDelta = MathHelper.WrapAngle(angleTarget - angleCurrent);
            var attack = new List<ActionWrapper>()
            {
                new ActionMaceAttack(Curio, world.PlayerCurio, UpSwingTime, DownSwingTime).InSlot(ActionSlot.Active),
                new ActionTurn(Curio, angleDelta, TurnTime).InSlot(ActionSlot.Active),
            };
            attack.Apply(Curio);
        }
    }

    class BehaviorMaceGoreAttack : BehaviorGruntAttack
    {
        public float TurnTime;
        public float UpSwingTime;
        public float UpSwingSlashTime;
        public float DownSwingTime;

        public BehaviorMaceGoreAttack() : base()
        {
        }

        public BehaviorMaceGoreAttack(ICurio curio, float priority, float turnTime, float upSwingTime, float upSwingSlashTime, float downSwingTime) : base(curio, priority)
        {
            TurnTime = turnTime;
            UpSwingTime = upSwingTime;
            UpSwingSlashTime = upSwingSlashTime;
            DownSwingTime = downSwingTime;
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorMaceGoreAttack(curio, Priority, TurnTime, UpSwingTime, UpSwingSlashTime, DownSwingTime), Curio);
        }

        public override bool CanUse()
        {
            var world = Curio.GetWorld();
            var tile = Curio.GetMainTile();
            var map = tile.Map;
            var mace = Curio.GetBehavior<BehaviorMace>();

            return mace != null && mace.IsInArea(world.PlayerCurio) && map.CanSee(Curio.GetVisualTarget(), world.PlayerCurio.GetVisualTarget());
        }

        public override void Perform()
        {
            var world = Curio.GetWorld();
            var delta = world.PlayerCurio.GetVisualTarget() - Curio.GetVisualTarget();
            var orientable = Curio.GetBehavior<BehaviorOrientable>();
            var angleCurrent = orientable.Angle;
            var angleTarget = Enumerable.Range(0, 8).Select(x => MathHelper.PiOver4 * x).GetClosestAngle(Util.VectorToAngle(delta));
            var angleDelta = MathHelper.WrapAngle(angleTarget - angleCurrent);
            var attack = new List<ActionWrapper>()
            {
                new ActionMaceGoreAttack(Curio, world.PlayerCurio, UpSwingTime, UpSwingSlashTime, DownSwingTime).InSlot(ActionSlot.Active),
                new ActionTurn(Curio, angleDelta, TurnTime).InSlot(ActionSlot.Active),
            };
            attack.Apply(Curio);
        }
    }
}
