using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _7DRL_2021.Behaviors
{
    class BehaviorKillTarget : Behavior, IDrawable
    {
        public ICurio Curio;

        public double DrawOrder => 0;

        public BehaviorKillTarget()
        {
        }

        public BehaviorKillTarget(ICurio curio)
        {
            Curio = curio;
        }

        public bool IsCompleted()
        {
            //return true;
            return Curio.IsDead();
        }

        public override void Apply()
        {
            Curio.AddBehavior(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorKillTarget(curio));
        }

        public void Draw(SceneGame scene, DrawPass pass)
        {
            var pos = Curio.GetVisualTarget();
            SkillUtil.DrawPointer(scene, pos, "KILL");
        }

        public void DrawIcon(SceneGame scene, Vector2 pos)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass.UI;
        }

        public bool ShouldDraw(SceneGame scene)
        {
            return Curio.GetMap() == scene.Map && !IsCompleted();
        }
    }
}
