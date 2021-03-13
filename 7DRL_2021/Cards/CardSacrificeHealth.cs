using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;

namespace _7DRL_2021.Cards
{
    class CardSacrificeHP : Card
    {
        int HealthCost;
        int Score;

        public CardSacrificeHP(int healthCost, int score) : base("sacrifice_hp", 3)
        {
            Sprite = SpriteLoader.Instance.AddSprite("content/card_sacrifice");
            Name = "Sacrifice";
            Description = $"Lose {Symbol.Heart.FormatDescribe(healthCost)}. +{score} points.";
            HealthCost = healthCost;
            Score = score;
        }

        public override void Apply(SceneGame world, Vector2 cardPos)
        {
            var alive = world.PlayerCurio.GetBehavior<BehaviorAlive>();
            alive.HP -= HealthCost;
            world.AddUIScore(Score, cardPos, ScoreType.Big);
        }

        public override void Destroy(SceneGame world, Vector2 cardPos)
        {
            throw new NotImplementedException();
        }

        public override bool IsValid(SceneGame world)
        {
            var alive = world.PlayerCurio.GetBehavior<BehaviorAlive>();
            return alive != null && alive.HP > HealthCost && alive.CurrentHP > HealthCost;
        }

        public override bool CanDestroy(SceneGame world)
        {
            return false;
        }
    }
}
