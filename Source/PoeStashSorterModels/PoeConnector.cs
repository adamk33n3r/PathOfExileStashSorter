using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoeStashSorterModels.Exceptions;
using PoeStashSorterModels.Servers;

namespace POEStashSorterModels
{

    public static class PoeConnector
    {
        public static Server server;
        private static string accountName;
       
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
                List<Tab> tabs = stash.Tabs;
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

            System.IO.File.WriteAllText(string.Format(
                $"get-stash-items.accountName-{{0}}_league-{{1}}_tab-{{2}}.json",
                accountName,
                league.Name,
                tabIndex
            ), jsonData);

            Stash stash = JsonConvert.DeserializeObject<Stash>(jsonData);
            Tab tab = stash.Tabs.FirstOrDefault(x => x.Index == tabIndex);
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
