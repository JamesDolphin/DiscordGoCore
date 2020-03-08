using Discord;
using Discord.WebSocket;
using DiscordGo.Classes;
using DiscordGo.Constants;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordGo
{
    internal class Program
    {
        public Config Config { get; set; }

        private readonly DiscordSocketClient _client;

        public BotManager GuildManager { get; set; }

        private static void Main()
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public Program()
        {
            if (File.Exists("./config.json"))
            {
                using (StreamReader reader = new StreamReader("./config.json"))
                {
                    string json = reader.ReadToEnd();
                    Config = JsonConvert.DeserializeObject<Config>(json);
                }

                _client = new DiscordSocketClient();

                _client.Ready += ReadyAsync;
                _client.MessageReceived += MessageReceivedAsync;
                _client.JoinedGuild += JoinedNewGuildAsync;
            }
            else
            {
                Console.WriteLine("NO CONFIG FILE");
            }
        }

        private async Task JoinedNewGuildAsync(SocketGuild guild)
        {
            await GuildManager.AddNewGuildAsync(guild);

            return;
        }

        public async Task MainAsync()
        {
            // Tokens should be considered secret data, and never hard-coded.
            await _client.LoginAsync(TokenType.Bot, Config.DcToken);
            await _client.StartAsync();

            await _client.SetGameAsync($"{Config.Prefix}{CommandConstants.Help}");

            // Block the program until it is closed.
            await Task.Delay(-1);
        }

        private Task ReadyAsync()
        {
            Console.WriteLine($"{_client.CurrentUser} is connected!");

            GuildManager = new BotManager(_client.Guilds, Config);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            if (message.Author.IsBot)
            {
                return;
            }

            if ((message as IMessage).Type == MessageType.GuildMemberJoin)
            {
                return;
            }

            // The bot should never respond to itself.
            if (message.Author.Id == _client.CurrentUser.Id)
            {
                return;
            }

            // Messages that do not start with the Config.Prefix should be ignored
            if (message.Content.ToCharArray()[0] != Config.Prefix)
            {
                return;
            }

            var user = (message.Channel as SocketGuildChannel).Guild.Users.FirstOrDefault(x => x.Id == message.Author.Id);

            if ((message.Channel as SocketGuildChannel).Guild.Roles.FirstOrDefault(x => x.Name == "DiscordGo") == null)
            {
                await (message.Channel as SocketGuildChannel).Guild.CreateRoleAsync("DiscordGo", color: Color.Teal);
            }

            if (user.Roles.FirstOrDefault(x => x.Name == "DiscordGo") == null)
            {
                await message.Channel.SendMessageAsync("You need the DiscordGo role before I can help you.");
            }
            else
            {
                await GuildManager.MessageReceivedAsync(message);
            }

            return;
        }
    }
}