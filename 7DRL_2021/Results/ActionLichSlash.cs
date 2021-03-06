using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionLichSlash : IActionHasOrigin, ITickable, IDrawable
    {
        static Random Random = new Random();

        public ICurio Origin { get; set; }
        public float StartAngle;
        public float EndAngle;
        public Slider StartTime;
        public Slider SlashTime;
        public Slider EndTime;
        public List<ICurio> AlreadyHit = new List<ICurio>();

        public bool Done => EndTime.Done;

        public double DrawOrder => 0;

        public Color ColorStart => Color.IndianRed;
        public Color ColorEnd => Color.Orange;

        public ActionLichSlash(ICurio origin, float startAngle, float endAngle, float startTime, float slashTime, float endTime)
        {
            Origin = origin;
            StartAngle = startAngle;
            EndAngle = endAngle;
            StartTime = new Slider(startTime);
            SlashTime = new Slider(slashTime);
            EndTime = new Slider(endTime);
        }

        public void Run()
        {
            var world = Origin.GetWorld();
            var lich = Origin.GetBehavior<BehaviorLich>();
            lich.SwordAngle.Set(StartAngle);
            lich.SwordScale.Set(0, 1, LerpHelper.QuadraticIn, StartTime.EndTime);
        }

        private void PerformSlash(BehaviorLich lich)
        {
            foreach (var neighbor in lich.GetImpactArea())
            {
                foreach (var target in neighbor.Contents.Except(AlreadyHit))
                {
                    Hit(target);
                }
            }
        }

        private void SummonWraiths(SceneGame world, BehaviorLich lich)
        {
            var target = world.PlayerCurio;
            for (int i = 0; i < 4; i++)
            {
                var targetTile = target.GetMainTile().GetNearby(5).ToList().Pick(Random);

                var wraith = new Curio(Template.Wraith);
                wraith.MoveTo(targetTile);
                Behavior.Apply(new BehaviorWraith(wraith, Origin.GetVisualTarget() + Util.AngleToVector(lich.SwordAngle) * 8 * i, Random.NextFloat(30, 60)));
            }
        }

        private void Hit(ICurio target)
        {
            AlreadyHit.Add(target);
            var actions = new List<ActionWrapper>();
            actions.Add(new ActionEnemyHit(Origin, target, SoundLoader.AddSound("content/sound/hit.wav")).InSlot(ActionSlot.Active));
            actions.Apply(target);
        }

        public void Tick(SceneGame world)
        {
            var lich = Origin.GetBehavior<BehaviorLich>();

            if(!StartTime.Done)
            {
                StartTime += world.TimeModCurrent;
                if(StartTime.Done)
                {
                    lich.SwordAngle.Set(StartAngle, EndAngle, LerpHelper.Quartic, SlashTime.EndTime);
                }
                return;
            }

            if (!SlashTime.Done)
            {
                PerformSlash(lich);
                SlashTime += world.TimeModCurrent;
                if (SlashTime.Done)
                {
                    lich.SwordScale.Set(1, 0, LerpHelper.QuadraticOut, EndTime.EndTime);
                    SummonWraiths(world, lich);
                }
                return;
            }

            if (!EndTime.Done)
            {
                EndTime += world.TimeModCurrent;
            }
        }

        public bool ShouldDraw(SceneGame scene, Vector2 cameraPosition)
        {
            return scene.Map == Origin.GetMap();
        }

        public IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass.EffectLowAdditive;
        }

        public void Draw(SceneGame scene, DrawPass pass)
        {
            var lich = Origin.GetBehavior<BehaviorLich>();

            if (!SlashTime.Done)
            {
                SkillUtil.DrawArea(scene, lich.GetImpactArea(), ColorStart, ColorEnd, StartTime.Slide);
            }
        }

        public void DrawIcon(SceneGame scene, Vector2 pos)
        {
            throw new NotImplementedException();
        }
    }
}
