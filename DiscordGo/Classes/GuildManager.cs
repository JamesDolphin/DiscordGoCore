using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordGo.Classes
{
    public class GuildManager
    {
        public SocketGuild Guild { get; set; }

        public List<CsServer> Servers = new List<CsServer>();
        public GuildManager(SocketGuild guild)
        {
            Guild = guild;
        }
    }
}