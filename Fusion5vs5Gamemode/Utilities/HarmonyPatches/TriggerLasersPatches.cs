using System;
using HarmonyLib;
using LabFusion.Data;
using MelonLoader;
using SLZ.Bonelab;
using UnityEngine;

namespace Fusion5vs5Gamemode.Utilities.HarmonyPatches
{
    public class TriggerLasersEvents
    {
        public static Action<TriggerLasers> OnTriggerEntered;
        public static Action<TriggerLasers> OnTriggerExited;
    }

    [HarmonyPatch(typeof(TriggerLasers))]
    public static class TriggerLasersPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(TriggerLasers.OnTriggerEnter))]
        public static void OnTriggerEnter(TriggerLasers __instance, Collider other)
        {
            if (other != null && other.CompareTag("Player"))
            {
#if DEBUG
                MelonLogger.Msg("OnTriggerEnterEvent()");
#endif
                if (__instance.rigManager != null)
                {
#if DEBUG
                    MelonLogger.Msg("Did local player enter trigger: " +
                                    (__instance.rigManager == RigData.RigReferences.RigManager));
#endif
                    if (TriggerLasersEvents.OnTriggerEntered != null &&
                        __instance.rigManager == RigData.RigReferences.RigManager)
                    {
                        TriggerLasersEvents.OnTriggerEntered.Invoke(__instance);
                    }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(TriggerLasers.OnTriggerExit))]
        public static void OnTriggerExit(TriggerLasers __instance, Collider other)
        {
            if (other != null && other.CompareTag("Player"))
            {
#if DEBUG
                MelonLogger.Msg("OnTriggerExitEvent()");
#endif
                if (__instance.rigManager != null)
                {
#if DEBUG
                    MelonLogger.Msg("Did local player enter trigger: " +
                                    (__instance.rigManager == RigData.RigReferences.RigManager));
#endif
                    if (TriggerLasersEvents.OnTriggerExited != null &&
                        __instance.rigManager == RigData.RigReferences.RigManager)
                    {
                        TriggerLasersEvents.OnTriggerExited.Invoke(__instance);
                    }
                }
            }
        }
    }
}