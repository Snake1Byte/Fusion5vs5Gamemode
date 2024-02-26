using System;
using HarmonyLib;
using LabFusion.Network;
using MelonLoader;
using SLZ.Marrow.Pool;
using UnityEngine;

namespace  Fusion5vs5Gamemode.Utilities.HarmonyPatches;

[HarmonyPatch(typeof(AssetPoolee))]
public static class AssetPooleePatches
{
    public static Action<AssetPoolee>? OnAssetDespawn;
        
    [HarmonyPrefix]
    [HarmonyPatch(nameof(AssetPoolee.Despawn))]
    public static void Despawn(AssetPoolee __instance)
    {
        BoneLib.SafeActions.InvokeActionSafe(OnAssetDespawn, __instance);
    }
}