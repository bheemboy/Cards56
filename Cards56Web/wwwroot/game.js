"use strict";

class Game
{
    constructor(registername, tableType, tablename, lang, players, table, bidPanel, watchOnly)
    {
        this.registerName = registername;
        this.tableType = tableType;
        this.tablename = tablename;
        this.lang = lang;
        this.players = players;
        this.table = table;
        this.bidPanel = bidPanel;
       
        this.table.SetNewGameClicked(this.onNewGameClicked);
        this.table.SetForfeitClicked(this.onForfeitClicked);
        this.bidPanel.setBidBtnClicked(this.onBidEvent);
        
        this.gameState;
        this.currentPlayer = -1;
        this.AutoPlayTimer = null;

        // Setup websocket connection
        this.targeturl = '/Cards56Hub';
        if (window.location.href.startsWith('file:///'))
        {
            this.targeturl = 'http://localhost:5000/Cards56Hub';
        }
        this.hubConnection = new signalR.HubConnectionBuilder()
            .withUrl(this.targeturl)
            .configureLogging(signalR.LogLevel.Information)
            .withAutomaticReconnect()
            .build();
        this.hubConnection.onreconnecting(error =>
        {
            console.assert(this.hubConnection.state === signalR.HubConnectionState.Reconnecting);
            this.table.ShowCenterMessage("Reconnecting...");
        });
        this.hubConnection.onreconnected(connectionId =>
        {
            console.assert(this.hubConnection.state === signalR.HubConnectionState.Connected);
            this.table.ShowCenterMessage("");
            this.registerPlayer(watchOnly);
        });
        this.hubConnection.onclose(error =>
        {
            this.table.ShowCenterMessage("");
            this.table.ShowAlertMessage("Connection lost. Try refreshing the page.", 0);
        });

        // Register websocket events
        this.hubConnection.on("OnError", this.onError);
        this.hubConnection.on("OnRegisterPlayerCompleted", this.onRegisterPlayerCompleted);
        this.hubConnection.on("OnStateUpdated", this.OnStateUpdated);

        this.start(watchOnly);
    }

    ///// CONNECTION EVENTS & METHODS //////////////////
    start = (watchOnly) =>
    {
        let self = this;
        try
        {
            this.hubConnection.start().then(function ()
            {
                self.registerPlayer(watchOnly);
            });
        }
        catch (error)
        {
            self.LogMessage('Error joining table: ' + self.htmlEscape(error));
        }
    }

    registerPlayer = (watchOnly) =>
    {
        let self = this;
        try
        {
            this.hubConnection.invoke("RegisterPlayer", this.registerName, this.lang, watchOnly);
        }
        catch (error)
        {
            self.LogMessage('Error Registering Player: ' + self.htmlEscape(error));
        }
    }

    onRegisterPlayerCompleted = (player) =>
    {
        this.playerID = player.playerID;
        this.hubConnection.invoke("JoinTable", this.tableType, this.tablename);
    }

    OnStateUpdated = (jsonState) =>
    {
        // Deserialize the state into an object
        this.gameState = JSON.parse(jsonState);
        this.playerID = this.gameState.PlayerID;
        this.watchOnly = this.gameState.WatchOnly;
        this.my_team = this.gameState.PlayerPosition % 2;
        this.other_team = (this.my_team + 1) % 2;
        this.currentPlayer = this.ShowCurrentPlayer();

        try
        {
            this.table.ShowAlertMessage('', 0);
            this.table.SetPlayerCardClicked(null);
            this.table.SetTrumpCardClicked(null);

            this.SetPlayerTeamsAndNames();
            this.ShowDealer();
            this.ShowBids();
            if (this.high_bid != 57)
            {
                this.table.ShowTrump((this.gameState.GameStage == 4), this.gameState.TrumpExposed, this.gameState.TrumpCard);
            }
            this.ShowRoundCards();
            this.table.ShowPlayerCards(this.gameState.PlayerCards);

            this.table.SetKoolies(
                this.my_team,
                this.gameState.TableInfo.CoolieCount[this.my_team],
                this.gameState.TableInfo.CoolieCount[this.other_team]);

            this.ShowKodies();

            this.ShowScores();

            this.bidPanel.hide();
            if (this.currentPlayer == 0 && this.gameState.GameStage == 2 && !this.watchOnly)
            {
                this.bidPanel.show(this.gameState.TableInfo.Bid.NextMinBid);
            }

            this.table.ShowCenterMessage("");
            this.table.ShowGameOverButton(false);
            if (this.currentPlayer == 0 && this.gameState.GameStage == 3)
            {
                this.table.ShowCenterMessage("Select Trump");
            }
            else if (this.gameState.GameStage == 5)
            {
                if (this.gameState.TableInfo.GameCancelled)
                {
                    this.table.ShowCenterMessage("Game Cancelled :(");
                }
                else if (this.gameState.TableInfo.GameForfeited)
                {
                    this.table.ShowCenterMessage("Game Forfeited");
                }
                else
                {
                    this.table.ShowCenterMessage("Game Over!!!");
                }
                if (this.KodiJustInstalled)
                {
                    let self = this;
                    setTimeout(self.table.ShowGameOverButton, 3000, true);
                }
                else
                {
                    this.table.ShowGameOverButton(true);
                }
            }
            else if (0 == this.currentPlayer && !this.watchOnly)
            {
                this.AutoPlayNextCardIfRequired(5000);
            }
        }
        catch (error)
        {
            alert(error);
        }
        finally
        {
            if (0 == this.currentPlayer && !this.watchOnly)
            {
                this.table.SetPlayerCardClicked(this.onPlayerCardClicked);
                this.table.SetTrumpCardClicked(this.onTrumpCardClicked);
            }
        }
    }

    onError = (errorCode, hubMethodID, message, errordata) =>
    {
        this.LogMessage(message);
    }

    onPlayerCardClicked = (card) =>
    {
        let self = this;
        try
        {
            if (this.AutoPlayTimer)
            {
                clearTimeout(this.AutoPlayTimer);
            }

            let shortName = card.shortName;
            if (shortName.substr(1) == "14")
            {
                shortName = shortName.substr(0, 1) + "1";
            }
            if (this.gameState.GameStage == 3)
            {
                this.hubConnection.invoke("SelectTrump", shortName);
            }
            else if (this.gameState.GameStage == 4)
            {
                this.hubConnection.invoke("PlayCard", shortName, 2000);
            }
        }
        catch (error)
        {
            self.LogMessage('Error in onPlayerCardClicked: ' + self.htmlEscape(error));
        }
    }

    onTrumpCardClicked = (card) =>
    {
        let self = this;
        try
        {
            this.hubConnection.invoke("ShowTrump", 2000);
        }
        catch (error)
        {
            self.LogMessage('Error in onTrumpCardClicked: ' + self.htmlEscape(error));
        }
    }

    onBidEvent = (bid) =>
    {
        let self = this;
        try
        {
            if (bid == 0)
            {
                this.hubConnection.invoke("PassBid");
            }
            else
            {
                this.hubConnection.invoke("PlaceBid", bid);
            }
        }
        catch (error)
        {
            self.LogMessage('Error placing bid: ' + self.htmlEscape(error));
        }

    }

    onNewGameClicked = () =>
    {
        let self = this;
        try
        {
            this.hubConnection.invoke("StartNextGame");
        }
        catch (error)
        {
            self.LogMessage('Error Starting Next Game: ' + self.htmlEscape(error));
        }
    }

    onForfeitClicked = () =>
    {
        let self = this;
        try
        {
            if (confirm("Are you sure you want to forfeit this game?"))
            {
                this.hubConnection.invoke("ForfeitGame");
            }
        }
        catch (error)
        {
            self.LogMessage('Error Forfeiting Game: ' + self.htmlEscape(error));
        }
    }

    ///// UI HELPER METHODS ////////////////////
    _get_offsetted_player = (posn) =>
    {
        return (posn - this.gameState.PlayerPosition + this.gameState.TableInfo.MaxPlayers) % this.gameState.TableInfo.MaxPlayers;
    }

    SetPlayerTeamsAndNames = () =>
    {
        for (let i = 0; i < this.gameState.TableInfo.Chairs.length; i++)
        {
            let chair = this.gameState.TableInfo.Chairs[i];
            let player_posn = this._get_offsetted_player(chair.Position);
            if (chair.Occupant)
            {
                this.players[player_posn].set_name(chair.Occupant.Name);
            }
            else
            {
                this.players[player_posn].set_name("");
            }
            this.players[player_posn].set_team(chair.Position % 2);
            this.players[player_posn].set_watchers(chair.Watchers);
        }
    }

    ShowDealer = () =>
    {
        let dealer_posn = this._get_offsetted_player(this.gameState.TableInfo.DealerPos);
        for (let i = 0; i < this.gameState.TableInfo.Chairs.length; i++)
        {
            this.players[i].set_dealer((i == dealer_posn));
        }
    }

    ShowKodies = () =>
    {
        this.KodiJustInstalled = false;
        for (let i = 0; i < this.gameState.TableInfo.Chairs.length; i++)
        {
            let chair = this.gameState.TableInfo.Chairs[i];
            this.players[this._get_offsetted_player(i)].show_kodies(chair.KodiCount, chair.KodiJustInstalled);
            if (chair.KodiJustInstalled)
            {
                this.KodiJustInstalled = true;
            }
        }
    }

    ShowScores = () =>
    {
        let str_my_team_score = '';
        let str_opponent_score = '';
        if (this.gameState.TableInfo.TeamScore && this.gameState.GameStage >= 4 && !this.gameState.TableInfo.GameCancelled && !this.gameState.TableInfo.GameForfeited)
        {
            let my_team_target;
            let other_team_target;
            let bidding_team = this.gameState.TableInfo.Bid.HighBidder % 2;
            
            if (this.high_bid == 57)
            {
              if (bidding_team == this.my_team)
              {
                my_team_target = 8;
                other_team_target = 1;
              }
              else
              {
                my_team_target = 1;
                other_team_target = 8;
              }
            }
            else
            {
              if (bidding_team == this.my_team)
              {
                my_team_target = this.high_bid;
                other_team_target = 56-this.high_bid+1;
              }
              else
              {
                my_team_target = 56-this.high_bid+1;
                other_team_target = this.high_bid;
              }
            }
            str_my_team_score = this.gameState.TableInfo.TeamScore[this.my_team] + "/" + my_team_target;
            str_opponent_score = this.gameState.TableInfo.TeamScore[this.other_team] + "/" + other_team_target;
        }
        this.table.ShowScores(this.my_team, str_my_team_score, str_opponent_score);
    }

    ShowBids = () =>
    {
        this.high_bidder_posn = -1;
        this.high_bid = -1;

        var bidShown = new Array(this.gameState.TableInfo.Chairs.length);
        for (let i = 0; i < this.gameState.TableInfo.Chairs.length; i++)
        {
            this.players[i].clear_bid();
            bidShown[i] = false;
        }

        if (this.gameState.TableInfo.Bid)
        {
            this.high_bidder_posn = this._get_offsetted_player(this.gameState.TableInfo.Bid.HighBidder);
            this.high_bid = this.gameState.TableInfo.Bid.HighBid;

            if (this.gameState.GameStage >= 2 && this.gameState.GameStage <= 4)
            {
                // Show Highbid
                this.players[this.high_bidder_posn].set_high_bid(this.high_bid);
                bidShown[this.high_bidder_posn] = true;
    
                if (this.gameState.GameStage == 2)
                {
                    for (let i = this.gameState.TableInfo.Bid.BidHistory.length-1; i >= 0; i--)
                    {
                        var player_posn = this._get_offsetted_player(this.gameState.TableInfo.Bid.BidHistory[i].Position);
                        if (!bidShown[player_posn])
                        {
                            bidShown[player_posn] = true;
                            if (i == this.gameState.TableInfo.Bid.BidHistory.length-1)
                            {
                                this.players[player_posn].set_current_bid(this.gameState.TableInfo.Bid.BidHistory[i].Bid);
                            }
                            else
                            {
                                this.players[player_posn].set_previous_bid(this.gameState.TableInfo.Bid.BidHistory[i].Bid);
                            }
                        }
                    }
                }
            }    
        }
    }

    ShowCurrentPlayer = () =>
    {
        let currentPlayer = null;
        if (this.gameState.GameStage == 2 || this.gameState.GameStage == 3)
        {
            currentPlayer = this.gameState.TableInfo.Bid.NextBidder;
        }
        else if (this.gameState.GameStage == 4 && this.gameState.TableInfo.Rounds.length > 0)
        {
            // find the last round
            currentPlayer = this.gameState.TableInfo.Rounds[this.gameState.TableInfo.Rounds.length - 1].NextPlayer;
        }
        if (currentPlayer != -1)
        {
            currentPlayer = this._get_offsetted_player(currentPlayer);
        }
        if (this.gameState.GameStage >= 2)
        {
            for (let i = 0; i < this.gameState.TableInfo.Chairs.length; i++)
            {
                this.players[i].set_focus((i == currentPlayer));
            }
        }
        return currentPlayer;
    }

    ShowRoundCards = () =>
    {
        if (this.gameState.TableInfo.Rounds && this.gameState.TableInfo.Rounds.length > 0)
        {
            let round_cards = this.gameState.TableInfo.Rounds[this.gameState.TableInfo.Rounds.length - 1].PlayedCards;
            let first_player = this.gameState.TableInfo.Rounds[this.gameState.TableInfo.Rounds.length - 1].FirstPlayer;
            this.table.ShowRoundCards((this.high_bid == 57), this._get_offsetted_player(first_player), round_cards);

            if (this.gameState.TableInfo.Rounds.length >= 2)
            {
                let prev_round_cards = this.gameState.TableInfo.Rounds[this.gameState.TableInfo.Rounds.length - 2].PlayedCards
                this.table.ShowLastRoundCards(prev_round_cards);
            }
            else
            {
                this.table.ShowLastRoundCards([]);
            }
        }
        else
        {
            this.table.ShowRoundCards(false, 0, []);
            this.table.ShowLastRoundCards([]);
        }
    }

    AutoPlayNextCardIfRequired = (timeout) =>
    {
        this.AutoPlayTimer = null;
        // Check if autoplay has been requested 
        if (this.gameState.TableInfo.Rounds && this.gameState.TableInfo.Rounds.length > 0)
        {
            var autoplaycard = this.gameState.TableInfo.Rounds[this.gameState.TableInfo.Rounds.length - 1].AutoPlayNextCard; 
            if (autoplaycard != "")
            {
                // sleep a little and then play the card
                var self = this;
                this.AutoPlayTimer = setTimeout(function() 
                {
                    self.table.SetPlayerCardClicked(null);
                    self.hubConnection.invoke("PlayCard", autoplaycard, 2000);
                }, timeout);
            }
        }
    }

    htmlEscape = (str) =>
    {
        return str.toString().replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/'/g, '&#39;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
    }
    LogMessage = (message) =>
    {
        this.table.ShowAlertMessage(message, 2500);
    }
}
