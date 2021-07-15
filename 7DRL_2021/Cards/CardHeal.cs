using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Cards
{
    class CardHeal : Card
    {
        public CardHeal(int stack) : base("heal", stack)
        {
            Sprite = SpriteLoader.Instance.AddSprite("content/card_bubble");
            Name = "Prolong";
            Description = (textBuilder) => {
                textBuilder.AppendText("Restore ");
                textBuilder.AppendDescribe(Symbol.Heart, "all", Color.White);
            };
            Value = 1500;
        }

        public override void Apply(SceneGame world, Vector2 cardPos)
        {
            var alive = world.PlayerCurio.GetBehavior<BehaviorAlive>();
            alive.SetDamage(0);
        }

        public override void Destroy(SceneGame world, Vector2 cardPos)
        {
            throw new NotImplementedException();
        }

        public override bool IsValid(SceneGame world)
        {
            var alive = world.PlayerCurio.GetBehavior<BehaviorAlive>();
            return alive != null && alive.Damage > 0;
        }

        public override bool CanDestroy(SceneGame world)
        {
            return false;
        }
    }
}
