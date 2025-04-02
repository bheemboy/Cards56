using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Cards56Lib
{
    public static class Players
    {
        private static readonly ConcurrentDictionary<(string PlayerID, string ConnectionID), Player> _players = new ConcurrentDictionary<(string PlayerID, string ConnectionID), Player>();

        public static Player AddOrUpdatePlayer(string playerID, string connectionID, string name, string lang, bool watchOnly)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Non-empty name is required.", nameof(name));

            name = name.Trim();
            
            // Try to get existing player
            var player = GetPlayerById(playerID);
            if (player != null)
            {
                // Update connection ID if needed
                if (player.ConnID != connectionID)
                {
                    player.ConnID = connectionID;
                    Console.WriteLine($"--> Player connectionID updated: {playerID}, Connection: {connectionID}, Name: '{name}'");
                }
                
                // Remove existing player entries
                RemoveByPlayerId(playerID);
            }
            else
            {
                // Create new player
                player = new Player(playerID, connectionID, name, lang, watchOnly);
            }
            
            // Add player with updated information
            if (!_players.TryAdd((player.PlayerID, player.ConnID), player))
            {
                throw new InvalidOperationException($"Failed to add player '{name}' to ALLPlayers");
            }
            
            Console.WriteLine($"--> Player added: {player.PlayerID}, Connection: {player.ConnID}, Name: '{player.Name}'");
            return player;
        }
        
        public static void RemoveByPlayerId(string playerID)
        {
            var keysToRemove = _players.Keys.Where(key => key.PlayerID == playerID).ToList();
            
            foreach (var key in keysToRemove)
            {
                Console.WriteLine($"--> Removing player with Key: ({key.PlayerID}, {key.ConnectionID})");
                if (!_players.TryRemove(key, out _))
                {
                    throw new InvalidOperationException($"Failed to remove player with Key: ({key.PlayerID}, {key.ConnectionID})");
                }
            }
        }
        
        public static Player? GetPlayerById(string playerID)
        {
            return _players
                .Where(kvp => kvp.Key.PlayerID == playerID)
                .Select(kvp => kvp.Value)
                .FirstOrDefault();
        }
        
        public static Player? GetPlayerByConnectionId(string connectionID)
        {
            return _players
                .Where(kvp => kvp.Key.ConnectionID == connectionID)
                .Select(kvp => kvp.Value)
                .FirstOrDefault();
        }
    }
}