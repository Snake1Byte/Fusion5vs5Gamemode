using System;
using System.Collections.Generic;
using BoneLib;
using Fusion5vs5Gamemode.Utilities.HarmonyPatches;
using LabFusion.Data;
using LabFusion.Utilities;
using TriangleNet;
using UnityEngine;
using static Fusion5vs5Gamemode.Shared.Commons;

namespace Fusion5vs5Gamemode.Utilities;

public static class FusionSpawning
{
    private struct SpawnedObject
    {
        public string Barcode;
        public byte Owner;
    }

    private static readonly Dictionary<SpawnedObject, Action<byte, string, GameObject>> SpawnQueue = new();

    private static void EmptyDictionaries(LevelInfo obj)
    {
        Log(obj);
        lock (SpawnQueue)
        {
            SpawnQueue.Clear();
        }
    }

    public static void RequestSpawn(string barcode, SerializedTransform transform, byte owner,
        Action<byte, string, GameObject> onSpawn)
    {
        Log(barcode, transform, owner, onSpawn);
        lock (SpawnQueue)
        {
            if (SpawnQueue.Count == 0)
            {
                SpawnResponseMessagePatches.OnSpawnFinished += OnSpawn;
                Hooking.OnLevelInitialized += EmptyDictionaries;
            }

            SpawnQueue.Add(new SpawnedObject { Barcode = barcode, Owner = owner }, onSpawn);
        }

        PooleeUtilities.RequestSpawn(barcode, transform, owner);
    }

    private static void OnSpawn(byte owner, string barcode, GameObject go)
    {
        Log(owner, barcode, go);
        Action<byte, string, GameObject> onSpawn;
        lock (SpawnQueue)
        {
            SpawnedObject spawnedObject = new SpawnedObject { Barcode = barcode, Owner = owner };
            if (!SpawnQueue.TryGetValue(spawnedObject, out onSpawn)) return;
            SpawnQueue.Remove(spawnedObject);
            if (SpawnQueue.Count == 0)
            {
                SpawnResponseMessagePatches.OnSpawnFinished -= OnSpawn;
                Hooking.OnLevelInitialized -= EmptyDictionaries;
            }
        }
        onSpawn?.Invoke(owner, barcode, go);
    }
}