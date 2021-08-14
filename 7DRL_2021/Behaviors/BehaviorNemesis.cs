using _7DRL_2021.Events;
using _7DRL_2021.Results;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    class BehaviorNemesis : Behavior, ITickable
    {
        static Random Random = new Random();

        public enum NemesisState
        {
            Forward,
            Back,
            Parry,
        }

        public ICurio Curio;

        public Slider ParryTimer = new Slider(25);
        public Slider ChaseTimer = new Slider(10);
        public NemesisState State;
        public LerpFloat WingsOpen = new LerpFloat(0);
        public LerpFloat ForwardBack = new LerpFloat(0);

        public BehaviorNemesis()
        {
        }

        public BehaviorNemesis(ICurio curio)
        {
            Curio = curio;
        }

        public override void Apply()
        {
            Curio.AddBehavior(this);
            EventBus.Register(this);
        }

        public override void Remove()
        {
            base.Remove();
            EventBus.Unregister(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorNemesis(curio), Curio);
        }

        public void Tick(SceneGame scene)
        {
            var active = Curio.GetActionHolder(ActionSlot.Active);
            var lastSeen = Curio.GetBehavior<BehaviorLastSeen>();
            var pathfinder = Curio.GetBehavior<BehaviorPathfinder>();
            var orientable = Curio.GetBehavior<BehaviorOrientable>();
            var alive = Curio.GetBehavior<BehaviorAlive>();
            var tile = Curio.GetMainTile();

            var enemyDistance = Vector2.Distance(scene.PlayerCurio.GetVisualTarget(), Curio.GetVisualTarget());
            var isEnemyFar = enemyDistance > 200;
            var isEnemyClose = enemyDistance < 100;
            var isEnemyArmed = scene.PlayerCurio.HasBehaviors<BehaviorSword>();
          

            WingsOpen.Update();
            ForwardBack.Update();

            if (active.Done && alive.CurrentDead)
            {
                var actions = new List<ActionWrapper>();
                actions.Add(new ActionNemesisRevive(Curio, 120).InSlot(ActionSlot.Active));
                actions.Apply(Curio);
                if (!actions.Any(x => x.Action is ActionNemesisRevive))
                    scene.GameOver(GameOverType.NemesisKill);
            }

            if (active.Done && Curio.IsAlive())
            {
                if (ForwardBack.End > 0 && State != NemesisState.Forward)
                    ForwardBack.Set(0, LerpHelper.Quadratic, 30);
                if (ForwardBack.End < 1 && State == NemesisState.Forward)
                    ForwardBack.Set(1, LerpHelper.Quadratic, 30);

                if (State == NemesisState.Back)
                {
                    if (isEnemyArmed)
                    {
                        ParryTimer += scene.TimeModCurrent;
                        if (ParryTimer.Done)
                        {
                            State = NemesisState.Parry;
                            ParryTimer.Time = 0;
                        }
                    }
                    if(isEnemyFar)
                    {
                        ChaseTimer += scene.TimeModCurrent;
                        if (ChaseTimer.Done)
                        {
                            State = NemesisState.Forward;
                            ChaseTimer.Time = 0;
                        }
                    }
                }
                if (State == NemesisState.Parry)
                {
                    if (!isEnemyArmed || isEnemyFar)
                    {
                        ParryTimer += scene.TimeModCurrent;
                        if (ParryTimer.Done)
                        {
                            State = NemesisState.Back;
                            ParryTimer.Time = 0;
                        }
                    }
                }
                if (State == NemesisState.Forward)
                {
                    if (isEnemyClose)
                        State = NemesisState.Back;

                    ChaseTimer += 1;

                    if (lastSeen.LastSeenTile != null)
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
                            if (ChaseTimer.Done)
                            {
                                var randomPath = pathfinder.Path.Pick(Random);
                                var randomPathTile = Curio.GetMap().GetTileOrNull(randomPath.X, randomPath.Y);
                                if (randomPathTile != tile)
                                {
                                    var dist = Vector2.Distance(randomPathTile.VisualTarget, Curio.GetVisualTarget());
                                    movement.Add(new ActionMoveNemesis(Curio, scene.PlayerCurio, randomPathTile, dist / 8f + 10).InSlot(ActionSlot.Active));
                                    ChaseTimer.Time = 0;
                                }
                            }
                            else if (Math.Abs(angleDelta) < 0.0001)
                            {
                                movement.Add(new ActionMoveForward(Curio, nextMove.Value.ToVector2(), 5).InSlot(ActionSlot.Active));
                            }
                            else if (angleDelta < 0)
                            {
                                movement.Add(new ActionTurn(Curio, -MathHelper.PiOver4, 3).InSlot(ActionSlot.Active));
                            }
                            else if (angleDelta > 0)
                            {
                                movement.Add(new ActionTurn(Curio, MathHelper.PiOver4, 3).InSlot(ActionSlot.Active));
                            }
                            movement.Apply(Curio);
                        }
                    }
                }
            }
        }

        [EventSubscribe(100)]
        public void OnAction(EventAction e)
        {
            var hits = e.Actions.GetEffectsTargetting<IWeaponHit>(Curio).ToList();
            var alive = Curio.GetBehavior<BehaviorAlive>();

            foreach (var hit in hits)
            {
                if (hit is ActionEnemyHit)
                    continue;

                if (WingsOpen > 0)
                {
                    e.Actions.Clear();
                }
                if (State == NemesisState.Parry)
                {
                    e.Actions.Clear();
                    e.Actions.Add(new ActionParrySword(hit.Origin, hit.Target).InSlot(ActionSlot.Active));
                }
            }

            e.Actions.RemoveAll(x => x.Action is ActionGib gib && gib.Target == Curio);
        }

        [EventSubscribe]
        public void OnSeen(EventSeen e)
        {
            var lastSeen = Curio.GetBehavior<BehaviorLastSeen>();
            lastSeen.LastSeenTile = e.SeenPosition;
        }
    }
}
