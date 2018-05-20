using System;
using System.Collections.Generic;
using System.Text;

namespace Pinger
{
	class Server : IComparable
	{
		public string RegionName { get; set; }
		public string Country { get; set; }
		public string City { get; set; }
		public string IP { get; set; }
		public long Ping { get; set; }

		public Server(string regionName, string country, string city, string ip)
		{
			RegionName = regionName;
			Country = country;
			City = city;
			IP = ip;
		}

		public int CompareTo(object obj)
		{
			Server otherServer = obj as Server;
			return Ping.CompareTo(otherServer.Ping);
		}
	}
}
