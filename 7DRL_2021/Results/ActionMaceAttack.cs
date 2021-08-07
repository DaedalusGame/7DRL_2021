﻿using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionMaceAttack : IActionHasOrigin, ITickable, IDrawable
    {
        static Random Random = new Random();

        public bool Done => IsDone();
        public ICurio Origin { get; set; }

        public ICurio Target;
        private Point Offset;
        private Slider Frame;
        private float TimeUpswing;
        public bool Swinging;

        public double DrawOrder => 0;

        public static SoundReference SoundSwish = SoundLoader.AddSound("content/sound/swish.wav");
        public static SoundReference SoundImpact = SoundLoader.AddSound("content/sound/wallkick.wav");

        public Color ColorStart => Color.IndianRed;
        public Color ColorEnd => Color.Orange;

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
            }
        }

        private void Hit(ICurio target)
        {
            var actions = new List<ActionWrapper>();
            actions.Add(new ActionEnemyHit(Origin, target, SoundLoader.AddSound("content/sound/hit.wav")).InSlot(ActionSlot.Active));
            actions.Apply(target);
        }

        private void DamageArea()
        {
            var world = Origin.GetWorld();
            var targetTile = GetTarget();
            if (targetTile != null)
            {
                Wave wave = new Wave(world, SpriteLoader.Instance.AddSprite("content/ring_spark_thin"), 10)
                {
                    Radius = 15,
                    Precision = 20,
                    StartRadius = 0.3f,
                    Thickness = 0.5f,
                    InnerLerp = LerpHelper.QuadraticOut,
                    OuterLerp = LerpHelper.QuadraticOut,
                };
                new StaticEffect(world, wave, targetTile.VisualTarget)
                {
                    DrawPass = DrawPass.EffectLowAdditive,
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
                if(!Swinging)
                {
                    SoundSwish.Play(1.0f, Random.NextFloat(-1.0f, 0f), 0);
                    Swinging = true;
                }
                bool shouldAttack = !Frame.Done;
                Frame += scene.TimeModCurrent;
                if (Frame.Done && shouldAttack)
                {
                    var target = GetTarget();
                    mace.Mace(target.VisualTarget, 20);
                    SoundImpact.Play(1, Random.NextFloat(-0.5f, +0.5f), 0);
                    DamageArea();
                }
            }
            else
            {
                Retarget();
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
            var mace = Origin.GetBehavior<BehaviorMace>();
            if (pass == DrawPass.EffectLowAdditive)
            {
                SkillUtil.DrawArea(scene, new[] { target }, ColorStart, ColorEnd, mace.Upswing.Slide);
                SkillUtil.DrawImpact(scene, target, ColorStart, ColorEnd, mace.Upswing.Slide);
            }
            if (pass == DrawPass.EffectAdditive)
            {
                SkillUtil.DrawImpactLine(scene, AoEVisual.GetStraight(Origin.GetVisualTarget(), target.GetVisualTarget()), ColorStart, ColorEnd, mace.Upswing.Slide);
                if(mace.Upswing.Done && !Frame.Done)
                    SkillUtil.DrawStrike(scene, Origin.GetVisualTarget(), target.GetVisualTarget(), Frame.Slide, Color.White);
            }
        }

        public void DrawIcon(SceneGame scene, Vector2 pos)
        {
            throw new NotImplementedException();
        }
    }
}
