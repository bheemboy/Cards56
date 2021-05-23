
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cards56Lib

{
    [JsonObject(MemberSerialization.OptIn)]
    public class Chair
    {
        [JsonProperty]
        public int Position {get; set;}
        public List<string> Cards {get; set;}
        [JsonProperty]
        public Player Occupant {get; set;}
        [JsonProperty]
        public List<Player> Watchers {get; set;}
        [JsonProperty]
        public int KodiCount {get; set;}
        [JsonProperty]
        public bool KodiJustInstalled {get; set;}
        public bool WatchersFull  => Watchers.Count>=2;
        public Chair(int posn)
        {
            Position = posn;
            Occupant = null;
            KodiCount = 0;
            KodiJustInstalled = false;
            Watchers = new List<Player>();
        }
   }
}
