using NGettext;
using System.Diagnostics;
using System.Threading;

namespace Cards56Lib
{
    public class Card56ErrorData
    {
        public int ErrorCode {get; set;}
        public string ErrorName {get;set;}
        public int MinBid {get; set;}
        public int MaxBid {get; set;}
        public string PlayerName {get; set;}
        public string Suit {get; set;}
        public string ThuruppuCard {get;set;}
        public string TrumpSuit {get;set;}
        public Card56ErrorData()
        {
            ErrorCode = Cards56Error.UnKnownException;
            ErrorName = "";
            MinBid = 0;
            MaxBid = 0;
            PlayerName = "";
            Suit = "";
            ThuruppuCard = "";
            TrumpSuit = "";
        }
        public static Card56ErrorData GetBidOutOfRangeErrorData(int MinBid, int MaxBid)
        {
            Card56ErrorData error = new Card56ErrorData();
            error.MinBid = MinBid;
            error.MaxBid = MaxBid;
            return error;
        }
        public static Card56ErrorData GetNotPlayersTurnErrorData(string playerName)
        {
            Card56ErrorData error = new Card56ErrorData();
            error.PlayerName = playerName;
            return error;
        }
        public static Card56ErrorData GetNotHighBidderErrorData(string playerName)
        {
            Card56ErrorData error = new Card56ErrorData();
            error.PlayerName = playerName;
            return error;
        }
        public static Card56ErrorData GetCardNotOfRoundSuitErrorData(string suit)
        {
            Card56ErrorData error = new Card56ErrorData();
            error.Suit = suit;
            return error;
        }
        public static Card56ErrorData GetMustPlayTheTrumpCardErrorData(string thuruppuCard)
        {
            Card56ErrorData error = new Card56ErrorData();
            error.ThuruppuCard = thuruppuCard;
            return error;
        }
        public static Card56ErrorData GetMustPlayTrumpSuitErrorData(string trumpSuit)
        {
            Card56ErrorData error = new Card56ErrorData();
            error.TrumpSuit = trumpSuit;
            return error;
        }
        public static Card56ErrorData GetRoundSuitCardExistsErrorData(string suit)
        {
            Card56ErrorData error = new Card56ErrorData();
            error.Suit = suit;
            return error;
        }
    }


    [System.Serializable]
    public class Card56Exception : System.Exception
    {
        public Card56ErrorData ErrorData {get;}
        ICatalog _catalog;
        public override string Message {get;}
        public Card56Exception(int errorCode, Card56ErrorData errorData=null) : base() 
        {
            ErrorData = (errorData!=null)? errorData: new Card56ErrorData();
            ErrorData.ErrorCode = errorCode;
            ErrorData.ErrorName = this.GetType().Name;

            string StringsDir = Card56Exception.AssemblyDirectory + System.IO.Path.DirectorySeparatorChar + "locale";
        	_catalog = new Catalog("strings", StringsDir, Thread.CurrentThread.CurrentCulture);
            Message = _catalog.GetString(Cards56Error.MSG[errorCode]);

            switch (errorCode)
            {
                case Cards56Error.NotPlayersTurnException:
                    Debug.Assert(errorData!=null);
                    if (errorData!=null) Message = string.Format(Message, errorData.PlayerName);
                    break;
                case Cards56Error.BidOutOfRangeException:
                    Debug.Assert(errorData!=null);
                    if (errorData!=null) Message = string.Format(Message, errorData.MinBid, errorData.MaxBid);
                    break;
                case Cards56Error.NotHighBidderException:
                    Debug.Assert(errorData!=null);
                    if (errorData!=null) Message = string.Format(Message, errorData.PlayerName);
                    break;
                case Cards56Error.CardNotOfRoundSuitException:
                    Debug.Assert(errorData!=null);
                    if (errorData!=null) Message = string.Format(Message, errorData.Suit);
                    break;
                case Cards56Error.MustPlayTheTrumpCardException:
                    Debug.Assert(errorData!=null);
                    if (errorData!=null) Message = string.Format(Message, errorData.ThuruppuCard);
                    break;
                case Cards56Error.MustPlayTrumpSuitException:
                    Debug.Assert(errorData!=null);
                    if (errorData!=null) Message = string.Format(Message, errorData.TrumpSuit);
                    break;
                case Cards56Error.RoundSuitCardExistsException:
                    Debug.Assert(errorData!=null);
                    if (errorData!=null) Message = string.Format(Message, errorData.Suit);
                    break;
            }
        }
        private Card56Exception() { }
        private Card56Exception(string message) : base(message) { }
        private Card56Exception(string message, System.Exception inner) : base(message, inner) { }
        protected Card56Exception(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        private static string AssemblyDirectory
        {
            get
            {
                string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                System.UriBuilder uri = new System.UriBuilder(codeBase);
                string path = System.Uri.UnescapeDataString(uri.Path);
                return System.IO.Path.GetDirectoryName(path);
            }
        }
    }
    public class BiddingNotStartedException : Card56Exception
    {
        public BiddingNotStartedException():base(Cards56Error.BiddingNotStartedException){}
    }
    public class BiddingOverException: Card56Exception
    {
        public BiddingOverException():base(Cards56Error.BiddingOverException){}
    }
    public class BiddingNotOverException: Card56Exception
    {
        public BiddingNotOverException():base(Cards56Error.BiddingNotOverException){}
    }
    public class NotPlayersTurnException: Card56Exception
    {
        public  NotPlayersTurnException(string playerName):base(Cards56Error.NotPlayersTurnException, 
            Card56ErrorData.GetNotPlayersTurnErrorData(playerName)){}
    }
    public class BidOutOfRangeException: Card56Exception
    {
        public BidOutOfRangeException(int MinBid, int MaxBid):base(Cards56Error.BidOutOfRangeException,
            Card56ErrorData.GetBidOutOfRangeErrorData(MinBid, MaxBid)){}
    }
    public class PassNotAllowedException: Card56Exception
    {
        public  PassNotAllowedException():base(Cards56Error.PassNotAllowedException){}
    }
    public class GameNotStartedException: Card56Exception
    {
        public GameNotStartedException():base(Cards56Error.GameNotStartedException){}
    }
    public class GameIsOverException: Card56Exception
    {
        public GameIsOverException():base(Cards56Error.GameIsOverException){}
    }
    public class ThaniGameException: Card56Exception
    {
        public ThaniGameException():base(Cards56Error.ThaniGameException){}
    }
    public class NotHighBidderException: Card56Exception
    {
        public NotHighBidderException(string playerName):base(Cards56Error.NotHighBidderException,
            Card56ErrorData.GetNotHighBidderErrorData(playerName)){}
    }
    public class TrumpAlreadySelectedException: Card56Exception
    {
        public TrumpAlreadySelectedException():base(Cards56Error.TrumpAlreadySelectedException){}
    }
    public class TrumpNotYetSelectedException: Card56Exception
    {
        public TrumpNotYetSelectedException():base(Cards56Error.TrumpNotYetSelectedException){}
    }
    public class AllCardsPlayedForRoundException: Card56Exception
    {
        public AllCardsPlayedForRoundException():base(Cards56Error.AllCardsPlayedForRoundException){}
    }
    public class PlayerAlreadyOnTableException: Card56Exception
    {
        public PlayerAlreadyOnTableException():base(Cards56Error.PlayerAlreadyOnTableException){}
    }
    public class TableIsFullException: Card56Exception
    {
        public TableIsFullException():base(Cards56Error.TableIsFullException){}
    }
    public class CardNotFoundException: Card56Exception
    {
        public CardNotFoundException():base(Cards56Error.CardNotFoundException){}
    }
    public class CardNotOfRoundSuitException: Card56Exception
    {
        public CardNotOfRoundSuitException(string suit):base(Cards56Error.CardNotOfRoundSuitException,
            Card56ErrorData.GetCardNotOfRoundSuitErrorData(suit)){}
    }
    public class CannotPlayTrumpSuitException: Card56Exception
    {
        public CannotPlayTrumpSuitException():base(Cards56Error.CannotPlayTrumpSuitException){}
    }
    public class CannotPlayTheTrumpCardException: Card56Exception
    {
        public CannotPlayTheTrumpCardException():base(Cards56Error.CannotPlayTheTrumpCardException){}
    }
    public class MustPlayTheTrumpCardException: Card56Exception
    {
        public MustPlayTheTrumpCardException(string thuruppuCard):base(Cards56Error.MustPlayTheTrumpCardException,
            Card56ErrorData.GetMustPlayTheTrumpCardErrorData(thuruppuCard)){}
    }
    public class MustPlayTrumpSuitException: Card56Exception
    {
        public MustPlayTrumpSuitException(string trumpSuit):base(Cards56Error.MustPlayTrumpSuitException,
            Card56ErrorData.GetMustPlayTrumpSuitErrorData(trumpSuit)){}
    }
    public class FirstPlayerCannotShowTrumpException: Card56Exception
    {
        public FirstPlayerCannotShowTrumpException():base(Cards56Error.FirstPlayerCannotShowTrumpException){}
    }
    public class RoundSuitCardExistsException: Card56Exception
    {
        public RoundSuitCardExistsException(string suit):base(Cards56Error.RoundSuitCardExistsException,
            Card56ErrorData.GetRoundSuitCardExistsErrorData(suit)){}
    }
    public class TrumpAlreadyExposedException: Card56Exception
    {
        public TrumpAlreadyExposedException():base(Cards56Error.TrumpAlreadyExposedException){}
    }
    public class PlayerNotRegisteredException: Card56Exception
    {
        public PlayerNotRegisteredException():base(Cards56Error.PlayerNotRegisteredException){}
    }
    public class PlayerNotOnAnyTableException: Card56Exception
    {
        public PlayerNotOnAnyTableException():base(Cards56Error.PlayerNotOnAnyTableException){}
    }
    public class GameNotOverException: Card56Exception
    {
        public GameNotOverException():base(Cards56Error.GameNotOverException){}
    }
    public class NotEnoughPlayersOnTableException: Card56Exception
    {
        public NotEnoughPlayersOnTableException():base(Cards56Error.NotEnoughPlayersOnTableException){}
    }
    public class OppositeTeamHasNoTrumpCardsException: Card56Exception
    {
        public OppositeTeamHasNoTrumpCardsException():base(Cards56Error.OppositeTeamHasNoTrumpCardsException){}
    }
    public class FirstBidderHasNoPointsException: Card56Exception
    {
        public FirstBidderHasNoPointsException():base(Cards56Error.FirstBidderHasNoPointsException){}
    }
    public class CardPlayNotStartedException: Card56Exception
    {
        public CardPlayNotStartedException():base(Cards56Error.CardPlayNotStartedException){}
    }
    public class TooManyWatchersOnChairException: Card56Exception
    {
        public TooManyWatchersOnChairException():base(Cards56Error.TooManyWatchersOnChair){}
    }
}
