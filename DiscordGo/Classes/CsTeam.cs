﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordGo.Classes
{
    public class CsTeam
    {
        public string Name { get; set; }

        public int Score { get; set; }

        public string HudName { get; set; }

        public int SeriesScore { get; set; }

        public List<CsPlayer> Players { get; set; } = new List<CsPlayer>();

        public CsTeam()
        {
        }
    }
}