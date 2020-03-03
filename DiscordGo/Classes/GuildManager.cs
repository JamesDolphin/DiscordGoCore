using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DiscordGo.Classes
{
    public class GuildManager
    {
        private List<FullGuild> _guilds { get; set; } = new List<FullGuild>();

        private List<FullGuild> Guilds
        {
            get { return _guilds; }
            set
            {
                _guilds = value;
                SaveGuilds();
            }
        }

        private string Path { get; set; }

        public GuildManager(string path)
        {
            Path = path;
        }

        private void SaveGuilds()
        {
            try
            {
                using (StreamWriter file = File.CreateText(Path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    //serialize object directly into file stream
                    serializer.Serialize(file, JsonConvert.SerializeObject(Guilds));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"EXCEPTION: {e}");
            }
        }

        public void AddNewGuild(SocketGuild guild)
        {
            var guilds = Guilds;

            guilds.Add(new FullGuild(guild));
            Guilds = guilds;

            Console.WriteLine(Guilds.Count);
        }
    }
}