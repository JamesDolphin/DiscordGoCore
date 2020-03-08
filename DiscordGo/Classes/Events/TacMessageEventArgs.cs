using Discord.WebSocket;
using System;

namespace DiscordGo.Classes.Events
{
    public class TacMessageEventArgs
    {
        public SocketGuild Guild { get; set; }

        public string PausingTeam { get; set; }

        public int ServerId { get; set; }

        public int CTScore { get; set; }

        public int TScore { get; set; }

        public string CTName { get; set; }

        public string TName { get; set; }

        public DateTime TimeStamp { get; set; }
        public int MatchId { get; set; }
    }
}