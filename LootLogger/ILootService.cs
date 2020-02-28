using LootLogger.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LootLogger
{
    public interface ILootService
    {
        void AddLootForPlayer(Loot loot, string playerName, Dictionary<string, string> playerDictionary);
        void SaveLootsToFile();

        void UploadLoots();

    }
}
