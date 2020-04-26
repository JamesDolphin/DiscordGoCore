namespace DiscordGo.Classes
{
    public class CsMatch
    {
        public int CTScore { get; set; }

        public int TScore { get; set; }

        public bool IsLive { get; set; }

        public string CTName { get; set; } = "CT";

        public string TName { get; set; } = "T";
        public string MapName { get; set; }
        public bool IsFreezeTime { get; set; }

        public int SwapCount { get; set; }

        public int SeriesMaxScore { get; set; }

        public int CTSeriesScore { get; set; }

        public int TSeriesScore { get; set; }
        public bool IsOT { get; internal set; }
        public bool Paused { get; internal set; }
        public int ReadyCount { get; internal set; }
        public int PlayerCount { get; internal set; }

        public int TSwapScore { get; set; }

        public int CtSwapScore { get; set; }

        public CsMatch()
        {
        }

        internal void SwapSides()
        {
            (TName, CTName) = (CTName, TName);

            SwapCount++;
        }

        internal bool ShouldSwapSides(int score)
        {
            if (IsLive)
            {
                var number = 33;

                while (number <= score)
                {
                    if (number == score)
                    {
                        return true;
                    }
                    number += 6;
                }
                return false;
            }
            return false;
        }

        internal void EndMatch()
        {
            CTScore = 0;
            TScore = 0;
            SwapCount = 0;
            CTSeriesScore = 0;
            TSeriesScore = 0;
            SeriesMaxScore = 0;
            IsLive = false;
        }
    }
}