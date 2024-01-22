using Fusion5vs5Gamemode.SDK;

namespace Fusion5vs5Gamemode
{
    public class SDKIntegration
    {
        public static void InvokeCounterTerroristTeamJoined(string playerName)
        {
            foreach (var ultEvent in Invoke5vs5UltEvent.Cache.Components)
            {
                if (ultEvent.Event == Invoke5vs5UltEvent.Fusion5vs5GamemodeUltEvents.CounterTerroristTeamJoined)
                {
                    ultEvent.CounterTerroristTeamJoinedValue = playerName;
                    ultEvent.Invoke();
                }
            }
        }
        
        public static void InvokeTerroristTeamJoined(string playerName)
        {
            foreach (var ultEvent in Invoke5vs5UltEvent.Cache.Components)
            {
                if (ultEvent.Event == Invoke5vs5UltEvent.Fusion5vs5GamemodeUltEvents.TerroristTeamJoined)
                {
                    ultEvent.TerroristTeamJoinedValue = playerName;
                    ultEvent.Invoke();
                }
            }
        }
        
        public static void InvokeCounterTerroristTeamScored(int totalScore)
        {
            foreach (var ultEvent in Invoke5vs5UltEvent.Cache.Components)
            {
                if (ultEvent.Event == Invoke5vs5UltEvent.Fusion5vs5GamemodeUltEvents.CounterTerroristTeamScored)
                {
                    ultEvent.CounterTerroristTeamScoredValue = totalScore;
                    ultEvent.Invoke();
                }
            }
        }
        
        public static void InvokeTerroristTeamJoinedScored(int totalScore)
        {
            foreach (var ultEvent in Invoke5vs5UltEvent.Cache.Components)
            {
                if (ultEvent.Event == Invoke5vs5UltEvent.Fusion5vs5GamemodeUltEvents.TerroristTeamScored)
                {
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
        
        public static void InvokePlayerKilledAnotherPlayer(string killerName, string killedName)
        {
            foreach (var ultEvent in Invoke5vs5UltEvent.Cache.Components)
            {
                if (ultEvent.Event == Invoke5vs5UltEvent.Fusion5vs5GamemodeUltEvents.PlayerKilledAnotherPlayer)
                {
                    ultEvent.PlayerKilledAnotherPlayerValueKiller = killerName;
                    ultEvent.PlayerKilledAnotherPlayerValueKilled = killedName;
                    ultEvent.Invoke();
                }
            }
        }
        
        public static void InvokePlayerSuicide(string playerName)
        {
            foreach (var ultEvent in Invoke5vs5UltEvent.Cache.Components)
            {
                if (ultEvent.Event == Invoke5vs5UltEvent.Fusion5vs5GamemodeUltEvents.PlayerSuicide)
                {
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