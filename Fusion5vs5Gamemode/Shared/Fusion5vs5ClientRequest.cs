using System;
using LabFusion.Data;
using LabFusion.Network;
using LabFusion.SDK.Gamemodes;

namespace Fusion5vs5Gamemode.Shared;

public class Fusion5vs5ClientRequest : IFusionSerializable, IDisposable
{
    public string Value = null!;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(Value);
    }

    public void Deserialize(FusionReader reader)
    {
        Value = reader.ReadString();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public static Fusion5vs5ClientRequest Create(string value)
    {
        return new Fusion5vs5ClientRequest { Value = value };
    }
}

public class Fusion5vs5ClientRequestHandler : ModuleMessageHandler
{
    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using (var reader = FusionReader.Create(bytes))
        {
            using (var data = reader.ReadFusionSerializable<Fusion5vs5ClientRequest>())
            {
                if (NetworkInfo.IsServer && Client.Fusion5vs5Gamemode.Instance != null)
                {
                    var info = data.Value;
                    if (Client.Fusion5vs5Gamemode.Instance.Tag.HasValue &&
                        GamemodeManager.TryGetGamemode(Client.Fusion5vs5Gamemode.Instance.Tag.Value, out _))
                    {
                        Client.Fusion5vs5Gamemode.Instance.Server.OnClientRequested(info);
                    }
                }
            }
        }
    }
}