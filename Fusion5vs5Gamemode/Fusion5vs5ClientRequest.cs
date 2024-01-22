using System;
using LabFusion.Data;
using LabFusion.Network;
using LabFusion.SDK.Gamemodes;

namespace Fusion5vs5Gamemode
{
    public class Fusion5vs5ClientRequest : IFusionSerializable, IDisposable
    {
        public string value;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(value);
        }

        public void Deserialize(FusionReader reader)
        {
            value = reader.ReadString();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static Fusion5vs5ClientRequest Create(string value)
        {
            return new Fusion5vs5ClientRequest() { value = value };
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
                    if (NetworkInfo.IsServer && Fusion5vs5Gamemode.Instance != null)
                    {
                        var info = data.value;
                        if (Fusion5vs5Gamemode.Instance.Tag.HasValue && GamemodeManager.TryGetGamemode(Fusion5vs5Gamemode.Instance.Tag.Value, out var gamemode))
                        {
                            Fusion5vs5Gamemode.Instance._Server.ClientRequested(info);
                        }
                    }
                }
            }
        }
    }
}