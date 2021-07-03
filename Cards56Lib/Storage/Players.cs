using System;
using System.Collections.Concurrent;

namespace Cards56Lib
{
    public static class Players
    {
        private static ConcurrentDictionary<string, Player> _players;
        public static ConcurrentDictionary<string, Player> All
        {
            get
            {
                if (_players == null)
                {
                    _players = new ConcurrentDictionary<string, Player>();
                }
                return _players;
            }
        }
        public static Player AddPlayer(string connectionid, string name, string lang, bool watchOnly)
        {
            name = name.Trim();
            if (name.Length <=0 ) throw new Exception($"Non-empty name is required.");

            Player player = new Player(connectionid, name, lang, watchOnly);
            if (!All.TryAdd(player.ConnID, player))
            {
                throw new Exception($"Failed to add player '{name}' to AllPlayers");
            }
            
            Console.WriteLine($"--> Player Registered: {connectionid}, Name: '{name}'");
            return player;
        }
        public static void RemovePlayerById(string playerId)
        {
            Player ignored;
            if (All.ContainsKey(playerId) && !All.TryRemove(playerId, out ignored))
            {
                throw new Exception($"Failed to remove player '{playerId}' from AllPlayers");
            }
        }
    }
}

