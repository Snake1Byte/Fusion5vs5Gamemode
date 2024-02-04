using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MelonLoader;
using SLZ.UI;

namespace Fusion5vs5Gamemode.Utilities
{
    /*
    [HarmonyPatch(typeof(PageView))]
    public static class PageViewPatches
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
    }
    */
}