using Discord.WebSocket;
using System.Collections.Generic;

namespace DiscordGo.Classes
{
    public class CommandRequest
    {
        public SocketGuild Guild { get; set; }
        public SocketMessage Message { get; set; }
        public string[] Arguments { get; set; }
        public bool NeedHelp { get; set; }
        public string ReturnString { get; set; }
    }
}