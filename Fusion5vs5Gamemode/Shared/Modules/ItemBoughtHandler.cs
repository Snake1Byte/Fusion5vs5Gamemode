using LabFusion.Network;

namespace Fusion5vs5Gamemode.Shared.Modules;

internal class ItemBoughtHandler : ModuleMessageHandler
{
    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        if (NetworkInfo.IsServer && Client.Client.Instance != null)
        {
            using (var reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<ItemBoughtData>())
                {
                    ModuleMessages.InvokeItemBought(data.SyncId, data.Owner);
                }
            }
        }
    }
}
