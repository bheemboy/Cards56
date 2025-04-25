namespace Cards56Lib
{
    public class GameController
    {
        private readonly string _ConnID, _Lang;
        private readonly StateUpdatedDelegate OnStateUpdated;
        public GameController(string connID, string lang, StateUpdatedDelegate onStateUpdated)
        {
            _ConnID = connID;
            _Lang = lang;
            OnStateUpdated = onStateUpdated;
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture(lang);
        }
        private Player GetCurrentPlayer()
        {
            return Players.GetPlayerByConnectionId(_ConnID) ?? throw new PlayerNotRegisteredException();
        }
        private TableController GetTableController() 
        {
            Player player = GetCurrentPlayer();
            if (string.IsNullOrEmpty(player.TableName)) throw new PlayerNotOnAnyTableException();
            return new TableController(GameTables.All[player.TableName], OnStateUpdated);
        }
        public Player AddOrUpdatePlayer(string playerID, string playerName, bool watchOnly)
        {
            // Save the player's name to a list of players
            return Players.AddOrUpdatePlayer(playerID, _ConnID, playerName, _Lang, watchOnly);
        }
        public void DisconnectPlayer()
        {
            // Try to find player by connection ID and remove them if found
            Player? playerLeft = Players.GetPlayerByConnectionId(_ConnID);
            if (playerLeft != null)
            {
                Console.WriteLine($"--> Player disconnected: PlayerID: '{playerLeft.PlayerID}', ConnID: '{_ConnID}', Name: '{playerLeft.Name}'");
                if (!string.IsNullOrEmpty(playerLeft.TableName))
                {
                    TableController table = new TableController(GameTables.All[playerLeft.TableName], OnStateUpdated);
                    table.LeaveTable(playerLeft);
                    if (table.TableEmpty) GameTables.RemoveTable(table.TableName);
                }
                Players.RemoveByPlayerId(playerLeft.PlayerID);
            }
            else
            {
                // Somtimes it is possible that the connection ID is not present in the list of players
                // This can happen if the player reconnects to the server and the connection ID is updated
                Console.WriteLine($"--> Disconnected orphan connection: '{_ConnID}'");
            }
        }
        public TableController JoinTable(int tableType, string privateTableId)
        {
            Player player = GetCurrentPlayer();
            if (!string.IsNullOrEmpty(player.TableName))
            {
                // Player is already on a table
                // Maybe wants to join a different table? Client need to log out and join a new table
                // I am going to throw an exception here
                throw new PlayerAlreadyOnTableException();
            }

            TableController? table=null; // need to get a table to join

            if (string.IsNullOrEmpty(privateTableId)) // Join any public table
            {
                bool tableJoined=false;
                int joinAttempts=0;
                while (!tableJoined && joinAttempts<10)
                {
                    joinAttempts++;
                    try
                    {
                        table = new TableController(GameTables.GetFreeTable(tableType, player.WatchOnly), OnStateUpdated);
                        table.JoinTable(player); // try to add player to table
                        tableJoined=true;
                    }
                    catch
                    {
                        tableJoined=false;
                    }
                }
                if (!tableJoined) throw new Exception("Could not find a free table to join.");
            }
            else
            {
                privateTableId = $"{tableType}-{privateTableId}";

                if (GameTables.All.TryGetValue(privateTableId, out GameTable? value)) // Private table by name already exists
                {
                    table = new TableController(value, OnStateUpdated);
                    table.JoinTable(player); // Add player to table
                }
                else // Let us add a new private table
                {
                    table = new TableController(GameTables.AddTable(tableType, privateTableId), OnStateUpdated);
                    table.JoinTable(player); // Add player to table
                }
            }

            if (table == null) throw new Exception("Could not find a free table to join.");

            // If table is full send ready to start event
            if (table.TableFull && table.Game.Stage == GameStage.WaitingForPlayers)
            {
                table.StartNextGame(new Random().Next(table.T.MaxPlayers)); // Deal cards and initialize bidding
            }

            return table;
        }
        public void PlaceBid(int bid)
        {
            GetTableController().PlaceBid(GetCurrentPlayer(), bid);
        }
        public void PassBid()
        {
            try
            {
                GetTableController().PassBid(GetCurrentPlayer());
            }
            catch (FirstBidderHasNoPointsException)
            {
                // No need to send this exception.
            }
        }
        public void SelectTrump(string card)
        {
            try
            {
                GetTableController().SelectTrump(GetCurrentPlayer(), card);
            }
            catch (OppositeTeamHasNoTrumpCardsException)
            {
                // No need to send this exception. 
            }
        }

        public void PlayCard(string card, int roundOverDelay)
        {
            GetTableController().PlayCard(GetCurrentPlayer(), card, roundOverDelay);
        }
        public void ShowTrump(int roundOverDelay = 0)
        {
            GetTableController().ShowTrump(GetCurrentPlayer(), roundOverDelay);
        }
        public void StartNextGame()
        {
            TableController table = GetTableController();
            table.StartNextGame(table.T.PlayerAt(table.Game.DealerPos+1)); // Deal cards and initialize bidding
        }
        public void RefreshState()
        {
            GetTableController().SendStateUpdatedEvents(GetCurrentPlayer());
        }
        public void ForfeitGame()
        {
            GetTableController().ForfeitGame(GetCurrentPlayer());
        }
    }
}
