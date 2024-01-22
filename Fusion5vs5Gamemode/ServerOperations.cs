using LabFusion.Data;

namespace Fusion5vs5Gamemode
{
    public interface IServerOperations

    {
    bool SetMetadata(string key, string value);
    string GetMetadata(string key);
    FusionDictionary<string, string> GetMetadata();
    bool InvokeTrigger(string value);
    }
}