using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Linq.Expressions;

namespace Cards56Lib
{
    public delegate void StateUpdatedDelegate(string PlayerID, string jsonState);
    public class TableController
    {
        public GameTable Game {get;}
        private StateUpdatedDelegate StateUpdated;

        private DeckController DeckCtl;
        public TableType T => Game.T;
        public bool IsThani => Game.Bid.HighBid == T.MaxBid;
        public string TableName => Game.TableName;
        public bool AutoPlayedToCompletion;
        public bool TableFull => Game.TableFull;
        public bool TableEmpty => Game.Chairs.TrueForAll(c => c.Occupant == null) && Game.Chairs.TrueForAll(c => c.Watchers.Count <= 0);
        private int[] NextBidderFromTeam => Game.Bid.NextBidderFromTeam;
        private int[] OutBidChance => Game.Bid.OutBidChance;
        private int TeamScoreOf(int position) => Game.TeamScore[T.TeamOf(position)]; 
        private bool CurrentRoundAllCardsPlayed => CurrentRound!.PlayedCards.Count >= (IsThani? T.PlayersPerTeam+1: T.MaxPlayers); 
        private int CurrentRoundWinningTeam => CurrentRoundAllCardsPlayed ? CurrentRound!.Winner % 2 : -1; 
        public RoundInfo? CurrentRound => ((Game.Rounds?.Count ?? 0) > 0) ? Game.Rounds?.Last() : null;
        public char CurrentRoundSuit => ((CurrentRound?.PlayedCards.Count ?? 0) > 0)? CurrentRound!.PlayedCards[0][0] : ' ';
        public bool AllowBidPass => Game.Bid.HighBid>=28;
        private string PlayerName(int posn) => Game.Chairs[posn].Occupant?.Name ?? posn.ToString();
        private List<string> CardsAt(int posn) => Game.Chairs[posn].Cards;

        public TableController(GameTable gameTable, StateUpdatedDelegate stateUpdated)
        {
            Game = gameTable;
            StateUpdated = stateUpdated;
            DeckCtl = new DeckController(Game.T, Game.Deck);
        }
        public IReadOnlyList<Player> CurrentPlayers
        {
            get
            {
                lock (Game)
                {
                    List<Player> players = new List<Player>();
                    Game.Chairs.ForEach(c => {if (c.Occupant!=null) players.Add(c.Occupant);});
                    return players.AsReadOnly();
                }
            }
        }
        public int JoinTable(Player player)
        {
            lock (Game)
            {
                if (string.IsNullOrEmpty(player.TableName))
                {
                    if (!player.WatchOnly && TableFull) throw new TableIsFullException();

                    int watchersMinCount = Game.Chairs.Min(c => c.Watchers.Count); // for round robin allocation

                    Chair freeChair = Game.Chairs.First(c => player.WatchOnly? c.Watchers.Count<=watchersMinCount: c.Occupant==null); 
                    if(!player.WatchOnly) 
                    {
                        freeChair.Occupant = player;
                    }
                    else
                    {
                        if (freeChair.Watchers.Count > 2) throw new TooManyWatchersOnChairException();
                        freeChair.Watchers.Add(player);
                    }
                    player.TableName = TableName;
                    player.Position = freeChair.Position;
                    Console.WriteLine($"--> Player '{player.Name}({player.ConnID})' joined gametable: '{player.TableName}' @ position: {player.Position}");
                }
                else
                {
                    Console.WriteLine($"--> Player '{player.Name}({player.ConnID})' already on gametable: '{player.TableName}' @ position: {player.Position}");
                }
                
                SendStateUpdatedEvents();

                return player.Position;
            }
        }
        public string GetJsonState(Player player)
        {
            var state = new 
            {
                PlayerPosition = player.Position,
                PlayerID = player.PlayerID,
                WatchOnly = player.WatchOnly,
                TableFull = TableFull,
                GameStage = Game.Stage,
                PlayerCards = Game.Chairs[player.Position].Cards,
                TrumpExposed = Game.TrumpExposed,
                CurrentRoundSuit = CurrentRoundSuit,
                // Include Trumpcard only after trump is exposed or the player is the highbidder 
                TrumpCard = ((player.Position==(Game.Bid?.HighBidder ?? -1)) || (Game.TrumpExposed))? Game.TrumpCard: "",
                TableInfo = Game
            };

            string jsonTxt = JsonConvert.SerializeObject(state, Formatting.Indented);
#if DEBUG
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                System.IO.File.WriteAllText(Path.GetTempPath() + $"GameState-{player.Position}.json", jsonTxt);
            }
#endif
            return jsonTxt;
        }

        public void SendStateUpdatedEvents(Player? player =null)
        {
            if (StateUpdated!=null)
            {
                if (player != null)
                {
                    StateUpdated(player.ConnID, GetJsonState(player));
                }
                else
                {
                    foreach (Chair c in Game.Chairs)
                    {
                        if (c.Occupant != null)
                        {
                            StateUpdated(c.Occupant.ConnID, GetJsonState(c.Occupant));
                        }
                        foreach (var w in c.Watchers)
                        {
                            StateUpdated(w.ConnID, GetJsonState(w));
                        }
                    }
                }
            }
        }

        public void LeaveTable(Player player)
        {
            lock (Game)
            {
                if (player.TableName != TableName)
                {
                    Console.WriteLine($"RemovePlayer Failed. {player.TableName} != {TableName}");
                    return;
                }

                // clear out the player from the table depeding on the player in proxy or not
                if(!player.WatchOnly) 
                {
                    Game.Chairs[player.Position].Occupant = null;
                }
                else 
                {
                    Game.Chairs[player.Position].Watchers.Remove(player);
                }
                player.TableName = "";
                player.Position = -1;
                
                SendStateUpdatedEvents();
            }
        }

        public void StartNextGame(int dealerPos)
        {
            lock (Game)
            {
                if (!TableFull) throw new NotEnoughPlayersOnTableException();
                if (Game.Stage > GameStage.WaitingForPlayers && Game.Stage < GameStage.GameOver) throw new GameNotOverException();

                InitializeNextGame(dealerPos);
                Game.Stage = GameStage.Bidding;

                // Deal cards
                List<string>[] dealtCards = DeckCtl.DealCards();
                for(int i=0; i<Game.Chairs.Count; i++)
                {
                    Game.Chairs[i].Cards = dealtCards[i];
                    Game.Chairs[i].KodiJustInstalled = false;
                }
                SendStateUpdatedEvents();
            }
        }
        public void PlaceBid(Player player, int bid)
        {
            lock (Game)
            {
                if (Game.Stage < GameStage.Bidding) throw new BiddingNotStartedException();
                if (Game.Stage > GameStage.Bidding) throw new BiddingOverException();
                if (player.Position != Game.Bid.NextBidder) throw new NotPlayersTurnException(PlayerName(Game.Bid.NextBidder));
                if (bid < Game.Bid.NextMinBid || bid > T.MaxBid) throw new BidOutOfRangeException(Game.Bid.NextMinBid, T.MaxBid);
                
                int LastHighBidder = Game.Bid.HighBidder;

                // set high bid and highbidder
                Game.Bid.HighBid = bid;
                Game.Bid.HighBidder = player.Position;
                Game.Bid.BidHistory.Add(new BidPass(player.Position, bid));

                // Update this players outbidchance
                OutBidChance[player.Position] = bid;
                // Update next bidder for my team
                NextBidderFromTeam[T.TeamOf(player.Position)] = T.PlayerAt(player.Position+2);

                // If thani called
                if (bid == T.MaxBid)
                {
                    // Update game stage
                    Game.Stage = GameStage.PlayingCards;
                    // Add new round to start the game
                    Game.Rounds.Add(new RoundInfo(Game.Bid.HighBidder));
                }
                // bidding is over if this player bid over himself
                else if (LastHighBidder == player.Position)
                {
                    // Bidding over. So update game stage
                    Game.Stage = GameStage.SelectingTrump;
                }
                // Continue Bidding
                else
                {
                    // Give turn to next player on other team
                    Game.Bid.NextBidder = NextBidderFromTeam[T.TeamOf(player.Position+1)];

                    // and update NextBidderFromTeam for other team
                    NextBidderFromTeam[T.TeamOf(Game.Bid.NextBidder)] = T.PlayerAt(Game.Bid.NextBidder+2);

                    // minimum is the higher of current highbid+1 and the minimum for his next stage
                    Game.Bid.NextMinBid = Math.Max(Game.Bid.HighBid+1, T.NextMinBidAfterBid(OutBidChance[Game.Bid.NextBidder]));
                }
                SendStateUpdatedEvents();
            }
        }

        public void PassBid(Player player)
        {
            lock (Game)
            {
                try
                {
                    if (Game.Stage < GameStage.Bidding) throw new BiddingNotStartedException();
                    if (Game.Stage > GameStage.Bidding) throw new BiddingOverException();
                    if (Game.Bid.NextBidder != player.Position) throw new NotPlayersTurnException(PlayerName(Game.Bid.NextBidder));
                    if (!AllowBidPass) throw new PassNotAllowedException();

                    // bidding is over when the player with the high bid is passing bid
                    if (Game.Bid.HighBidder == player.Position)
                    {
                        Game.Stage = GameStage.SelectingTrump;
                    }
                    // we are still bidding 
                    else
                    {
                        // Update this players outbidchance
                        OutBidChance[player.Position] = Game.Bid.NextMinBid; // Game.Bid.HighBid;
                        // Update NextBidderFromTeam for my team
                        NextBidderFromTeam[T.TeamOf(player.Position)] = T.PlayerAt(player.Position+2);

                        // determine who gets the next chance
                        // if I am from the highbidding team then turn goes to my next team mate.
                        if (T.SameTeam(player.Position, Game.Bid.HighBidder))
                        {
                            Game.Bid.NextBidder = NextBidderFromTeam[T.TeamOf(player.Position)];
                        }
                        // I am from non-bidding team then
                        else
                        {
                            // If next bidder from my team has not had a chance to outbid the current highbid give him the turn.
                            if (OutBidChance[NextBidderFromTeam[T.TeamOf(player.Position)]] < Game.Bid.HighBid + 1)
                            {
                                Game.Bid.NextBidder = NextBidderFromTeam[T.TeamOf(player.Position)];
                            }
                            else
                            {
                                // Give turn to next player on other team
                                Game.Bid.NextBidder = NextBidderFromTeam[T.TeamOf(player.Position+1)];
                            }
                        }

                        // Update NextBidderFromTeam for NextBidder
                        NextBidderFromTeam[T.TeamOf(Game.Bid.NextBidder)] = T.PlayerAt(Game.Bid.NextBidder+2);

                        // Set next min bid
                        // if next bidder is from the current highbid team
                        if (T.SameTeam(Game.Bid.NextBidder, Game.Bid.HighBidder))
                        {
                            // minimum is the minimum for the next stage after the high bid stage
                            Game.Bid.NextMinBid = Math.Max(Game.Bid.HighBid+1, T.NextMinBidAfterBid(Game.Bid.HighBid));
                        }
                        // next bidder is not from the current highbid team 
                        else
                        {
                            // minimum is the higher of current highbid+1 or the minimum for his next stage
                            Game.Bid.NextMinBid = Math.Max(Game.Bid.HighBid+1, T.NextMinBidAfterBid(OutBidChance[Game.Bid.NextBidder]));
                        }
                    }
                    Game.Bid.BidHistory.Add(new BidPass(player.Position, 0));
                }
                catch (PassNotAllowedException)
                {
                    // Passbig is not allowed for first bidder in close trump games.
                    // But if the player has all zero points cards it is ok to pass bid.
                    // Game will be cancelled though.
                    int totalPoints = 0;
                    CardsAt(player.Position).ForEach(c => totalPoints+=T.PointsForCard(c));

                    if (totalPoints == 0)
                    {
                        // cancel the game
                        Game.Chairs.ForEach(c => DeckCtl.ReturnCards(c.Cards));
                        if (!Game.TrumpExposed && Game.TrumpCard != "") DeckCtl.ReturnCard(Game.TrumpCard);
                        InitializeNextGame(Game.DealerPos);
                        Game.Stage = GameStage.GameOver;
                        Game.GameCancelled = true;

                        // throw exception
                        throw new FirstBidderHasNoPointsException();
                    }
                    else
                    {
                        throw;
                    }
                }
                finally
                {
                    SendStateUpdatedEvents();
                }
            }
        }
        public void SelectTrump(Player player, string card)
        {
            lock (Game)
            {
                try
                {
                    if (!CardsAt(player.Position).Contains(card)) throw new CardNotFoundException();
                    if (IsThani) throw new ThaniGameException();
                    if (Game.Stage < GameStage.SelectingTrump) throw new BiddingNotOverException();
                    if (Game.Stage > GameStage.SelectingTrump) throw new TrumpAlreadySelectedException();
                    if (Game.Bid.HighBidder != player.Position) throw new NotHighBidderException(PlayerName(Game.Bid.HighBidder));

                    // if the opposing team has at least one trump card
                    if (TeamHasSuit(T.TeamOf(player.Position+1), card))
                    {
                        // save TrumpCard
                        Game.TrumpCard = card;

                        // remove card from highbidder's players cards
                        Game.Chairs[player.Position].Cards.Remove(card);

                        Game.Rounds.Add(new RoundInfo(Game.DealerPos));
                        Game.Stage = GameStage.PlayingCards;
                    }
                    else
                    {
                        // cancel the game
                        Game.Chairs.ForEach(c => DeckCtl.ReturnCards(c.Cards));
                        if (!Game.TrumpExposed && Game.TrumpCard != "") DeckCtl.ReturnCard(Game.TrumpCard);
                        InitializeNextGame(Game.DealerPos);
                        Game.Stage = GameStage.GameOver;
                        Game.GameCancelled = true;

                        // throw exception
                        throw new OppositeTeamHasNoTrumpCardsException();
                    }
                }
                finally
                {
                    SendStateUpdatedEvents();
                }
            }
        }
        public void ShowTrump(Player player, int roundOverDelay = 0)
        {
            lock (Game)
            {
                if (IsThani) throw new ThaniGameException();
                if (Game.Stage < GameStage.PlayingCards) throw new CardPlayNotStartedException();
                if (Game.Stage > GameStage.PlayingCards) throw new GameIsOverException();
                if (CurrentRound!.NextPlayer != player.Position) throw new NotPlayersTurnException(PlayerName(CurrentRound.NextPlayer));
                if (Game.TrumpExposed) throw new TrumpAlreadyExposedException();

                // 1. First player in the round cannot ask to show trump 
                //    Unless the player is the high bidder and he has only the trump card
                if (player.Position == CurrentRound.FirstPlayer) 
                {
                    if (!(player.Position == Game.Bid.HighBidder && CardsAt(player.Position).Count <= 1))
                        throw new FirstPlayerCannotShowTrumpException();
                }
                else
                {
                    // 2. If the player has cards matching the round's suit
                    string? anotherCard = CardsAt(player.Position).Find(s => s[0] == CurrentRoundSuit);
                    if (!string.IsNullOrEmpty(anotherCard))
                    {
                        throw new RoundSuitCardExistsException(T.GetSuitName(CurrentRoundSuit));
                    }
                }

                Game.TrumpExposed = true;

                // return trumpcard to highbidder's players cards
                Game.Chairs[Game.Bid.HighBidder].Cards.Add(Game.TrumpCard);
                Game.Chairs[Game.Bid.HighBidder].Cards.Sort(T.CompareCards);

                SendStateUpdatedEvents();
                if (Game.Bid.HighBidder == player.Position)
                {
                    PlayCard(player, Game.TrumpCard, roundOverDelay);
                }
            }
        }
        private void InitializeNextGame(int dealerPos)
        {
            Game.DealerPos = dealerPos;
            Game.Stage = GameStage.WaitingForPlayers;
            Game.GameCancelled = false;
            Game.GameForfeited = false;
            Game.TeamScore = new List<int>(){0,0};
            Game.WinningTeam = -1;
            Game.WinningScore = 0;

            Game.Bid = new BidInfo(T, Game.DealerPos);

            Game.TrumpCard = "";
            Game.TrumpExposed = false;

            Game.Rounds = new List<RoundInfo>();
            AutoPlayedToCompletion = false;
        }
        private bool TeamHasSuit(int team, string card)
        {
            for (int i = 0; i < T.PlayersPerTeam; i++)
            {
                string? aCard = CardsAt(team + i*2).Find(s => s[0] == card[0]);
                if (!string.IsNullOrEmpty(aCard)) return true;
            }
            return false;
        }
        private bool CanPlayCard(Player player, string card)
        {
            if (Game.Stage < GameStage.PlayingCards) throw new CardPlayNotStartedException();
            if (Game.Stage > GameStage.PlayingCards) throw new GameIsOverException();
            if (CurrentRound!.NextPlayer != player.Position) throw new NotPlayersTurnException(PlayerName(CurrentRound.NextPlayer));
            if (CurrentRoundAllCardsPlayed) throw new AllCardsPlayedForRoundException();
            if (!CardsAt(player.Position).Contains(card)) throw new CardNotFoundException();

            // 2nd, 3rd and 4th players must play matching suit if they have it
            if (CurrentRound.FirstPlayer != player.Position && CurrentRoundSuit != card[0]) 
            {
                // if the player has cards of matching suit
                string? anotherCard = CardsAt(player.Position).Find(s => s[0] == CurrentRoundSuit);
                if (!string.IsNullOrEmpty(anotherCard))
                {    
                    throw new CardNotOfRoundSuitException(T.GetSuitName(CurrentRoundSuit));
                }
            }

            // if trump is not yet exposed there are some restriction on high bidder
            if (!IsThani && !Game.TrumpExposed && player.Position == Game.Bid.HighBidder)
            {
                // 1. He cannot start a round with a trump card
                if (player.Position == CurrentRound.FirstPlayer && card.StartsWith(Game.TrumpCard[0].ToString()))
                {
                    string? nonTrumpSuitCard = CardsAt(player.Position).Find(s => s[0] != Game.TrumpCard[0]);
                    if (!string.IsNullOrEmpty(nonTrumpSuitCard)) // unless he doesn't have any other card.
                    {
                        throw new CannotPlayTrumpSuitException();
                    }
                }
            }

            // If trump has been exposed
            if (Game.TrumpExposed)
            {
                // See if the trump was just exposed - check Round.TrumpExposed for the last play in the current round
                // It doesn't matter when it is the first play of a round (CurrentRound.TrumpExposed.Count == 0)
                if (CurrentRound.TrumpExposed.Count > 0 && !CurrentRound.TrumpExposed.Last())
                {
                    if (player.Position == Game.Bid.HighBidder)
                    {
                        // 1. The bidder must play the trump card itself.
                        if (!card.Equals(Game.TrumpCard)) throw new MustPlayTheTrumpCardException(Game.TrumpCard);
                    }
                    else 
                    {
                        // 2. the exposing player must play a trump card if he has one.
                        string? trumpCard = CardsAt(player.Position).Find(s => s[0] == Game.TrumpCard[0]);
                        if (card[0] != Game.TrumpCard[0] && !string.IsNullOrEmpty(trumpCard))
                            throw new MustPlayTrumpSuitException(T.GetSuitName(Game.TrumpCard[0]));
                    }
                }
            }

            return true;
        }
        public void PlayCard(Player player, string card, int cardroundOverDelay)
        {
            lock (Game)
            {
                if (CanPlayCard(player, card))
                {
                    CurrentRound!.AutoPlayNextCard = "";
                    CurrentRound.PlayedCards.Add(card);
                    CurrentRound.TrumpExposed.Add(Game.TrumpExposed);

                    // return played card to deck and remove the card from the player
                    DeckCtl.ReturnCard(card);
                    CardsAt(player.Position).Remove(card);

                    ProcessRound(cardroundOverDelay);

                    ProcessGame();

                    AutoPlayWhenPosible();
                }
                SendStateUpdatedEvents();
            }
        }
        public void ForfeitGame(Player player)
        {
            lock (Game)
            {
                if (player.WatchOnly) throw new WatcherCannotForfeitException();
                if (Game.Stage < GameStage.PlayingCards) throw new GameNotStartedException();
                if (Game.Stage > GameStage.PlayingCards) throw new GameIsOverException();
                
                // Give opposite team all points
                Game.TeamScore[T.TeamOf(player.Position)] = 0; 
                Game.TeamScore[T.TeamOf(player.Position+1)] = IsThani? 8: 56; 

                // Set the Forfeited flag 
                Game.GameForfeited = true;

                ProcessGame();

                SendStateUpdatedEvents();
            }
        }
        private void ProcessRound(int cardroundOverDelay)
        {
            Game.RoundOver = CurrentRoundAllCardsPlayed; 
            
            if (Game.RoundOver)
            {
                // CurrentRound.NextPlayer = -1;
                if (cardroundOverDelay > 0)
                {
                    SendStateUpdatedEvents();
                    System.Threading.Thread.Sleep(cardroundOverDelay);
                }

                SetRoundWinnerAndScore();

                if (Game.Rounds.Count() < 8)
                {
                    // prepare next round
                    Game.Rounds.Add(new RoundInfo(CurrentRound!.Winner));
                }
            }
            else
            {
                CurrentRound!.NextPlayer = T.PlayerAt((CurrentRound.NextPlayer+1));
                if (IsThani && (T.SameTeam(CurrentRound.NextPlayer, CurrentRound.FirstPlayer))) // Skip teammate
                {
                    CurrentRound.NextPlayer = T.PlayerAt((CurrentRound.NextPlayer+1));
                }

                SetAutoPlayNextCard();
            }
        }
        private void SetRoundWinnerAndScore()
        {
            // Set the Round winner
            int winner = 0;
            string winnerCard = CurrentRound!.PlayedCards[winner];

            for(int i = 1; i < CurrentRound.PlayedCards.Count; i++)
            {
                string playerCard = CurrentRound.PlayedCards[i];
                
                // If player card is the same suit, just check the rank
                if (winnerCard[0] == playerCard[0])
                {
                    if (T.CompareRank(winnerCard, playerCard) > 0) // playerCard beats winner card, change the winner
                    {
                        winner = i;
                        winnerCard = playerCard;
                    }
                }
                // playerCard is the first trumpCard in this round, change the winner
                else if (!IsThani)
                {
                    if (CurrentRound.TrumpExposed[i] && 
                        (CurrentRound.PlayedCards[i].StartsWith(Game.TrumpCard[0].ToString())))
                    {
                        winner = i;
                        winnerCard = playerCard;
                    }
                } 
            }
            if (IsThani)
            {
                if (winner != 0) // Winner is someone from the other team
                {
                    winner = winner*2-1; // get the player's actual player position from the play order  
                }
            }
            CurrentRound.Winner = T.PlayerAt((winner + CurrentRound.FirstPlayer));
            Game.RoundWinner = CurrentRound.Winner;

            // Set the Round Score
            if (IsThani)
            {
                CurrentRound.Score = 1;  // One point for each round
            }
            else
            {
                CurrentRound.Score = 0;
                for(int i = 0; i < CurrentRound.PlayedCards.Count; i++)
                {
                    CurrentRound.Score += T.PointsForCard(CurrentRound.PlayedCards[i]);
                }
            }
            // Update team scores
            Game.TeamScore[CurrentRoundWinningTeam] += CurrentRound.Score;
        }
        private void SetAutoPlayNextCard()
        {
            // Check if nextplayer's card can be autoplayed
            // If there is only one card to play
            if (CardsAt(CurrentRound!.NextPlayer).Count == 1)
            {
                CurrentRound.AutoPlayNextCard = CardsAt(CurrentRound.NextPlayer).First();
            }
            else
            {
                // check if nextplayer has one and only one card of the current round suit
                var CurrentRoundSuitMatchingCards = CardsAt(CurrentRound.NextPlayer).Where(x => x.StartsWith(CurrentRoundSuit.ToString()));
                if (CurrentRoundSuitMatchingCards.Count() == 1)
                {
                    CurrentRound.AutoPlayNextCard = CurrentRoundSuitMatchingCards.First();
                }
            }
        }
        private void ProcessGame()
        {
            // Is the game over?
            bool gameOver = false;
            if (IsThani)
            {
                gameOver = (TeamScoreOf(Game.Bid.HighBidder) >= 8) || (TeamScoreOf(Game.Bid.HighBidder+1) >= 1);
            }
            else
            {
                gameOver = TeamScoreOf(Game.Bid.HighBidder) >= Game.Bid.HighBid || TeamScoreOf(Game.Bid.HighBidder+1) > (56-Game.Bid.HighBid);
            }

            if (gameOver)
            {
                //Update game stage
                Game.Stage = GameStage.GameOver;

                // determine the winning team 
                if (IsThani)
                {
                    Game.WinningTeam = (TeamScoreOf(Game.Bid.HighBidder) >= 8) ? T.TeamOf(Game.Bid.HighBidder) : T.TeamOf(Game.Bid.HighBidder+1);
                }
                else
                {
                    Game.WinningTeam = (TeamScoreOf(Game.Bid.HighBidder) >= Game.Bid.HighBid) ? T.TeamOf(Game.Bid.HighBidder) : T.TeamOf(Game.Bid.HighBidder+1);
                }

                // Update CoolieCount
                bool bidderWon = T.TeamOf(Game.Bid.HighBidder) == Game.WinningTeam;
                if (bidderWon)
                {
                    Game.WinningScore = T.GetWinPointsForBid(Game.Bid.HighBid);
                    Game.CoolieCount[T.TeamOf(Game.Bid.HighBidder)] += Game.WinningScore;
                    Game.CoolieCount[T.TeamOf(Game.Bid.HighBidder+1)] -= Game.WinningScore;
                }
                else
                {
                    Game.WinningScore = T.GetLosePointsForBid(Game.Bid.HighBid);
                    Game.CoolieCount[T.TeamOf(Game.Bid.HighBidder)] -= Game.WinningScore;
                    Game.CoolieCount[T.TeamOf(Game.Bid.HighBidder+1)] += Game.WinningScore;
                }

                // Remove Kodis for the entire team if it is a KodiIrakkamRound, or just for bidder  
                if (bidderWon) 
                {
                    if (Game.KodiIrakkamRound[T.TeamOf(Game.Bid.HighBidder)])
                    {
                        for (int i = 0; i < T.PlayersPerTeam; i++)
                        {
                            RemoveKodi(T.PlayerAt((Game.Bid.HighBidder+i*2)));
                        }
                    }
                    else
                    {
                        RemoveKodi(Game.Bid.HighBidder);
                    }
                }
                
                // reset KodiIrakkamRound 
                Game.KodiIrakkamRound = new List<bool>(){false, false};

                // Install kodis for team with no coolies
                for (int i=0; i<Game.CoolieCount.Count; i++)
                    if (Game.CoolieCount[i]<=0) InstallKodi(i);

                // return all player cards and trump card to deck
                ReturnCardsToDeck();

                // Print summary
                PrintGameSummary();
            }
        }
        private void RemoveKodi(int player)
        {
            if (Game.Chairs[player].KodiCount>0) Game.Chairs[player].KodiCount--;
        }
        private void InstallKodi(int team)
        {
            for (int i = 0; i < T.PlayersPerTeam; i++)
            {
                Game.Chairs[team+i*2].KodiCount++;
                Game.Chairs[team+i*2].KodiJustInstalled = true;
            }
            Game.CoolieCount = new List<int>(){T.BaseCoolieCount,T.BaseCoolieCount};
            Game.KodiIrakkamRound[team] = true;
            Game.KodiIrakkamRound[(team+1)%2] = false;
        }
        private void AutoPlayWhenPosible()
        {
            if (!IsThani) // Autoplay works for thani also, but do not enable it - because it is more fun to play it out. 
            {
                if (CurrentRound!.PlayedCards.Count == 0) // this is a new round
                {
                    // Check if no one else has trump cards and the next player has all bigcards
                    if (Game.Stage!=GameStage.GameOver && (IsThani || Game.TrumpExposed) && PlayerHasAllBiggerCards(CurrentRound.NextPlayer))
                    {
                        // System.Console.WriteLine($"Player {CurrentRound.NextPlayer} get all next rounds!!!");
                        while (Game.Stage!=GameStage.GameOver)
                        {
                            AutoPlayNextRound(CurrentRound.NextPlayer);
                            ProcessRound(0);
                            ProcessGame();
                        }
                        AutoPlayedToCompletion = true;
                    }
                }
            }
        }
        private bool PlayerHasAllBiggerCards(int posn)
        {
            // System.Console.WriteLine($"Checking if player {posn} gets all future rounds...");
            for (int i = 1; i < T.MaxPlayers; i++)
            {
                // If thani, skip player from same team
                if (!IsThani || !T.SameTeam(posn, posn+i))
                {
                    // Check if PlayerAt(posn+i) has trump cards
                    if (!IsThani && !CardsAt(T.PlayerAt(posn+i)).TrueForAll(card => card[0] != Game.TrumpCard[0])) return false;

                    // Check if PlayerAt(posn+i) has bigger cards
                    if (!CardsAt(posn).TrueForAll(NextPlayersCard =>
                            CardsAt(T.PlayerAt(posn+i)).TrueForAll(card => 
                            (NextPlayersCard[0] != card[0]) || (T.CompareRank(NextPlayersCard, card) <= 0)))) return false;
                    // System.Console.WriteLine($"--player {T.PlayerAt(posn+i)} has all lower cards.");
                }
            }
            return true;
        }
        private void AutoPlayNextRound(int posn)
        {
            string FirstCard = CardsAt(T.PlayerAt(posn)).First();
            
            // Add cards from other players
            for (int i = 0; i < T.MaxPlayers; i++)
            {
                // If thani skip team members 
                if (i==0 || !IsThani || !T.SameTeam(posn, posn+i))
                {
                    string? card = CardsAt(T.PlayerAt(posn+i)).FirstOrDefault(c2 => c2[0]==FirstCard[0]);
                    if (string.IsNullOrEmpty(card)) card = CardsAt(T.PlayerAt(posn+i)).First();

                    CurrentRound!.PlayedCards.Add(card);
                    CurrentRound.TrumpExposed.Add(Game.TrumpExposed);

                    // return played card to deck and remove the card from the player
                    DeckCtl.ReturnCard(card);
                    CardsAt(T.PlayerAt(posn+i)).Remove(card);
                }
            }
            // System.Console.WriteLine($"Auto played {string.Join(",",CurrentRound.PlayedCards)}.");
        }
        private void PrintGameSummary()
        {
            string BidderWinLose = (TeamScoreOf(Game.Bid.HighBidder) >= (IsThani? 8 : Game.Bid.HighBid))? "WON" : "LOST";
            System.Console.WriteLine($"Team [{T.TeamOf(Game.Bid.HighBidder)}] bid [{Game.Bid.HighBid}] {BidderWinLose}");
        }

        private void ReturnCardsToDeck()
        {
            // return any remain cards players have
            Game.Chairs.ForEach(c => DeckCtl.ReturnCards(c.Cards));
            if (!Game.TrumpExposed && Game.TrumpCard != "") DeckCtl.ReturnCard(Game.TrumpCard);
        }
    }
}

