using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordGo.Classes
{
    public class FullGuild
    {
        public Discord.IGuild Guild { get; set; }

        public Discord.IRole NotificationRole { get; set; }
        public Discord.IChannel AdminChannel { get; set; }
        public Discord.IChannel NotificationChannel { get; set; }
        public Discord.IChannel EndMatchChannel { get; set; }

        public Discord.IChannel MatchChatChannel { get; set; }

        public Discord.IChannel ScoreboardChannel { get; set; }

        public List<CsMatch> Matches { get; set; } = new List<CsMatch>();

        public FullGuild(Discord.IGuild guild)
        {
            Guild = guild;
        }
    }
}