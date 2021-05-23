using System;
using System.Collections.Concurrent;

namespace Cards56Lib
{
    public static class GameTables
    {
        private static object _lock = new object();
        private static ConcurrentDictionary<string, GameTable> _tables;
        public static ConcurrentDictionary<string, GameTable> All
        {
            get
            {
                if (_tables == null || _tables.IsEmpty)
                {
                    _tables = new ConcurrentDictionary<string, GameTable>();
                }
                return _tables;
            }
        }
        public static GameTable AddTable(int tableType, string tableId)
        {
            lock (_lock)
            {
                GameTable table = new GameTable(new TableType(tableType), tableId); 
                if (!_tables.TryAdd(tableId, table))
                {
                    throw new Exception($"Failed to add table '{tableId}' to AllPlayers");
                }
                Console.WriteLine($"Added new gametable {tableId}");
                return table;
            }
        }
        public static void RemoveTable(string tableId)
        {
            lock (_lock)
            {
                GameTable ignored;
                if (!All.TryRemove(tableId, out ignored))
                {
                    throw new Exception($"Failed to remove table '{tableId}' from AllTables");
                }
                Console.WriteLine($"Removed empty gametable {tableId}");
            }
        }
        public static GameTable GetFreeTable(int tableType, bool watchOnly)
        {
            lock (_lock)
            {
                foreach (var table in All.Values)
                {
                    if (table.T.Type==tableType && table.TableName.StartsWith("PUBLIC_TABLE_"))
                    {
                        if (watchOnly)
                        {
                            if (!table.WatchersFull) return table;
                        }
                        else
                        {
                            if (!table.TableFull) return table;
                        }
                    }
                }

                // Let us add a new table since there are no free tables...
                return AddTable(tableType, "PUBLIC_TABLE_" + Guid.NewGuid().ToString("N"));
            }
        }
    }
}

