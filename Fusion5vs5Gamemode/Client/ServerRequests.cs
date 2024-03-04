using Fusion5vs5Gamemode.Shared;
using Fusion5vs5Gamemode.Shared.Modules;
using LabFusion.Network;
using static Fusion5vs5Gamemode.Shared.Commons;

namespace Fusion5vs5Gamemode.Client;

public static class ServerRequests
{
    public static void GenericRequestToServer(string genericRequest)
    {
        Log(genericRequest);
        
        if (NetworkInfo.HasServer)
        {
            using (var writer = FusionWriter.Create())
            {
                using (var data = GenericClientRequestData.Create(genericRequest))
                {
                    writer.Write(data);
                    using (var message = FusionMessage.ModuleCreate<GenericClientRequestHandler>(writer))
                    {
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }
        }
    }

    public static void BroadcastItemBought(ushort syncId, byte owner)
    {
        Log(syncId, owner);
        
        using (var writer = FusionWriter.Create(ItemBoughtData.Size))
        {
            using (var data = ItemBoughtData.Create(syncId, owner))
            {
                writer.Write(data);
                using (var message = FusionMessage.ModuleCreate<ItemBoughtHandler>(writer))
                {
                    MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
                }
            }
        }
    }
}
