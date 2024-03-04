using System;
using LabFusion.Data;
using LabFusion.Network;

namespace Fusion5vs5Gamemode.Shared.Modules;

internal class ItemBoughtData : IFusionSerializable, IDisposable
{
    public const int Size = sizeof(byte) + sizeof(ushort);
    public ushort SyncId;
    public byte Owner;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(SyncId);
        writer.Write(Owner);
    }

    public void Deserialize(FusionReader reader)
    {
        SyncId = reader.ReadUInt16();
        Owner = reader.ReadByte();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public static ItemBoughtData Create(ushort syncId, byte owner)
    {
        return new ItemBoughtData { SyncId = syncId, Owner = owner };
    }
}
