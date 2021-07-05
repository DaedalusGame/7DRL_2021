using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionBloodThorn : IActionHasOrigin
    {
        public ICurio Origin { get; set; }
        public int SlashStart;
        public int SlashEnd;
        public bool Done => true;

        public ActionBloodThorn(ICurio origin, int slashStart, int slashEnd)
        {
            Origin = origin;
            SlashStart = slashStart;
            SlashEnd = slashEnd;
        }

        public void Run()
        {
            var world = Origin.GetWorld();
            var sword = Origin.GetBehavior<BehaviorSword>();

            FireDaggers();

            if (sword != null)
                sword.HasBlood = false;
        }

        private void FireDaggers()
        {
            var world = Origin.GetWorld();
            var tile = Origin.GetMainTile();

            //new TimeFade(world, 0.05f, LerpHelper.ExponentialIn, 20);

            int index = 0;
            var position = SlashEnd;
            var angle = Origin.GetAngle() + position * MathHelper.PiOver4;
            var offset = Util.AngleToVector(angle).ToTileOffset();
            var neighbor = tile.GetNeighborOrNull(offset.X, offset.Y);
            if (neighbor != null)
            {
                var dagger = new Curio(Template.BloodThorn);
                dagger.MoveTo(tile);
                dagger.GetBehavior<BehaviorProjectile>().Fire(Origin, Util.AngleToVector(angle), 3, 20);
            }
            index++;
        }
    }
}
