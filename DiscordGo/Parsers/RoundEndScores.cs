using CoreRCON.Parsers;
using System.Text.RegularExpressions;

namespace DiscordGo.Parsers
{
    public class RoundEndScores : IParseable
    {
        public string WinningTeam { get; set; }
        public int TScore { get; set; }
        public int CTScore { get; set; }
    }

    public class RoundEndScoresParser : DefaultParser<RoundEndScores>
    {
        public override string Pattern { get; } = @"Team ""(?<winning_team>.+?)"" triggered ""SFUI_Notice_.+?"" \(CT ""(?<ct_score>\d+)""\) \(T ""(?<t_score>\d+)""\)";

        //Team "CT" triggered "SFUI_Notice_CTs_Win" (CT "3") (T "0")

        public override RoundEndScores Load(GroupCollection groups)
        {
            return new RoundEndScores
            {
                WinningTeam = groups["winning_team"].Value,
                TScore = int.Parse(groups["t_score"].Value),
                CTScore = int.Parse(groups["ct_score"].Value),
            };
        }
    }
}