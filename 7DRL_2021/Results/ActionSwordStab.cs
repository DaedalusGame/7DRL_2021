using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionSwordStab : IActionHasOrigin, ITickable
    {
        public bool Done => true;
        public ICurio Origin { get; set; }

        Vector2 Direction;

        List<ICurio> AlreadyHit = new List<ICurio>();
        public List<IModifier> Modifiers = new List<IModifier>();

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
            actions.Add(new ActionStabHit(Origin, target, this).InSlot(ActionSlot.Active));
            actions.Apply(target);
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

        public void Tick(SceneGame scene)
        {
            DamageArea();
        }
    }
}
