using CoreRCON;
using CoreRCON.Parsers.Standard;
using DiscordGo.Classes.Events;
using DiscordGo.Parsers;

using DiscordGo.Utils;
using System;
using System.Net;
using System.Threading.Tasks;

namespace DiscordGo.Classes
{
    public class CsServer
    {
        private string IP { get; set; }
        private ushort Port { get; set; }
        public string IpAddress { get { return $"{IP}:{Port}"; } }
        private string RconPassword { get; set; }

        public bool Authed { get; set; }

        private ushort LogPort { get; set; }
        private LogReceiver LogReceiver { get; set; }

        private Config Config { get; set; }

        private RCON Rcon { get; set; }

        public event EventHandler<ChatMessageEventArgs> ChatMessageEventArgs;

        public event EventHandler<MatchStartEventArgs> MatchStartEventArgs;

        public event EventHandler<TechMessageEventArgs> TechMessageEventArgs;

        public event EventHandler<TacMessageEventArgs> TacMessageEventArgs;

        public event EventHandler<MatchSwapSidesEventArgs> MatchSwapSidesEventArgs;

        public event EventHandler<UnpauseMessageEventArgs> UnpauseMessageEventArgs;

        public event EventHandler<ScoreUpdateEventsArgs> ScoreUpdateEventsArgs;

        public event EventHandler<OnGenericUpdateEventArgs> OnGenericUpdateEventArgs;

        public event EventHandler<MatchEndEventArgs> MatchEndEventArgs;

        public CsMatch Match { get; set; } = new CsMatch();

        public GuildManager Manager { get; set; }

        public int ID { get; set; }

        public CsServer(string ip, ushort port, string rconPassword, Config config, GuildManager guild, int id)
        {
            try
            {
                IP = ip;
                Port = port;
                RconPassword = rconPassword;
                Config = config;
                LogPort = Convert.ToUInt16(config.LogPort);
                Manager = guild;
                ID = id;

                LogReceiver = new LogReceiver(LogPort, new IPEndPoint(IPAddress.Parse(IP), Port));

                StartListeners(LogReceiver);
            }
            catch (Exception e)
            {
                Console.WriteLine($"EXCEPTION: {e}");
            }
        }

        private void StartListeners(LogReceiver logReceiver)
        {
            logReceiver.Listen<ChatMessage>(chat =>
            {
                ProcessChatMessage(chat);
            });

            logReceiver.Listen<RoundEndScores>(roundEndScore =>
            {
                if (Match.IsLive)
                {
                    Match.CTScore = roundEndScore.CTScore + Match.CtSwapScore;
                    Match.TScore = roundEndScore.TScore + Match.TSwapScore;

                    if (Match.CTScore + Match.TScore == 15 || ((Match.CTScore + Match.TScore) > 29 && (Match.CTScore + Match.TScore) % 3 == 0))
                    {
                        Match.CtSwapScore = Match.TScore;
                        Match.TSwapScore = Match.CTScore;
                    }

                    if ((Match.CTScore + Match.TScore) == 15 || Match.ShouldSwapSides(Match.CTScore + Match.TScore))
                    {
                        Match.SwapSides();
                        Match.Paused = true;
                        ProcessSwapSides();
                    }

                    ProcessScoreUpdate();
                }
            });

            logReceiver.ListenRaw(result => { ProcessRaw(result); });

            logReceiver.Listen<KillFeed>(e =>
            {
            });

            logReceiver.Listen<MatchLive>(live =>
            {
                Match.MapName = live.MapName;

                Match.IsLive = true;

                ProcessMatchStarting(Match);
            });

            logReceiver.Listen<CTTeamName>(ctSide =>
            {
                Match.CTName = ctSide.CTName;
            });

            logReceiver.Listen<TTeamName>(tSide =>
            {
                Match.TName = tSide.TName;
            });

            logReceiver.Listen<StartFreezeTime>(startfreezeTime =>
            {
                Match.IsFreezeTime = true;
            });

            logReceiver.Listen<EndFreezeTime>(endFreezeTime =>
            {
                if (Match.Paused == true)
                {
                    ProcessUnpauseMatch();
                }
                Match.IsFreezeTime = false;
                Match.Paused = false;
            });

            logReceiver.Listen<MatchEnd>(matchEnd =>
            {
                Console.WriteLine(matchEnd.MatchId);

                ProcessMatchEnd(matchEnd.MatchId);

                Match.EndMatch();
            });
        }

        private void ProcessMatchEnd(int matchId)
        {
            MatchEndEventArgs args = new MatchEndEventArgs
            {
                Guild = Manager.Guild,
                TName = Match.TName,
                CTName = Match.CTName,
                TScore = Match.TScore + Match.TSwapScore,
                CTScore = Match.CTScore + Match.CtSwapScore,
                TimeStamp = DateTime.UtcNow.AddHours(Config.TimeZoneOffset),
                MatchId = matchId
            };

            OnMatchEnd(args);
        }

        private void ProcessUnpauseMatch()
        {
            OnGenericUpdateEventArgs args = new OnGenericUpdateEventArgs
            {
                Message = $"{Match.CTName} vs {Match.TName} is live again",
                Guild = Manager.Guild,
                TimeStamp = DateTime.UtcNow.AddHours(Config.TimeZoneOffset),
                ServerId = ID
            };

            OnGenericUpdate(args);
        }

        private void ProcessRaw(string result)
        {
            if (Match.IsLive)
            {
                if (result.Contains(".ready") || result.Contains(".READY") || result.Contains(".gaben") || result.Contains(".GABEN"))
                {
                    Match.ReadyCount++;
                }
            }

            if (result.Contains("Log file started"))
            {
                if (Match.ReadyCount > 8)
                {
                    if (Match.SwapCount > 0)
                    {
                        if (Match.SwapCount == 1)
                        {
                            OnGenericUpdateEventArgs args = new OnGenericUpdateEventArgs
                            {
                                Message = $"{Match.CTName} vs {Match.TName} is now live in the second half",
                                Guild = Manager.Guild,
                                TimeStamp = DateTime.UtcNow.AddHours(Config.TimeZoneOffset),
                                ServerId = ID
                            };

                            Match.ReadyCount = 0;
                            Match.Paused = false;

                            OnGenericUpdate(args);
                        }

                        if (Match.SwapCount > 1)
                        {
                            OnGenericUpdateEventArgs args = new OnGenericUpdateEventArgs
                            {
                                Message = $"{Match.CTName} vs {Match.TName} is now live in OT {Match.SwapCount - 1}",
                                Guild = Manager.Guild,
                                TimeStamp = DateTime.UtcNow.AddHours(Config.TimeZoneOffset),
                                ServerId = ID
                            };

                            OnGenericUpdate(args);

                            Match.ReadyCount = 0;
                            Match.Paused = false;
                        }
                    }
                }
            }
        }

        #region Log Processors

        private void ProcessScoreUpdate()
        {
            if (Match.CTScore + Match.CTScore == 30)
            {
                OnGenericUpdateEventArgs args = new OnGenericUpdateEventArgs
                {
                    Message = $"{Match.CTName} vs {Match.TName} is going OT",
                    Guild = Manager.Guild,
                    TimeStamp = DateTime.UtcNow.AddHours(Config.TimeZoneOffset),
                    ServerId = ID
                };
            }

            ScoreUpdateEventsArgs scoreUpdateArgs = new ScoreUpdateEventsArgs
            {
                Match = Match,
                GuildManager = Manager,
                TimeStamp = DateTime.UtcNow.AddHours(Config.TimeZoneOffset),
            };

            OnScoreUpdate(scoreUpdateArgs);
        }

        private void ProcessSwapSides()
        {
            if (Match.IsLive)
            {
                MatchSwapSidesEventArgs swapSidesEventArgs = new MatchSwapSidesEventArgs
                {
                    Guild = Manager.Guild,
                    TName = Match.TName,
                    CTName = Match.CTName,
                    TScore = Match.TScore,
                    CTScore = Match.CTScore,
                    TimeStamp = DateTime.UtcNow.AddHours(Config.TimeZoneOffset),
                    ServerId = ID
                };

                OnSwapSides(swapSidesEventArgs);
            }
        }

        private void ProcessMatchStarting(CsMatch match)
        {
            Match.CtSwapScore = 0;
            Match.TSwapScore = 0;
            Match.TScore = 0;
            Match.CTScore = 0;

            MatchStartEventArgs args = new MatchStartEventArgs
            {
                MapName = match.MapName,
                CTName = match.CTName,
                Guild = Manager.Guild,
                TName = match.TName,
                ServerId = ID,

                TimeStamp = DateTime.UtcNow.AddHours(Config.TimeZoneOffset),
            };

            OnMatchLive(args);
        }

        private void ProcessChatMessage(ChatMessage chat)
        {
            var timeStamp = DateTime.UtcNow.AddHours(Config.TimeZoneOffset);

            if (Match.IsFreezeTime && Match.IsLive && !Match.Paused)
            {
                if (chat.Message.ToLower() == ".tac")
                {
                    Match.Paused = true;

                    TacMessageEventArgs tacArgs = new TacMessageEventArgs
                    {
                        Guild = Manager.Guild,
                        CTScore = Match.CTScore,
                        TScore = Match.TScore,
                        TimeStamp = timeStamp,
                        CTName = Match.CTName,
                        TName = Match.TName,
                        PausingTeam = chat.Player.Team == "CT" ? Match.CTName : Match.TName,
                        ServerId = ID
                    };

                    OnTacPause(tacArgs);
                }
                else if (chat.Message.ToLower() == ".tech")
                {
                    Match.Paused = true;

                    TechMessageEventArgs techArgs = new TechMessageEventArgs
                    {
                        Guild = Manager.Guild,
                        CTScore = Match.CTScore,
                        TScore = Match.TScore,
                        TimeStamp = timeStamp,
                        CTName = Match.CTName,
                        TName = Match.TName,
                        PausingTeam = chat.Player.Team == "CT" ? Match.CTName : Match.TName,
                        ServerId = ID
                    };

                    OnTechPause(techArgs);
                }
                else if (chat.Message.ToLower() == ".unpause")
                {
                    Match.Paused = false;
                    UnpauseMessageEventArgs unpauseMessageArgs = new UnpauseMessageEventArgs
                    {
                        Guild = Manager.Guild,

                        TimeStamp = timeStamp,

                        ServerId = ID
                    };

                    OnUnpause(unpauseMessageArgs);
                }
            }

            ChatMessageEventArgs chatArgs = new ChatMessageEventArgs
            {
                ChatMessage = chat,
                TimeStamp = timeStamp,
                Guild = Manager.Guild,
                ServerId = ID
            };

            OnChatMessage(chatArgs);
        }

        #endregion Log Processors

        #region Event Emitters

        private void OnMatchEnd(MatchEndEventArgs args)
        {
            MatchEndEventArgs?.Invoke(this, args);
        }

        private void OnGenericUpdate(OnGenericUpdateEventArgs args)
        {
            OnGenericUpdateEventArgs?.Invoke(this, args);
        }

        private void OnScoreUpdate(ScoreUpdateEventsArgs scoreUpdateArgs)
        {
            ScoreUpdateEventsArgs?.Invoke(this, scoreUpdateArgs);
        }

        private void OnUnpause(UnpauseMessageEventArgs unpauseMessageArgs)
        {
            UnpauseMessageEventArgs?.Invoke(this, unpauseMessageArgs);
        }

        private void OnSwapSides(MatchSwapSidesEventArgs swapSidesEventArgs)
        {
            MatchSwapSidesEventArgs?.Invoke(this, swapSidesEventArgs);
        }

        private void OnMatchLive(MatchStartEventArgs e)
        {
            MatchStartEventArgs?.Invoke(this, e);
        }

        private void OnTechPause(TechMessageEventArgs techArgs)
        {
            TechMessageEventArgs?.Invoke(this, techArgs);
        }

        private void OnTacPause(TacMessageEventArgs tacArgs)
        {
            TacMessageEventArgs?.Invoke(this, tacArgs);
        }

        private void OnChatMessage(ChatMessageEventArgs chatArgs)
        {
            ChatMessageEventArgs?.Invoke(this, chatArgs);
        }

        #endregion Event Emitters

        public async Task InitializeRconAsync()
        {
            try
            {
                Rcon = new RCON(IPAddress.Parse(IP), Port, RconPassword);

                await Rcon.ConnectAsync();

                //await Rcon.SendCommandAsync("say DiscordGo connected.");
                await Rcon.SendCommandAsync($"logaddress_add {Helpers.GetPublicIPAddress()}:{Config.LogPort}");

                //Match.IsLive = true;

                Authed = true;
                return;
            }
            catch (Exception e)
            {
                Authed = false;
                Console.WriteLine($"EXCEPTION: {e}");
                return;
            }
        }

        internal async Task SendCommandAsync(string command)
        {
            await Rcon.SendCommandAsync(command);

            return;
        }
    }
}