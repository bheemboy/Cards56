using Newtonsoft.Json;

namespace Cards56Lib
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Player
    {
        public string ConnID {get;} // Should not be shared
        [JsonProperty]
        public string PlayerID {get;}
        [JsonProperty]
        public string Name {get;}
        [JsonProperty]
        public string Lang {get;}
        public string TableName {get; set;}
        public int Position {get; set;}
        [JsonProperty]
        public bool WatchOnly {get; set;}

        public Player(string ConnId, string name, string lang, bool watchOnly)
        {
            ConnID = ConnId;
            PlayerID = System.Guid.NewGuid().ToString().ToUpper();
            Name = name;
            Lang = lang;
            Position = -1;
            WatchOnly = watchOnly;
        }
    }
}
