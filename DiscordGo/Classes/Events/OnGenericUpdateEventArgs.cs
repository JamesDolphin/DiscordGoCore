using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

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