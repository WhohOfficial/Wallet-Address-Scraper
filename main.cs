using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Scraper
{
    class Program
    {
        static async Task Main() {
            var target = @"https://www.blockchain.com/btc/unconfirmed-transactions";
            while(true)
            {
                Console.WriteLine("Gathering New Html Page");
                var response_str = await Gather(target);
                Console.WriteLine("Dumping Logs");
                await DumpLog(response_str);
                var content = await File.ReadAllTextAsync($"dump{fc}");
                var parsed_lst = Parse(content);
                await LogWrite(parsed_lst);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Starting new cycle..\n");
                Console.ForegroundColor = ConsoleColor.White;
                Thread.Sleep(3000);
            }
        }

        static List<string> Cache = new List<string>();

        static Regex reg = new Regex("\"address\":\"([13][a-km-zA-HJ-NP-Z1-9]{25,35})\"");
        static List<string> Parse(string content) {
            var tcache = reg.Matches(content).Cast<Match>().Select(match => match.Groups[0].Value).ToList();
            var clean_lst = new List<string>();
            foreach(var c in tcache) {
                var match = Cache.FirstOrDefault(stringToCheck => stringToCheck.Contains(c));
                if (match is null)
                    clean_lst.Add(c);
            }
            Cache = clean_lst;
            return clean_lst;
        }

        static int fs = 0;
        static async Task LogWrite(List<string> ls)
        {
            while (File.Exists($"succ{fs}"))
                fs++;

            Console.ForegroundColor = ConsoleColor.Green;
            var toWrite = string.Empty;
            foreach (var s in ls) {
                Console.WriteLine($"New Address: \"{s}\"");
                toWrite += $"{s}\n";
            }
            await File.WriteAllTextAsync($"succ{fs}", toWrite);
            Console.WriteLine($"Saved addresses file to: \"succ{fs}\"");
            Console.ForegroundColor = ConsoleColor.White;
        }

        static async Task<string> Gather(string uri) {
            using var client = new HttpClient();
            var response = await client.GetStringAsync(uri);
            return response;
        }

        static int fc = 0;
        static async Task DumpLog(string content) {
            while (File.Exists($"dump{fc}"))
                fc++;

            await File.WriteAllTextAsync($"dump{fc}", content);
            Console.WriteLine($"Logs Dumped at \"dump{fc}\"");
        }
    }
}
