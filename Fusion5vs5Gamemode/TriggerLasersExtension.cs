using System;
using HarmonyLib;
using LabFusion.Data;
using MelonLoader;
using SLZ.Bonelab;
using UnityEngine;

namespace Fusion5vs5Gamemode
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
                MelonLogger.Msg("TriggerLasers Harmony Patch: OnTriggerEnterEvent()");
                if (__instance.rigManager != null)
                {
                    MelonLogger.Msg("sos " + (__instance.rigManager == RigData.RigReferences.RigManager));
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
                MelonLogger.Msg("TriggerLasers Harmony Patch: OnTriggerExitEvent()");
                if (__instance.rigManager != null)
                {
                    MelonLogger.Msg("sos " + (__instance.rigManager == RigData.RigReferences.RigManager));
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