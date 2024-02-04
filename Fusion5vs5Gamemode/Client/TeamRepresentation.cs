using Fusion5vs5Gamemode.SDK;

namespace Fusion5vs5Gamemode.Client
{
    // The "interface" between the server and client when it comes to representing a Team
    public struct TeamRepresentation
    {
        public Fusion5vs5GamemodeTeams Team;
        public string DisplayName; // Name for the UI
    }
}