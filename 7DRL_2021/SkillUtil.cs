using _7DRL_2021.Behaviors;
using _7DRL_2021.Events;
using _7DRL_2021.Results;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    static class SkillUtil
    {
        public static ActionWrapper InSlot(this IAction action, ActionSlot slot)
        {
            return new ActionWrapper(action, slot);
        }

        public static void Apply(this List<ActionWrapper> actions, ICurio curio)
        {
            EventBus.PushEvent(new EventAction(actions));
            var slots = actions.Select(x => x.Slot).Distinct().ToDictionary(x => x, x => curio.GetActionHolder(x));
            foreach (var slot in slots)
                slot.Value?.Cleanup();
            foreach (var wrapper in actions)
            {
                var slot = slots.GetOrDefault(wrapper.Slot, null);
                var action = wrapper.Action;
                action.Run();
                slot?.Add(action);
            }
        }

        public static IEnumerable<T> GetEffectsTargetting<T>(this List<ActionWrapper> actions, ICurio target) where T : IActionHasTarget
        {
            return actions.Select(x => x.Action).OfType<T>().Where(x => x.Target == target);
        }

        public static IEnumerable<T> GetEffectsBy<T>(this List<ActionWrapper> actions, ICurio origin) where T : IActionHasOrigin
        {
            return actions.Select(x => x.Action).OfType<T>().Where(x => x.Origin == origin);
        }

        public static void CreateBloodCircle(SceneGame world, Vector2 position, int particles, float radius, Random random)
        {
            var bloodSmall = SpriteLoader.Instance.AddSprite("content/effect_blood_small");
            var bloodBig = SpriteLoader.Instance.AddSprite("content/effect_blood_medium");
            for (int i = 0; i < particles; i++)
            {
                var sprite = bloodBig;
                if (random.NextDouble() < 4)
                    sprite = bloodSmall;
                Vector2 angleVector = Util.AngleToVector(random.NextAngle());
                new SmokeParticle(world, sprite, position + angleVector * random.NextFloat() * 8, random.Next(16, 48))
                {
                    StartTime = 0.4f,
                    StartVelocity = angleVector * random.NextFloat() * radius,
                    StartVelocityLerp = LerpHelper.QuadraticOut,
                    EndTime = 0.8f,
                    EndVelocity = Util.AngleToVector(random.NextAngle()) * random.Next(0, 16),
                    EndVelocityLerp = LerpHelper.Quadratic,
                    DissipateTime = 0.5f,
                    Color = new Color(215, 63, 36),
                    DrawPass = DrawPass.Effect,
                };
            }
        }

        public static void CreateSpatter(SceneGame world, Vector2 position, int splats, Vector2 directionOffset, float directionMultiplier, Random random)
        {
            for (int i = 0; i < splats; i++)
            {
                Vector2 offset = (directionOffset + Util.AngleToVector(random.NextAngle()) * random.NextFloat(16, 48)) * directionMultiplier;
                var blood = SpriteLoader.Instance.AddSprite("content/effect_blood_large");
                new BloodStain(world, blood, random.Next(1000), position + offset, random.NextFloat(0.5f, 2.0f), random.NextAngle(), 8000);
            }
        }

        public static ICurio GetGrappleTarget(this ICurio origin,Vector2 direction)
        {
            var tile = origin.GetMainTile();
            var offset = Util.ToTileOffset(direction);
            var neighbor = tile;
            for (int i = 0; i < 10; i++)
            {
                neighbor = neighbor.GetNeighborOrNull(offset.X, offset.Y);
                if (neighbor == null)
                    break;
                var grappleTarget = neighbor.GetGrappleTarget();
                if (grappleTarget != null)
                    return grappleTarget;
            }
            return null;
        }

        public static void DrawStrike(SceneGame scene, Vector2 start, Vector2 end, float slide, Color color)
        {
            var sprite = SpriteLoader.Instance.AddSprite("content/effect_bash");
            float angle = Util.VectorToAngle(end - start);
            var pos = Vector2.Lerp(start, end, (float)LerpHelper.QuadraticIn(0.3, 1, slide));
            var middle = new Vector2(sprite.Width / 2, sprite.Height / 4);
            var size = (float)LerpHelper.CubicOut(0.3, 1, slide);
            var lengthMod = (float)LerpHelper.CubicIn(1, 0.3, slide);
            scene.DrawSpriteExt(sprite, 0, pos - middle, middle, angle, new Vector2(1, lengthMod) * size, SpriteEffects.None, color, 0);
        }

        public static void DrawArea(SceneGame scene, IEnumerable<MapTile> tiles, Color a, Color b, float appear)
        {
            var cursor = SpriteLoader.Instance.AddSprite("content/effect_area");
            var cursor_hit = SpriteLoader.Instance.AddSprite("content/effect_area_hit");

            float flash = 0.5f + (float)Math.Sin(scene.Frame / 10f) * 0.5f;
            Color color = Color.Lerp(a.WithAlpha(appear), b.WithAlpha(appear), flash);
            foreach (var tile in tiles)
            {
                var pos = new Rectangle(16 * tile.X, 16 * tile.Y, 16, 16);
                if (appear < 1)
                    scene.SpriteBatch.Draw(cursor.Texture, pos, new Rectangle(scene.Frame, 0, 16, 16), color.WithAlpha(0.3f));
                else
                    scene.SpriteBatch.Draw(cursor_hit.Texture, pos, new Rectangle(scene.Frame, 0, 16, 16), color.WithAlpha(0.5f));
            }
        }

        public static void DrawImpact(SceneGame scene, MapTile target, Color a, Color b, float appear)
        {
            var cursor = SpriteLoader.Instance.AddSprite("content/effect_area_target");

            float flash = 0.5f + (float)Math.Sin(scene.Frame / 10f) * 0.5f;
            Color color = Color.Lerp(a.WithAlpha(appear), b.WithAlpha(appear), flash);

            var pos = target.GetVisualPosition();
            scene.DrawSprite(cursor, (int)(scene.Frame * 0.25f), pos, SpriteEffects.None, color, 0);
        }

        public static void DrawImpactLine(SceneGame scene, Func<float, Vector2> curve, Color a, Color b, float appear)
        {
            var line = SpriteLoader.Instance.AddSprite("content/line_dotted");
            var flash = 0.5f + (float)Math.Sin(scene.Frame / 10f) * 0.5f;
            Color color = Color.Lerp(a.WithAlpha(appear), b.WithAlpha(appear), flash);
            scene.DrawBeamCurve(line, curve, 100, (slide) => 0.5f, 2, scene.Frame, 0.0f, 1.0f, ColorMatrix.Tint(color), BlendState.Additive);
        }

        public static void DrawPointer(SceneGame scene, Vector2 pos, string text)
        {
            var pointer = SpriteLoader.Instance.AddSprite("content/ui_pointer_small");

            var uiPos = Vector2.Transform(pos, scene.WorldTransform);
            //var textParameters = new TextParameters().SetColor(Color.White, Color.Black);

            float angle = 0;
            float distance = 24;
            int width = 19 * 32;
            int height = 19 * 32;
            var area = new Rectangle((scene.Viewport.Width - width) / 2, (scene.Viewport.Height - height) / 2, width, height);

            if (!area.Contains(uiPos))
            {
                var center = new Vector2(scene.Viewport.Width / 2, scene.Viewport.Height / 2);
                var delta = uiPos - center;
                uiPos = center + Util.Scuff(delta, 19 * 16);
                angle = Util.VectorToAngle(-delta);
                distance = 0;
            }
            //scene.DrawText(Game.ConvertToSmallPixelText(text), uiPos + new Vector2(0, -24 - distance), Alignment.Center, textParameters);
            scene.DrawSpriteExt(pointer, 0, uiPos + distance * Util.AngleToVector(angle) - pointer.Middle, pointer.Middle, angle, SpriteEffects.None, 0);
        }
    }
}
