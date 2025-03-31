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
                player.ConnID = connectionID;
                Console.WriteLine($"--> Player Updated: {playerID}, Connection: {connectionID}, Name: '{name}'");

                // Remove any existing player entry with old connectionID
                RemoveByPlayerId(playerID);
            }
            else
            {
                // If the player does not exist, create a new one
                player = new Player(playerID, connectionID, name, lang, watchOnly);
            }
            
            // Add a new player with current info
            if (!_players.TryAdd((playerID, connectionID), player))
            {
                throw new Exception($"Failed to add player '{name}' to ALLPlayers");
            }
            
            Console.WriteLine($"--> Player Registered: {playerID}, Connection: {connectionID}, Name: '{name}'");
            return player;
        }
        
        public static void RemoveByPlayerId(string playerID)
        {
            var playerKey = _players.Keys.FirstOrDefault(key => key.PlayerID == playerID);
            
            if (playerKey != default)
            {
                Player ignored;
                if (!_players.TryRemove(playerKey, out ignored))
                {
                    throw new Exception($"Failed to remove player '{playerID}' from ALLPlayers");
                }
                
                Console.WriteLine($"--> Player Removed: {playerID}, Connection: {playerKey.ConnectionID}");
            }
        }
        
        // Helper method to find a player by their playerID
        public static Player GetPlayerById(string playerID)
        {
            var playerKey = _players.Keys.FirstOrDefault(key => key.PlayerID == playerID);
            
            if (playerKey != default && _players.TryGetValue(playerKey, out Player player))
            {
                return player;
            }
            
            return null;
        }
        
        // Helper method to find a player by their connectionID
        public static Player GetPlayerByConnectionId(string connectionID)
        {
            var playerKey = _players.Keys.FirstOrDefault(key => key.ConnectionID == connectionID);
            
            if (playerKey != default && _players.TryGetValue(playerKey, out Player player))
            {
                return player;
            }
            
            return null;
        }
    }
}
