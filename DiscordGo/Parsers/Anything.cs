using CoreRCON.Parsers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DiscordGo.Parsers
{
    public class Anything : IParseable
    {
        public string TName { get; set; }
    }

    public class AnythingParser : DefaultParser<Anything>
    {
        public override string Pattern { get; } = @"(<anything>.+)";

        // INPUT Team playing "CT": Counter-Terrorists

        // RETURN C

        public override Anything Load(GroupCollection groups)
        {
            return new Anything
            {
                TName = groups["anything"].Value,
            };
        }
    }
}