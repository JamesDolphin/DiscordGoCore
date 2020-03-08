using CoreRCON.Parsers.Standard;
using Discord.WebSocket;
using System;

namespace DiscordGo.Classes.Events
{
    public class ChatMessageEventArgs
    {
        public ChatMessage ChatMessage { get; set; }

        public DateTime TimeStamp { get; set; }

        public SocketGuild Guild { get; set; }

        public int ServerId { get; set; }
    }
}