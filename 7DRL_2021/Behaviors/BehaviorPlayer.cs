using _7DRL_2021.Results;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    class Momentum
    {
        public Vector2 Direction;
        public int Amount;

        public Momentum(Vector2 direction, int amount)
        {
            Direction = direction;
            Amount = amount;
        }
    }

    class BehaviorPlayer : Behavior, ITickable, IColored
    {
        Random Random = new Random();

        public ICurio Curio;
        public Momentum Momentum = new Momentum(Vector2.Zero, 0);

        public bool SwordReady => Curio.HasBehaviors<BehaviorSword>();
        public bool SwordStuck => Curio.GetBehavior<BehaviorSword>()?.StabTargets.Any() ?? false;
        public bool Gripped => Curio.GetBehavior<BehaviorGrapplingHook>()?.GripDirection != null;
        public LerpFloat Fade = new LerpFloat(0);
        public float Footstep;
        public int FootstepOffset = +1;

        static SoundReference SoundDeath = SoundLoader.AddSound("content/sound/kill.wav");

        public BehaviorPlayer()
        {
        }

        public BehaviorPlayer(ICurio curio)
        {
            Curio = curio;
        }

        public override void Apply()
        {
            Curio.AddBehavior(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            Apply(new BehaviorPlayer(mapper.Map(Curio)), Curio);
        }

        public void Tick(SceneGame scene)
        {
            Fade.Update();

            var tile = Curio.GetMainTile();
            var passive = Curio.GetActionHolder(ActionSlot.Passive);
            var active = Curio.GetActionHolder(ActionSlot.Active);
            var levelEnd = tile?.GetBehavior<BehaviorLevelEnd>();
            if (levelEnd != null && !scene.WaitForCutscene && passive.Done && active.Done && levelEnd.CanEscape() && Curio.IsAlive())
            {
                EndLevel(scene);
            }

            UpdateFootstep(scene);

            if (Curio.IsDead() && !scene.IsGameOver)
            {
                scene.GameOver("GLORY TO THE BLOOD GOD.", false);
                SoundDeath.Play(1, -0.5f, 0);
            }

            if (!scene.WaitForPlayer)
            {
                //MovePassive();
            }
            else if (Momentum.Amount <= 0 && SwordReady)
            {
                SheatheSword();
            }
        }

        private bool CanFootstep(IAction action)
        {
            return action is ActionAccelerate || action is ActionWaitForAction;
        }

        private void UpdateFootstep(SceneGame scene)
        {
            var active = Curio.GetActionHolder(ActionSlot.Active);
            var tile = Curio.GetMainTile();
            Footstep += scene.TimeMod;
            if (Momentum.Amount >= 32 && Footstep >= 4 && tile != null && !tile.IsChasm() && active.CurrentActions.Any(CanFootstep))
            {
                Footstep = Footstep % 4;
                var angle = Curio.GetVisualAngle();
                var offset = Util.AngleToVector(angle);
                var lateral = Util.AngleToVector(angle + MathHelper.PiOver2);
                var particle = new ExplosionParticle(scene, SpriteLoader.Instance.AddSprite("content/effect_moon"), Curio.GetVisualTarget() + FootstepOffset * lateral * 6 + offset * -12, 20)
                {
                    Color = Color.White,
                    DrawPass = DrawPass.EffectLowAdditive,
                };
                particle.Angle = angle + MathHelper.Pi;
                //new ScreenShakeRandom(scene, 0.5f, 5, LerpHelper.QuadraticIn);
                FootstepOffset *= -1;
            }
        }

        private void EndLevel(SceneGame scene)
        {
            Momentum.Amount = 0;
            if (Gripped || SwordReady)
            {
                SheatheSword();
                DashCancel(0);
            }
            else
            {
                scene.Cutscene = Scheduler.Instance.RunAndWait(scene.RoutineEndLevel());
            }
        }

        private bool IsAngleAlmostEqual(Vector2 a, Vector2 b)
        {
            return IsAngleAlmostEqual(Util.VectorToAngle(a), Util.VectorToAngle(b));
        }

        private bool IsAngleAlmostEqual(float a, float b)
        {
            return Math.Abs(Util.GetAngleDistance(a, b)) < 0.0001;
        }

        /*public void MovePassive()
        {
            var tile = Curio.GetMainTile();
            var active = Curio.GetActionHolder(ActionSlot.Active);
            var passive = Curio.GetActionHolder(ActionSlot.Passive);
            if (passive.Done)
            {
                if (Momentum.Amount > 0)
                {
                    var offset = Momentum.Direction.ToTileOffset();
                    var slide = MathHelper.Clamp(Momentum.Amount / 32f, 0, 1);
                    var speed = (float)LerpHelper.QuadraticOut(0.1f, 0.5f, slide);
                    var time = 1 / speed;

                    var neighbor = tile.GetNeighborOrNull(offset.X, offset.Y);
                    if (neighbor == null || neighbor.IsSolid())
                    {
                        if (Momentum.Amount > 10 || (neighbor.IsSpiky() && Momentum.Amount > 2))
                        {
                            var actions = new List<IAction>() { new ActionCollision(Curio, neighbor, 10) };
                            passive.Set(actions);
                        }
                        Momentum.Amount = 0;
                    }
                    else
                    {
                        var sword = Curio.GetBehavior<BehaviorSword>();
                        var orientable = Curio.GetBehavior<BehaviorOrientable>();
                        var actions = new List<IAction>() { new ActionMoveForward(Curio, Momentum.Direction, time) };
                        if (sword != null && active.Done && sword.Position == 0 && IsAngleAlmostEqual(Util.VectorToAngle(Momentum.Direction), orientable.Angle))
                            actions.Add(new ActionSwordStab(Curio, Momentum.Direction));

                        Momentum.Amount--;
                        passive.Set(actions);
                    }
                }
                else if(tile.HasBehaviors<BehaviorChasm>())
                {
                    var actions = new List<IAction>() { new ActionFall(Curio, 30) };
                    passive.Set(actions);
                }
            }
        }*/

        public void WaitForPassive()
        {
            var active = Curio.GetActionHolder(ActionSlot.Active);
            var passive = Curio.GetActionHolder(ActionSlot.Passive);

            if (SwordStuck)
                return;

            var actions = new List<ActionWrapper>();
            if (passive.Done)
            {
                AddDefaultMove(actions);
                AddDefaultStab(actions);
            }
            actions.Add(new ActionChangeMomentum(Curio, -1).InSlot(ActionSlot.Active));
            actions.Add(new ActionWaitForAction(Curio, ActionSlot.Passive).InSlot(ActionSlot.Active));
            actions.Apply(Curio);
        }

        public void AddDefaultMove(List<ActionWrapper> actions)
        {
            var tile = Curio.GetMainTile();
            if (tile == null)
                return;

            if (Momentum.Amount > 0)
            {
                var offset = Momentum.Direction.ToTileOffset();
                var slide = MathHelper.Clamp(Momentum.Amount / 32f, 0, 1);
                var speed = (float)LerpHelper.QuadraticOut(0.1f, 0.5f, slide);
                var time = 1 / speed;

                var neighbor = tile.GetNeighborOrNull(offset.X, offset.Y);
                if (neighbor == null || neighbor.IsSolid())
                {
                    if (neighbor != null && (Momentum.Amount > 10 || (neighbor.IsSpiky() && Momentum.Amount > 2)))
                    {
                        actions.Add(new ActionCollision(Curio, neighbor, 10).InSlot(ActionSlot.Passive));
                    }
                    actions.Add(new ActionStop(Curio).InSlot(ActionSlot.Passive));
                }
                else
                {
                    actions.Add(new ActionMoveForward(Curio, Momentum.Direction, time).InSlot(ActionSlot.Passive));
                }
            }
            else if (tile.HasBehaviors<BehaviorChasm>())
            {
                actions.Add(new ActionFall(Curio, 30).InSlot(ActionSlot.Passive));
            }
        }

        public void AddDefaultStab(List<ActionWrapper> actions)
        {
            var sword = Curio.GetBehavior<BehaviorSword>();
            var orientable = Curio.GetBehavior<BehaviorOrientable>();
            if (actions.Any(x => x.Action is ActionMoveForward) && sword != null && sword.Position == 0 && IsAngleAlmostEqual(Util.VectorToAngle(Momentum.Direction), orientable.Angle))
            {
                actions.Add(new ActionSwordStab(Curio, Momentum.Direction).InSlot(ActionSlot.Passive));
            }
        }

        public void Accelerate()
        {
            var active = Curio.GetActionHolder(ActionSlot.Active);
            var passive = Curio.GetActionHolder(ActionSlot.Passive);
            var orientable = Curio.GetBehavior<BehaviorOrientable>();
            var tile = Curio.GetMainTile();

            if (SwordStuck)
                return;

            var actions = new List<ActionWrapper>();
            if (!tile.IsChasm())
            {
                actions.Add(new ActionAccelerate(Curio, Util.AngleToVector(orientable.Angle), +1, 5).InSlot(ActionSlot.Active));
                actions.Add(new ActionKeepMoving(Curio, 0, true).InSlot(ActionSlot.Active));
                actions.Add(new ActionWaitForAction(Curio, ActionSlot.Passive).InSlot(ActionSlot.Active));
            }
            else
            {
                if (passive.Done)
                {
                    AddDefaultMove(actions);
                    actions.Add(new ActionChangeMomentum(Curio, -1).InSlot(ActionSlot.Active));
                }
                actions.Add(new ActionWaitForAction(Curio, ActionSlot.Passive).InSlot(ActionSlot.Active));
            }
            actions.Apply(Curio);
        }

        public void Decelerate()
        {
            var active = Curio.GetActionHolder(ActionSlot.Active);
            var passive = Curio.GetActionHolder(ActionSlot.Passive);
            var tile = Curio.GetMainTile();

            if (SwordStuck)
                return;

            var actions = new List<ActionWrapper>();
            if (!tile.IsChasm() && Momentum.Direction != Vector2.Zero)
            {
                actions.Add(new ActionDecelerate(Curio, -2, 5).InSlot(ActionSlot.Active));
                actions.Add(new ActionKeepMoving(Curio, 0, true).InSlot(ActionSlot.Active));
                actions.Add(new ActionWaitForAction(Curio, ActionSlot.Passive).InSlot(ActionSlot.Active));
            }
            actions.Apply(Curio);
        }

        public void Turn(float addAngle)
        {
            var active = Curio.GetActionHolder(ActionSlot.Active);
            var passive = Curio.GetActionHolder(ActionSlot.Passive);
            var orientable = Curio.GetBehavior<BehaviorOrientable>();
            var sword = Curio.GetBehavior<BehaviorSword>();
            var tile = Curio.GetMainTile();

            var actions = new List<ActionWrapper>();
            if (!tile.IsChasm())
            {
                actions.Add(new ActionClearStab(Curio).InSlot(ActionSlot.Active));
                if (Momentum.Amount > 2)
                {
                    //actions.Add(new ActionDecelerate(Curio, -1).InSlot(ActionSlot.Passive));
                    actions.Add(new ActionSetMomentum(Curio, Util.AngleToVector(orientable.Angle + addAngle)).InSlot(ActionSlot.Passive));
                }
                else
                {
                    actions.Add(new ActionStop(Curio).InSlot(ActionSlot.Passive));
                }
                //actions.Add(new ActionWaitForAction(Curio, ActionSlot.Passive).InSlot(ActionSlot.Active));
                actions.Add(new ActionTurn(Curio, addAngle, 5).InSlot(ActionSlot.Active));
                actions.Add(new ActionKeepMoving(Curio).InSlot(ActionSlot.Active));
            }
            actions.Apply(Curio);
        }

        public void TurnLeft()
        {
            Turn(-MathHelper.PiOver4);
        }

        public void TurnRight()
        {
            Turn(+MathHelper.PiOver4);
        }

        public void Steppy(float addAngle)
        {
            var active = Curio.GetActionHolder(ActionSlot.Active);
            var passive = Curio.GetActionHolder(ActionSlot.Passive);
            var orientable = Curio.GetBehavior<BehaviorOrientable>();
            var tile = Curio.GetMainTile();

            var actions = new List<ActionWrapper>();
            if (!tile.IsChasm())
            {
                if (Momentum.Amount >= 16)
                {
                    var direction = Util.RotateVector(Momentum.Direction, addAngle);
                    var offset = direction.ToTileOffset();
                    var slide = MathHelper.Clamp(Momentum.Amount / 32f, 0, 1);
                    var speed = (float)LerpHelper.QuadraticOut(0.1f, 0.5f, slide);
                    var time = 1 / speed;

                    var neighbor = tile.GetNeighborOrNull(offset.X, offset.Y);
                    if (neighbor != null && !neighbor.IsSolid())
                    {
                        actions.Add(new ActionClearStab(Curio).InSlot(ActionSlot.Active));
                        actions.Add(new ActionChangeMomentum(Curio, -4).InSlot(ActionSlot.Active));
                        actions.Add(new ActionMoveForward(Curio, direction, 5).InSlot(ActionSlot.Active));
                    }
                }
            }
            actions.Apply(Curio);
        }

        public void SlashSword(int targetPos)
        {
            var active = Curio.GetActionHolder(ActionSlot.Active);
            var sword = Curio.GetBehavior<BehaviorSword>();

            if (targetPos != sword.Position)
            {
                var actions = new List<ActionWrapper>();
                actions.Add(new ActionSwordSlash(Curio, sword.Position, targetPos, Math.Abs(targetPos - sword.Position) * 3).InSlot(ActionSlot.Active));
                actions.Add(new ActionKeepMoving(Curio).InSlot(ActionSlot.Active));
                actions.Apply(Curio);
            }
        }

        public void DrawSword(int position)
        {
            var active = Curio.GetActionHolder(ActionSlot.Active);

            if(!SwordReady && Momentum.Amount > 0)
            {
                var actions = new List<ActionWrapper>();
                actions.Add(new ActionSwordDraw(Curio, position, 10).InSlot(ActionSlot.Active));
                actions.Add(new ActionKeepMoving(Curio).InSlot(ActionSlot.Active));
                actions.Apply(Curio);
            }
        }

        public void SheatheSword()
        {
            var active = Curio.GetActionHolder(ActionSlot.Active);

            if(SwordReady)
            {
                var actions = new List<ActionWrapper>();
                actions.Add(new ActionSwordSheathe(Curio, 10).InSlot(ActionSlot.Active));
                actions.Add(new ActionKeepMoving(Curio).InSlot(ActionSlot.Active));
                actions.Apply(Curio);
            }
        }

        public void Grapple(int targetPos)
        {
            var angle = Curio.GetAngle() + targetPos * MathHelper.PiOver4;
            var offset = Util.AngleToVector(angle);

            var grappleTarget = Curio.GetGrappleTarget(offset);

            var actions = new List<ActionWrapper>();
            if (grappleTarget != null)
            {
                foreach (var grapple in grappleTarget.GetBehaviors<IGrappleTarget>())
                    grapple.AddGrappleAction(actions, Curio, offset);
            }
            else
            {
                actions.Add(new ActionGrappleNothing(Curio, GetFurthestGrapple(offset), offset, 10, 5).InSlot(ActionSlot.Active));
                actions.Add(new ActionKeepMoving(Curio).InSlot(ActionSlot.Active));
            }
            actions.Apply(Curio);
        }

        public ICurio GetGrappleTarget(Vector2 direction)
        {
            var tile = Curio.GetMainTile();
            var offset = Util.ToTileOffset(direction);
            var neighbor = tile;
            for(int i = 0; i < 10; i++)
            {
                neighbor = neighbor.GetNeighborOrNull(offset.X, offset.Y);
                if(neighbor == null)
                    break;
                var grappleTarget = neighbor.GetGrappleTarget();
                if (grappleTarget != null)
                    return grappleTarget;
            }
            return null;
        }

        public ICurio GetFurthestGrapple(Vector2 direction)
        {
            var tile = Curio.GetMainTile();
            var offset = Util.ToTileOffset(direction);
            var neighbor = tile;
            for (int i = 0; i < 10; i++)
            {
                var next = neighbor.GetNeighborOrNull(offset.X, offset.Y);
                if (next == null)
                    break;
                neighbor = next;
            }
            return neighbor;
        }

        public void Dash(int targetPos)
        {
            var angle = Curio.GetAngle() + targetPos * MathHelper.PiOver4;
            var actions = new List<ActionWrapper>();
            actions.Add(new ActionGripDash(Curio, Util.AngleToVector(angle), 15).InSlot(ActionSlot.Active));
            actions.Add(new ActionKeepMoving(Curio, 0, true).InSlot(ActionSlot.Active));
            actions.Apply(Curio);
        }

        public void DashCancel(int targetPos)
        {
            var angle = Curio.GetAngle() + targetPos * MathHelper.PiOver4;
            var actions = new List<ActionWrapper>();
            actions.Add(new ActionGripCancel(Curio, Util.AngleToVector(angle)).InSlot(ActionSlot.Active));
            actions.Apply(Curio);
        }

        public ColorMatrix GetColor()
        {
            return ColorMatrix.Lerp(ColorMatrix.Scale(0), ColorMatrix.Identity, Fade);
        }

        public double GetColorPriority()
        {
            return 30;
        }
    }
}
