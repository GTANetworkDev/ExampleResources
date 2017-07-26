using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkServer;
using GTANetworkShared;

namespace poker.Server
{
    public class PokerLobby
    {
        private Random _rnd = new Random();
        private Deck _deck;
        public List<Player> Players;
        public GameState State;
        public int Pot;
        public Card[] Table;
        public int LobbyId;
        public List<NetHandle> Cleanup = new List<NetHandle>();
        public int MaxBetter; // Pos of max better
        public int CurrentBetter; // Pos of current better
        public int CurrentBet; // Max bet
        public int MaxPlayers;
        public int UnclaimedCash = 0;

        public event ExportedEvent OnPlayerMoneyChange; // client, new money
        public event ExportedEvent OnPlayerWinRound; // Client, take, Hand Name
        public event ExportedEvent OnPlayerLoseRound; // client
        public event ExportedEvent OnLobbyRoundEnd; // no args
        public event ExportedEvent OnLobbyRoundStart; // no args

        private int Blinds = 200;
        private int _starter;
        private int _numberOfRounds;
        private bool _hasTable;
        private Object[] _chairs;
        private Object _table;
        private Func<int, Vector3, Vector3, int, Object> createObject;
        private List<NetHandle> _cashObjects = new List<NetHandle>();

        public PokerLobby(int id, bool createTable, Vector3 table, int maxPlayers,  Func<int, Vector3, Vector3, int, Object> createObject)
        {
            this.createObject = createObject;
            _deck = new Deck();
            Players = new List<Player>();
            LobbyId = id;

            _hasTable = createTable;
            MaxPlayers = maxPlayers;
            if (createTable)
                CreateTable(table);

        }

        private void CreateTable(Vector3 ppos)
        {
            _table = createObject(API.shared.getHashKey("PROP_TABLE_02"), ppos - new Vector3(0, 0, 0.6f), new Vector3(), 0); ;

            _chairs = new Object[MaxPlayers];

            for (int i = 0; i < MaxPlayers; i++)
            {
                var origangle = (2 * Math.PI) / MaxPlayers;
                var ourAngle = origangle * i;
                float dist = 1.8f;
                var cpos = _table.position + new Vector3((float)Math.Cos(ourAngle) * dist, (float)Math.Sin(ourAngle) * dist, -0.4f);
                _chairs[i] = createObject(-1198343923, cpos, new Vector3(), 0);

                
                var angle = -Math.Atan2(_table.position.Y - _chairs[i].position.Y,
                    _table.position.X - _chairs[i].position.X);
                angle = angle * (180 / Math.PI);
                angle -= 90;
                _chairs[i].rotation = new Vector3(0, 0, (float)-angle);
            }
        }

        private void SendToPlayers(string msg)
        {
            foreach (Player player in Players)
            {
                player.Client.sendChatMessage(msg);
            }
        }

        public void StartGame()
        {
            if (State == GameState.PreGame)
            {
                State = GameState.NewRound;
            }
        }

        public void AddPlayer(Client cl, int money)
        {
            if (State != GameState.PreGame)
                return;

            if (Players.Any(p => p.Client == cl))
                return;

            if (_hasTable && Players.Count >= MaxPlayers)
                return;

            var player = new Player()
            {
                Client = cl,
                Cards = new Card[2],
                Money = money,
            };

            cl.triggerEvent("ENTER_GAME", LobbyId);

            object[] args2Send = new object[Players.Count + 1];
            args2Send[0] = Players.Count;
            for (int i = 0; i < Players.Count; i++)
            {
                args2Send[i + 1] = Players[i].Client.name;
            }

            cl.triggerEvent("SET_PLAYERS", args2Send);

            cl.triggerEvent("UPDATE_MONEY", money, 0);
            
            foreach (var player1 in Players)
            {
                player1.Client.triggerEvent("ADD_PLAYER", cl.name);
            }

            int tableSeat = 0;

            while (Players.Any(p => p.TableSeat == tableSeat))
            {
                tableSeat++;
                if (tableSeat >= MaxPlayers)
                    return;
            }

            player.TableSeat = tableSeat;
            Players.Add(player);

            SendToPlayers("Player ~g~~n~" + player.Client.name + "~n~~w~ has joined the lobby!");

            if (_hasTable)
            {
                API.shared.freezePlayer(cl, true);
                cl.position = _chairs[tableSeat].position + new Vector3(0, 0, 0.5f);
                cl.rotation = _chairs[tableSeat].rotation + new Vector3(0, 0, 180f);
                cl.collisionless = true;
                string[] animdicts = new[]
                {
                    "amb@prop_human_seat_chair@male@generic@base",
                    "amb@prop_human_seat_chair@male@generic@idle_a",
                    "amb@prop_human_seat_chair@male@generic@idle_a",
                    "amb@prop_human_seat_chair@male@generic@idle_a",
                    "amb@prop_human_seat_chair@male@elbows_on_knees@base",
                    "amb@prop_human_seat_chair@male@elbows_on_knees@idle_a",
                    "amb@prop_human_seat_chair@male@elbows_on_knees@idle_a",
                    "amb@prop_human_seat_chair@male@elbows_on_knees@idle_a",
                    "amb@prop_human_seat_chair@male@left_elbow_on_knee@base",
                    "amb@prop_human_seat_chair@male@left_elbow_on_knee@idle_a",
                    "amb@prop_human_seat_chair@male@left_elbow_on_knee@idle_a",
                    "amb@prop_human_seat_chair@male@left_elbow_on_knee@idle_a",
                    "amb@prop_human_seat_chair@male@right_foot_out@base",
                    "amb@prop_human_seat_chair@male@right_foot_out@idle_a",
                    "amb@prop_human_seat_chair@male@right_foot_out@idle_a",
                    "amb@prop_human_seat_chair@male@right_foot_out@idle_a",
                };

                string[] animnames = new[]
                {
                    "base",
                    "idle_a",
                    "idle_b",
                    "idle_c",
                    "base",
                    "idle_a",
                    "idle_b",
                    "idle_c",
                    "base",
                    "idle_a",
                    "idle_b",
                    "idle_c",
                    "base",
                    "idle_a",
                    "idle_b",
                    "idle_c",
                };

                int ranim = _rnd.Next(animdicts.Length);
                //API.shared.consoleOutput("Playing anim " + animdicts[ranim] + " " + animnames[ranim]);
                API.shared.playPlayerAnimation(cl, 1, animdicts[ranim], animnames[ranim]);

                RebuildCash();
            }
        }
        
        public void RebuildCash()
        {
            if (!_hasTable) return;
            _cashObjects.ForEach(n => API.shared.deleteEntity(n));
            _cashObjects.Clear();

            const float range = 0.2f;
            // Pot
            for (int i = 999; i <= Pot; i += 1000)
            {
                _cashObjects.Add(createObject(-1448063107, _table.position +
                    new Vector3(0, 0, 0.42f) +
                    Vector3.RandomXY() * range,
                    new Vector3(0, 0, _rnd.NextDouble() * 360), 0));
            }

            const float playerDist = 0.9f;
            // Players
            foreach (var player in Players)
            {
                float playerRange = (float)_rnd.NextDouble() * 0.15f + 0.1f;

                //if (player.Money > 10000)
                    //playerRange = 0.2f;

                for (int i = 999; i <= player.Money - player.CurrentBet; i += 1000)
                {
                    var dir = (player.Client.position - _table.position).Normalized * playerDist;
                    _cashObjects.Add(createObject(-1448063107, _table.position +
                        new Vector3(dir.X, dir.Y, 0.42f) +
                        Vector3.RandomXY() * playerRange,
                        new Vector3(0, 0, _rnd.NextDouble() * 360), 0));
                }
            }
        }

        public void RemovePlayer(Client cl)
        {
            Player pl = Players.FirstOrDefault(p => p.Client == cl);

            if (pl != null)
            {
                cl.stopAnimation();
                cl.collisionless = false;
                cl.freezePosition = false;
                cl.triggerEvent("LEAVE_GAME");
                int index = Players.IndexOf(pl);
                
                UnclaimedCash += pl.CurrentBet;
                pl.Money -= pl.CurrentBet;
                if (OnPlayerMoneyChange != null) OnPlayerMoneyChange.Invoke(pl.Client, pl.Money);
                Players.Remove(pl);

                if (CurrentBetter == index && Players.Count > 1)
                {
                    CurrentBetter--;
                    Advance();
                }

                foreach (Player player in Players)
                {
                    player.Client.triggerEvent("PLAYER_LEAVE_GAME", cl.name);
                }

                SendToPlayers("Player ~r~~n~" + pl.Client.name + "~n~~w~ has left the lobby!");

                if (_hasTable)
                    RebuildCash();
            }
        }

        private void GiveTurn(Player pl)
        {
            //API.shared.consoleOutput("Gave turn to " + pl.Client.name);
            pl.Client.triggerEvent("YOUR_TURN");
            foreach (Player player in Players.Where(p => p != pl))
            {
                player.Client.triggerEvent("SOMEONES_TURN", pl.Client.name);
            }
            State = GameState.Waiting;
            //SendToPlayers("It's ~n~" + pl.Client.name + "~n~'s turn now!");
        }

        public void ReceiveClientEvent(Client cl, string ev, object[] args)
        {
            Player pl = Players.FirstOrDefault(p => p.Client == cl);

            if (pl == null) return;

            int index = Players.IndexOf(pl);

            if (ev == "TURN_RESPONSE")
            {
                if (index != CurrentBetter)
                    return;
                API.shared.consoleOutput("index : " + index);
                API.shared.consoleOutput("response from " + pl.Client.name);
                API.shared.consoleOutput("args: {0} {1}", args[0], args[1]);

                int selection = (int) args[1];
                switch (selection)
                {
                    case 0: // raise
                        int newBet = (int) args[2];
                        API.shared.consoleOutput("newBet: {0}", newBet);
                        if (pl.Money >= newBet && newBet > CurrentBet)
                        {
                            MaxBetter = CurrentBetter;
                            CurrentBet = newBet;
                            pl.CurrentBet = newBet;
                            pl.Client.triggerEvent("UPDATE_MONEY", pl.Money, pl.CurrentBet);

                            foreach (Player player in Players)
                            {
                                player.Client.triggerEvent("SET_MAX_BET", CurrentBet);
                                if (player != pl)
                                    player.Client.triggerEvent("UPDATE_LAST_PLAYER_ACTION", pl.Client.name, "raised $" + newBet);
                            }

                            SendToPlayers("Player ~n~" + pl.Client.name + "~n~ has ~y~raised~w~ the bet to ~g~" + CurrentBet + "~w~!");
                        }
                        break;
                    case 1: // call
                        bool check = pl.CurrentBet == CurrentBet;
                        pl.CurrentBet = Math.Min(CurrentBet, pl.Money);
                        pl.Client.triggerEvent("UPDATE_MONEY", pl.Money, pl.CurrentBet);
                        foreach (Player player in Players.Where(p => p != pl))
                        {
                            player.Client.triggerEvent("UPDATE_LAST_PLAYER_ACTION", pl.Client.name, (check ? "checked" : "called") + " $" + pl.CurrentBet);
                        }

                        SendToPlayers("Player ~n~" + pl.Client.name + "~n~ has ~g~" + (check ? "checked." : "called."));
                        break;
                    case 2: // Fold
                        pl.State = PlayerState.Folded;
                        pl.Money -= pl.CurrentBet;
                        pl.Client.triggerEvent("UPDATE_MONEY", pl.Money, pl.CurrentBet);
                        foreach (Player player in Players.Where(p => p != pl))
                        {
                            player.Client.triggerEvent("UPDATE_LAST_PLAYER_ACTION", pl.Client.name, "folded");
                        }
                        if (OnPlayerMoneyChange != null) OnPlayerMoneyChange.Invoke(pl.Client, pl.Money);
                        SendToPlayers("Player ~n~" + pl.Client.name + "~n~ has ~r~folded!");
                        break;
                }

                int pot = UnclaimedCash;

                foreach (Player player in Players)
                    pot += player.CurrentBet;

                Pot = pot;

                Players.ForEach(p => p.Client.triggerEvent("SET_POT", pot));

                RebuildCash();

                if (Players.Count(p => p.State == PlayerState.Playing) == 1)
                {
                    //API.shared.consoleOutput("erryone folded.");
                    Player winner = Players.First(p => p.State == PlayerState.Playing);
                    winner.Money += Pot;
                    SendToPlayers("Everyone folded, player ~n~" + winner.Client.name + "~n~ has won the round!");

                    foreach (var player in Players)
                        foreach (var player1 in Players.Where(p => p != player))
                            player1.Client.triggerEvent("SET_PLAYER_CARDS", player.Client.name,
                                player.Cards[0].ToJsCode(), player.Cards[1].ToJsCode());

                    API.shared.sleep(5000);

                    EndRound();
                    return;
                }

                API.shared.sleep(1000);
                Advance();
            }
        }

        private void Advance()
        {
            do
            {
                CurrentBetter = (CurrentBetter + 1) % Players.Count;
                if (CurrentBetter == MaxBetter)
                {
                    // Advance
                    if (Table == null || Table.Length == 0)
                    {
                        Table = new Card[3];

                        for (int i = 0; i < 3; i++)
                            Table[i] = _deck.Pop();
                    }
                    else if (Table.Length == 3)
                    {
                        var tmp = new Card[4];
                        Array.Copy(Table, 0, tmp, 0, 3);
                        tmp[3] = _deck.Pop();
                        Table = tmp;
                    }
                    else if (Table.Length == 4)
                    {
                        var tmp = new Card[5];
                        Array.Copy(Table, 0, tmp, 0, 4);
                        tmp[4] = _deck.Pop();
                        Table = tmp;
                    }
                    else if (Table.Length == 5)
                    {
                        List<HandDescriptor> hands = new List<HandDescriptor>();

                        SendToPlayers("Revealing hands...");

                        // Get hands
                        foreach (Player player in Players.Where(p => p.State == PlayerState.Playing))
                        {
                            HandDescriptor hand = PokerEngine.GetHandResult(Table, player);

                            foreach (var player1 in Players.Where(p => p != player))
                                player1.Client.triggerEvent("SET_PLAYER_CARDS", player.Client.name,
                                    player.Cards[0].ToJsCode(), player.Cards[1].ToJsCode());

                            hands.Add(hand);
                        }

                        API.shared.sleep(5000);

                        foreach (var hand in hands)
                        {
                            SendToPlayers("Player ~n~" + hand.Player.Client.name + "~n~ has " + hand.HandName);
                        }

                        int min = hands.Min(h => (int)h.Rank);
                        var potentialwinners = hands.Where(h => (int) h.Rank == min).ToList();
                        int tiebreakermax = potentialwinners.Max(h => h.Tiebreaker);
                        var winners = potentialwinners.Where(h => h.Tiebreaker >= tiebreakermax).ToArray();
                        int take = Pot / winners.Length;
                        foreach (var winner in winners)
                        {
                            winner.Player.Money += take;
                            winner.Player.Client.triggerEvent("SHARD_CUSTOM", "winner", 5000);
                            SendToPlayers("Player ~n~" + winner.Player.Client.name + "~n~ has won the round with a ~g~" + winner.HandName);
                            if (OnPlayerWinRound != null) OnPlayerWinRound.Invoke(winner.Player.Client, take, winner.HandName);
                        }

                        foreach (var losers in Players.Except(winners.Select(w => w.Player)))
                        {
                            losers.Client.triggerEvent("SHARD_CUSTOM", "loser", 5000);
                            if (OnPlayerLoseRound != null) OnPlayerLoseRound.Invoke(losers.Client);
                        }

                        API.shared.sleep(5000);

                        EndRound();
                        return;
                    }

                    object[] newArgs = new object[Table.Length + 1];
                    newArgs[0] = Table.Length;
                    for (int i = 0; i < Table.Length; i++)
                    {
                        newArgs[i + 1] = Table[i].ToJsCode();
                    }

                    Players.ForEach(p => p.Client.triggerEvent("SET_TABLE_CARDS", newArgs));
                    API.shared.sleep(5000);
                }
            } while (Players[CurrentBetter].State == PlayerState.Folded);
            GiveTurn(Players[CurrentBetter]);
        }

        private void EndRound()
        {
            for (int i = Players.Count - 1; i >= 0; i--)
            {
                var player = Players[i];
                player.Money -= player.CurrentBet;
                player.Client.triggerEvent("UPDATE_MONEY", player.Money, 0);
                
                if (OnPlayerMoneyChange != null) OnPlayerMoneyChange.Invoke(player.Client, player.Money);

                if (player.Money <= 0)
                {
                    SendToPlayers("Player ~n~" + player.Client.name + "~n~ has lost all their money!");
                    RemovePlayer(player.Client);
                }

            }

            RebuildCash();
            if (OnLobbyRoundEnd != null) OnLobbyRoundEnd.Invoke();
            // TODO: remove
            API.shared.sleep(5000);
            State = GameState.NewRound;
        }

        public void Pulse()
        {
            if (State == GameState.PreGame)
            {
                Blinds = 200;
                return;
            }

            if (State == GameState.NewRound)
            {
                if (Players.Count <= 1)
                {
                    SendToPlayers("Not enough players to continue!");
                    State = GameState.PreGame;
                    return;
                }

                if (++_numberOfRounds > 10)
                {
                    _numberOfRounds = 0;
                    _deck.Shuffle();
                }

                if (_numberOfRounds % 5 == 0 && _numberOfRounds != 0)
                    Blinds = Blinds * 2;

                foreach (Player player in Players)
                {
                    player.CurrentBet = 0;
                    player.State = PlayerState.Playing;
                    player.Cards = new Card[2]
                    {
                        _deck.Pop(),
                        _deck.Pop()
                    };
                    player.Client.triggerEvent("SET_HAND_CARDS", player.Cards[0].ToJsCode(), player.Cards[1].ToJsCode());
                }

                CurrentBet = Blinds; // Blind?
                MaxBetter = 0;
                Pot = 0;
                UnclaimedCash = 0;
                Table = null;

                Players.ForEach(p => p.Client.triggerEvent("NEW_ROUND"));
                Players.ForEach(p => p.Client.triggerEvent("SET_MAX_BET", CurrentBet));
                Players.ForEach(p => p.Client.triggerEvent("SET_POT", CurrentBet));

                //API.shared.consoleOutput("starting");
                _starter = (_starter + 1) % Players.Count;
                //API.shared.consoleOutput("starter: " + _starter);
                CurrentBetter = _starter;
                MaxBetter = CurrentBetter;
                Players[CurrentBetter].CurrentBet = Math.Min(CurrentBet, Players[CurrentBetter].Money);
                CurrentBet = Players[CurrentBetter].CurrentBet;
                Players[CurrentBetter].Client.triggerEvent("UPDATE_MONEY", Players[CurrentBetter].Money, Players[CurrentBetter].CurrentBet);
                SendToPlayers("Player ~n~" + Players[CurrentBetter].Client.name + "~n~ has placed a blind of $" + CurrentBet + ".");

                foreach (Player player in Players.Where(p => p != Players[CurrentBetter]))
                {
                    player.Client.triggerEvent("UPDATE_LAST_PLAYER_ACTION", Players[CurrentBetter].Client.name, "blind $" + CurrentBet);
                }

                //API.shared.consoleOutput("Test test test");
                CurrentBetter = (CurrentBetter + 1) % Players.Count;

                //API.shared.consoleOutput("better: " + CurrentBetter);
                RebuildCash();
                GiveTurn(Players[CurrentBetter]);
                if (OnLobbyRoundStart != null) OnLobbyRoundStart.Invoke();
            }
        }

        public void Dispose()
        {
            _cashObjects.ForEach(n => API.shared.deleteEntity(n));
            _cashObjects.Clear();

            foreach (var chair in _chairs)
            {
                API.shared.deleteEntity(chair);
            }

            API.shared.deleteEntity(_table);
        }

    }
}