using NGettext;
using System.Threading;
using System.Collections.Generic;

namespace Cards56Lib
{
    public class TableType
    {
        public int Type {get;}
        public int BaseCoolieCount => (Type==0)? 5: (Type==1)? 6: 7; 
        public int MaxPlayers => (Type==0)? 4: (Type==1)? 6: 8;
        public int PlayersPerTeam => MaxPlayers/2; 
        public int DeckSize => (Type==0)? 32: (Type==1)? 48: 64;
        public const string CLUBS="Clubs";
        public const string HEARTS="Hearts";
        public const string DIAMOND="Diamond";
        public const string SPADE="Spade";
        public List<string> Suits => new List<string>{"h","s","d","c"};
        public List<string> Ranks => (Type==0)? new List<string>{"10","1","9","11"}: 
                                     (Type==1)? new List<string>{"12","13","10","1","9","11"}:
                                                new List<string>{"7","8","12","13","10","1","9","11"};
        public int PlayerAt(int position) => position%MaxPlayers;
        public int TeamOf(int position) => position%2;
        public bool SameTeam(int posn1, int posn2) => posn1%2 == posn2%2;
        public readonly int MaxBid = 57;
        public TableType(int tableType)
        {
            Type = tableType;
        }
        private int StageForBid(int bid)
        {
            if (bid >= 57) return 5; // THANI
            if (bid == 56) return 4;
            if (bid >= 48) return 3;
            if (bid >= 40) return 2;
            return 1;
        }
        public int NextMinBidAfterBid(int bid)
        {
            if (bid < 28) return 28;
            else if (bid <= 39) return 40;
            else return 48;
        }
        public int GetWinPointsForBid(int bid)
        {
            switch(StageForBid(bid))
            {
                case 5:
                    return BaseCoolieCount * 2;
                case 4:
                    return 4;
                case 3:
                    return 3;
                case 2:
                    return 2;
                default:
                    return 1;
            }
        }
        public int GetLosePointsForBid(int bid)
        {
            switch(StageForBid(bid))
            {
                case 5:
                    return BaseCoolieCount * 2;
                case 4:
                    return 5;
                case 3:
                    return 4;
                case 2:
                    return 3;
                default:
                    return 2;
            }
        }
        public int PointsForCard(string card)
        {
            int rank = int.Parse(card.Substring(1));

            if (rank == 11) return 3;
            else if (rank == 9) return 2;
            else if (rank == 1 || rank == 10)  return 1;
            return 0;
        }
        public string GetSuitName(char c)
        {
            string suitName = (c == 'c') ? CLUBS : (c == 'd') ? DIAMOND : (c == 'h') ? HEARTS : SPADE;
        	ICatalog catalog = new Catalog("strings", "./locale", Thread.CurrentThread.CurrentCulture);
            return catalog.GetString(suitName);;
        } 
        public int CompareSuit(string cardA, string cardB)
        {
            // return 1 if cardA comes before cardB in ascending sort order
            int aVal = Suits.IndexOf(cardA.Substring(0,1));
            int bVal = Suits.IndexOf(cardB.Substring(0,1));
            if (aVal < bVal) return 1;
            else if (aVal > bVal) return -1;
            else return 0;
        }
        public int CompareRank(string cardA, string cardB)
        {
            // return 1 if cardA comes before cardB in ascending sort order
            int aVal = Ranks.IndexOf(cardA.Substring(1));
            int bVal = Ranks.IndexOf(cardB.Substring(1));
            if (aVal < bVal) return 1;
            else if (aVal > bVal) return -1;
            else return 0;
        }
        public int CompareCards(string cardA, string cardB)
        {
            if (cardA == cardB) return 0;
            int suiteDiff = CompareSuit(cardA, cardB);
            if (suiteDiff != 0) 
                return suiteDiff;
            else 
                return CompareRank(cardA, cardB);
        }
    }
}