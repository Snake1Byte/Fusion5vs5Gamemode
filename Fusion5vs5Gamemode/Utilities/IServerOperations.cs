using System.Collections.Generic;

namespace Fusion5vs5Gamemode.Utilities;

public interface IServerOperations
{
    Dictionary<string, string> Metadata { get; }
    bool SetMetadata(string key, string value);
    string GetMetadata(string key);
    bool InvokeTrigger(string value);
}