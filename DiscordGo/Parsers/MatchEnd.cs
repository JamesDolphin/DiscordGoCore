using CoreRCON.Parsers;
using System;
using System.Text.RegularExpressions;

namespace DiscordGo.Parsers
{
    public class MatchEnd : IParseable
    {
        public int MatchId { get; set; }
    }

    public class MatchEndParser : DefaultParser<MatchEnd>
    {
        public override string Pattern { get; } = @".+""ESEA_Match_Finished"" \(canceled ""0""\) \(stats_id ""(?<match_id>\d+)""\).+";

        public override MatchEnd Load(GroupCollection groups)
        {
            try
            {
                var A = groups["match_id"].Value;

                return new MatchEnd
                {
                    MatchId = int.Parse(groups["match_id"].Value)
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new MatchEnd();
            }
        }
    }
}