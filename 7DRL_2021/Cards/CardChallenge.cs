using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;

namespace _7DRL_2021.Cards
{
    class CardNemesis : Card
    {
        public CardNemesis(int deckAmount) : base("nemesis", deckAmount)
        {
            Sprite = SpriteLoader.Instance.AddSprite("content/card_nemesis");
            Name = $"Omicron";
            Description = $"Nemesis becomes killable and must be killed next level. This is the final challenge.";
            Value = 2000;
        }

        public override void Apply(SceneGame world, Vector2 cardPos)
        {
            Behavior.Apply(new BehaviorOmicron(world.PlayerCurio));
        }

        public override bool CanDestroy(SceneGame world)
        {
            throw new NotImplementedException();
        }

        public override void Destroy(SceneGame world, Vector2 cardPos)
        {
            throw new NotImplementedException();
        }

        public override bool IsValid(SceneGame world)
        {
            return !world.PlayerCurio.HasBehaviors<BehaviorOmicron>();
        }
    }

    class CardWraith : Card
    {
        public CardWraith(int deckAmount) : base("wraith", deckAmount)
        {
            Sprite = SpriteLoader.Instance.AddSprite("content/card_bell");
            Name = $"Doom";
            Description = $"The Bell Wraiths are summoned instantly next level. Hearts give 500% score.";
            Value = 2000;
        }

        public override void Apply(SceneGame world, Vector2 cardPos)
        {
            Behavior.Apply(new BehaviorDoom(world.PlayerCurio, 5));
        }

        public override bool CanDestroy(SceneGame world)
        {
            throw new NotImplementedException();
        }

        public override void Destroy(SceneGame world, Vector2 cardPos)
        {
            throw new NotImplementedException();
        }

        public override bool IsValid(SceneGame world)
        {
            return true;
        }
    }
}
