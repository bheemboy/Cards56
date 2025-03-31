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
            
            // Check if the playerID already exists
            var existingPlayerKey = _players.Keys.FirstOrDefault(key => key.PlayerID == playerID);
            
            if (existingPlayerKey != default)
            {
                // Player exists, update the connectionID
                if (_players.TryRemove(existingPlayerKey, out Player existingPlayer))
                {
                    // Update the connection ID
                    existingPlayer = new Player(playerID, connectionID, existingPlayer.Name, existingPlayer.Lang, existingPlayer.WatchOnly);
                    
                    // Add back to the collection with updated connection ID
                    if (!_players.TryAdd((playerID, connectionID), existingPlayer))
                    {
                        throw new Exception($"Failed to update connection ID for player '{playerID}'");
                    }
                    
                    Console.WriteLine($"--> Player Updated: {playerID}, New Connection: {connectionID}, Name: '{existingPlayer.Name}'");
                    return existingPlayer;
                }
                else
                {
                    throw new Exception($"Failed to update player '{playerID}' in ALLPlayers");
                }
            }
            else
            {
                // New player
                Player player = new Player(playerID, connectionID, name, lang, watchOnly);
                if (!_players.TryAdd((playerID, connectionID), player))
                {
                    throw new Exception($"Failed to add player '{name}' to ALLPlayers");
                }
                
                Console.WriteLine($"--> Player Registered: {playerID}, Connection: {connectionID}, Name: '{name}'");
                return player;
            }
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
