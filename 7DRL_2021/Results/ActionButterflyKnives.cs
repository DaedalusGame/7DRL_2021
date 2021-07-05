using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionButterflyKnives : IActionHasOrigin
    {
        public ICurio Origin { get; set; }
        public int SlashStart;
        public int SlashEnd;
        public bool Done => true;

        public ActionButterflyKnives(ICurio origin, int slashStart, int slashEnd)
        {
            Origin = origin;
            SlashStart = slashStart;
            SlashEnd = slashEnd;
        }

        public void Run()
        {
            var world = Origin.GetWorld();

            new ScreenFlashSimple(world, ColorMatrix.Tint(new Color(192,128,255)), LerpHelper.QuadraticOut, 20);
            FireDaggers();
        }

        private void FireDaggers()
        {
            var world = Origin.GetWorld();
            var tile = Origin.GetMainTile();
            var areaPositions = Enumerable.Range(Math.Min(SlashStart, SlashEnd), Math.Abs(SlashEnd - SlashStart) + 1);

            new TimeFade(world, 0.05f, LerpHelper.ExponentialIn, 20);

            int index = 0;
            foreach (var position in areaPositions)
            {
                var angle = Origin.GetAngle() + position * MathHelper.PiOver4;
                var offset = Util.AngleToVector(angle).ToTileOffset();
                var neighbor = tile.GetNeighborOrNull(offset.X, offset.Y);
                if (neighbor != null)
                {
                    var dagger = new Curio(Template.EnergyKnife);
                    dagger.MoveTo(tile);
                    dagger.GetBehavior<BehaviorProjectile>().Fire(Origin, Util.AngleToVector(angle), 3, 20);
                }
                index++;
            }
        }
    }
}
