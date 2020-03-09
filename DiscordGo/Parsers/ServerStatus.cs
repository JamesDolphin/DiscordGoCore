using CoreRCON.Parsers;
using System.Text.RegularExpressions;

namespace DiscordGo.Parsers
{
    public class ServerStatus : IParseable
    {
        public string Hostname { get; set; }
        public byte PlayerCount { get; set; }
        public string Map { get; set; }
    }

    internal class StatusParser : DefaultParser<ServerStatus>
    {
        public override string Pattern { get; } = @"hostname\s*: (?<Hostname>.+?)\n.+\n.+\n.+\n.+\nmap : (?<MapName>.+?)\n.+\nplayers : (?<PlayerCount>\d+?).+";

        public override ServerStatus Load(GroupCollection groups)
        {
            return new ServerStatus
            {
                Hostname = groups["Hostname"].Value,
                Map = groups["MapName"].Value,
                PlayerCount = byte.Parse(groups["PlayerCount"].Value),
            };
        }
    }
}