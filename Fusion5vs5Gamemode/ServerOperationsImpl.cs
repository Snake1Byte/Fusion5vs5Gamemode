using System.Collections.Generic;
using LabFusion.Data;
using LabFusion.SDK.Gamemodes;
using TriangleNet;
using static Fusion5vs5Gamemode.Commons;

namespace Fusion5vs5Gamemode.Server
{
    public class ServerOperationsImpl : IServerOperations
    {
        private Gamemode _FusionGamemode { get; }
        public Dictionary<string, string> Metadata { get; }
        public ServerOperationsImpl(Gamemode fusionGamemode)
        {
            Log(fusionGamemode);
            _FusionGamemode = fusionGamemode;
            // We need a local copy of the Metadata dictionary since the sync'd one suffers from race conditions
            Metadata = new Dictionary<string, string>();
        }

        public bool SetMetadata(string key, string value)
        {
            Log(key, value);
            Metadata.Remove(key);
            Metadata.Add(key, value);
            return _FusionGamemode.TrySetMetadata(key, value);
        }

        public string GetMetadata(string key)
        {
            Log(key);
            string value = Metadata[key];
            return value;
        }

        public bool InvokeTrigger(string value)
        {
            Log(value);
            return _FusionGamemode.TryInvokeTrigger(value);
        }
    }
}