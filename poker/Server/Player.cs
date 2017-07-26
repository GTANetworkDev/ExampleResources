using GTANetworkServer;
using GTANetworkShared;

namespace poker.Server
{
    public class Player
    {
        public Client Client { get; set; }
        public Card[] Cards { get; set; }
        public int Money { get; set; }
        public int CurrentBet { get; set; }
        public PlayerState State { get; set; }
        public int TableSeat { get; set; }
    }
}