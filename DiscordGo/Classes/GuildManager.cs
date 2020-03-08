using Discord.Rest;
using Discord.WebSocket;
using System.Collections.Generic;

namespace DiscordGo.Classes
{
    public class GuildManager
    {
        public SocketGuild Guild { get; set; }

        public List<CsServer> Servers = new List<CsServer>();

        public int ServerId { get; set; } = 1;

        public RestUserMessage ScoresMessage { get; set; }

        public GuildManager(SocketGuild guild)
        {
            Guild = guild;
        }
    }
}