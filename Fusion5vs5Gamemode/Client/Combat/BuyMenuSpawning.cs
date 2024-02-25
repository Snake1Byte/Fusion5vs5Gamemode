using System.Collections.Concurrent;
using System.Collections.Generic;
using Fusion5vs5Gamemode.Utilities.HarmonyPatches;
using Il2CppSystem;
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

    private static readonly List<SpawnedObject> SpawnQueue = new();
    private static bool _Enabled = false;
    private static readonly object EnabledLock = new();

    public static void Enable()
    {
        lock (EnabledLock)
        {
            if (!_Enabled)
            {
                _Enabled = true;
                SpawnResponseMessagePatches.OnSpawnFinished += PlaceItemInInventory;
            }
        }
    }

    public static void Disable()
    {
        lock (EnabledLock)
        {
            if (_Enabled)
            {
                _Enabled = false;
                SpawnResponseMessagePatches.OnSpawnFinished -= PlaceItemInInventory;
                lock (SpawnQueue)
                {
                    SpawnQueue.Clear();
                }
            }
        }
    }

    public static void OnPlayerBoughtItem(PlayerId player, string barcode)
    {
        lock (SpawnQueue)
        {
            SpawnQueue.Add(new SpawnedObject { Barcode = barcode, Owner = player.SmallId });
        }

        if (player.IsSelf)
        {
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
            PooleeUtilities.RequestSpawn(barcode, finalTransform, player.SmallId);
        }
    }

    // Runs on every client
    private static void PlaceItemInInventory(byte owner, string spawnedBarcode, GameObject spawnedGo)
    {
        Log(owner, spawnedBarcode, spawnedGo);

        lock (SpawnQueue)
        {
            SpawnedObject spawnedObject = new SpawnedObject { Barcode = spawnedBarcode, Owner = owner };
            if (!SpawnQueue.Contains(spawnedObject)) return;
            SpawnQueue.Remove(spawnedObject);
        }

        foreach (Pallet pallet in AssetWarehouse.Instance.GetPallets())
        {
            if (pallet.Internal)
            {
                foreach (var crate in pallet.Crates)
                {
                    if (crate.Barcode.Equals(spawnedBarcode))
                    {
                        AttachmentDatabase.AddAttachmentSlots(spawnedBarcode, spawnedGo);
                    }
                }
            }
        }

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