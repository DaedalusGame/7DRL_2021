using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionSwordStab : IActionHasOrigin
    {
        public bool Done => true;
        public ICurio Origin { get; set; }

        Vector2 Direction;

        List<ICurio> AlreadyHit = new List<ICurio>();

        public ActionSwordStab(ICurio origin, Vector2 direction)
        {
            Origin = origin;
            Direction = direction;
        }

        public void Run()
        {
            DamageArea();
            if(AlreadyHit.Any())
            {
                var actions = new List<ActionWrapper>();
                actions.Add(new ActionWaitForAction(Origin, ActionSlot.Passive).InSlot(ActionSlot.Active));
                actions.Apply(Origin);
            }
        }

        private void Hit(ICurio target)
        {
            AlreadyHit.Add(target);
            var actions = new List<ActionWrapper>();
            actions.Add(new ActionStabHit(Origin, target).InSlot(ActionSlot.Active));
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
            var angle = Origin.GetAngle();
            var offset = Util.AngleToVector(angle).ToTileOffset();
            var hitArea = new[] { tile?.GetNeighborOrNull(offset.X, offset.Y) }.NonNull();
            foreach (var neighbor in hitArea)
            {
                foreach (var target in neighbor.Contents.Except(AlreadyHit))
                {
                    Hit(target);
                }
            }
        }
    }
}
