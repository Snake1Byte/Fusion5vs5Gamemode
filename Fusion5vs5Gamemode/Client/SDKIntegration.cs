using Fusion5vs5Gamemode.SDK;

namespace Fusion5vs5Gamemode.Client
{
    public class SDKIntegration
    {
        public static void InvokeCounterTerroristTeamJoined(string playerName, bool wasLocalTeam)
        {
            foreach (var ultEvent in Invoke5vs5UltEvent.Cache.Components)
            {
                if (ultEvent.Event == Invoke5vs5UltEvent.Fusion5vs5GamemodeUltEvents.CounterTerroristTeamJoined)
                {
                    ultEvent.WasLocalTeam = wasLocalTeam;
                    ultEvent.CounterTerroristTeamJoinedValue = playerName;
                    ultEvent.Invoke();
                }
            }
        }

        public static void InvokeTerroristTeamJoined(string playerName, bool wasLocalTeam)
        {
            foreach (var ultEvent in Invoke5vs5UltEvent.Cache.Components)
            {
                if (ultEvent.Event == Invoke5vs5UltEvent.Fusion5vs5GamemodeUltEvents.TerroristTeamJoined)
                {
                    ultEvent.WasLocalTeam = wasLocalTeam;
                    ultEvent.TerroristTeamJoinedValue = playerName;
                    ultEvent.Invoke();
                }
            }
        }

        public static void InvokeCounterTerroristTeamScored(int totalScore, bool wasLocalTeam)
        {
            foreach (var ultEvent in Invoke5vs5UltEvent.Cache.Components)
            {
                if (ultEvent.Event == Invoke5vs5UltEvent.Fusion5vs5GamemodeUltEvents.CounterTerroristTeamScored)
                {
                    ultEvent.WasLocalTeam = wasLocalTeam;
                    ultEvent.CounterTerroristTeamScoredValue = totalScore;
                    ultEvent.Invoke();
                }
            }
        }

        public static void InvokeTerroristTeamScored(int totalScore, bool wasLocalTeam)
        {
            foreach (var ultEvent in Invoke5vs5UltEvent.Cache.Components)
            {
                if (ultEvent.Event == Invoke5vs5UltEvent.Fusion5vs5GamemodeUltEvents.TerroristTeamScored)
                {
                    ultEvent.WasLocalTeam = wasLocalTeam;
                    ultEvent.TerroristTeamScoredValue = totalScore;
                    ultEvent.Invoke();
                }
            }
        }

        public static void InvokeNewRoundStarted(int roundNumber)
        {
            foreach (var ultEvent in Invoke5vs5UltEvent.Cache.Components)
            {
                if (ultEvent.Event == Invoke5vs5UltEvent.Fusion5vs5GamemodeUltEvents.NewRoundStarted)
                {
                    ultEvent.NewRoundStartedValue = roundNumber;
                    ultEvent.Invoke();
                }
            }
        }

        public static void InvokePlayerKilledAnotherPlayer(string killerName, string killedName, bool wasLocalPlayer)
        {
            foreach (var ultEvent in Invoke5vs5UltEvent.Cache.Components)
            {
                if (ultEvent.Event == Invoke5vs5UltEvent.Fusion5vs5GamemodeUltEvents.PlayerKilledAnotherPlayer)
                {
                    ultEvent.WasLocalPlayer = wasLocalPlayer;
                    ultEvent.PlayerKilledAnotherPlayerValueKiller = killerName;
                    ultEvent.PlayerKilledAnotherPlayerValueKilled = killedName;
                    ultEvent.Invoke();
                }
            }
        }

        public static void InvokePlayerSuicide(string playerName, bool wasLocalPlayer)
        {
            foreach (var ultEvent in Invoke5vs5UltEvent.Cache.Components)
            {
                if (ultEvent.Event == Invoke5vs5UltEvent.Fusion5vs5GamemodeUltEvents.PlayerSuicide)
                {
                    ultEvent.WasLocalPlayer = wasLocalPlayer;
                    ultEvent.PlayerSuicideValue = playerName;
                    ultEvent.Invoke();
                }
            }
        }

        public static void InvokeWarmupPhaseStarted()
        {
            foreach (var ultEvent in Invoke5vs5UltEvent.Cache.Components)
            {
                if (ultEvent.Event == Invoke5vs5UltEvent.Fusion5vs5GamemodeUltEvents.WarmupPhaseStarted)
                {
                    ultEvent.Invoke();
                }
            }
        }

        public static void InvokeBuyPhaseStarted()
        {
            foreach (var ultEvent in Invoke5vs5UltEvent.Cache.Components)
            {
                if (ultEvent.Event == Invoke5vs5UltEvent.Fusion5vs5GamemodeUltEvents.BuyPhaseStarted)
                {
                    ultEvent.Invoke();
                }
            }
        }

        public static void InvokePlayPhaseStarted()
        {
            foreach (var ultEvent in Invoke5vs5UltEvent.Cache.Components)
            {
                if (ultEvent.Event == Invoke5vs5UltEvent.Fusion5vs5GamemodeUltEvents.PlayPhaseStarted)
                {
                    ultEvent.Invoke();
                }
            }
        }

        public static void InvokeRoundEndPhaseStarted()
        {
            foreach (var ultEvent in Invoke5vs5UltEvent.Cache.Components)
            {
                if (ultEvent.Event == Invoke5vs5UltEvent.Fusion5vs5GamemodeUltEvents.RoundEndPhaseStarted)
                {
                    ultEvent.Invoke();
                }
            }
        }

        public static void InvokeMatchHalfPhaseStarted()
        {
            foreach (var ultEvent in Invoke5vs5UltEvent.Cache.Components)
            {
                if (ultEvent.Event == Invoke5vs5UltEvent.Fusion5vs5GamemodeUltEvents.MatchHalfPhaseStarted)
                {
                    ultEvent.Invoke();
                }
            }
        }

        public static void InvokeMatchEndPhaseStarted()
        {
            foreach (var ultEvent in Invoke5vs5UltEvent.Cache.Components)
            {
                if (ultEvent.Event == Invoke5vs5UltEvent.Fusion5vs5GamemodeUltEvents.MatchEndPhaseStarted)
                {
                    ultEvent.Invoke();
                }
            }
        }
    }
}