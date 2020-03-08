using Discord;
using Discord.WebSocket;
using DiscordGo.Classes.Events;
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

        private Config Config { get; set; }

        public BotManager(IReadOnlyCollection<SocketGuild> guilds, Config config)
        {
            foreach (var guild in guilds)
            {
                GuildManagers.Add(new GuildManager(guild));
            }

            Config = config;
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

                            eb.WithFooter($"Use command \"{Config.Prefix}{CommandConstants.Help}\" to list all available commands");

                            eb.Description = $"{Config.Prefix}{CommandConstants.AddServer}";

                            await commandRequest.Message.Channel.SendMessageAsync(string.Empty, false, eb.Build());
                        }
                        else
                        {
                            await GetChannelAsync(commandRequest.Guild, ChannelNames.Admin);
                            await commandRequest.Message.DeleteAsync();

                            if(commandRequest.Message.Channel.Name != ChannelNames.Admin)
                            {
                                var adminChannel = await GetChannelAsync(commandRequest.Guild, ChannelNames.Admin) as IChannel;

                                await commandRequest.Message.Channel.SendMessageAsync($"That command must be used in {MentionUtils.MentionChannel(adminChannel.Id)}");
                            }
                            else
                            {
                                var guild = GuildManagers.FirstOrDefault(x => x.Guild.Id == commandRequest.Guild.Id);

                                var server = guild.Servers.FirstOrDefault(x => x.IpAddress == commandRequest.Arguments[1]);

                                if(server != null)
                                {
                                    await commandRequest.Message.Channel.SendMessageAsync($"That server (ID: {server.ID}) is already being followed. Use {Config.Prefix}{CommandConstants.ServerList} to find it");
                                }
                                else
                                {
                                    bool error = false;

                                    var ipPort = commandRequest.Arguments[1].Split(':');

                                    if(ipPort.Length != 2)
                                    {
                                        error = true;
                                    }

                                    if(commandRequest.Arguments.Length != 3)
                                    {
                                        error = true;
                                    }

                                    if(!error)
                                    {
                                        var ip = ipPort[0];
                                        var port = Convert.ToUInt16(ipPort[1]);

                                        server = new CsServer(ip, port, commandRequest.Arguments[2], Config, guild, guild.ServerId);

                                        await server.InitializeRconAsync();

                                        if (server.Authed)
                                        {
                                            var eb = new EmbedBuilder
                                            {
                                                Title = $"Server Added ✅",

                                                ThumbnailUrl = "https://cdn.discordapp.com/attachments/546946476836782090/546955027210829825/no_backround.png",

                                                Color = Color.Teal
                                            };

                                            var idField = new EmbedFieldBuilder
                                            {
                                                Name = $"{server.ID}",
                                                Value = "field"
                                            };

                                            var ipField = new EmbedFieldBuilder
                                            {
                                                Name = $"{ip}:{port}",

                                                Value = "field"
                                            };

                                            eb.AddField("ID", idField.Build());

                                            eb.AddField("IP Address", ipField.Build());

                                            eb.WithFooter($"Use command \"{Config.Prefix}{CommandConstants.Help}\" to list all available commands");

                                            await commandRequest.Message.Channel.SendMessageAsync(string.Empty, false, eb.Build());

                                            server.ChatMessageEventArgs += CsServerChatMessageAsync;

                                            server.MatchStartEventArgs += CsServerMatchLiveAsync;

                                            server.TacMessageEventArgs += CsServerTacPauseAsync;

                                            server.TechMessageEventArgs += CsServerTechPauseAsync;

                                            server.MatchSwapSidesEventArgs += CsServerSwapSidesAsync;

                                            server.UnpauseMessageEventArgs += CsServerUnpauseAsync;

                                            server.ScoreUpdateEventsArgs += CsServerScoreUpdateAsycAsync;

                                            guild.Servers.Add(server);

                                            guild.ServerId ++;
                                        }
                                        else
                                        {
                                            var eb = new EmbedBuilder
                                            {
                                                Title = $"❌ FAILED TO AUTHENTICATE ❌",

                                                ThumbnailUrl = "https://cdn.discordapp.com/attachments/546946476836782090/546955027210829825/no_backround.png",

                                                Color = Color.Teal
                                            };

                                            var reasonField = new EmbedFieldBuilder
                                            {
                                                Name = $"Failed to authenticate, make sure rcon password is correct",
                                                Value = "field"
                                            };

                                            eb.AddField("Reason", reasonField.Build());

                                            eb.WithFooter($"Use command \"{Config.Prefix}{CommandConstants.Help}\" to list all available commands");

                                            await commandRequest.Message.Channel.SendMessageAsync(string.Empty, false, eb.Build());
                                        }
                                    }
                                    else
                                    {
                                        await commandRequest.Message.Channel.SendMessageAsync($"Command syntax is incorrect. Use {Config.Prefix}{CommandConstants.AddServer} 123.45.678:12345 rconPassword");
                                    }
                                }
                            }
                        }
                    }
                },
                 {
                    CommandConstants.ServerList, async (commandRequest) =>
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
                                Name = "Lists all servers being tracked on this server",

                                Value = "field"
                            };

                            eb.AddField("Usage", fieldBuilder.Build());

                            eb.WithFooter($"Use command \"{Config.Prefix}{CommandConstants.Help}\" to list all available commands");

                            eb.Description = $"{Config.Prefix}{CommandConstants.ServerList}";

                            await commandRequest.Message.Channel.SendMessageAsync(string.Empty, false, eb.Build());
                        }
                        else
                        {
                            await ValidateBotCategoryAsync(commandRequest);
                            await ValidateAdminChannel(commandRequest);

                            if(commandRequest.Message.Channel.Name != ChannelNames.Admin)
                            {
                                var adminChannel = await GetChannelAsync(commandRequest.Guild, ChannelNames.Admin) as IChannel;

                               await commandRequest.Message.Channel.SendMessageAsync($"That command must be used in {MentionUtils.MentionChannel(adminChannel.Id)}");
                            }
                            else
                            {
                                var serverList = string.Empty;

                                var guild = GuildManagers.FirstOrDefault(x => x.Guild.Id == commandRequest.Guild.Id);

                                foreach(var server in guild.Servers)
                                {
                                    serverList = $"{serverList} [{server.ID}] {server.IpAddress}\n";
                                }

                                serverList = $"```\n{serverList}\n```";

                                await commandRequest.Message.Channel.SendMessageAsync(serverList);
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

                        commandList = $"```\n{commandList}\n {Config.Prefix}CommandName Help for more information```";

                        await commandRequest.Message.Channel.SendMessageAsync(commandList);
                    }
                }
            };
        }

        #region Match Messages

        private async void CsServerScoreUpdateAsycAsync(object sender, ScoreUpdateEventsArgs e)
        {
            var serverList = string.Empty;

            var scoresChannel = await GetChannelAsync(e.GuildManager.Guild, ChannelNames.Scores) as ISocketMessageChannel;

            foreach (var server in e.GuildManager.Servers)
            {
                serverList = $"{serverList} ID: [{server.ID}] CT: {server.Match.CTName} ({server.Match.CTScore}) vs T: {server.Match.TName} ({server.Match.TScore})\n";
            }

            serverList = $"```cs\n{serverList}\n```";

            var oldMessage = e.GuildManager.ScoresMessage;

            try
            {
                await oldMessage.ModifyAsync(x => { x.Content = serverList; });
            }
            catch (Exception)
            {
                e.GuildManager.ScoresMessage = await scoresChannel.SendMessageAsync(serverList);
            }
        }

        private async void CsServerUnpauseAsync(object sender, UnpauseMessageEventArgs e)
        {
            var notification = await GetChannelAsync(e.Guild, ChannelNames.Notifications) as ISocketMessageChannel;

            await notification.SendMessageAsync($"[{e.ServerId}] {MentionUtils.MentionRole((await GetRoleAsync(e.Guild, RoleNames.Broadcast) as IRole).Id)} Match has unpaused - {e.TimeStamp.ToString("HH:mm:ss")}");
        }

        private async void CsServerSwapSidesAsync(object sender, MatchSwapSidesEventArgs e)
        {
            var notification = await GetChannelAsync(e.Guild, ChannelNames.Notifications) as ISocketMessageChannel;

            await notification.SendMessageAsync($"[{e.ServerId}] {MentionUtils.MentionRole((await GetRoleAsync(e.Guild, RoleNames.Broadcast) as IRole).Id)} Teams have swapped sides {e.CTName} ({e.CTScore}) - {e.TName} ({e.TScore}) - {e.TimeStamp.ToString("HH:mm:ss")}");
        }

        private async void CsServerTechPauseAsync(object sender, TechMessageEventArgs e)
        {
            var notification = await GetChannelAsync(e.Guild, ChannelNames.Notifications) as ISocketMessageChannel;

            await notification.SendMessageAsync($"[{e.ServerId}] {MentionUtils.MentionRole((await GetRoleAsync(e.Guild, RoleNames.Broadcast) as IRole).Id)} Technical pause ({e.PausingTeam}) Scores: {e.CTName} ({e.CTScore}) {e.TName} ({e.TScore}) - {e.TimeStamp.ToString("HH:mm:ss")}");
        }

        private async void CsServerTacPauseAsync(object sender, TacMessageEventArgs e)
        {
            var notification = await GetChannelAsync(e.Guild, ChannelNames.Notifications) as ISocketMessageChannel;

            await notification.SendMessageAsync($"[{e.ServerId}] {MentionUtils.MentionRole((await GetRoleAsync(e.Guild, RoleNames.Broadcast) as IRole).Id)} Tactical pause ({e.PausingTeam}) Scores: {e.CTName} ({e.CTScore}) {e.TName} ({e.TScore}) - {e.TimeStamp.ToString("HH:mm:ss")}");
        }

        private async void CsServerMatchLiveAsync(object sender, MatchStartEventArgs e)
        {
            var notification = await GetChannelAsync(e.Guild, ChannelNames.Notifications) as ISocketMessageChannel;

            await notification.SendMessageAsync($"[{e.ServerId}] {MentionUtils.MentionRole((await GetRoleAsync(e.Guild, RoleNames.Broadcast) as IRole).Id)} Match is now live on {e.MapName} - {e.TimeStamp.ToString("HH:mm:ss")}");
        }

        private async void CsServerChatMessageAsync(object sender, ChatMessageEventArgs e)
        {
            var chatChannel = await GetChannelAsync(e.Guild, ChannelNames.Chat) as ISocketMessageChannel;

            await chatChannel.SendMessageAsync($"[{e.ServerId}] {e.TimeStamp.ToString("HH:mm:ss")}: {e.ChatMessage.Player.Name} => ({e.ChatMessage.Channel}) {e.ChatMessage.Message}");
        }

        #endregion Match Messages

        internal async Task AddNewGuildAsync(SocketGuild guild)
        {
            try
            {
                if (GuildManagers.FirstOrDefault(x => x.Guild.Id == guild.Id) == null)
                {
                    await GetRoleAsync(guild, RoleNames.Admin);

                    await GetRoleAsync(guild, RoleNames.Broadcast);

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
            try
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
                    await message.Channel.SendMessageAsync($"{message.Author.Mention} that is not a valid command. Use {Config.Prefix}{CommandConstants.Help} to get a full list of available commands");
                }

                return;
            }
            catch (Exception e)
            {
                Console.WriteLine($"EXCEPTION: {e}");
                return;
            }
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
                var adminRole = await GetRoleAsync(commandRequest.Guild, RoleNames.Admin) as IRole;

                var everyoneRole = GetEveryoneRole(commandRequest.Guild) as IRole;

                var category = await GetBaseCategoryAsync(commandRequest.Guild);

                var adminChannel = await commandRequest.Guild.CreateTextChannelAsync(ChannelNames.Admin, func: x => { x.CategoryId = category.Id; });

                await adminChannel.AddPermissionOverwriteAsync(adminRole, AdminPermissions);

                await adminChannel.AddPermissionOverwriteAsync((adminRole as IRole), EveryonePermissions);
            }

            return;
        }

        #endregion Validators

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

        #region Get Role

        public async Task<SocketRole> GetRoleAsync(SocketGuild guild, string roleName)
        {
            var adminRole = guild.Roles.FirstOrDefault(x => x.Name == roleName);

            return adminRole ?? await GenerateRoleAsync(guild, roleName);
        }

        private async Task<SocketRole> GenerateRoleAsync(SocketGuild guild, string roleName)
        {
            await guild.CreateRoleAsync(roleName, color: Color.Teal);

            return await GetRoleAsync(guild, roleName);
        }

        #endregion Get Role

        #region Get Channel

        private async Task<SocketChannel> GetChannelAsync(SocketGuild guild, string channelName)
        {
            var channel = guild.TextChannels.FirstOrDefault(x => x.Name == channelName);

            return channel ?? await GenerateChannelAsync(guild, channelName);
        }

        private async Task<SocketChannel> GenerateChannelAsync(SocketGuild guild, string channelName)
        {
            var category = await GetBaseCategoryAsync(guild);
            var adminChannel = await guild.CreateTextChannelAsync(channelName, func: x => { x.CategoryId = category.Id; });

            return await GetChannelAsync(guild, channelName);
        }

        #endregion Get Channel

        #endregion Helpers
    }
}