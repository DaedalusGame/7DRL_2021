using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace _7DRL_2021.Cards
{
    class CardScore : Card
    {
        int Score;

        public CardScore(string id, int score, int deckAmount) : base(id, deckAmount)
        {
            Score = score;
            Sprite = SpriteLoader.Instance.AddSprite("content/card_blood");
            Name = $"{score} Score";
            Description = $"+{score} points";
        }

        public override void Apply(SceneGame world, Vector2 cardPos)
        {
            world.AddUIScore(Score, cardPos, ScoreType.Big);
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
