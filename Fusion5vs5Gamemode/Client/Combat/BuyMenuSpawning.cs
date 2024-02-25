using System.Collections.Generic;
using BoneLib;
using Fusion5vs5Gamemode.Utilities;
using Fusion5vs5Gamemode.Utilities.HarmonyPatches;
using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;
using MelonLoader;
using SLZ.Interaction;
using SLZ.Marrow.Warehouse;
using SLZ.Props.Weapons;
using SLZ.Rig;
using UnityEngine;
using static Fusion5vs5Gamemode.Shared.Commons;

namespace Fusion5vs5Gamemode.Client.Combat;

public class BuyMenuSpawning
{
    private struct SpawnedObject
    {
        public string Barcode;
        public byte Owner;
    }

    public static void OnPlayerBoughtItem(PlayerId player, string barcode)
    {
        Log(player, barcode);
        RigReferenceCollection rigReferences = RigData.RigReferences;
        if (rigReferences == null)
        {
            player.TryGetDisplayName(out string name);
            MelonLogger.Warning(
                $"Could not buy item since RigReferenceCollection for player {name ?? $"with ID {player.LongId.ToString()}"} could not be found in OnItemBought()!");
            return;
        }

        RigManager rm = rigReferences.RigManager;
        Transform headTransform = rm.physicsRig.m_pelvis;
        SerializedTransform finalTransform = new SerializedTransform(headTransform.position + headTransform.forward,
            headTransform.rotation);
        FusionSpawning.RequestSpawn(barcode, finalTransform, player.SmallId, PlaceItemInInventory);
    }

    // Runs on every client
    private static void PlaceItemInInventory(byte owner, string spawnedBarcode, GameObject spawnedGo)
    {
        Log(owner, spawnedBarcode, spawnedGo);

        AttachmentDatabase.AddAttachmentSlots(spawnedBarcode, spawnedGo, owner);

        if (owner != PlayerIdManager.LocalId.SmallId) return;
        WeaponSlot weaponSlot = spawnedGo.GetComponentInChildren<WeaponSlot>();
        if (weaponSlot == null) return;

        InteractableHost host = spawnedGo.GetComponentInChildren<InteractableHost>();
        if (host == null) return;

        foreach (var slot in RigData.RigReferences.RigSlots)
        {
            if (slot._slottedWeapon == null && (slot.slotType & weaponSlot.slotType) != 0)
            {
                slot.InsertInSlot(host);
                return;
            }
        }
    }
}