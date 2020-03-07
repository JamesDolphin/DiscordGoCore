using Discord;
using Discord.WebSocket;
using DiscordGo.Constants;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscordGo.Classes
{
    public class GuildManager
    {
        private List<SocketGuild> Guilds { get; set; } = new List<SocketGuild>();

        private Dictionary<string, Action<CommandRequest>> Commands = new Dictionary<string, Action<CommandRequest>>();

        public GuildManager(IReadOnlyCollection<SocketGuild> guilds)
        {
            Guilds = guilds.ToList();
            Commands = GenerateCommands();
        }

        private Dictionary<string, Action<CommandRequest>> GenerateCommands()
        {
            return new Dictionary<string, Action<CommandRequest>>()
            {
                {
                    CommandConstants.SetAdminChannel, async (commandRequest) =>
                    {
                        if(commandRequest.Guild.CategoryChannels.FirstOrDefault(x => x.Name == "DiscordGo") == null)
                        {
                            var category = await commandRequest.Guild.CreateCategoryChannelAsync("DiscordGo", func: x => {x.Position = 0; });

                            await category.ModifyAsync(func: x=>{x.Position  = 0; });
                        }

                        if(commandRequest.Guild.Channels.FirstOrDefault(x => x.Name == "AdminChannel") == null)
                        {
                            var category = commandRequest.Guild.CategoryChannels.FirstOrDefault(x => x.Name == "DiscordGo");

                            await commandRequest.Guild.CreateTextChannelAsync("name", func: x => {x.CategoryId = category.Id; });
                        }
                    }
                }
            };
        }

        internal void MessageReceived(SocketMessage message)
        {
            var channel = message.Channel as SocketGuildChannel;

            var arguments = message.Content.Substring(1).Split(' ');

            var myGuild = Guilds.FirstOrDefault(x => x.Id == channel.Guild.Id);

            var commandRequest = new CommandRequest()
            {
                Guild = channel.Guild,
                Message = message,
                Arguments = arguments
            };

            if (Commands.ContainsKey(arguments[0]))
            {
                Commands[arguments[0]](commandRequest);
            }
        }
    }
}