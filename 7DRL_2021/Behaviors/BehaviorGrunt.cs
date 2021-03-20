using _7DRL_2021.Results;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    class BehaviorGrunt : Behavior, ITickable
    {
        public ICurio Curio;

        public BehaviorGrunt()
        {
        }

        public BehaviorGrunt(ICurio curio)
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
            Apply(new BehaviorGrunt(curio), Curio);
        }

        public void Tick(SceneGame scene)
        {
            var active = Curio.GetActionHolder(ActionSlot.Active);
            var lastSeen = Curio.GetBehavior<BehaviorLastSeen>();
            var pathfinder = Curio.GetBehavior<BehaviorPathfinder>();
            var orientable = Curio.GetBehavior<BehaviorOrientable>();
            var tile = Curio.GetMainTile();

            if (active.Done && Curio.IsAlive())
            {

                var delta = scene.PlayerCurio.GetVisualTarget() - Curio.GetVisualTarget();
                var distance = delta.Length();
                if (distance < 24f)
                {
                    var angleCurrent = orientable.Angle;
                    var angleTarget = Enumerable.Range(0, 8).Select(x => MathHelper.PiOver4 * x).GetClosestAngle(Util.VectorToAngle(delta));
                    var angleDelta = MathHelper.WrapAngle(angleTarget - angleCurrent);
                    var attack = new List<ActionWrapper>()
                    {
                        new ActionDaggerAttack(Curio, Util.ToTileOffset(Util.AngleToVector(angleTarget)), 5f, 2.5f).InSlot(ActionSlot.Active),
                        new ActionTurn(Curio, angleDelta, 3).InSlot(ActionSlot.Active),
                    };
                    attack.Apply(Curio);
                }
                else if (lastSeen.LastSeenTile != null)
                {
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
                            movement.Add(new ActionMoveForward(Curio, nextMove.Value.ToVector2(), 10).InSlot(ActionSlot.Active));
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
                }
            }
        }
    }
}
