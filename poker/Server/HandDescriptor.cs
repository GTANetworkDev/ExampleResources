using System.Collections.Generic;

namespace poker.Server
{
    public class HandDescriptor
    {
        public Player Player;
        public HandRankings Rank; // lower, better.
        public int Tiebreaker; // Higher, better

        public void GenerateTiebreaker(IEnumerable<Card> hand)
        {
            foreach (var card in hand)
            {
                Tiebreaker |= (1 << card.Rank);
            }
        }

        public string HandName
        {
            get { return Rank.ToString().Replace('_', ' '); }
        }
    }

    public enum HandRankings
    {
        Royal_Flush,
        Straight_Flush,
        Four_of_a_Kind,
        Full_House,
        Flush,
        Straight,
        Three_of_a_Kind,
        Two_Pair,
        Pair,
        High_Card,
    }
}