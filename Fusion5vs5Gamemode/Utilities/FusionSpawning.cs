using System;
using System.Collections.Generic;
using BoneLib;
using Fusion5vs5Gamemode.Utilities.HarmonyPatches;
using LabFusion.Data;
using LabFusion.Utilities;
using UnityEngine;
using static Fusion5vs5Gamemode.Shared.Commons;

namespace Fusion5vs5Gamemode.Utilities;

public static class FusionSpawning
{
    private struct SpawnedObject
    {
        public string Barcode;
        public byte Owner;
        public Action<byte, string, ushort, GameObject> SpawnAction;
    }

    private static readonly List<SpawnedObject> SpawnQueue = new();

    private static void EmptyLists(LevelInfo obj)
    {
        Log(obj);
        lock (SpawnQueue)
        {
            SpawnQueue.Clear();
        }
    }

    public static void RequestSpawn(string barcode, SerializedTransform transform, byte owner,
        Action<byte, string, ushort, GameObject> onSpawn)
    {
        Log(barcode, transform, owner, onSpawn);
        lock (SpawnQueue)
        {
            if (SpawnQueue.Count == 0)
            {
                SpawnResponseMessagePatches.OnSpawnFinished += OnSpawn;
                Hooking.OnLevelInitialized += EmptyLists;
            }

            SpawnQueue.Add(new SpawnedObject { Barcode = barcode, Owner = owner, SpawnAction = onSpawn });
        }

        PooleeUtilities.RequestSpawn(barcode, transform, owner);
    }

    private static void OnSpawn(byte owner, string barcode, ushort syncId, GameObject go)
    {
        Log(owner, barcode, syncId, go);
        Action<byte, string, ushort, GameObject>? onSpawn = null;
        lock (SpawnQueue)
        {
            SpawnedObject? toRemove = null;
            foreach (SpawnedObject obj in SpawnQueue)
            {
                if (obj.Barcode.Equals(barcode) && obj.Owner.Equals(owner))
                {
                    toRemove = obj;
                    onSpawn = obj.SpawnAction;
                    break;
                }
            }

            if (toRemove == null) return;
            SpawnQueue.Remove(toRemove.Value);
            if (SpawnQueue.Count == 0)
            {
                SpawnResponseMessagePatches.OnSpawnFinished -= OnSpawn;
                Hooking.OnLevelInitialized -= EmptyLists;
            }
        }

        SafeActions.InvokeActionSafe(onSpawn, owner, barcode, syncId, go);
    }
}