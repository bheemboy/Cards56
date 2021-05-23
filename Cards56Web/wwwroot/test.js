"use strict";

class GamePanel
{
    constructor(id, registername, tableType, lang){
        this.id = id;
        this.registerName = registername;
        this.tableType = tableType;
        this.lang = lang;
        this.doc = document;

        // Bunch of UI elements
        this.divplayer = document.getElementById("divplayer_" + id);
        this.joinButton = document.getElementById("joinButton_" + id);
        this.closeButton = document.getElementById("closeButton_" + id);
        this.bidNumber = document.getElementById("bidNumber_" + id);
        this.bidButton = document.getElementById("bidButton_" + id);
        this.passButton = document.getElementById("passButton_" + id);
        this.startNextGameButton = document.getElementById("startNextGameButton_" + id);
        this.commsLog = document.getElementById("commsLog_" + id);
        this.playerName = document.getElementById("playerName_" + id);
        this.playerName.innerHTML = this.registerName + " [_]";
        this.message = document.getElementById("message_" + id);
        this.scoreMessage = document.getElementById("score_" + id);
        this.higBid = document.getElementById("highBid_" + id);
        this.selectedTrump = document.getElementById("selectedTrump_" + id);
        this.showTrumpBtn = document.getElementById("showTrump_" + id);
        this.roundsLabel = document.getElementById("rounds_" + id);

        // Register button clicks
        this.joinButton.onclick = this.join;
        this.closeButton.onclick = this.close;
        this.bidButton.onclick = this.placeBid;
        this.passButton.onclick = this.passBid;
        this.showTrumpBtn.onclick = this.showTrump;
        this.startNextGameButton.onclick = this.startNextGame;

        // Setup websocket connection
        this.targeturl =  '/Cards56Hub';
        if (window.location.href.startsWith('file:///')) {
            this.targeturl = 'http://localhost:5000/Cards56Hub';
        }
        this.hubConnection = new signalR.HubConnectionBuilder().withUrl(this.targeturl).build();

        // Register websocket events
        this.hubConnection.onclose(function (event) {this.onClose();});
        this.hubConnection.on("OnError", this.onError);
        this.hubConnection.on("OnRegisterPlayerCompleted", this.onRegisterPlayerCompleted);
        this.hubConnection.on("OnStateUpdated", this.OnStateUpdated);
    }

    ///// EVENTS ///////////////////////////
    onRegisterPlayerCompleted = (player) =>
    {
        this.hubConnection.invoke("JoinTable", this.tableType, "");
    }

    onClose = () =>
    {
        this.InitializeView();
        this.commsLog.innerHTML = '';
    }

    OnStateUpdated = (jsonState) =>
    {
        // Deserialize the state into an object
        var gameState = JSON.parse(jsonState);
        
        // ResetGameState => disable, clear, hide all controls
        this.InitializeView();

        // Since we are connected disable join button and enable close button
        this.joinButton.disabled = true;
        this.closeButton.disabled = false;

        // Show player position from state info
        this.playerName.innerHTML = this.registerName + ' [' + gameState.PlayerPosition + ']';

        // show scores, cards and few other generic values from state info
        this.ShowCommonStateInfo(gameState);

        // Show stage specific 
        switch(gameState.GameStage)
        {
            case 1: // WaitingForPlayers
                // show the game state 
                this.message.innerHTML = "Waiting for players...";
                break;
            
            case 2: // Bidding
                // If it is my turn to bid, enable bid/pass buttons and fill in the min bid
                if (gameState.PlayerPosition == gameState.TableInfo.Bid.NextBidder) this.RequestBid(gameState);
                break;

            case 3: // SelectingTrump
                // If it is my turn then show selecttrump status and enable card buttons
                if (gameState.PlayerPosition == gameState.TableInfo.Bid.NextBidder) this.RequestTrumpSelection(gameState);
                break;

            case 4: // PlayingCards
                // if it is my turn to play cards, show play card status
                var currentRound = gameState.TableInfo.Rounds.length;
                var nextPlayerPosn = gameState.TableInfo.Rounds[currentRound-1].NextPlayer; 
                if (gameState.PlayerPosition == nextPlayerPosn) this.RequestPlayCard(gameState);
                break;

            case 5: // GameOver
                if (gameState.TableInfo.WinningTeam % 2 == gameState.TableInfo.Bid.HighBidder % 2)
                    this.message.innerHTML = "Game over...bidding team won!!!";
                else
                    this.message.innerHTML = "Game over...bidding team lost!!!";

                // enable start new game button
                this.startNextGameButton.disabled = false;
                break;

            default: // Unknown
                alert("Unexpected game stage '" + gameState.GameStage + "'");
                break;
        } 
    }

    onError = (errorCode, hubMethodID, message, errordata) =>
    {
        var msg = "Error [" + errorCode + "]";
        switch(hubMethodID) 
        {
            case 1:
                msg += " in RegisterPlayer ";
                break;
            case 2:
                msg += " in JoinTable ";
                break;
            case 3:
                msg += " in PlaceBid ";
                break;
            case 4:
                msg += " in PassBid ";
                break;
            case 5:
                msg += " in SelectTrump ";
                break;
            case 6:
                msg += " in PlayCard ";
                break;
            case 7:
                msg += " in ShowTrump ";
                break;
            case 8:
                msg += " in StartNextGame ";
                break;
            case 9:
                msg += " in RefreshState ";
                break;
        }

        if (typeof errordata !== "undefined" && errordata !== null) 
        {
            msg += "[" + errordata.errorName + "] ";
        }
        msg += message;

        this.LogMessageFromServer(msg);
    }



    ///// ON CLICK METHODS //////////////////
    join = () =>
    {
        var self = this;
        try 
        {
            this.hubConnection.start().then(function () {
                self.hubConnection.invoke("RegisterPlayer", self.registerName, self.lang, false); //
            });
        } 
        catch (error) 
        {
            self.LogMessageFromServer('Error joining table: ' + self.htmlEscape(error.message));
        }
    }

    close = () =>
    {
        if(!this.hubConnection || this.hubConnection.state !== "Connected") {
            alert("Hub Not Connected");
        }
        else {
            var self = this;
            this.hubConnection.stop().then(function () {
                self.InitializeView();
                self.commsLog.innerHTML = '';
            });
        }
    }

    placeBid = () =>
    {
        var self = this;
        try
        {
            this.hubConnection.invoke("PlaceBid", parseInt(this.bidNumber.value));
        }
        catch (error) 
        {
            self.LogMessageFromServer('Error placing bid: ' + self.htmlEscape(error.message));
        }
    }

    passBid = () =>
    {
        var self = this;
        try
        {
            this.hubConnection.invoke("PassBid");
        }
        catch (error) 
        {
            self.LogMessageFromServer('Error passing bid: ' + self.htmlEscape(error.message));
        }    
    }

    selectTrumpClicked = (btnId) =>
    {
        var self = this;
        try
        {
            var card = this.doc.getElementById(btnId).innerHTML;
            this.hubConnection.invoke("SelectTrump", card);
        }
        catch (error)
        {
            self.LogMessageFromServer('Error placing bid: ' + self.htmlEscape(error.message));
        }
    }
    
    playCardClicked = (btnId) =>
    {
        var self = this;
        try
        {
            var card = this.doc.getElementById(btnId).innerHTML;
            this.hubConnection.invoke("PlayCard", card, 2000);
        }
        catch (error)
        {
            self.LogMessageFromServer('Error placing bid: ' + self.htmlEscape(error.message));
        }
    }

    showTrump = () =>
    {
        var self = this;
        try
        {
            this.hubConnection.invoke("ShowTrump", 2000);
        }
        catch (error) 
        {
            self.LogMessageFromServer('Error showing trump: ' + self.htmlEscape(error.message));
        }
    }

    startNextGame = () =>
    {
        var self = this;
        try
        {
            this.hubConnection.invoke("StartNextGame");
        }
        catch (error) 
        {
            self.LogMessageFromServer('Error Starting Next Game: ' + self.htmlEscape(error.message));
        }
    }


    ///// UI HELPER METHODS ////////////////////
    InitializeView = () =>
    {
        // disable all buttons
        this.joinButton.disabled = false;
        this.closeButton.disabled = true;
        this.bidButton.disabled = true;
        this.bidButton.innerHTML = "Bid";
        this.passButton.disabled = true;
        this.startNextGameButton.disabled = true;
        for (var i=0; i<8; i++)
        {
            var cardBtn = this.doc.getElementById("card" + i + "_" + this.id); 
            cardBtn.disabled = true;
            cardBtn.style = "";
            cardBtn.innerHTML = "_";
        }
        this.showTrumpBtn.disabled = true;
        this.showTrumpBtn.innerHTML = "Show Trump";
        this.higBid.innerHTML = "";
        this.selectedTrump.innerHTML = "";

        // reset player position
        this.playerPosition = -1;
        this.playerName.innerHTML = this.registerName + ' [_]';

        // reset highlighting
        this.divplayer.style = "height: 75px; padding: 8px;";
        
        // clear messages
        this.message.innerHTML = "";

        // clear scores
        this.scoreMessage.innerHTML = "";

        // Clear rounds
        this.roundsLabel.innerHTML = "";
    }

    ShowCommonStateInfo = (gameState) =>
    {
        if (gameState.TableInfo.TeamScore != null) this.scoreMessage.innerHTML += "Score: " + gameState.TableInfo.TeamScore[gameState.PlayerPosition % 2];
        this.scoreMessage.innerHTML += ", C:" + gameState.TableInfo.CoolieCount[gameState.PlayerPosition % 2];
        this.scoreMessage.innerHTML += ", K:" + gameState.TableInfo.Chairs[gameState.PlayerPosition].KodiCount;

        if (gameState.PlayerCards != null)
        {
            gameState.PlayerCards.forEach((card, index) =>
            {
                var btnId = "card" + index + "_" + this.id;
                var cardBtn = this.doc.getElementById(btnId);
                
                if (card[0] == 'd') cardBtn.style = "background-color: lightblue;";
                else if (card[0] == 'h') cardBtn.style = "background-color: yellow;";
                else if (card[0] == 'c') cardBtn.style = "background-color: orchid;";
                else cardBtn.style = "background-color: lightgreen;";
                cardBtn.innerHTML = card;
            });
        }

        if (gameState.GameStage == 3 || gameState.GameStage == 4)
            this.higBid.innerHTML = " HighBid: " + gameState.TableInfo.Bid.HighBid;
        if (gameState.TrumpExposed)
            this.selectedTrump.innerHTML = " Trump: " + gameState.TrumpCard;

        var rounds = gameState.TableInfo.Rounds; 
        if (rounds != null)
        {
            if (rounds.length >= 2)
            {
                var firstPlayerPosn = rounds[rounds.length-2].FirstPlayer;
                var playerName = gameState.TableInfo.Chairs[firstPlayerPosn].Occupant.Name;
                this.roundsLabel.innerHTML += "Last Round Started by " + playerName;
                this.roundsLabel.innerHTML += " [" + rounds[rounds.length-2].PlayedCards + "]  ";
            }
            if (rounds.length >= 1)
            {
                var firstPlayerPosn = rounds[rounds.length-1].FirstPlayer;
                var playerName = gameState.TableInfo.Chairs[firstPlayerPosn].Occupant.Name;
                this.roundsLabel.innerHTML += "Current Round Started by " + playerName;
                this.roundsLabel.innerHTML += " [" + rounds[rounds.length-1].PlayedCards + "]";
            }
        }
    }

    RequestBid = (gameState) =>
    {
        this.message.innerHTML = "Place your bid...";
        this.divplayer.style = "height: 75px; padding: 8px; background-color:lime";
        this.bidButton.innerHTML = 'Bid [' + gameState.TableInfo.Bid.NextMinBid + '..57]';
        this.bidNumber.value = gameState.TableInfo.Bid.NextMinBid;
        this.bidButton.disabled = false;
        this.passButton.disabled = false;
    }

    RequestTrumpSelection = (gameState) =>
    {
        this.message.innerHTML = "Select trump card...";
        this.divplayer.style = "height: 75px; padding: 8px; background-color:deepskyblue";

        // Enable card buttons and assign onClick methods
        var self = this;
        gameState.PlayerCards.forEach((card, index) =>
        {
            var btnId = "card" + index + "_" + this.id;
            var cardBtn = this.doc.getElementById(btnId);
            cardBtn.disabled = false;
            cardBtn.onclick = (function(j){
                return function() {
                    self.selectTrumpClicked(j);
                }; 
            })(btnId);
        });
    }

    RequestPlayCard = (gameState) =>
    {
        this.message.innerHTML = "Play a card...";
        this.divplayer.style = "height: 75px; padding: 8px; background-color:coral";

        // Disable the show trump button if trump is open 
        this.showTrumpBtn.disabled = gameState.TrumpExposed;

        // Enable card buttons and assign onClick methods
        var self = this;
        gameState.PlayerCards.forEach((card, index) =>
        {
            var btnId = "card" + index + "_" + this.id;
            var cardBtn = this.doc.getElementById(btnId);
            cardBtn.disabled = false;
            cardBtn.onclick = (function(j){
                return function() {
                    self.playCardClicked(j);
                }; 
            })(btnId);
        });
    }

    htmlEscape = (str) => 
    {
        return str.toString().replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/'/g, '&#39;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
    }
    LogMessage = (from, to, bgcolor, message) => 
    {
        this.commsLog.innerHTML += '<tr style="background-color:' + bgcolor + ';"><td>' + this.htmlEscape(from) + '</td><td>' + this.htmlEscape(to) + '</td><td>' + this.htmlEscape(message) + '</td></tr>';
    }    
    LogMessageToServer = (message) => {this.LogMessage('Client', 'Server', '#FFFFE0', message);}
    LogMessageFromServer = (message) => {this.LogMessage('Server', 'Client', 'lightblue', message);}
}
