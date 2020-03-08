using CoreRCON.Parsers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DiscordGo.Classes.Events
{
    public class StartFreezeTime : IParseable
    {
    }

    public class StartFreezeTimeParser : DefaultParser<StartFreezeTime>
    {
        public override string Pattern { get; } = @"Starting Freeze period";

        public override StartFreezeTime Load(GroupCollection groups)
        {
            return new StartFreezeTime();
        }
    }
}