using System;
using System.Collections;
using System.Collections.Generic;

namespace poker.Server
{
    public class Deck
    {
        private static Random _rng = new Random();
        public Queue<Card> Cards;

        public Deck()
        {
            Cards = new Queue<Card>();

            GenerateCards();

            Shuffle();
        }

        public void GenerateCards()
        {
            for (int i = Card.MinRank; i <= Card.MaxRank; i++)
            {
                for (int j = 0; j < 4; j++)
                    Cards.Enqueue(new Card(i, j));
            }
        }

        public void Shuffle()
        {
            int n = Cards.Count;
            var asArray = Cards.ToArray();

            while (n > 1)
            {
                n--;
                int k = _rng.Next(n + 1);
                Card value = asArray[k];
                asArray[k] = asArray[n];
                asArray[n] = value;
            }

            Cards = new Queue<Card>(asArray);
        }

        public Card Pop()
        {
            var card = Cards.Dequeue();
            Push(card);
            return card;
        }

        public void Push(Card c)
        {
            Cards.Enqueue(c);
        }
    }
}