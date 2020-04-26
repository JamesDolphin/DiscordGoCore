using Discord.WebSocket;
using System;

namespace DiscordGo.Classes.Events
{
    public class OnGenericUpdateEventArgs
    {
        public SocketGuild Guild { get; set; }

        public string Message { get; set; }

        public DateTime TimeStamp { get; set; }
        public int ServerId { get; set; }
    }
}