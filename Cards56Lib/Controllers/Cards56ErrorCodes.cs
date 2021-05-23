namespace Cards56Lib
{
    public static class Cards56Error
    {
        public const int UnKnownException=0;
        public const int BiddingNotStartedException=1;
        public const int BiddingOverException=2;
        public const int BiddingNotOverException=3;
        public const int NotPlayersTurnException=4;
        public const int BidOutOfRangeException=5;
        public const int PassNotAllowedException=6;
        public const int GameNotStartedException=7;
        public const int GameIsOverException=8;
        public const int ThaniGameException=9;
        public const int NotHighBidderException=10;
        public const int TrumpAlreadySelectedException=11;
        public const int TrumpNotYetSelectedException=12;
        public const int AllCardsPlayedForRoundException=13;
        public const int PlayerAlreadyOnTableException=14;
        public const int TableIsFullException=15;
        public const int CardNotFoundException=16;
        public const int CardNotOfRoundSuitException=17;
        public const int CannotPlayTrumpSuitException=18;
        public const int CannotPlayTheTrumpCardException=19;
        public const int MustPlayTheTrumpCardException=20;
        public const int MustPlayTrumpSuitException=21;
        public const int FirstPlayerCannotShowTrumpException=22;
        public const int RoundSuitCardExistsException=23;
        public const int TrumpAlreadyExposedException=24;
        public const int PlayerNotRegisteredException=25;
        public const int PlayerNotOnAnyTableException=26;
        public const int GameNotOverException=27;
        public const int NotEnoughPlayersOnTableException=28;
        public const int OppositeTeamHasNoTrumpCardsException=29;
        public const int FirstBidderHasNoPointsException=30;
        public const int CardPlayNotStartedException=31;
        public const int TooManyWatchersOnChair=32;
        public static string[] MSG = 
        {
            "Unknown Error.",                                   // 0
            "Bidding has not yet started.",                     // 1
            "Bidding is over.",                                 // 2
            "Bidding is not yet over.",                         // 3
            "Waiting for {0} to play. It is not your turn.",    // 4
            "Bid must be between {0} and {1}.",                 // 5
            "Passing is not allowed.",                          // 6
            "Game not yet started.",                            // 7
            "Game is over.",                                    // 8
            "This is a thani game.",                            // 9
            "You are not the high bidder. High bidder is {0}.",     // 10
            "Trump already selected.",                          // 11
            "Trump not yet selected.",                          // 12
            "All cards for the round are played.",              // 13
            "You are already on the table.",                    // 14
            "Table is full.",                                   // 15
            "Card not found.",                                  // 16
            "Must play a ({0}).",                               // 17
            "Cannot play trump suit.",                          // 18
            "Cannot play the trump card.",                      // 19
            "Bidder must play THE trump card '{0}' immediately after exposing it.", // 20
            "Must play card from trump suit '{0}'.",            // 21
            "First player cannot show trump.",                  // 22
            "Must play '{0}' while you have them.",             // 23
            "Trump card is already exposed.",                   // 24
            "Player not registered.",                           // 25
            "Player is not on any table.",                      // 26
            "Game is not yet over.",                            // 27
            "Not enough players on table.",                     // 28
            "Opposite team has no trump cards. Game cancelled.",// 29 
            "First bidder has no points. Game cancelled.",      // 30
            "Card play has not started",                        // 31
            "Too many watcher on chair",                        // 32
        };
    }
}
