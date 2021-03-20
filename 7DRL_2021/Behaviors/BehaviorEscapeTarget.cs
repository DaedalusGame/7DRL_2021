using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    class BehaviorEscapeTarget : Behavior, IDrawable
    {
        public ICurio Curio;
        public Rectangle Area;

        public double DrawOrder => 0;

        public BehaviorEscapeTarget()
        {
        }

        public BehaviorEscapeTarget(ICurio curio, Rectangle area)
        {
            Curio = curio;
            Area = area;
        }

        public bool IsReady()
        {
            var tile = Curio.GetMainTile();
            var levelEnd = tile.GetBehavior<BehaviorLevelEnd>();
            if (levelEnd != null)
                return levelEnd.CanEscape();
            return false;
        }

        public bool IsCompleted()
        {
            var world = Curio.GetWorld();
            return world.WaitForCutscene;
        }

        public override void Apply()
        {
            Curio.AddBehavior(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorEscapeTarget(curio, Area), Curio);
        }

        public void Draw(SceneGame scene, DrawPass pass)
        {
            var sprite = SpriteLoader.Instance.AddSprite("content/ui_area");
            var pos = Curio.GetVisualTarget();
            var area = new Rectangle(Area.X * 16, Area.Y * 16, Area.Width * 16, Area.Height * 16);
            if(pass == DrawPass.UIWorld)
                scene.DrawUI(sprite, area, Color.White);
            if (pass == DrawPass.UI)
                SkillUtil.DrawPointer(scene, pos, "ESCAPE");
        }

        public void DrawIcon(SceneGame scene, Vector2 pos)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass.UI;
            yield return DrawPass.UIWorld;
        }

        public bool ShouldDraw(SceneGame scene, Vector2 cameraPosition)
        {
            return Curio.GetMap() == scene.Map && !IsCompleted() && IsReady();
        }
    }
}
