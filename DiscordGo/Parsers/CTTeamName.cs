using CoreRCON.Parsers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DiscordGo.Parsers
{
    public class CTTeamName : IParseable
    {
        public string CTName { get; set; }
    }

    public class CTNameParser : DefaultParser<CTTeamName>
    {
        public override string Pattern { get; } = @"Team playing ""CT"": (?<ct_name>.+)";

        // INPUT Team playing "CT": Counter-Terrorists

        // RETURN C

        public override CTTeamName Load(GroupCollection groups)
        {
            return new CTTeamName
            {
                CTName = groups["ct_name"].Value,
            };
        }
    }
}