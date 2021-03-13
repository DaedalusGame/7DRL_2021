using _7DRL_2021.Results;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    class BehaviorRat : Behavior, ITickable
    {
        static Random Random = new Random();
        
        public ICurio Curio;
        public Slider Decay = new Slider(10);

        public BehaviorRat()
        {
        }

        public BehaviorRat(ICurio curio)
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
            Apply(new BehaviorRat(curio));
        }

        public void Tick(SceneGame scene)
        {
            var active = Curio.GetActionHolder(ActionSlot.Active);
            var lastSeen = Curio.GetBehavior<BehaviorLastSeen>();
            var pathfinder = Curio.GetBehavior<BehaviorPathfinder>();
            var orientable = Curio.GetBehavior<BehaviorOrientable>();
            var tile = Curio.GetMainTile();
            var map = Curio.GetMap();

            if (Curio.IsDead())
                Decay += scene.TimeMod;

            if (Decay.Done)
            {
                var actions = new List<ActionWrapper>();
                actions.Add(new ActionRatGib(scene.PlayerCurio, Curio, 2000).InSlot(ActionSlot.Active));
                actions.Apply(Curio);
            }

            if (active.Done && Curio.IsAlive())
            {
                if (lastSeen.LastSeenTile != null)
                {
                    var nextMove = pathfinder.GetNextMove(new Point(tile.X, tile.Y));
                    if (!pathfinder.HasPath)
                    {
                        var positions = map.EnumerateTiles().Shuffle(Random);
                        foreach (var escapeTile in positions)
                        {
                            if (Vector2.Distance(escapeTile.VisualTarget, tile.VisualTarget) < 300)
                                continue;
                            if (escapeTile.IsChasm() || escapeTile.IsSolid())
                                continue;
                            pathfinder.FindPath(escapeTile);
                            break;
                        }
                    }
                    else if (nextMove.HasValue)
                    {
                        var angleCurrent = orientable.Angle;
                        var angleTarget = Util.PointToAngle(nextMove.Value);
                        var angleDelta = MathHelper.WrapAngle(angleTarget - angleCurrent);

                        var movement = new List<ActionWrapper>();
                        if (Math.Abs(angleDelta) < 0.0001)
                        {
                            movement.Add(new ActionMoveForward(Curio, nextMove.Value.ToVector2(), 4).InSlot(ActionSlot.Active));
                        }
                        else if (angleDelta < 0)
                        {
                            movement.Add(new ActionTurn(Curio, -MathHelper.PiOver4, 5).InSlot(ActionSlot.Active));
                        }
                        else if (angleDelta > 0)
                        {
                            movement.Add(new ActionTurn(Curio, MathHelper.PiOver4, 5).InSlot(ActionSlot.Active));
                        }
                        movement.Apply(Curio);
                    }
                    else
                    {
                        var actions = new List<ActionWrapper>();
                        actions.Add(new ActionRatGib(scene.PlayerCurio, Curio, 2000).InSlot(ActionSlot.Active));
                        actions.Apply(Curio);
                    }
                }
            }
        }
    }
}
