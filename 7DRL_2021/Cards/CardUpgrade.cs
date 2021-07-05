using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;

namespace _7DRL_2021.Cards
{
    class CardSwordBeam : Card
    {
        public CardSwordBeam() : base("sword_beam", 3)
        {
            Name = "Vacuum Wave";
            Description = $"When flicking your blade, fire a vacuum wave that stuns an enemy.";
        }

        public override void Apply(SceneGame world, Vector2 cardPos)
        {
            
        }

        public override bool CanDestroy(SceneGame world)
        {
            return true;
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

    class CardDoubleHeal : Card
    {
        public CardDoubleHeal(int deckAmount) : base("double_heal", deckAmount)
        {
            Sprite = SpriteLoader.Instance.AddSprite("content/card_eater");
            Name = $"Ravenous Hunger";
            Description = $"Consumed Hearts heal {Symbol.Heart.FormatDescribe(2)}";
            Value = 5000;
        }

        public override void Apply(SceneGame world, Vector2 cardPos)
        {
            Behavior.Apply(new BehaviorDoubleHeal(world.PlayerCurio));
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
            return !world.PlayerCurio.HasBehaviors<BehaviorDoubleHeal>();
        }
    }

    class CardDestructionWave : Card
    {
        public CardDestructionWave(int deckAmount) : base("destruction_wave", deckAmount)
        {
            Sprite = SpriteLoader.Instance.AddSprite("content/card_moon");
            Name = $"Night Terror";
            Description = $"Performing a full slash while holding a heart destroys all nearby enemies.";
            Value = 5000;
        }

        public override void Apply(SceneGame world, Vector2 cardPos)
        {
            Behavior.Apply(new BehaviorSkillDestructionWave(world.PlayerCurio));
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
            return !world.PlayerCurio.HasBehaviors<BehaviorSkillDestructionWave>();
        }
    }

    class CardBloodThorn : Card
    {
        public CardBloodThorn(int deckAmount) : base("blood_thorn", deckAmount)
        {
            Sprite = SpriteLoader.Instance.AddSprite("content/card_thorn");
            Name = $"Blood Thorn";
            Description = $"Performing a slash while blade is bloody releases a wall of thorns.";
            Value = 5000;
        }

        public override void Apply(SceneGame world, Vector2 cardPos)
        {
            Behavior.Apply(new BehaviorSkillBloodThorn(world.PlayerCurio));
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
            return !world.PlayerCurio.HasBehaviors<BehaviorSkillBloodThorn>();
        }
    }

    class CardButterflyKnives : Card
    {
        public CardButterflyKnives(int deckAmount) : base("butterfly_knife", deckAmount)
        {
            Sprite = SpriteLoader.Instance.AddSprite("content/card_butterfly_knife");
            Name = $"Monarch Knife";
            Description = $"Performing a slash from forward to back releases 3 knives.";
            Value = 5000;
        }

        public override void Apply(SceneGame world, Vector2 cardPos)
        {
            Behavior.Apply(new BehaviorSkillButterflyKnives(world.PlayerCurio));
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
            return !world.PlayerCurio.HasBehaviors<BehaviorSkillButterflyKnives>();
        }
    }
}
