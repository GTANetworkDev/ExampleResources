using System;
using System.Collections.Generic;
using System.Linq;

namespace poker.Server
{
    public static class PokerEngine
    {
        // Yes, I know hashtables are much faster but I want move names too.
        public static HandDescriptor GetHandResult(Card[] table, Player player)
        {
            Card[] allCards = new Card[table.Length + player.Cards.Length];
            Array.Copy(table, 0, allCards, 0, table.Length);
            Array.Copy(player.Cards, 0, allCards, table.Length, player.Cards.Length);

            HandDescriptor hand = new HandDescriptor();
            hand.Player = player;

            for (int i = 0; i < 10; i++)
            {
                if (TryHand((HandRankings) i, ref allCards, hand))
                    return hand;
            }

            return null;
        }

        private static bool TryHand(HandRankings hand, ref Card[] table, HandDescriptor result)
        {
            switch (hand)
            {
                case HandRankings.Royal_Flush:
                    return TryRoyalFlush(ref table, result);
                case HandRankings.Four_of_a_Kind:
                    return TryFourOfAKind(ref table, result);
                case HandRankings.Straight_Flush:
                    return TryStraightFlush(ref table, result);
                case HandRankings.Full_House:
                    return TryFullHouse(ref table, result);
                case HandRankings.Flush:
                    return TryFlush(ref table, result);
                case HandRankings.Straight:
                    return TryStraight(ref table, result);
                case HandRankings.Three_of_a_Kind:
                    return TryThreeOfAKind(ref table, result);
                case HandRankings.Two_Pair:
                    return TryTwoPair(ref table, result);
                case HandRankings.Pair:
                    return TryPair(ref table, result);
                case HandRankings.High_Card:
                    return HighCard(ref table, result);
            }

            return false;
        }

        private static bool TryRoyalFlush(ref Card[] table, HandDescriptor result)
        {
            for (int suit = 0; suit < 4; suit++)
            {
                for (int i = Card.MaxRank; i >= 9; i--)
                {
                    if (i == 9)
                    {
                        result.Rank = HandRankings.Royal_Flush;
                        result.Tiebreaker = 0;
                        return true;
                    }
                    else if (!table.Any(c => c.Suit == (Suit)suit && c.Rank == i))
                        break;
                }
            }

            return false;
        }

        private static bool TryFourOfAKind(ref Card[] table, HandDescriptor result)
        {
            for (int i = Card.MaxRank; i >= 2; i--)
            {
                if (table.Count(c => c.Rank == i) == 4)
                {
                    result.Rank = HandRankings.Four_of_a_Kind;
                    result.Tiebreaker = i;
                    return true;
                }
            }

            return false;
        }

        private static bool TryStraightFlush(ref Card[] table, HandDescriptor result)
        {
            for (int i = Card.MaxRank; i >= 2 + 4; i--)
            {
                for (int suitIndex = 0; suitIndex < 4; suitIndex++)
                {
                    for (int j = i; j >= i - 5; j--)
                    {
                        if (j == i - 5)
                        {
                            result.Rank = HandRankings.Straight_Flush;
                            result.Tiebreaker = i;
                            return true;
                        } else if (!table.Any(c => c.Suit == (Suit)suitIndex && c.Rank == j))
                            break;
                    }
                }
            }

            return false;
        }

        private static bool TryFullHouse(ref Card[] table, HandDescriptor result)
        {
            int hasThree = 0, hasTwo = 0;
            for (int i = Card.MaxRank; i >= 2; i--)
            {
                int count = table.Count(c => c.Rank == i);

                if (count >= 3 && hasThree == 0) hasThree = i;
                else if (count >= 2 && hasTwo == 0) hasTwo = i;

                if (hasThree > 0 && hasTwo > 0)
                {
                    result.Rank = HandRankings.Full_House;
                    result.Tiebreaker = hasThree << 16 | hasTwo;
                    return true;
                }
            }

            return false;
        }

        private static bool TryFlush(ref Card[] table, HandDescriptor result)
        {
            for (int suit = 0; suit < 4; suit++)
            {
                if (table.Count(c => c.Suit == (Suit) suit) >= 5)
                {
                    result.Rank = HandRankings.Flush;
                    result.GenerateTiebreaker(table.Where(c => c.Suit == (Suit) suit));
                    return true;
                }
            }

            return false;
        }

        private static bool TryStraight(ref Card[] table, HandDescriptor result)
        {
            for (int i = Card.MaxRank; i >= 2 + 4; i--)
            {
                for (int j = i; j >= i - 5; j--)
                {
                    if (j == i - 5)
                    {
                        result.Rank = HandRankings.Straight;
                        result.Tiebreaker = i;
                        return true;
                    } else if (!table.Any(c => c.Rank == j))
                        break;
                }
            }

            return false;
        }

        private static bool TryThreeOfAKind(ref Card[] table, HandDescriptor result)
        {
            for (int i = Card.MaxRank; i >= 2; i--)
            {
                if (table.Count(c => c.Rank == i) == 3)
                {
                    result.Rank = HandRankings.Three_of_a_Kind;
                    result.Tiebreaker = i;
                    return true;
                }
            }

            return false;
        }

        private static bool TryTwoPair(ref Card[] table, HandDescriptor result)
        {
            int firstPair = 0;
            int secondPair = 0;
            for (int i = Card.MaxRank; i >= 2; i--)
            {
                if (table.Count(c => c.Rank == i) >= 2)
                {
                    if (firstPair == 0) firstPair = i;
                    else if (secondPair == 0) secondPair = i;
                    else break;
                }
            }

            if (firstPair != 0 && secondPair != 0)
            {
                result.Rank = HandRankings.Two_Pair;
                result.Tiebreaker = firstPair << 16 | secondPair;
                return true;
            }

            return false;
        }

        private static bool TryPair(ref Card[] table, HandDescriptor result)
        {
            for (int i = Card.MaxRank; i >= 2; i--)
            {
                if (table.Count(c => c.Rank == i) >= 2)
                {
                    result.Rank = HandRankings.Pair;
                    result.Tiebreaker = i;
                    return true;
                }
            }

            return false;
        }

        private static bool HighCard(ref Card[] table, HandDescriptor result)
        {
            result.Rank = HandRankings.High_Card;
            result.GenerateTiebreaker(table);

            return true;
        }

    }
}