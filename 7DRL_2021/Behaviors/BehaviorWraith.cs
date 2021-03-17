using _7DRL_2021.Results;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    abstract class BehaviorArc : Behavior, ITickable, IDrawable
    {
        public ICurio Curio;
        public Slider Frame;
        public Vector2 Source;

        public abstract Color ColorStart { get; } 
        public abstract Color ColorEnd { get; }

        public double DrawOrder => 0;

        public BehaviorArc()
        {
        }

        public BehaviorArc(ICurio curio, Vector2 source, float time)
        {
            Curio = curio;
            Source = source;
            Frame = new Slider(time);
        }

        public abstract void DrawArcObject(SceneGame scene, DrawPass drawPass);

        public abstract IEnumerable<MapTile> GetImpactArea();

        public abstract void Impact();

        public override void Apply()
        {
            Curio.AddBehavior(this);
        }

        public void Tick(SceneGame scene)
        {
            Frame += scene.TimeMod;
            if(Frame.Done)
            {
                Impact();
                Remove();
            }
        }

        public bool ShouldDraw(SceneGame scene)
        {
            return scene.Map == Curio.GetMap();
        }

        public IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass.Effect;
            yield return DrawPass.EffectAdditive;
            yield return DrawPass.EffectLowAdditive;
        }

        public void Draw(SceneGame scene, DrawPass pass)
        {
            var tile = Curio.GetMainTile();
            Color colorStart = ColorStart.WithAlpha(Frame.Slide);
            Color colorEnd = ColorStart.WithAlpha(Frame.Slide);
            if (pass == DrawPass.EffectLowAdditive)
            {
                SkillUtil.DrawArea(scene, GetImpactArea(), colorStart, colorEnd);
                SkillUtil.DrawImpact(scene, tile, ColorStart, ColorEnd);
            }
            DrawArcObject(scene, pass);
        }

        public void DrawIcon(SceneGame scene, Vector2 pos)
        {
            throw new NotImplementedException();
        }
    }

    class BehaviorWraith : BehaviorArc
    {
        static Random Random = new Random();

        Vector2 VelocityStart;
        Vector2 VelocityEnd;

        public override Color ColorStart => Color.IndianRed;
        public override Color ColorEnd => Color.Orange;

        public BehaviorWraith()
        {
        }

        public BehaviorWraith(ICurio curio, Vector2 source, float time) : base(curio, source, time)
        {
            VelocityStart = Util.AngleToVector(Random.NextAngle()) * Random.NextFloat(40, 80);
            VelocityEnd = Util.AngleToVector(Random.NextAngle()) * Random.NextFloat(40, 80);
        }

        public override void Clone(ICurioMapper mapper)
        {
            Apply(new BehaviorWraith(mapper.Map(Curio), Source, Frame.EndTime));
        }

        public override void DrawArcObject(SceneGame scene, DrawPass pass)
        {
            var spriteWraith = SpriteLoader.Instance.AddSprite("content/wraith");
            var spriteWraithTrail = SpriteLoader.Instance.AddSprite("content/wraith_trail");
            var tile = Curio.GetMainTile();
            var startPos = Source;
            var endPos = tile.VisualTarget;

            Vector2 curve(float slide)
            {
                var a = Vector2.Lerp(startPos, startPos + VelocityStart, (float)LerpHelper.Quadratic(0, 1, slide));
                var b = Vector2.Lerp(endPos + VelocityEnd, endPos, (float)LerpHelper.Quadratic(0, 1, slide));
                return Vector2.Lerp(a, b, (float)LerpHelper.Quadratic(0, 1, slide));
            }

            Color colorStart = new Color(215, 63, 36);
            Color colorEnd = new Color(118, 39, 102);
            if (pass == DrawPass.Effect)
            {
                int segments = 5;
                for (int i = 0; i < segments; i++)
                {
                    float trailSlide = (float)i / (segments - 1);
                    float trailPos = (float)LerpHelper.QuadraticOut(Frame.Slide - 0.05f, Frame.Slide, trailSlide);
                    Vector2 pos = curve(trailPos);
                    scene.DrawSpriteExt(spriteWraithTrail, 0, pos - spriteWraithTrail.Middle, spriteWraithTrail.Middle, 0, Vector2.One, SpriteEffects.None, Color.Lerp(colorStart.WithAlpha(0), colorEnd, trailSlide), 0);
                }
            }
            if (pass == DrawPass.EffectAdditive)
            {
                scene.DrawSpriteExt(spriteWraith, 0, curve(Frame.Slide) - spriteWraith.Middle, spriteWraith.Middle, 0, Vector2.One, SpriteEffects.None, colorStart, 0);
            }
        }

        public override IEnumerable<MapTile> GetImpactArea()
        {
            var tile = Curio.GetMainTile();
            return tile.GetNearby(1);
        }

        public override void Impact()
        {
           
            foreach (var target in GetImpactArea().SelectMany(x => x.Contents))
            {
                var actions = new List<ActionWrapper>();
                actions.Add(new ActionEnemyHit(Curio, target, SoundLoader.AddSound("content/sound/sinister.wav")).InSlot(ActionSlot.Active));
                actions.Apply(target);
            }
            Curio.Destroy();
        }
    }
}
