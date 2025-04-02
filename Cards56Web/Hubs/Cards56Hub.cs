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
        private GameController _GameController 
        { 
            get
            {
                string lang = Context.Items.TryGetValue("Lang", out object? value) ? value?.ToString() ?? "en-US" : "en-US";
                return new GameController(Context.ConnectionId, lang, OnStateUpdated);
            }
        }
        private async Task HandleCard56Exception(Exception e, Cards56HubMethod hubMethod)
        {
            int errorCode = 0;
            Card56ErrorData errorData = new Card56ErrorData();
            
            if (e is Card56Exception card56Exception)
            {
                errorCode = card56Exception.ErrorData!.ErrorCode;
                errorData = card56Exception.ErrorData;
            }
            
            await Clients.Caller.OnError(
                errorCode,
                hubMethod,
                e.Message,
                errorData);
        }
        public void OnStateUpdated(string connID, string jsonState)
        {
            Clients.Client(connID)?.OnStateUpdated(jsonState);
        }
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"--> Connection Opened: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                _GameController.DisconnectPlayer(); // Disconnect player from game
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            await base.OnDisconnectedAsync(exception);
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

                Player player = _GameController.AddOrUpdatePlayer(playerID, playerName, watchOnly); // Add or update player

                // Return player
                await Clients.Caller.OnRegisterPlayerCompleted(player);
            }
            catch (Exception e)
            {
                await HandleCard56Exception(e, Cards56HubMethod.RegisterPlayer);
            }
        }

        public async Task JoinTable(int tableType, string privateTableId)
        {
            try
            {
                TableController table = _GameController.JoinTable(tableType, privateTableId); // Join table
            }
            catch (Exception e)
            {
                await HandleCard56Exception(e, Cards56HubMethod.JoinTable);
            }
        }
        public async Task PlaceBid(int bid)
        {
            try
            {
                _GameController.PlaceBid(bid);
            }
            catch (Exception e)
            {
                await HandleCard56Exception(e, Cards56HubMethod.PlaceBid);
            }
        }

        public async Task PassBid()
        {
            try
            {
                _GameController.PassBid();
            }
            catch (Exception e)
            {
                await HandleCard56Exception(e, Cards56HubMethod.PassBid);
            }
        }

        public async Task SelectTrump(string card)
        {
            try
            {
                _GameController.SelectTrump(card);
            }
            catch (Exception e)
            {
                await HandleCard56Exception(e, Cards56HubMethod.SelectTrump);
            }
        }

        public async Task PlayCard(string card, int roundOverDelay)
        {
            try
            {
                _GameController.PlayCard(card, roundOverDelay);
            }
            catch (Exception e)
            {
                await HandleCard56Exception(e, Cards56HubMethod.PlayCard);
            }
        }
        public async Task ShowTrump(int roundOverDelay = 0)
        {
            try
            {
                _GameController.ShowTrump(roundOverDelay);
            }
            catch (Exception e)
            {
                await HandleCard56Exception(e, Cards56HubMethod.ShowTrump);
            }
        }
        public async Task StartNextGame()
        {
            try
            {
                _GameController.StartNextGame();
            }
            catch (Exception e)
            {
                await HandleCard56Exception(e, Cards56HubMethod.StartNextGame);
            }
        }
        public async Task RefreshState()
        {
            try
            {
                _GameController.RefreshState();
            }
            catch (Exception e)
            {
                await HandleCard56Exception(e, Cards56HubMethod.RefreshState);
            }
        }
        public async Task ForfeitGame()
        {
            try
            {
                _GameController.ForfeitGame();
            }
            catch (Exception e)
            {
                await HandleCard56Exception(e, Cards56HubMethod.ForfeitGame);
            }
        }
    }
}
