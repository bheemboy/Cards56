using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cards56Lib
{
    public enum GameStage {Unknown=0, WaitingForPlayers=1, Bidding=2, SelectingTrump=3, PlayingCards=4, GameOver=5}

    [JsonObject(MemberSerialization.OptIn)]
    public class GameTable
    {
        public TableType T {get;}
        [JsonProperty]
        public int Type => T.Type; // Used for JSON export only
        [JsonProperty]
        public int MaxPlayers => T.MaxPlayers; // Used for JSON export only
        [JsonProperty]
        public string TableName {get;}
        public GameStage Stage {get; set;}
        [JsonProperty]
        public bool GameCancelled {get; set;}
        [JsonProperty]
        public bool GameForfeited {get; set;}
        public List<string> Deck;
        [JsonProperty]
        public List<Chair> Chairs;
        [JsonProperty]
        public BidInfo Bid {get; set;}
        [JsonProperty]
        public List<RoundInfo> Rounds {get; set;}
        [JsonProperty]
        public int DealerPos {get; set;}
        public string TrumpCard {get; set;}
        public bool TrumpExposed {get; set;}
        [JsonProperty]
        public int RoundWinner  {get; set;}
        [JsonProperty]
        public bool RoundOver  {get; set;}
        [JsonProperty]
        public int WinningTeam {get; set;}
        [JsonProperty]
        public int WinningScore {get; set;}
        [JsonProperty]
        public List<int> TeamScore {get; set;}
        [JsonProperty]
        public List<int> CoolieCount {get; set;}
        public List<bool> KodiIrakkamRound {get; set;}
        public bool TableFull => Chairs.TrueForAll(c => c.Occupant != null);
        public bool WatchersFull => Chairs.TrueForAll(c => c.WatchersFull);
        public GameTable(TableType tableType, string tableName)
        {
            T = tableType;
            TableName = tableName;
            Stage = GameStage.WaitingForPlayers;
            GameCancelled = false;
            GameForfeited = false;
            Deck = new List<string>(); 
            WinningTeam = -1;
            WinningScore = 0;
            CoolieCount = new List<int>(){T.BaseCoolieCount,T.BaseCoolieCount};
            KodiIrakkamRound = new List<bool>(){false, false};
            Chairs = Enumerable.Range(0, T.MaxPlayers).Select(i => new Chair(i)).ToList();
        }
    }
}
