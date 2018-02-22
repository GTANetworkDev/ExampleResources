namespace poker.Server
{
    public enum Suit
    {
        diams = 0,
        hearts,
        clubs,
        spades
    }

    public struct Card
    {
        public Card(int rank, int suit)
        {
            Rank = rank;
            Suit = (Suit) suit;
        }

        public const int MaxRank = 14;
        public const int MinRank = 2;

        public int Rank;
        public Suit Suit;

        public string ToJsCode()
        {
            string rank;
            switch (Rank)
            {
                case 11:
                    rank = "j";
                    break;
                case 12:
                    rank = "q";
                    break;
                case 13:
                    rank = "k";
                    break;
                case 14:
                    rank = "a";
                    break;
                default:
                    rank = Rank.ToString();
                    break;
            }

            return rank + ":" + Suit;
        }
    }
}