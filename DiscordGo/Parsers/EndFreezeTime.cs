using CoreRCON.Parsers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DiscordGo.Parsers
{
    public class EndFreezeTime : IParseable
    {
    }

    public class EndFreezeTimeParser : DefaultParser<EndFreezeTime>
    {
        public override string Pattern { get; } = @"World triggered ""Round_Start""";

        public override EndFreezeTime Load(GroupCollection groups)
        {
            return new EndFreezeTime();
        }
    }
}