using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Cards
{
    class CardMaxHP : Card
    {
        public CardMaxHP(int deckAmount) : base("maxHP", deckAmount)
        {
            Sprite = SpriteLoader.Instance.AddSprite("content/card_crystal");
            Name = "Mutate";
            Description = (textBuilder) => {
                textBuilder.AppendText("Gain ");
                textBuilder.AppendDescribe(Symbol.Heart, "1", Color.White);
            };
            Value = 12000;
        }

        public override void Apply(SceneGame world, Vector2 cardPos)
        {
            var alive = world.PlayerCurio.GetBehavior<BehaviorAlive>();
            alive.HP++;
        }

        public override void Destroy(SceneGame world, Vector2 cardPos)
        {
            throw new NotImplementedException();
        }

        public override bool IsValid(SceneGame world)
        {
            var alive = world.PlayerCurio.GetBehavior<BehaviorAlive>();
            return alive != null && alive.HP < 10;
        }

        public override bool CanDestroy(SceneGame world)
        {
            return false;
        }
    }
}
