using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionSwordSlash : IActionHasOrigin, ITickable, ISlider
    {
        static Random Random = new Random();

        public bool Done => Frame.Done;
        public ICurio Origin { get; set; }

        public int SlashStart, SlashEnd;
        private Slider Frame;
        public float Slide => Frame.Slide;

        List<ICurio> AlreadyHit = new List<ICurio>();

        public static SoundReference Sound = SoundLoader.AddSound("content/sound/swish.wav");

        public ActionSwordSlash(ICurio origin, int slashStart, int slashEnd, float time)
        {
            Origin = origin;
            SlashStart = slashStart;
            SlashEnd = slashEnd;
            Frame = new Slider(time);
        }

        public void Run()
        {
            var world = Origin.GetWorld();
            var sword = Origin.GetBehavior<BehaviorSword>();
            sword.SetPositionVisual(SlashStart);
            sword.OrientTo(SlashEnd, LerpHelper.QuarticOut, this);
            var slash = new Slash(world, Origin.GetVisualTarget, Origin.GetVisualAngle, 16);
            slash.Perform(BehaviorSword.GetAngle(SlashStart), BehaviorSword.GetAngle(SlashEnd), 16, LerpHelper.QuadraticOut, this);
            new ScreenShakeJerk(world, Util.AngleToVector(Origin.GetVisualAngle() + BehaviorSword.GetAngle(SlashEnd)) * 3, 10);
            Sound.Play(1.0f, Random.NextFloat(-0.5f, +0.5f), 0);
            RipHeart(world, sword);
            DamageArea();
        }

        private void RipHeart(SceneGame world, BehaviorSword sword)
        {
            foreach (var stabTarget in sword.StabTargets.Where(x => !x.Removed))
            {
                var actions = new List<ActionWrapper>();
                actions.Add(new ActionRipHeartSword(Origin, stabTarget).InSlot(ActionSlot.Active));
                actions.Apply(stabTarget);
            }
            sword.StabTargets.Clear();
        }

        private void Hit(ICurio target)
        {
            AlreadyHit.Add(target);
            var actions = new List<ActionWrapper>();
            actions.Add(new ActionSlashHit(Origin, target, SlashStart, SlashEnd).InSlot(ActionSlot.Active));
            actions.Apply(target);
            /*Random random = new Random();
            var world = Origin.GetWorld();
            target.GetFlashHelper()?.AddFlash(ColorMatrix.Flat(Color.White), 20);
            target.GetShakeHelper()?.AddShakeRandom(3, LerpHelper.QuadraticOut, 30);
            new TimeFade(world, 0, LerpHelper.QuadraticIn, 50);
            for(int i = 0; i < 3; i++)
            {
                Vector2 offset = Util.AngleToVector(random.NextAngle()) * random.NextFloat(16, 48);
                var blood = SpriteLoader.Instance.AddSprite("content/effect_blood_large");
                new BloodStain(world, blood, random.Next(1000), target.GetVisualTarget() + offset, random.NextFloat(0.5f, 2.0f), random.NextAngle(), 8000);
            }
            world.AddWorldScore(1000, target.GetVisualTarget(), ScoreType.Small);
            var alive = target.GetBehavior<BehaviorAlive>();
            alive.TakeDamage(1);*/
        }

        private void DamageArea()
        {
            var tile = Origin.GetMainTile();
            var areaPositions = Enumerable.Range(Math.Min(SlashStart, SlashEnd), Math.Abs(SlashEnd - SlashStart));
            foreach(var position in areaPositions)
            {
                var angle = Origin.GetAngle() + position * MathHelper.PiOver4;
                var offset = Util.AngleToVector(angle).ToTileOffset();
                var neighbor = tile.GetNeighborOrNull(offset.X, offset.Y);
                if(neighbor != null)
                {
                    foreach(var target in neighbor.Contents.Except(AlreadyHit))
                    {
                        Hit(target);
                    }
                }
            }
        }

        public void Tick(SceneGame scene)
        {
            Frame += scene.TimeMod;
            DamageArea();
        }
    }
}
