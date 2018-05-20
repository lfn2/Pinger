using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace Pinger
{
    class Program
    {
		private static readonly string SERVERS_URLS = "https://support.purevpn.com/vpn-servers";

		static void Main(string[] args)
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
			Console.WriteLine("Downloading html");
			HtmlDocument htmlDocument = DownloadHtml();

			Console.WriteLine("Parsing servers");
			List<Server> serverList = ParseServers(htmlDocument);

			while (true)
			{
				Console.WriteLine("start pinging");
				List<Server> pingedServers = PingServers(serverList);
				pingedServers.Sort();
				pingedServers.Reverse();
				foreach (Server server in pingedServers)
				{
					Console.WriteLine($"{server.Ping}, {server.IP}, {server.RegionName}, {server.Country}, {server.City}");
				}
				sw.Stop();
				Console.WriteLine(sw.ElapsedMilliseconds);

				Console.ReadLine();
			}
		}

		private static HtmlDocument DownloadHtml()
		{
			using (WebClient webClient = new WebClient())
			{
				var html = webClient.DownloadString(SERVERS_URLS);
				HtmlDocument htmlDocument = new HtmlDocument();
				htmlDocument.LoadHtml(html);

				return htmlDocument;
			}
		}

		private static List<Server> ParseServers(HtmlDocument htmlDocument)
		{
			List<Server> serverList = new List<Server>();

			HtmlNode serversTable = htmlDocument.GetElementbyId("servers_data");
			int count = 0;
			int max = serversTable.ChildNodes.Count;
			Parallel.ForEach(serversTable.ChildNodes, row =>
			{
				Interlocked.Increment(ref count);
				Console.WriteLine($"{count}/{max}");
				HtmlNodeCollection childNodes = row.ChildNodes;
				string region = childNodes[0].InnerText;
				string country = childNodes[1].InnerText;
				string city = childNodes[2].InnerText;
				if (region == "South America" || region == "North America")
				{
					serverList.Add(new Server(region, country, city, childNodes[3].InnerText));
					serverList.Add(new Server(region, country, city, childNodes[4].InnerText));
					serverList.Add(new Server(region, country, city, childNodes[5].InnerText));
				}
			});

			return serverList;
		}

		private static List<Server> PingServers(List<Server> serverList)
		{
			List<Server> pingedServers = new List<Server>();
			HashSet<Thread> threads = new HashSet<Thread>();
			int count = 0;
			int max = serverList.Count;
			foreach (Server server in serverList)
			{
				Thread thread = new Thread(() =>
				{
					try
					{
						count++;
						Console.WriteLine($"pinging {server.IP} ({count}/{max})");
						PingReply reply = new Ping().Send(server.IP, 130);
						if (reply.Status == IPStatus.Success)
						{
							server.Ping = reply.RoundtripTime;
							pingedServers.Add(server);
						}
					}
					catch (Exception) { }
				});
				threads.Add(thread);
				thread.Start();
			}

			foreach (Thread thread in threads)
			{
				thread.Join();
			}

			return pingedServers;
		}
	}
}
