﻿using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordGo.Classes;
using Newtonsoft.Json;

namespace DiscordGo
{
    internal class Program
    {
        public Config Config { get; set; }

        private readonly DiscordSocketClient _client;

        public GuildManager GuildManager { get; set; }

        private static void Main(string[] args)
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
            }

            // It is recommended to Dispose of a client when you are finished
            // using it, at the end of your app's lifetime.
            _client = new DiscordSocketClient();

            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;
            _client.MessageReceived += MessageReceivedAsync;
            _client.JoinedGuild += JoinedNewGuildAsync;
        }

        private async Task JoinedNewGuildAsync(SocketGuild guild)
        {
            //await GuildManager.AddNewGuild(guild);
            return;
        }

        public async Task MainAsync()
        {
            // Tokens should be considered secret data, and never hard-coded.
            await _client.LoginAsync(TokenType.Bot, Config.DcToken);
            await _client.StartAsync();

            // Block the program until it is closed.
            await Task.Delay(-1);
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private Task ReadyAsync()
        {
            Console.WriteLine($"{_client.CurrentUser} is connected!");

            GuildManager = new GuildManager(_client.Guilds);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            char messagePrefix = message.Content.ToCharArray()[0];

            // The bot should never respond to itself.
            if (message.Author.Id == _client.CurrentUser.Id)
            {
                return;
            }

            // Messages that do not start with the Config.Prefix should be ignored
            if (messagePrefix != Config.Prefix)
            {
                return;
            }

            GuildManager.MessageReceived(message);
        }
    }
}