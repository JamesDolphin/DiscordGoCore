using DiscordGo.Utils;
using System;

namespace DiscordGo.Classes
{
    public class CsMatch
    {
        public int CTScore { get; set; }

        public int TScore { get; set; }

        public bool IsLive { get; set; }

        public string CTName { get; set; }

        public string TName { get; set; }
        public string MapName { get; set; }
        public bool IsFreezeTime { get; set; }

        public int SwapCount { get; set; }

        public int SeriesMaxScore { get; set; }

        public int CTSeriesScore { get; set; }

        public int TSeriesScore { get; set; }
        public bool IsOT { get; internal set; }
        public bool Paused { get; internal set; }

        public CsMatch()
        {
        }

        internal void SwapSides()
        {
            (TScore, CTScore) = (CTScore, TScore);

            (TName, CTName) = (CTName, TName);

            SwapCount++;
        }

        internal bool ShouldSwapSides()
        {
            if (IsLive)
            {
                if (TScore + CTScore >= 30)
                {
                    var score = 33;

                    while (score <= TScore + CTScore)
                    {
                        if (score == TScore + CTScore)
                        {
                            return true;
                        }

                        score += 6;
                    }
                    return false;
                }
                else if (TScore + CTScore == 15)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
    }
}