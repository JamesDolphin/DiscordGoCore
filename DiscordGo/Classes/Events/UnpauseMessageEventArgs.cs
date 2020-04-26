using Discord.WebSocket;
using System;

namespace DiscordGo.Classes.Events
{
    public class UnpauseMessageEventArgs
    {
        public SocketGuild Guild { get; set; }

        public int ServerId { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}