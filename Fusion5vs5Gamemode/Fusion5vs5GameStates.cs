namespace Fusion5vs5Gamemode
{
    
    // TODO decide later whether we want to expose this to the client or whether we just wanna use event triggers
    public enum Fusion5vs5GameStates
    {
        Unknown = 0,
        Warmup = 1,
        BuyPhase = 2,
        PlayPhase = 3,
        RoundEndPhase = 4,
        MatchHalfPhase = 5,
        MatchEndPhase = 6
    }
}