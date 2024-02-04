using LabFusion.Network;
using LabFusion.SDK.Modules;
using static Fusion5vs5Gamemode.Shared.Commons;

namespace Fusion5vs5Gamemode.Shared
{
    public class Fusion5vs5CustomModule : Module
    {
        public static Fusion5vs5CustomModule Instance { get; private set; }

        public override void OnModuleLoaded()
        {
            Instance = this;
        }

        public static void RequestToServer(string request)
        {
            Log(request);
            if (NetworkInfo.HasServer)
            {
                using (var writer = FusionWriter.Create())
                {
                    using (var data = Fusion5vs5ClientRequest.Create(request))
                    {
                        writer.Write(data);
                        using (var message = FusionMessage.ModuleCreate<Fusion5vs5ClientRequestHandler>(writer))
                        {
                            MessageSender.SendToServer(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }
    }
}