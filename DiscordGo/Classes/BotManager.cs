using Discord;
using Discord.WebSocket;
using DiscordGo.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordGo.Classes
{
    public class BotManager
    {
        private List<GuildManager> GuildManagers { get; set; } = new List<GuildManager>();

        private Dictionary<string, Action<CommandRequest>> Commands = new Dictionary<string, Action<CommandRequest>>();

        private char Prefix { get; set; }

        public BotManager(IReadOnlyCollection<SocketGuild> guilds, char prefix)
        {
            foreach (var guild in guilds)
            {
                GuildManagers.Add(new GuildManager(guild));
            }

            Prefix = prefix;
            Commands = GenerateCommands();
        }

        private Dictionary<string, Action<CommandRequest>> GenerateCommands()
        {
            return new Dictionary<string, Action<CommandRequest>>()
            {
                {
                    CommandConstants.SetAdminChannel, async (commandRequest) =>
                    {
                        if(commandRequest.NeedHelp)
                        {
                           var eb = new EmbedBuilder
                           {
                               Title = $"{CommandConstants.SetAdminChannel}",

                               ThumbnailUrl = "https://cdn.discordapp.com/attachments/546946476836782090/546955027210829825/no_backround.png",

                               Color = Color.Teal
                           };

                            var fieldBuilder = new EmbedFieldBuilder
                            {
                                Name = "Creates DiscordGo admin channel where all commands should be initiated from",

                                Value = "field"
                            };

                            eb.AddField("Usage", fieldBuilder.Build());

                            eb.WithFooter("Use command \"help\" to list all available commands");

                            eb.Description = $"\"{CommandConstants.SetAdminChannel}\"";

                            await commandRequest.Message.Channel.SendMessageAsync(string.Empty, false, eb.Build());
                        }
                        else
                        {
                            await ValidateBotCategoryAsync(commandRequest);
                        }
                    }
                },
                 {
                    CommandConstants.AddServer, async (commandRequest) =>
                    {
                        await ValidateAdminChannel(commandRequest);

                        if(commandRequest.NeedHelp)
                        {
                           var eb = new EmbedBuilder
                           {
                               Title = $"{CommandConstants.AddServer}",

                               ThumbnailUrl = "https://cdn.discordapp.com/attachments/546946476836782090/546955027210829825/no_backround.png",

                               Color = Color.Teal
                           };

                            var fieldBuilder = new EmbedFieldBuilder
                            {
                                Name = "Adds a server to the tracking list",

                                Value = "field"
                            };

                            eb.AddField("Usage", fieldBuilder.Build());

                            eb.WithFooter("Use command \"help\" to list all available commands");

                            eb.Description = $"\"{CommandConstants.SetAdminChannel}\"";

                            await commandRequest.Message.Channel.SendMessageAsync(string.Empty, false, eb.Build());
                        }
                        else
                        {
                            await ValidateBotCategoryAsync(commandRequest);
                            await ValidateAdminChannel(commandRequest);

                            if(commandRequest.Guild.Channels.FirstOrDefault(x => x.Name == ChannelNames.Admin) == null)
                            {
                                var category = commandRequest.Guild.CategoryChannels.FirstOrDefault(x => x.Name == ChannelNames.Category);

                                await commandRequest.Guild.CreateTextChannelAsync(ChannelNames.Admin, func: x => {x.CategoryId = category.Id; });
                            }

                            if(commandRequest.Message.Channel.Name != ChannelNames.Admin)
                            {
                                var adminChannel = commandRequest.Guild.Channels.FirstOrDefault(x => x.Name == ChannelNames.Admin);

                                await commandRequest.Message.Channel.SendMessageAsync($"That command must be used in {MentionUtils.MentionChannel(adminChannel.Id)}");
                            }
                        }
                    }
                },
                {
                    CommandConstants.Help, async (commandRequest) =>
                    {
                        var commandList = string.Empty;

                        foreach(var key in Commands.Keys.Where(x => x != CommandConstants.Help))
                        {
                            commandList = $"{commandList} {key}\n";
                        }

                        commandList = $"```\n{commandList}\n CommandName Help for more information```";

                        await commandRequest.Message.Channel.SendMessageAsync(commandList);
                    }
                }
            };
        }

        internal async Task AddNewGuildAsync(SocketGuild guild)
        {
            try
            {
                if (GuildManagers.FirstOrDefault(x => x.Guild.Id == guild.Id) == null)
                {
                    Discord.Rest.RestRole role;

                    if (guild.Roles.FirstOrDefault(x => x.Name == "DiscordGo") == null)
                    {
                        role = await guild.CreateRoleAsync("DiscordGo", color: Color.Teal);

                        await guild.CurrentUser.AddRoleAsync(role);

                        await guild.CreateRoleAsync("DiscordGoViewer", color: Color.DarkTeal);
                    }

                    GuildManagers.Add(new GuildManager(guild));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"EXCEPTION: {e}");
                return;
            }
        }

        internal async Task ValidateBotCategoryAsync(CommandRequest commandRequest)
        {
            if (commandRequest.Guild.CategoryChannels.FirstOrDefault(x => x.Name == ChannelNames.Category) == null)
            {
                var category = await commandRequest.Guild.CreateCategoryChannelAsync(ChannelNames.Category, func: x => { x.Position = 0; });

                await category.ModifyAsync(func: x => { x.Position = 0; });
            }

            return;
        }

        internal async Task ValidateAdminChannel(CommandRequest commandRequest)
        {
            if (commandRequest.Guild.Channels.FirstOrDefault(x => x.Name == ChannelNames.Admin) == null)
            {
                var category = commandRequest.Guild.CategoryChannels.FirstOrDefault(x => x.Name == ChannelNames.Category);

                var adminChannel = await commandRequest.Guild.CreateTextChannelAsync(ChannelNames.Admin, func: x => { x.CategoryId = category.Id; });

                var role = commandRequest.Guild.Roles.First(x => x.Name == "DiscordGo");

                var adminPermissions = new ChannelPermissions(false, false, true, true, true, false, false, false, false, true, false, true, true, true, false, false, false, true, false, false, false);
            }

            return;
        }

        internal async Task MessageReceivedAsync(SocketMessage message)
        {
            var channel = message.Channel as SocketGuildChannel;

            var arguments = message.Content.Substring(1).Split(' ');

            var commandRequest = new CommandRequest()
            {
                Guild = channel.Guild,
                Message = message,
                Arguments = arguments,
                ReturnString = string.Empty,
                NeedHelp = (arguments.Length > 1 && arguments[1] == CommandConstants.Help) ? true : false
            };

            if (Commands.ContainsKey(arguments[0]))
            {
                Commands[arguments[0]](commandRequest);

                Console.WriteLine(commandRequest.ReturnString);
            }
            else
            {
                await message.Channel.SendMessageAsync($"{message.Author.Mention} that is not a valid command. Use {Prefix}{CommandConstants.Help} to get a full list of available commands");
            }

            return;
        }
    }
}