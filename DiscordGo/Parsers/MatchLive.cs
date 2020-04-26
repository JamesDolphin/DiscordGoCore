namespace DiscordGo.Parsers
{
    using CoreRCON.Parsers;
    using System.Text.RegularExpressions;

    public class MatchLive : IParseable
    {
        public string MapName { get; set; }
    }

    public class MatchLiveParser : DefaultParser<MatchLive>
    {
        public override string Pattern { get; } = @"""ESEA"" triggered ""ESEA_Match_Begin""";

        //World triggered "Match_Start" on "de_dust2"

        public override MatchLive Load(GroupCollection groups)
        {
            return new MatchLive
            {
                MapName = groups["map_name"].Value.Replace("de_", "")
            };
        }
    }
}