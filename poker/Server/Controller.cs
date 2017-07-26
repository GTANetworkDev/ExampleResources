using System;
using System.Collections.Generic;
using GTANetworkServer;
using GTANetworkShared;

namespace poker.Server
{
    public class Controller : Script
    {
        private Dictionary<int, PokerLobby> _lobbies;
        private bool _enableCommands;
        private int _lobbyCounter;

        public Controller()
        {
            _lobbies = new Dictionary<int, PokerLobby>();

            API.onClientEventTrigger += (player, name, arguments) =>
            {
                if (arguments[0] is int)
                {
                    int lobbyid = (int) arguments[0];

                    if (_lobbies.ContainsKey(lobbyid))
                        _lobbies[lobbyid].ReceiveClientEvent(player, name, arguments);
                }
            };

            API.onPlayerDisconnected += (player, reason) =>
            {
                foreach (var lobby in _lobbies)
                {
                    lobby.Value.RemovePlayer(player);
                }
            };

            API.onUpdate += () =>
            {
                foreach (var lobby in _lobbies)
                {
                    lobby.Value.Pulse();
                }
            };

            API.onResourceStop += () =>
            {
                foreach (var lobby in _lobbies)
                {
                    for (int i = lobby.Value.Players.Count - 1; i >= 0; i--)
                    {
                        lobby.Value.RemovePlayer(lobby.Value.Players[i].Client);
                    }
                }
            };

            API.onResourceStart += () =>
            {
                _enableCommands = API.getSetting<bool>("enable_commands");
            };
        }

        // EXPORTED FUNCS

        public event ExportedEvent OnPlayerMoneyChange;
        public event ExportedEvent OnPlayerWinRound;
        public event ExportedEvent OnPlayerLoseRound;
        public event ExportedEvent OnLobbyRoundEnd;
        public event ExportedEvent OnLobbyRoundStart;

        public int CreateLobby(Vector3 position, int maxPlayers)
        {
            int id = ++_lobbyCounter;

            _lobbies.Add(id, new PokerLobby(id, true, position, maxPlayers, API.createObject));

            _lobbies[id].OnLobbyRoundEnd += parameters => { if (OnLobbyRoundEnd != null) OnLobbyRoundEnd.Invoke(id); };
            _lobbies[id].OnLobbyRoundStart += parameters => { if (OnLobbyRoundStart != null) OnLobbyRoundStart.Invoke(id); };
            _lobbies[id].OnPlayerLoseRound += parameters => { if (OnPlayerLoseRound != null) OnPlayerLoseRound.Invoke(id, parameters[0]); }; // id, client
            _lobbies[id].OnPlayerWinRound += parameters => { if (OnPlayerWinRound != null) OnPlayerWinRound.Invoke(id, parameters[0], parameters[1], parameters[2]); }; // id, client, take, handname
            _lobbies[id].OnPlayerMoneyChange += parameters => { if (OnPlayerMoneyChange != null) OnPlayerMoneyChange.Invoke(id, parameters[0], parameters[1]); }; // id, client, newmoney

            return id;
        }

        public void DestroyLobby(int lobby)
        {
            if (_lobbies.ContainsKey(lobby))
            {
                for (int i = _lobbies[lobby].Players.Count - 1; i >= 0; i--)
                {
                    _lobbies[lobby].RemovePlayer(_lobbies[lobby].Players[i].Client);
                }

                _lobbies[lobby].Dispose();

                _lobbies.Remove(lobby);
            }
        }

        public bool DoesLobbyExist(int lobby)
        {
            return _lobbies.ContainsKey(lobby);
        }

        public void AddPlayerToLobby(Client player, int lobby, int money)
        {
            if (_lobbies.ContainsKey(lobby))
            {
                _lobbies[lobby].AddPlayer(player, money);
            }
        }

        public void RemovePlayerFromLobby(Client player, int lobby)
        {
            if (_lobbies.ContainsKey(lobby))
            {
                _lobbies[lobby].RemovePlayer(player);
            }
        }

        public void StartLobby(int lobby)
        {
            if (_lobbies.ContainsKey(lobby))
            {
                _lobbies[lobby].StartGame();
            }
        }

        // COMMANDS

        [Command]
        public void leavetable(Client sender)
        {
            foreach (var lobby in _lobbies)
            {
                lobby.Value.RemovePlayer(sender);
            }
        }

        [Command]
        public void makeLobby(Client player, int maxPlayers)
        {
            if (!_enableCommands) return;
            int id = ++_lobbyCounter;

            _lobbies.Add(id, new PokerLobby(id, true, player.position, maxPlayers, API.createObject));

            player.sendChatMessage("Your lobby id is " + id);
        }

        [Command]
        public void JoinLobby(Client player, int lobby, uint money)
        {
            if (!_enableCommands) return;
            if (_lobbies.ContainsKey(lobby))
            {
                _lobbies[lobby].AddPlayer(player, (int) money);
            }
            else player.sendChatMessage("That lobby doesn't exist!");
        }
        
        [Command]
        public void StartLobby(Client player, int lobby)
        {
            if (!_enableCommands) return;
            if (_lobbies.ContainsKey(lobby))
            {
                _lobbies[lobby].StartGame();
            }
            else player.sendChatMessage("That lobby doesn't exist!");
        }
    }
}