using _7DRL_2021.Results;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    class BehaviorLich : Behavior, ITickable
    {
        public ICurio Curio;

        public LerpFloat SwordAngle = new LerpFloat(0);
        public LerpFloat SwordScale = new LerpFloat(0);

        public BehaviorLich()
        {
        }

        public BehaviorLich(ICurio curio)
        {
            Curio = curio;
        }

        public IEnumerable<MapTile> GetImpactArea()
        {
            var tile = Curio.GetMainTile();
            List<Point> offsets = new List<Point>();
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = Util.AngleToVector(SwordAngle) * (i + 1) * 0.5f;
                offsets.Add(offset.ToTileOffset());
            }
            return offsets.Distinct().Select(o => tile.GetNeighborOrNull(o.X, o.Y)).NonNull();
        }

        public override void Apply()
        {
            Curio.AddBehavior(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorLich(curio));
        }

        public void Tick(SceneGame scene)
        {
            var active = Curio.GetActionHolder(ActionSlot.Active);
            var lastSeen = Curio.GetBehavior<BehaviorLastSeen>();
            var pathfinder = Curio.GetBehavior<BehaviorPathfinder>();
            var orientable = Curio.GetBehavior<BehaviorOrientable>();
            var tile = Curio.GetMainTile();

            SwordAngle.Update(scene.TimeMod);
            SwordScale.Update(scene.TimeMod);

            if (active.Done && Curio.IsAlive())
            {
                var delta = scene.PlayerCurio.GetVisualTarget() - Curio.GetVisualTarget();
                var distance = delta.Length();
                if (distance < 48f)
                {
                    var angleCurrent = orientable.Angle;
                    var angleTarget = Enumerable.Range(0, 8).Select(x => MathHelper.PiOver4 * x).GetClosestAngle(Util.VectorToAngle(delta));
                    var angleDelta = MathHelper.WrapAngle(angleTarget - angleCurrent);
                    var attack = new List<ActionWrapper>()
                    {
                        new ActionLichSlash(Curio, angleTarget - MathHelper.PiOver4 * 3, angleTarget + MathHelper.PiOver4 * 3, 10, 60, 10).InSlot(ActionSlot.Active),
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
