using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Il2CppSystem;
using Il2CppSystem.Collections;
using Il2CppSystem.Text;
using LabFusion.Data;
using MelonLoader;
using SLZ.Rig;
using SLZ.UI;
using UltEvents;
using UnityEngine;

namespace Fusion5vs5Gamemode
{
    public class RadialMenu
    {
        // Root Menu
        private static PageItem _BuyMenu;
        
        // Weapon Category
        private static PageItem _Back;
        private static PageItem _Pistols;
        private static PageItem _SMGs;
        private static PageItem _Rifles;
        private static PageItem _Shotguns;
        private static PageItem _SniperRifles;
        private static PageItem _Utilities;
        
        //
        
        private static List<PageItem> backup;
        internal static ActivatePopupParams? _LastActivatePopupParams;

        /// <summary>
        /// Adds the buy menu to the radial menu (quick menu).
        /// </summary>
        public static void AddBuyMenu()
        {
            RigManager rm = RigData.RigReferences.RigManager;

            PopUpMenuView popupMenu = rm.uiRig.popUpMenu;
            Page mainMenu = popupMenu.radialPageView.m_HomePage;
            if (backup == null)
            {
                backup = new List<PageItem>(mainMenu.items.ToArray());
            }

            PageItem item1 = new PageItem("Item1", PageItem.Directions.WEST, (Action)(() => { }));
            PageItem item2 = new PageItem("Item2", PageItem.Directions.EAST, (Action)(() => { }));
            PageItem item3 = new PageItem("Item3", PageItem.Directions.NORTH, (Action)(() =>
            {
                popupMenu.Deactivate();

                mainMenu.items.Clear();
                foreach (var backupItem in backup)
                {
                    mainMenu.items.Add(backupItem);
                }

                mainMenu.items.Add(_BuyMenu);

                popupMenu.Activate(_LastActivatePopupParams?.headTransform, _LastActivatePopupParams?.rootTransform,
                    _LastActivatePopupParams?.controllerInput, _LastActivatePopupParams?.controller);
            }));

            _BuyMenu = new PageItem("Buy", PageItem.Directions.WEST, (Action)(() =>
            {
                popupMenu.Deactivate();

                mainMenu.items.Clear();

                mainMenu.items.Add(item1);
                mainMenu.items.Add(item2);
                mainMenu.items.Add(item3);

                popupMenu.Activate(_LastActivatePopupParams?.headTransform, _LastActivatePopupParams?.rootTransform,
                    _LastActivatePopupParams?.controllerInput, _LastActivatePopupParams?.controller);
            }));
            mainMenu.items.Add(_BuyMenu);
        }

        public static void RemoveBuyMenu()
        {
            RigManager rm = RigData.RigReferences.RigManager;
            Page menu = rm.uiRig.popUpMenu.radialPageView.m_HomePage;
            menu.items.Clear();
            foreach (var backupItem in backup)
            {
                menu.items.Add(backupItem);
            }
        }

        internal struct ActivatePopupParams
        {
            internal Transform headTransform;
            internal Transform rootTransform;
            internal UIControllerInput controllerInput;
            internal BaseController controller;
        }
    }


    [HarmonyPatch(typeof(PopUpMenuView))]
    public static class PopUpMenuViewPatches
    {
        public static int CallCounter = 1;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PopUpMenuView.Activate))]
        public static void Activate(Transform headTransform, Transform rootTransform, UIControllerInput controllerInput,
            BaseController controller)
        {
            if (RadialMenu._LastActivatePopupParams == null)
            {
                RadialMenu._LastActivatePopupParams = new RadialMenu.ActivatePopupParams
                {
                    headTransform = headTransform,
                    rootTransform = rootTransform,
                    controllerInput = controllerInput,
                    controller = controller
                };
            }

            MelonLogger.Msg($"{CallCounter}: PageView.Activate()");
            ++CallCounter;
        }
/*
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
        [HarmonyPatch(nameof(PopUpMenuView.Deactivate))]
        public static void Deactivate()
        {
            MelonLogger.Msg($"{CallCounter}: PopUpMenuView.Deactivate()");
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
        }*/
    }

/*
    [HarmonyPatch(typeof(PageView))]
    public static class RadialMenuPatches
    {
        public static int CallCounter = 1;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PageView.Activate))]
        public static void Activate()
        {
            MelonLogger.Msg($"{CallCounter}: PageView.Activate()");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PageView.ChangePage))]
        public static void ChangePage(Page page)
        {
            var a = PageItemsToString(page);
            MelonLogger.Msg($"{CallCounter}: PageView.ChangePage(page = List<PageItem>: {a})");
            ++CallCounter;
        }

        private static string PageItemsToString(Page page)
        {
            string a = string.Join(", ", new List<PageItem>(page.items.ToArray()).Select(obj => obj.name));
            return a;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PageView.CheckRefs))]
        public static void CheckRefs()
        {
            MelonLogger.Msg($"{CallCounter}: PageView.CheckRefs()");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PageView.Clear))]
        public static void Clear()
        {
            MelonLogger.Msg($"{CallCounter}: PageView.Clear()");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PageView.CoChangePage))]
        public static void CoChangePage(Page page)
        {
            var a = PageItemsToString(page);
            MelonLogger.Msg($"{CallCounter}: PageView.CoChangePage(page = List<PageItem>: {a})");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PageView.CoCloseAnimation))]
        public static void CoCloseAnimation()
        {
            MelonLogger.Msg($"{CallCounter}: PageView.CoCloseAnimation()");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PageView.CoSummonAnimation))]
        public static void CoSummonAnimation()
        {
            MelonLogger.Msg($"{CallCounter}: PageView.CoSummonAnimation()");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PageView.Deactivate))]
        public static void Deactivate()
        {
            MelonLogger.Msg($"{CallCounter}: PageView.Deactivate()");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PageView.GetHomePage))]
        public static void GetHomePage()
        {
            MelonLogger.Msg($"{CallCounter}: PageView.GetHomePage()");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PageView.PlayAudioClip))]
        public static void PlayAudioClip()
        {
            MelonLogger.Msg($"{CallCounter}: PageView.PlayAudioClip()");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PageView.PlayHaptic))]
        public static void PlayHaptic(int hapticAction)
        {
            MelonLogger.Msg($"{CallCounter}: PageView.PlayHaptic(hapticAction = {hapticAction})");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PageView.RadialText))]
        public static void RadialText(bool activation)
        {
            MelonLogger.Msg($"{CallCounter}: PageView.RadialText(activation = {activation})");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PageView.Render))]
        public static void Render(Page page)
        {
            var a = PageItemsToString(page);
            MelonLogger.Msg($"{CallCounter}: PageView.Render(page = List<PageItem>: {a})");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PageView.SetHomePage))]
        public static void SetHomePage(Page page)
        {
            var a = PageItemsToString(page);
            MelonLogger.Msg($"{CallCounter}: PageView.SetHomePage(page = List<PageItem>: {a})");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PageView.ShutOff))]
        public static void ShutOff()
        {
            MelonLogger.Msg($"{CallCounter}: PageView.ShutOff()");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PageView.Start))]
        public static void Start()
        {
            MelonLogger.Msg($"{CallCounter}: PageView.Start()");
            ++CallCounter;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PageView.Trigger))]
        public static void Trigger()
        {
            MelonLogger.Msg($"{CallCounter}: PageView.Trigger()");
            ++CallCounter;
        }
    }*/
}