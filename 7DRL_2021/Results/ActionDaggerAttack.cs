using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionDaggerAttack : IActionHasOrigin, ITickable, IDrawable
    {
        public bool Done => Frame.Done;
        public ICurio Origin { get; set; }

        private Point Offset;
        private Slider Frame;
        private float TimeUpswing;

        public double DrawOrder => 0;

        public Color ColorStart => Color.IndianRed;
        public Color ColorEnd => Color.Orange;

        public ActionDaggerAttack(ICurio origin, Point offset, float timeUpswing, float time)
        {
            Origin = origin;
            Offset = offset;
            TimeUpswing = timeUpswing;
            Frame = new Slider(time);
        }

        public void Run()
        {
            var world = Origin.GetWorld();
            var dagger = Origin.GetBehavior<BehaviorDagger>();
            var target = GetTarget();
            dagger.Upswing = new Slider(TimeUpswing);
        }

        private MapTile GetTarget()
        {
            var tile = Origin.GetMainTile();
            var neighbor = tile?.GetNeighborOrNull(Offset.X, Offset.Y);
            return neighbor;
        }

        private void Hit(ICurio target)
        {
            var world = Origin.GetWorld();
            var actions = new List<ActionWrapper>();
            actions.Add(new ActionEnemyHit(Origin, target, SoundLoader.AddSound("content/sound/stab.wav")).InSlot(ActionSlot.Active));
            actions.Apply(target);
            /*target.GetFlashHelper()?.AddFlash(ColorMatrix.Flat(Color.White), 20);
            target.GetShakeHelper()?.AddShakeRandom(3, LerpHelper.QuadraticOut, 30);
            new TimeFade(world, 0, LerpHelper.QuadraticIn, 50);
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
                /*new Wave(world, 10)
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
                };*/

                foreach (var target in targetTile.Contents)
                {
                    Hit(target);
                }
            }
        }

        public void Tick(SceneGame scene)
        {
            var tile = Origin.GetMainTile();
            var dagger = Origin.GetBehavior<BehaviorDagger>();
            if (dagger.Upswing.Done)
            {
                bool shouldAttack = !Frame.Done;
                Frame += scene.TimeModCurrent;
                if (Frame.Done && shouldAttack)
                {
                    DamageArea();
                }
            }
        }

        public bool ShouldDraw(SceneGame scene, Vector2 cameraPosition)
        {
            return scene.Map == Origin.GetMap();
        }

        public IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass.EffectAdditive;
            yield return DrawPass.EffectLowAdditive;
        }

        public void Draw(SceneGame scene, DrawPass pass)
        {
            if (Origin.IsDeadOrDestroyed())
                return;
            var tile = Origin.GetMainTile();
            var target = GetTarget();
            var dagger = Origin.GetBehavior<BehaviorDagger>();
            if (pass == DrawPass.EffectLowAdditive)
            {
                SkillUtil.DrawArea(scene, new[] { target }, ColorStart, ColorEnd, dagger.Upswing.Slide);
                SkillUtil.DrawImpact(scene, target, ColorStart, ColorEnd, dagger.Upswing.Slide);
            }
            if (pass == DrawPass.EffectAdditive)
            {
                SkillUtil.DrawImpactLine(scene, AoEVisual.GetStraight(Origin.GetVisualTarget(), target.GetVisualTarget()), ColorStart, ColorEnd, dagger.Upswing.Slide);
                if(dagger.Upswing.Done && !Frame.Done)
                    SkillUtil.DrawStrike(scene, Origin.GetVisualTarget(), target.GetVisualTarget(), Frame.Slide, Color.White);
            }
        }

        public void DrawIcon(SceneGame scene, Vector2 pos)
        {
            throw new NotImplementedException();
        }
    }
}
