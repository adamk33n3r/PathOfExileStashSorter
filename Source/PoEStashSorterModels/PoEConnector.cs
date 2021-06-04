using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoEStashSorterModels.Exceptions;
using PoEStashSorterModels.Servers;
using System.IO;

namespace PoEStashSorterModels
{

    public static class PoeConnector
    {
        public static Server server;
        private static string accountName;
        private static List<string> supportedTabTypes = new List<string>() {
            "NormalStash",
            "PremiumStash",
            "QuadStash",
        };
       
        public static void Connect(Server server, string email, string password, bool useSessionId = false)
        {
            PoeConnector.server = server;
            server.Connect(email, password, useSessionId);
            accountName = server.GetAccountName();
        }

        public static List<League> FetchLeagues()
        {
            var characters = FetchCharacters();
            return characters
                .GroupBy(x => x.League)
                .Select(x => new League(x.First().League))
                .ToList();
        }

        public static List<Tab> FetchTabs(League league)
        {
            string jsonData = server.WebClient.DownloadString(string.Format(server.StashUrl, accountName, league.Name, 0));

            // System.IO.File.WriteAllText(string.Format(
            //     $"get-stash-items.accountName-{{0}}_league-{{1}}_tab-0.json",
            //     accountName,
            //     league.Name
            // ), jsonData);

            if (jsonData != "false")
            {
                Stash stash = JsonConvert.DeserializeObject<Stash>(jsonData);
                List<Tab> tabs = stash.Tabs.Where(t => supportedTabTypes.Contains(t.Type)).ToList();
                tabs.ForEach(x => x.League = league);
                return tabs;
            }
            return new List<Tab>();
        }

        // [Obsolete]
        // public static Tab FetchTab(int tabIndex, League league)
        // {
        //     string jsonData = server.WebClient.DownloadString(string.Format(server.StashUrl, league.Name, tabIndex));

        //     Stash stash = JsonConvert.DeserializeObject<Stash>(jsonData);
        //     Tab tab = stash.Tabs.FirstOrDefault(x => x.Index == tabIndex);
        //     tab.Items = stash.Items;
        //     return tab;
        // }

        public static async Task<Tab> FetchTabAsync(int tabIndex, League league)
        {
            while (server.WebClient.IsBusy) { }
            string jsonData = await server.WebClient.DownloadStringTaskAsync(
                new Uri(string.Format(server.StashUrl, accountName, league.Name, tabIndex)));

            Stash stash = JsonConvert.DeserializeObject<Stash>(jsonData);
            Tab tab = stash.Tabs.FirstOrDefault(x => x.Index == tabIndex);

            using (var stringReader = new StringReader(jsonData))
            using (var stringWriter = new StringWriter())
            {
                using (var jsonReader = new JsonTextReader(stringReader))
                using (var jsonWriter = new JsonTextWriter(stringWriter) { Formatting = Formatting.Indented })
                {
                    jsonWriter.WriteToken(jsonReader);
                    File.WriteAllText(string.Format(
                        $"_stash.{{0}}.{{1}}.tab-{{2}}.{{3}}.json",
                        accountName,
                        league.Name,
                        Uri.EscapeDataString(tab.Name),
                        tab.ID
                    ), stringWriter.ToString());
                }
            }

            tab.Items = stash.Items;
            return tab;
        }

        public static List<Character> FetchCharacters()
        {
            string jsonData = server.WebClient.DownloadString(string.Format(server.CharacterUrl, accountName));
            if (jsonData == "false")
                throw new CharacterInfoException();
            var characters = JsonConvert.DeserializeObject<List<Character>>(jsonData);
            return characters;
        }
    }
}
