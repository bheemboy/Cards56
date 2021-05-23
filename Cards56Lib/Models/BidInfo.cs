using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cards56Lib
{
    [JsonObject(MemberSerialization.OptIn)]
    public class BidPass
    {
        [JsonProperty]
        public int Position {get; set;}
        [JsonProperty]
        public int Bid {get; set;}
        public BidPass(int position, int bid)
        {
            Position = position;
            Bid = bid;
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class BidInfo
    {
        [JsonProperty]
        public int HighBid {get; set;}
        [JsonProperty]
        public int HighBidder {get; set;}
        [JsonProperty]
        public int NextBidder {get; set;}
        [JsonProperty]
        public int NextMinBid {get; set;}
        public int[] NextBidderFromTeam {get; set;}
        public int[] OutBidChance {get; set;}
        [JsonProperty]
        public List<BidPass> BidHistory {get; set;}
        public BidInfo(TableType T, int firstBidder)
        {
            HighBid = -1;
            HighBidder = -1;
            NextBidder = firstBidder;
            NextMinBid = 28;
            NextBidderFromTeam = new int[2];
            NextBidderFromTeam[T.TeamOf(firstBidder)] = firstBidder;
            NextBidderFromTeam[T.TeamOf(firstBidder+1)] = T.PlayerAt(firstBidder+1);
            OutBidChance = Enumerable.Repeat(0, T.MaxPlayers).ToArray();
            BidHistory = new List<BidPass>(); 
        }
    }
}
