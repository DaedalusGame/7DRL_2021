using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionMaceAttack : IActionHasOrigin, ITickable, ISlider
    {
        public bool Done => IsDone();
        public ICurio Origin { get; set; }

        public ICurio Target;
        private Point Offset;
        private Slider Frame;
        private float TimeUpswing;
        public float Slide => Frame.Slide;
        public AoEVisual VisualAoE;
        public Strike VisualStrike;
        

        public ActionMaceAttack(ICurio origin, ICurio target, float timeUpswing, float time)
        {
            Origin = origin;
            Target = target;
            TimeUpswing = timeUpswing;
            Frame = new Slider(time);
        }

        private bool IsDone()
        {
            var mace = Origin.GetBehavior<BehaviorMace>();
            return mace.MaceReturn.Done && Frame.Done;
        }

        public void Run()
        {
            var world = Origin.GetWorld();
            var mace = Origin.GetBehavior<BehaviorMace>();
            var target = GetTarget();
            mace.Upswing = new Slider(TimeUpswing);
            VisualAoE = new AoEVisual(world, Origin.GetVisualTarget());
            VisualAoE.Set(target);
            Retarget();
        }

        private MapTile GetTarget()
        {
            var tile = Origin.GetMainTile();
            var neighbor = tile?.GetNeighborOrNull(Offset.X, Offset.Y);
            return neighbor;
        }

        private void Retarget()
        {
            var origin = Origin.GetMainTile();
            var target = Target.GetMainTile();
            var mace = Origin.GetBehavior<BehaviorMace>();
            var map = origin.Map;
            if(origin != null && target != null && mace != null && mace.IsInArea(Target) && map.CanSee(Origin.GetVisualTarget(), Target.GetVisualTarget()))
            {
                Offset = new Point(target.X - origin.X, target.Y - origin.Y);
                VisualAoE.Set(target);
            }
        }

        private void Hit(ICurio target)
        {
            var actions = new List<ActionWrapper>();
            actions.Add(new ActionEnemyHit(Origin, target).InSlot(ActionSlot.Active));
            actions.Apply(target);
            /*var world = Origin.GetWorld();
            target.GetFlashHelper()?.AddFlash(ColorMatrix.Flat(Color.White), 20);
            target.GetShakeHelper()?.AddShakeRandom(3, LerpHelper.QuadraticOut, 30);
            //new TimeFade(world, 0, LerpHelper.QuadraticIn, 50);
            var alive = target.GetBehavior<BehaviorAlive>();
            if (alive != null)
            {
                alive.TakeDamage(1);
            }*/
        }

        private void DamageArea()
        {
            var world = Origin.GetWorld();
            var targetTile = GetTarget();
            if (targetTile != null)
            {
                new Wave(world, 10)
                {
                    WaveSprite = SpriteLoader.Instance.AddSprite("content/ring_spark_thin"),
                    Position = targetTile.VisualTarget,
                    Radius = 15,
                    Precision = 20,
                    StartRadius = 0.3f,
                    Thickness = 0.5f,
                    DrawPass = DrawPass.EffectLowAdditive,
                    InnerLerp = LerpHelper.QuadraticOut,
                    OuterLerp = LerpHelper.QuadraticOut,
                };

                foreach (var target in targetTile.Contents)
                {
                    Hit(target);
                }
            }
        }

        public void Tick(SceneGame scene)
        {
            var tile = Origin.GetMainTile();
            var mace = Origin.GetBehavior<BehaviorMace>();
            if (mace.Upswing.Done)
            {
                if(VisualStrike == null)
                {
                    var neighbor = tile.GetNeighborOrNull(Offset.X, Offset.Y);
                    if (neighbor != null)
                    {
                        VisualStrike = new Strike(scene, Origin.GetVisualTarget(), neighbor.VisualTarget, this);
                    }
                }
                bool shouldAttack = !Frame.Done;
                Frame += scene.TimeMod;
                if (Frame.Done && shouldAttack)
                {
                    var target = GetTarget();
                    mace.Mace(target.VisualTarget, 20);
                    DamageArea();
                    VisualAoE.Destroy();
                }
            }
            else
            {
                Retarget();
            }
        }
    }
}
