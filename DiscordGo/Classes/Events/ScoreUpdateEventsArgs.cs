using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordGo.Classes.Events
{
    public class ScoreUpdateEventsArgs
    {
        public CsMatch Match { get; set; }

        public DateTime TimeStamp { get; set; }

        public GuildManager GuildManager { get; set; }
    }
}