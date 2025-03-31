using System;
using System.Collections.Concurrent;
using System.Linq;
namespace Cards56Lib
{
    public static class Players
    {
        private static ConcurrentDictionary<(string PlayerID, string ConnectionID), Player> _players = new ConcurrentDictionary<(string PlayerID, string ConnectionID), Player>();
        public static Player AddOrUpdatePlayer(string playerID, string connectionID, string name, string lang, bool watchOnly)
        {
            name = name.Trim();
            if (name.Length <= 0) throw new Exception($"Non-empty name is required.");

            Player player = GetPlayerById(playerID);
            if (player != null)
            {
                // If the player already exists, update their connectionID
                if (player.ConnID != connectionID)
                {
                    player.ConnID = connectionID;
                    Console.WriteLine($"--> Player conectionid updated: {playerID}, Connection: {connectionID}, Name: '{name}'");
                }
                // Remove the found player from _players. It will be added back below...
                RemoveByPlayerId(playerID);
            }
            else
            {
                // If the player does not exist, create a new one
                player = new Player(playerID, connectionID, name, lang, watchOnly);
            }
            
            // Add a new player with current info
            if (!_players.TryAdd((player.PlayerID, player.ConnID), player))
            {
                throw new Exception($"Failed to add player '{name}' to ALLPlayers");
            }
            
            Console.WriteLine($"--> Player added: {player.PlayerID}, Connection: {player.ConnID}, Name: '{player.Name}'");
            return player;
        }
        
        public static void RemoveByPlayerId(string playerID)
        {
            if (string.IsNullOrEmpty(playerID)) return;

            var keysToRemove = _players.Keys.Where(key => key.PlayerID == playerID).ToList();
            
            foreach (var key in keysToRemove)
            {
                Console.WriteLine($"--> Removing player with Key: ({key.PlayerID}, {key.ConnectionID})");
                Player ignored;
                if (!_players.TryRemove(key, out ignored))
                {
                    throw new Exception($"Failed to remove player '{playerID}' from ALLPlayers");
                }
            }
        }
        
        public static Player GetPlayerById(string playerID)
        {
            if (string.IsNullOrEmpty(playerID)) return null;

            Player p = _players.Where(kvp => kvp.Key.PlayerID == playerID)
                .Select(kvp => kvp.Value)
                .FirstOrDefault();
            return p;
        }

        public static Player GetPlayerByConnectionId(string connectionID)
        {
            if (string.IsNullOrEmpty(connectionID)) return null;
            
            Player p = _players.Where(kvp => kvp.Key.ConnectionID == connectionID)
                .Select(kvp => kvp.Value)
                .FirstOrDefault();
            return p;
        }
    }
}
