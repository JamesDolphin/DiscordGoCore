using CoreRCON.Parsers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DiscordGo.Parsers
{
    public class TTeamName : IParseable
    {
        public string TName { get; set; }
    }

    public class TNameParser : DefaultParser<TTeamName>
    {
        public override string Pattern { get; } = @"Team playing ""TERRORIST"": (?<t_name>.+)";

        // INPUT Team playing "CT": Counter-Terrorists

        // RETURN C

        public override TTeamName Load(GroupCollection groups)
        {
            return new TTeamName
            {
                TName = groups["t_name"].Value,
            };
        }
    }
}