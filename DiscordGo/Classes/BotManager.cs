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

        private readonly Dictionary<string, Action<CommandRequest>> Commands = new Dictionary<string, Action<CommandRequest>>();

        private OverwritePermissions AdminPermissions { get; set; }

        private OverwritePermissions EveryonePermissions { get; set; }

        private char Prefix { get; set; }

        public BotManager(IReadOnlyCollection<SocketGuild> guilds, char prefix)
        {
            foreach (var guild in guilds)
            {
                GuildManagers.Add(new GuildManager(guild));
            }

            Prefix = prefix;
            Commands = GenerateCommands();

            GenerateRolePermissions();
        }

        internal void GenerateRolePermissions()
        {
            AdminPermissions = new OverwritePermissions(viewChannel: PermValue.Allow);

            EveryonePermissions = new OverwritePermissions(viewChannel: PermValue.Deny);
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

                            if(commandRequest.Message.Channel.Name != ChannelNames.Admin)
                            {
                                var adminChannel = GetAdminChannelAsync(commandRequest.Guild) as IChannel;

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
                    await GetAdminRoleAsync(guild);

                    await GetViewerRoleAsync(guild);

                    GuildManagers.Add(new GuildManager(guild));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"EXCEPTION: {e}");
                return;
            }
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

        #region Helpers

        #region Validators

        internal async Task ValidateBotCategoryAsync(CommandRequest commandRequest)
        {
            await GetBaseCategoryAsync(commandRequest.Guild);

            return;
        }

        internal async Task ValidateAdminChannel(CommandRequest commandRequest)
        {
            if (commandRequest.Guild.Channels.FirstOrDefault(x => x.Name == ChannelNames.Admin) == null)
            {
                var adminRole = await GetAdminRoleAsync(commandRequest.Guild) as IRole;

                var everyoneRole = GetEveryoneRole(commandRequest.Guild) as IRole;

                var category = await GetBaseCategoryAsync(commandRequest.Guild);

                var adminChannel = await commandRequest.Guild.CreateTextChannelAsync(ChannelNames.Admin, func: x => { x.CategoryId = category.Id; });

                await adminChannel.AddPermissionOverwriteAsync(adminRole, AdminPermissions);

                await adminChannel.AddPermissionOverwriteAsync((adminRole as IRole), EveryonePermissions);
            }

            return;
        }

        #endregion Validators

        #region Admin Role

        public async Task<SocketRole> GetAdminRoleAsync(SocketGuild guild)
        {
            var adminRole = guild.Roles.FirstOrDefault(x => x.Name == "DiscordGo");

            return adminRole ?? await GenerateAdminRoleAsync(guild);
        }

        private async Task<SocketRole> GenerateAdminRoleAsync(SocketGuild guild)
        {
            var adminRole = await guild.CreateRoleAsync("DiscordGo", color: Color.Teal);

            return guild.Roles.FirstOrDefault(x => x.Name == "DiscordGo");
        }

        #endregion Admin Role

        #region Viewer Role

        private async Task<SocketRole> GetViewerRoleAsync(SocketGuild guild)
        {
            var viewerRole = guild.Roles.FirstOrDefault(x => x.Name == "DiscordGoViewer");

            return viewerRole ?? await GenerateViewerRoleAsync(guild);
        }

        private async Task<SocketRole> GenerateViewerRoleAsync(SocketGuild guild)
        {
            var vierwerRole = await guild.CreateRoleAsync("DiscordGoViewer", color: Color.DarkTeal);

            return guild.Roles.FirstOrDefault(x => x.Name == "DiscordViewer");
        }

        #endregion Viewer Role

        #region Everyone Role

        private SocketRole GetEveryoneRole(SocketGuild guild)
        {
            return guild.EveryoneRole;
        }

        #endregion Everyone Role

        #region Base Category

        private async Task<SocketCategoryChannel> GetBaseCategoryAsync(SocketGuild guild)
        {
            var baseCategory = guild.CategoryChannels.FirstOrDefault(x => x.Name == ChannelNames.Category);

            return baseCategory ?? await GenerateBaseCategory(guild);
        }

        private async Task<SocketCategoryChannel> GenerateBaseCategory(SocketGuild guild)
        {
            await guild.CreateCategoryChannelAsync(ChannelNames.Category, func: x => { x.Position = -1; });

            return guild.CategoryChannels.FirstOrDefault(x => x.Name == ChannelNames.Category);
        }

        #endregion Base Category

        #region Admin Channel

        private async Task<SocketChannel> GetAdminChannelAsync(SocketGuild guild)
        {
            var adminChannel = guild.TextChannels.FirstOrDefault(x => x.Name == ChannelNames.Admin);

            return adminChannel ?? await GenerateAdminChannelAsync(guild);
        }

        private async Task<SocketChannel> GenerateAdminChannelAsync(SocketGuild guild)
        {
            var category = await GetBaseCategoryAsync(guild);
            var adminChannel = await guild.CreateTextChannelAsync(ChannelNames.Admin, func: x => { x.CategoryId = category.Id; });

            return await GetAdminChannelAsync(guild);
        }

        #endregion Admin Channel

        #endregion Helpers
    }
}