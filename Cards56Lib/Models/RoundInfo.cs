using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cards56Lib
{
    [JsonObject(MemberSerialization.OptIn)]
    public class RoundInfo // Class that hold one round of game
    {
        [JsonProperty]
        public int FirstPlayer {get; set;}
        [JsonProperty]
        public int NextPlayer {get; set;}
        [JsonProperty]
        public List<string> PlayedCards {get; set;}
        [JsonProperty]
        public string AutoPlayNextCard {get; set;}
        [JsonProperty]
        public List<bool> TrumpExposed;
        [JsonProperty]
        public int Winner  {get; set;}
        [JsonProperty]
        public int Score  {get; set;}
        public RoundInfo(int firstPlayer)
        {
            FirstPlayer = firstPlayer;
            NextPlayer = FirstPlayer;
            AutoPlayNextCard = "";
            PlayedCards = new List<string>();
            TrumpExposed = new List<bool>();
            Winner = -1;
            Score = -1;
        }
    }
}
