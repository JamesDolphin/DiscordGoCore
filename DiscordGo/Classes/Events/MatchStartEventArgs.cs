using Discord.WebSocket;
using System;

namespace DiscordGo.Classes
{
    public class MatchStartEventArgs
    {
        public SocketGuild Guild { get; set; }
        public string MapName { get; set; }

        public string CTName { get; set; }

        public string TName { get; set; }

        public DateTime TimeStamp { get; set; }
        public int ServerId { get; set; }
    }
}