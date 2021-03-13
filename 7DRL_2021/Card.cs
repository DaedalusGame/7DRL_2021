using _7DRL_2021.Cards;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    abstract class Card : RegistryEntry<Card>
    {
        public int DeckAmount;
        public SpriteReference Sprite;
        public string Name = "UNNAMED";
        public string Description = "REPORT THIS";
        public int Value = 0;

        static Card()
        {
            foreach(var card in Registry)
            {
                Console.WriteLine();
            }
        }

        public Card(string id, int deckAmount) : base(id)
        {
            DeckAmount = deckAmount;
        }

        public abstract bool IsValid(SceneGame world);

        public abstract bool CanDestroy(SceneGame world);

        public abstract void Apply(SceneGame world, Vector2 cardPos);

        public abstract void Destroy(SceneGame world, Vector2 cardPos);

        public static Card SacrificeHealth = new CardSacrificeHP(1, 25000);
        //public static Card Score1000 = new CardScore("score1000", 1000, 3);
        //public static Card Score2000 = new CardScore("score2000", 2000, 3);
        //public static Card Score3000 = new CardScore("score3000", 3000, 3);
        public static Card Heal = new CardHeal(4);
        public static Card MaxHP = new CardMaxHP(2);
        public static Card DoubleHeal = new CardDoubleHeal(2);
        public static Card DestructionWave = new CardDestructionWave(2);
        public static Card Nemesis = new CardNemesis(2);
        public static Card Wraith = new CardWraith(3);
    }

    class Deck
    {
        SceneGame World;
        List<Card> Cards = new List<Card>();
        Random Random = new Random();

        public Deck(SceneGame world)
        {
            World = world;
        }

        public void FillStandard()
        {
            var test = Card.SacrificeHealth;
            foreach (Card card in Card.Registry)
            {
                if (!card.IsValid(World))
                    continue;
                for(int i = 0; i < card.DeckAmount; i++)
                {
                    Cards.Add(card);
                }
            }
        }

        public void ApplyFlames(int n)
        {
            var cardsToRemove = Cards.Where(x => x.DeckAmount > 1).ToList();
            for(int i = 0; i < n; i++)
            {
                var card = cardsToRemove.PickAndRemove(Random);
                Cards.Remove(card);
            }
        }

        public void Shuffle()
        {
            Cards = Cards.Shuffle(Random).ToList();
        }

        public Card Draw()
        {
            if(Cards.Empty())
            {
                FillStandard();
            }
            var card = Cards[0];
            Cards.RemoveAt(0);
            return card;
        }
    }
}
