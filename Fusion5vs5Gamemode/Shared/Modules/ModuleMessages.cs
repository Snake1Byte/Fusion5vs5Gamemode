using System;

namespace Fusion5vs5Gamemode.Shared.Modules;

public static class ModuleMessages
{
    public static Action<string>? GenericClientRequest;
    public static Action<ushort, byte>? ItemBought;

    internal static void InvokeGenericClientRequest(string eventTrigger)
    {
        BoneLib.SafeActions.InvokeActionSafe(GenericClientRequest, eventTrigger);
    }

    internal static void InvokeItemBought(ushort syncId, byte owner)
    {
        BoneLib.SafeActions.InvokeActionSafe(ItemBought, syncId, owner);
    }
}
