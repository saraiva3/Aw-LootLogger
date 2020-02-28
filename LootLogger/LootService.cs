using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LootLogger.Model;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Diagnostics;

namespace LootLogger
{
    public class LootService : ILootService
    {
        /*https://discordapp.com/api/webhooks/638028770938322944/d7BOBFRCcLVbYFOvSGX_raNDjYQUCQ6Jz2_6wKFSv3ARy4jpFk9AXLELn8J7utGkBA2G
         * 
         * 
         * MF: https://discordapp.com/api/webhooks/648977458837848086/p8h3IimhqUCKbeWHWM-47sqFP2cs1oaMK0fnIUWmF8iMhuO6E7huiC92CesbWjtTjt6A
         */
        private const string url = "https://discordapp.com/api/webhooks/650081433607602197/1P33ZBJ6epG8bf_Er6fy4BWvyTeOx7JV-orgXnr_h8AcvTqut2-30grnjl9XeXjwC8z5";
       
        private List<Player> players;
        private readonly HttpClient client;

        private DateTime lastUploadDate = DateTime.MinValue;

        public LootService()
        {
            players = new List<Player>();
            client = new HttpClient();
        }

        public void AddLootForPlayer(Loot loot, string playerName, Dictionary<string, string> playerDictionary)
        {
            var existingPlayer = this.players.FirstOrDefault(p => p.Name == playerName);
            

            if (existingPlayer != null)
            {
                existingPlayer.Loots.Add(loot);
            }
            else
            {
                if(playerDictionary.Count > 0) { 
                    if (playerDictionary.ContainsKey(playerName))
                    {
                        players.Add(new Player { Name = playerName, Loots = new List<Loot>() { loot } });
                    }
                }
                else
                {
                    players.Add(new Player { Name = playerName, Loots = new List<Loot>() { loot } });
                }
            }

            try
            {

                string content = string.Empty;
                this.players.SelectMany(p => p.Loots).OrderBy(l => l.PickupTime)
                                                         .ToList()
                                                         .ForEach(t => content += t.GetLine());

                Console.WriteLine(content.Length);
                if (content.Length > 1800)
                {
                    UploadLoots();
                    SaveLootsToFile();
                    players = new List<Player>();

                }

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Write(e.StackTrace);
            }

        }

        public void SaveLootsToFile()
        {

            string content = string.Empty;
            content = "Time,Pegou,Quantidade,Item,Morto";
            this.players.SelectMany(p => p.Loots).OrderBy(l => l.PickupTime)
                                                 .ToList()
                                                 .ForEach(loot => content += loot.GetLine());


            using (var fs = File.Create(Path.Combine(Directory.GetCurrentDirectory(), $"CombatLoots-{DateTime.Now.ToString("dd-MMM-HH-mm-ss")}.txt")))
            {
                Byte[] bytes = new UTF8Encoding(true).GetBytes(content);
                fs.Write(bytes, 0, bytes.Length);
            }
        }

        public async void UploadLoots()
        {
            try
            {
              
                string content = string.Empty;

                this.players.SelectMany(p => p.Loots).OrderBy(l => l.PickupTime)
                                                     .ToList()
                                                     .ForEach(loot => content += loot.GetLine() +"\n");

                using (var fs = File.Create(Path.Combine(Directory.GetCurrentDirectory(), $"Upload-{DateTime.UtcNow.ToString("dd-MMM-HH-mm-ss")}.txt")))
                {
                    Byte[] bytes = new UTF8Encoding(true).GetBytes(content);
                    fs.Write(bytes, 0, bytes.Length);
                }

                if (content.Length < 1)
                {
                    Console.WriteLine("Nenhum loot capturado");
                    return;
                }
          
                content = "{\"content\":\"" + content + "\"}";
              

                var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, httpContent);
               
                if (response.IsSuccessStatusCode)
                {                   
                    this.lastUploadDate = DateTime.UtcNow;
                    string url = await response.Content.ReadAsStringAsync();
                    Process.Start(url);

                    Console.WriteLine("Postado no chat.");
                }
                else
                {
                    Console.WriteLine("Falha ao upar os logs");
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Write(e.StackTrace);
            }
        }
    }
}
