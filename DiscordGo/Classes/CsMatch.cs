using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordGo.Classes
{
    public class CsMatch
    {
        public int Id { get; set; }
        public string Ip { get; set; }

        public int Port { get; set; }

        public string RconPassword { get; set; }

        public CsTeam CtTeam { get; set; }

        public CsTeam TTeam { get; set; }

        public CsMatch(int id)
        {
            Id = id;
        }
    }
}