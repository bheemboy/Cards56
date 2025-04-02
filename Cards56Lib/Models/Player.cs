using Newtonsoft.Json;

namespace Cards56Lib
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Player
    {
        public string ConnID {get; set;} // Should not be shared
        [JsonProperty]
        public string PlayerID {get;}
        [JsonProperty]
        public string Name {get;}
        [JsonProperty]
        public string Lang {get;}
        public string? TableName {get; set;}
        public int Position {get; set;}
        [JsonProperty]
        public bool WatchOnly {get; set;}

        public Player(string playerID, string connId, string name, string lang, bool watchOnly)
        {
            ConnID = connId;
            PlayerID = string.IsNullOrEmpty(playerID)? System.Guid.NewGuid().ToString().ToUpper() : playerID;
            Name = name;
            Lang = lang;
            Position = -1;
            WatchOnly = watchOnly;
        }
    }
}
