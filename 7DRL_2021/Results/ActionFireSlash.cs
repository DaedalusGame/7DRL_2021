using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionFireSlash : IActionHasOrigin
    {
        static Random Random = new Random();

        public bool Done => Slash.Done;
        public ICurio Origin { get; set; }

        public int SlashStart => Slash.SlashStart;
        public int SlashEnd => Slash.SlashEnd;
        public ActionSwordSlash Slash;

        public ActionFireSlash(ICurio origin, ActionSwordSlash slash)
        {
            Origin = origin;
            Slash = slash;
        }

        public void Run()
        {
            var world = Origin.GetWorld();
            var sword = Origin.GetBehavior<BehaviorSword>();
            var fire = Origin.GetBehavior<BehaviorSkillBloodfireBlade>();
            int slashDelta = Math.Abs(SlashEnd - SlashStart);

            fire.Extinguish();

            new TimeFade(world, 0.05f, LerpHelper.ExponentialIn, 40);

            for (float position = SlashStart; Math.Abs(position - SlashStart) <= slashDelta; position += Math.Sign(SlashEnd - SlashStart) * 0.33f)
            {
                var angle = Origin.GetAngle() + position * MathHelper.PiOver4;
                var particle = new ExplosionParticleAnchored(world, SpriteLoader.Instance.AddSprite("content/effect_explosion"), () => Origin.GetVisualTarget() + Util.AngleToVector(angle) * 16, Random.Next(5, 15))
                {
                    Angle = angle + Random.NextFloat(-0.3f, +0.3f),
                    Color = Color.White,
                    DrawPass = DrawPass.EffectAdditive,
                };
                particle.Size.Set(Random.NextFloat(1.0f, 1.5f));
            }

            Slash.Modifiers.Add(new ModifierBloodfire());
        }
    }
}
