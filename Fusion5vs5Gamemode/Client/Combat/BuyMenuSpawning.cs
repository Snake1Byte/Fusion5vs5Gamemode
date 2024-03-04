using Fusion5vs5Gamemode.Utilities;
using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Syncables;
using MelonLoader;
using SLZ.Interaction;
using SLZ.Marrow.Pool;
using SLZ.Props.Weapons;
using SLZ.Rig;
using UnityEngine;
using static Fusion5vs5Gamemode.Shared.Commons;

namespace Fusion5vs5Gamemode.Client.Combat;

public class BuyMenuSpawning
{
    public static void LocalPlayerBoughtItem(string barcode)
    {
        Log(barcode);

        PlayerId player = PlayerIdManager.LocalId;
        var rigReferences = RigData.RigReferences;

        if (rigReferences == null)
        {
            player.TryGetDisplayName(out string name);
            MelonLogger.Warning($"Could not buy item with barcode {barcode} since RigReferenceCollection for player {name ?? $"with ID {player.LongId.ToString()}"} could not be found!");
            return;
        }

        RigManager rm = rigReferences.RigManager;
        Transform headTransform = rm.physicsRig.m_pelvis;
        SerializedTransform finalTransform = new SerializedTransform(headTransform.position + headTransform.forward, headTransform.rotation);

        FusionSpawning.RequestSpawn(barcode, finalTransform, player.SmallId, (owner, _, syncId, _) => ServerRequests.BroadcastItemBought(syncId, owner));
    }

    // This runs on every client
    internal static void ModifyAndPlaceItemInInventory(ushort syncId, byte owner)
    {
        Log(syncId, owner);

        SyncManager.TryGetSyncable(syncId, out PropSyncable syncable);
        GameObject spawnedGo = syncable.GameObject;
        WeaponModification.ModifyWeapon(spawnedGo, owner);
        
        WeaponSlot weaponSlot = spawnedGo.GetComponentInChildren<WeaponSlot>();
        if (weaponSlot == null) return;

        InteractableHost host = spawnedGo.GetComponentInChildren<InteractableHost>();
        if (host == null) return;

        RigReferenceCollection rigReferences;
        if (owner == PlayerIdManager.LocalSmallId)
        {
            rigReferences = RigData.RigReferences;
        }
        else
        {
            PlayerRepManager.TryGetPlayerRep(owner, out PlayerRep playerRep);
            if (playerRep == null) return;
            rigReferences = playerRep.RigReferences;
        }

        foreach (var slot in rigReferences.RigSlots)
        {
            if (slot._slottedWeapon == null && (slot.slotType & weaponSlot.slotType) != 0)
            {
                slot.InsertInSlot(host);
                return;
            }
        }
    }
}
