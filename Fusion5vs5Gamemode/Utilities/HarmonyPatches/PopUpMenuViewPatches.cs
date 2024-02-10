using System;
using HarmonyLib;
using LabFusion.Data;
using SLZ.Rig;
using SLZ.UI;
using UnityEngine;

namespace  Fusion5vs5Gamemode.Utilities.HarmonyPatches
{
    [HarmonyPatch(typeof(PopUpMenuView))]
    public static class PopUpMenuViewPatches
    {
        public static Action<Transform, Transform, UIControllerInput, BaseController> OnPopUpMenuActivate;
        public static Action OnPopUpMenuDeactivate;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PopUpMenuView.Activate))]
        public static void Activate(PopUpMenuView __instance, Transform headTransform, Transform rootTransform, UIControllerInput controllerInput,
            BaseController controller)
        {
            if (OnPopUpMenuActivate != null && __instance.GetComponentInParent<RigManager>() == RigData.RigReferences.RigManager)
            {
                OnPopUpMenuActivate.Invoke(headTransform, rootTransform, controllerInput, controller);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PopUpMenuView.Deactivate))]
        public static void Deactivate()
        {
            if (OnPopUpMenuDeactivate != null)
            {
                OnPopUpMenuDeactivate.Invoke();
            }
        }

        /*
        private static int CallCounter = 1;
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PopUpMenuView.AddAvatarsMenu))]
        public static void AddAvatarsMenu()
        {
            MelonLogger.Msg($"{CallCounter}: PageView.AddAvatarsMenu()");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PopUpMenuView.AddDevMenu))]
        public static void AddDevMenu()
        {
            MelonLogger.Msg($"{CallCounter}: PageView.AddDevMenu()");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PopUpMenuView.AddInventoryMenu))]
        public static void AddInventoryMenu()
        {
            MelonLogger.Msg($"{CallCounter}: PageView.AddInventoryMenu()");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PopUpMenuView.AddMagEjectMenu))]
        public static void AddMagEjectMenu(UIControllerInput uiInput, Action action)
        {
            MelonLogger.Msg($"{CallCounter}: PageView.AddMagEjectMenu()");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PopUpMenuView.AddPreferencesMenu))]
        public static void AddPreferencesMenu()
        {
            MelonLogger.Msg($"{CallCounter}: PageView.AddPreferencesMenu()");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PopUpMenuView.AddScenesMenu))]
        public static void AddScenesMenu()
        {
            MelonLogger.Msg($"{CallCounter}: PopUpMenuView.AddScenesMenu()");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PopUpMenuView.AddSpawnMenu))]
        public static void AddSpawnMenu()
        {
            MelonLogger.Msg($"{CallCounter}: PopUpMenuView.AddSpawnMenu()");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PopUpMenuView.Awake))]
        public static void Awake()
        {
            MelonLogger.Msg($"{CallCounter}: PopUpMenuView.Awake()");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PopUpMenuView.BypassToPreferences))]
        public static void BypassToPreferences()
        {
            MelonLogger.Msg($"{CallCounter}: PopUpMenuView.BypassToPreferences()");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PopUpMenuView.CoHideCursor))]
        public static void CoHideCursor(float duration)
        {
            MelonLogger.Msg($"{CallCounter}: PageView.CoHideCursor(duration = {duration})");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PopUpMenuView.CoShowCursor))]
        public static void CoShowCursor(float duration)
        {
            MelonLogger.Msg($"{CallCounter}: PageView.CoShowCursor(duration = {duration})");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PopUpMenuView.DefaultRadial))]
        public static void DefaultRadial()
        {
            MelonLogger.Msg($"{CallCounter}: PageView.DefaultRadial()");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PopUpMenuView.ForceHideCursor))]
        public static void ForceHideCursor()
        {
            MelonLogger.Msg($"{CallCounter}: PageView.ForceHideCursor()");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PopUpMenuView.HideCursor))]
        public static void HideCursor()
        {
            MelonLogger.Msg($"{CallCounter}: PageView.HideCursor()");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PopUpMenuView.RemoveAvatarsMenu))]
        public static void RemoveAvatarsMenu()
        {
            MelonLogger.Msg($"{CallCounter}: PageView.RemoveAvatarsMenu()");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PopUpMenuView.RemoveDevMenu))]
        public static void RemoveDevMenu()
        {
            MelonLogger.Msg($"{CallCounter}: PageView.RemoveDevMenu()");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PopUpMenuView.RemoveMagEjectMenu))]
        public static void RemoveMagEjectMenu(UIControllerInput uiInput)
        {
            MelonLogger.Msg($"{CallCounter}: PageView.RemoveMagEjectMenu()");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PopUpMenuView.RemoveScenesMenu))]
        public static void RemoveScenesMenu()
        {
            MelonLogger.Msg($"{CallCounter}: PageView.RemoveScenesMenu()");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PopUpMenuView.RemoveSpawnMenu))]
        public static void RemoveSpawnMenu()
        {
            MelonLogger.Msg($"{CallCounter}: PageView.RemoveSpawnMenu()");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PopUpMenuView.ShowCursor))]
        public static void ShowCursor()
        {
            MelonLogger.Msg($"{CallCounter}: PageView.ShowCursor()");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PopUpMenuView.Start))]
        public static void Start()
        {
            MelonLogger.Msg($"{CallCounter}: PageView.Start()");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PopUpMenuView.Trigger))]
        public static void Trigger(bool isDown, bool isSecondaryDown, UIControllerInput controllerInput)
        {
            MelonLogger.Msg($"{CallCounter}: PageView.Trigger(isDown = {isDown}, isSecondaryDown = {isSecondaryDown})");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PopUpMenuView.UpdateStartPosition))]
        public static void UpdateStartPosition()
        {
            MelonLogger.Msg($"{CallCounter}: PageView.UpdateStartPosition()");
            ++CallCounter;
        }
        */
    }
}