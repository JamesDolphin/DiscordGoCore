using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordGo.Classes.Events
{
    public class MatchEndEventArgs
    {
        public SocketGuild Guild { get; set; }
        public string MapName { get; set; }

        public string CTName { get; set; }

        public string TName { get; set; }

        public int CTScore { get; set; }

        public int TScore { get; set; }
        public int MatchId { get; set; }

        public DateTime TimeStamp { get; set; }
        public int ServerId { get; set; }
    }
}