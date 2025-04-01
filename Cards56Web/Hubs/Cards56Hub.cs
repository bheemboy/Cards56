using System;
using System.Linq;
using System.Globalization;
using System.Threading;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Cards56Lib;

namespace Cards56Web
{
    public interface ICards56Hub
    {
        // METHODS
        Task RegisterPlayer(string playerID, string playerName, string lang, bool watchOnly); // lang = 'en' or 'ml'
        Task JoinTable(int tableType, string privateTableId); // tableType=0, privateTableId="" === public table
        Task PlaceBid(int bid);
        Task PassBid();
        Task SelectTrump(string card);
        Task PlayCard(string card, int roundOverDelay);
        Task ShowTrump(int roundOverDelay);
        Task StartNextGame();        
        Task RefreshState();
        Task ForfeitGame();
    }
    public enum Cards56HubMethod
    {
        RegisterPlayer=1, 
        JoinTable=2, 
        PlaceBid=3, 
        PassBid=4, 
        SelectTrump=5, 
        PlayCard=6, 
        ShowTrump=7, 
        StartNextGame=8,
        RefreshState=9,
        ForfeitGame=10,
    }
    public interface ICards56HubEvents
    {
        // OnError
        Task OnError(int errorCode, Cards56HubMethod hubMethodID, string errorMessage, Card56ErrorData errorData);

        Task OnStateUpdated(string jsonState);

        // Events for RegisterPlayer(string playerName);
        Task OnRegisterPlayerCompleted(Player player);
    }
    public class Cards56Hub : Hub<ICards56HubEvents>, ICards56Hub
    {
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"--> Connection Opened: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                // Try to find player by connection ID and remove them if found
                Player playerLeft = Players.GetPlayerByConnectionId(Context.ConnectionId);
                if (playerLeft != null)
                {
                    Console.WriteLine($"--> Player disconnected: {Context.ConnectionId}, Name: '{playerLeft.Name}', Table: '{playerLeft.TableName}'");
                    if (!String.IsNullOrEmpty(playerLeft.TableName))
                    {
                        TableController table = new TableController(GameTables.All[playerLeft.TableName], StateUpdated);
                        table.LeaveTable(playerLeft);
                        await Groups.RemoveFromGroupAsync(Context.ConnectionId, table.TableName);
                        if (table.TableEmpty) GameTables.RemoveTable(table.TableName);
                    }
                    Players.RemoveByPlayerId(playerLeft.PlayerID);
                }
                else
                {
                    // Somtimes it is possible that the connection ID is not present in the list of players
                    // This can happen if the player reconnects to the server and the connection ID is updated
                    Console.WriteLine($"--> Player disconnected: {Context.ConnectionId}, Name: 'Unknown'");
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public void StateUpdated(string PlayerID, string jsonState)
        {
            Clients.Client(PlayerID)?.OnStateUpdated(jsonState);
        }

        public async Task RegisterPlayer(string playerID, string playerName, string lang, bool watchOnly)
        {
            try
            {
                // If lang is not supported - default it to en-US
                if (lang == "ml" || lang == "ml-IN") 
                    lang = "ml-IN";
                else
                    lang = "en-US";
                
                Context.Items.Add("Lang", lang);
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(lang);

                // Save the player's name to a list of players
                Player player = Players.AddOrUpdatePlayer(playerID, Context.ConnectionId, playerName, lang, watchOnly);

                // Return player and the list of tables
                await Clients.Caller.OnRegisterPlayerCompleted(player);
            }
            catch (System.Exception e)
            {
                await Clients.Caller.OnError((e is Card56Exception)?((Card56Exception)e).ErrorData.ErrorCode: 0,
                    Cards56HubMethod.RegisterPlayer, 
                    e.Message, 
                    (e is Card56Exception)?((Card56Exception)e).ErrorData: null);
            }
        }

        public async Task JoinTable(int tableType, string privateTableId)
        {
            try
            {
                if (Context.Items.ContainsKey("Lang"))
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture((string)Context.Items["Lang"]);

                Player player = Players.GetPlayerByConnectionId(Context.ConnectionId) ?? throw new PlayerNotRegisteredException();
                if (!string.IsNullOrEmpty(player.TableName)) throw new PlayerAlreadyOnTableException();

                TableController table=null;
                if (string.IsNullOrEmpty(privateTableId))
                {
                    bool tableJoined=false;
                    int joinAttempts=0;
                    while (!tableJoined && joinAttempts<10)
                    {
                        joinAttempts++;
                        try
                        {
                            table = new TableController(GameTables.GetFreeTable(tableType, player.WatchOnly), StateUpdated);
                            table.JoinTable(player); // Add player to table
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

                    if (GameTables.All.ContainsKey(privateTableId)) // Private table by name already exists
                    {
                        table = new TableController(GameTables.All[privateTableId], StateUpdated);
                        table.JoinTable(player); // Add player to table
                    }
                    else // Let us add a new private table
                    {
                        table = new TableController(GameTables.AddTable(tableType, privateTableId), StateUpdated);
                        table.JoinTable(player); // Add player to table
                    }
                }

                // add new player to table group
                await Groups.AddToGroupAsync(Context.ConnectionId, table.TableName);

                // If table is full send ready to start event
                if (table.TableFull && table.Game.Stage == GameStage.WaitingForPlayers)
                {
                    table.StartNextGame(new Random().Next(table.T.MaxPlayers)); // Deal cards and initialize bidding
                }
            }
            catch (System.Exception e)
            {
                await Clients.Caller.OnError((e is Card56Exception)?((Card56Exception)e).ErrorData.ErrorCode: 0,
                    Cards56HubMethod.JoinTable, 
                    e.Message, 
                    (e is Card56Exception)?((Card56Exception)e).ErrorData: null);
            }
        }
       
        private Player GetValidPlayerOnATable()
        {
            Player player = Players.GetPlayerByConnectionId(Context.ConnectionId) ?? throw new PlayerNotRegisteredException();
            if (string.IsNullOrEmpty(player.TableName)) throw new PlayerNotOnAnyTableException();
            return player;            
        }

        public async Task PlaceBid(int bid)
        {
            try
            {
                if (Context.Items.ContainsKey("Lang"))
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture((string)Context.Items["Lang"]);

                Player player = GetValidPlayerOnATable();
                TableController table = new TableController(GameTables.All[player.TableName], StateUpdated);
                
                table.PlaceBid(player, bid);
            }
            catch (Exception e)
            {
                await Clients.Caller.OnError((e is Card56Exception)?((Card56Exception)e).ErrorData.ErrorCode: 0,
                    Cards56HubMethod.PlaceBid, 
                    e.Message, 
                    (e is Card56Exception)?((Card56Exception)e).ErrorData: null);
            }
        }

        public async Task PassBid()
        {
            try
            {
                if (Context.Items.ContainsKey("Lang"))
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture((string)Context.Items["Lang"]);

                Player player = GetValidPlayerOnATable();
                TableController table = new TableController(GameTables.All[player.TableName], StateUpdated);

                try
                {
                    table.PassBid(player);
                }
                catch (FirstBidderHasNoPointsException)// e)
                {
                    // send error message to all clients on the table in their own language
                    // foreach (Chair c in table.Game.Chairs)
                    // {
                    //     NGettext.ICatalog _catalog = new NGettext.Catalog("strings", "./locale", new CultureInfo(c.Occupant.Lang));
                    //     string message = _catalog.GetString(Cards56Error.MSG[e.ErrorData.ErrorCode]);

                    //     await Clients.Client(c.Occupant.ID).OnError(e.ErrorData.ErrorCode, Cards56HubMethod.PassBid, message, e.ErrorData);
                    // }
                    
                    // await Clients.Group(table.TableName).OnGameCompleted(table.Game.CoolieCount, table.Game.KodiCount);
                }
            }
            catch (Exception e)
            {
                await Clients.Caller.OnError((e is Card56Exception)?((Card56Exception)e).ErrorData.ErrorCode: 0,
                    Cards56HubMethod.PassBid, 
                    e.Message, 
                    (e is Card56Exception)?((Card56Exception)e).ErrorData: null);
            }
        }

        public async Task SelectTrump(string card)
        {
            try
            {
                if (Context.Items.ContainsKey("Lang"))
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture((string)Context.Items["Lang"]);

                Player player = GetValidPlayerOnATable();
                TableController table = new TableController(GameTables.All[player.TableName], StateUpdated);

                try
                {
                    table.SelectTrump(player, card);
                }
                catch (OppositeTeamHasNoTrumpCardsException)// e)
                {
                    // send error message to all clients on the table in their own language
                    // foreach (Chair c in table.Game.Chairs)
                    // {
                    //     NGettext.ICatalog _catalog = new NGettext.Catalog("strings", "./locale", new CultureInfo(c.Occupant.Lang));
                    //     string message = _catalog.GetString(Cards56Error.MSG[e.ErrorData.ErrorCode]);

                    //     await Clients.Client(c.Occupant.ID).OnError(e.ErrorData.ErrorCode, Cards56HubMethod.SelectTrump, message, e.ErrorData);
                    // }
                    
                    // await Clients.Group(table.TableName).OnGameCompleted(table.Game.CoolieCount, table.Game.KodiCount);
                }
            }
            catch (System.Exception e)
            {
                await Clients.Caller.OnError((e is Card56Exception)?((Card56Exception)e).ErrorData.ErrorCode: 0,
                    Cards56HubMethod.SelectTrump,
                    e.Message, 
                    (e is Card56Exception)?((Card56Exception)e).ErrorData: null);
            }
        }

        public async Task PlayCard(string card, int roundOverDelay)
        {
            try
            {
                if (Context.Items.ContainsKey("Lang"))
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture((string)Context.Items["Lang"]);

                Player player = GetValidPlayerOnATable();
                TableController table = new TableController(GameTables.All[player.TableName], StateUpdated);

                table.PlayCard(player, card, roundOverDelay);
            }
            catch (System.Exception e)
            {
                await Clients.Caller.OnError((e is Card56Exception)?((Card56Exception)e).ErrorData.ErrorCode: 0,
                    Cards56HubMethod.PlayCard, 
                    e.Message, 
                    (e is Card56Exception)?((Card56Exception)e).ErrorData: null);
            }
        }
        public async Task ShowTrump(int roundOverDelay = 0)
        {
            try
            {
                if (Context.Items.ContainsKey("Lang"))
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture((string)Context.Items["Lang"]);

                Player player = GetValidPlayerOnATable();
                TableController table = new TableController(GameTables.All[player.TableName], StateUpdated);

                table.ShowTrump(player);

                if (table.Game.Bid.HighBidder == player.Position)
                {
                    table.PlayCard(player, table.Game.TrumpCard, roundOverDelay);
                }
            }
            catch (System.Exception e)
            {
                await Clients.Caller.OnError((e is Card56Exception)?((Card56Exception)e).ErrorData.ErrorCode: 0,
                    Cards56HubMethod.ShowTrump, 
                    e.Message, 
                    (e is Card56Exception)?((Card56Exception)e).ErrorData: null);
            }
        }
        public async Task StartNextGame()
        {
            try
            {
                if (Context.Items.ContainsKey("Lang"))
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture((string)Context.Items["Lang"]);

                TableController table = new TableController(GameTables.All[GetValidPlayerOnATable().TableName], StateUpdated);
                table.StartNextGame(table.T.PlayerAt(table.Game.DealerPos+1)); // Deal cards and initialize bidding
            }
            catch (System.Exception e)
            {
                await Clients.Caller.OnError((e is Card56Exception)?((Card56Exception)e).ErrorData.ErrorCode: 0,
                    Cards56HubMethod.StartNextGame, 
                    e.Message, 
                    (e is Card56Exception)?((Card56Exception)e).ErrorData: null);
            }
        }
        public async Task RefreshState()
        {
            try
            {
                if (Context.Items.ContainsKey("Lang"))
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture((string)Context.Items["Lang"]);

                Player player = GetValidPlayerOnATable();
                TableController table = new TableController(GameTables.All[player.TableName], StateUpdated);
                table.SendStateUpdatedEvents(player);
            }
            catch (System.Exception e)
            {
                await Clients.Caller.OnError((e is Card56Exception)?((Card56Exception)e).ErrorData.ErrorCode: 0,
                    Cards56HubMethod.RefreshState, 
                    e.Message, 
                    (e is Card56Exception)?((Card56Exception)e).ErrorData: null);
            }
        }
        public async Task ForfeitGame()
        {
            try
            {
                if (Context.Items.ContainsKey("Lang"))
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture((string)Context.Items["Lang"]);

                Player player = GetValidPlayerOnATable();
                TableController table = new TableController(GameTables.All[player.TableName], StateUpdated);
                
                table.ForfeitGame(player);
            }
            catch (System.Exception e)
            {
                await Clients.Caller.OnError((e is Card56Exception)?((Card56Exception)e).ErrorData.ErrorCode: 0,
                    Cards56HubMethod.ForfeitGame, 
                    e.Message, 
                    (e is Card56Exception)?((Card56Exception)e).ErrorData: null);
            }
        }
    }
}
