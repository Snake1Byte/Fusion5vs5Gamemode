using System;
using HarmonyLib;
using MelonLoader;
using SLZ.Props.Weapons;

namespace Fusion5vs5Gamemode.Utilities.HarmonyPatches;

[HarmonyPatch(typeof(Gun))]
public static class GunPatches
{
    public static Action<Gun>? OnGunFired;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.OnFire))]
    public static void OnFire(Gun __instance)
    {
        try
        {
            if (__instance == null) return;

            BoneLib.SafeActions.InvokeActionSafe(OnGunFired, __instance);
        }
        catch (Exception e)
        {
#if DEBUG
            MelonLogger.Msg(
                $"Exception {e} fired in Gun.OnFire() HarmonyPatch.");
#endif
        }
    }

    /*
    private static int Counter = 0;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.Awake))]
    public static void Awake()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.Awake() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.Start))]
    public static void Start()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.Start() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.OnDestroy))]
    public static void OnDestroy()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.OnDestroy() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.OnCollisionEnter))]
    public static void OnCollisionEnter(Collision collision)
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.OnCollisionEnter(collision = {collision}) called");
    }


    // [HarmonyPrefix]
    // [HarmonyPatch(nameof(Gun.Update))]
    // public static void Update()
    // {
    //     Counter++;
    //     MelonLogger.Msg($"{Counter}: Gun.Update() called");
    // }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.InstantLoad))]
    public static void InstantLoad()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.InstantLoad() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.OnPostSpawn))]
    public static void OnPostSpawn(GameObject go)
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.OnPostSpawn(go = {go}) called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.OnDespawn))]
    public static void OnDespawn(GameObject go)
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.OnDespawn(go = {go}) called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.OnMagazineInserted))]
    public static void OnMagazineInserted()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.OnMagazineInserted() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.UpdateMagazineArt))]
    public static void UpdateMagazineArt()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.UpdateMagazineArt() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.Charge))]
    public static void Charge()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.Charge() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.OnMagazineRemoved))]
    public static void OnMagazineRemoved()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.OnMagazineRemoved() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.ApplyBulletArt))]
    public static void ApplyBulletArt(GameObject go, Transform parent)
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.ApplyBulletArt(go = {go}, parent = {parent}) called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.CoBlinkHighlight))]
    public static void CoBlinkHighlight(Renderer renderer)
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.CoBlinkHighlight(renderer = {renderer}) called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.AmmoCount))]
    public static void AmmoCount()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.AmmoCount() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.CheckGunRequirements))]
    public static void CheckGunRequirements()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.CheckGunRequirements() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.RequiresChargingHandle))]
    public static void RequiresChargingHandle()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.RequiresChargingHandle() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.AggroItem))]
    public static void AggroItem(TriggerRefProxy proxy, SceneZone zone, Boolean isSubZone)
    {
        Counter++;
        MelonLogger.Msg(
            $"{Counter}: Gun.AggroItem(proxy = {proxy}, zone = {zone}, isSubZone = {isSubZone}) called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.BroadcastGunFiredEvent))]
    public static void BroadcastGunFiredEvent()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.BroadcastGunFiredEvent() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.SlideOverride))]
    public static void SlideOverride()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.SlideOverride() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.SlideOverrideReleased))]
    public static void SlideOverrideReleased()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.SlideOverrideReleased() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.SlideGrabbed))]
    public static void SlideGrabbed()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.SlideGrabbed() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.SlideGrabbedReleased))]
    public static void SlideGrabbedReleased()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.SlideGrabbedReleased() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.UpdateSlidePercentage))]
    public static void UpdateSlidePercentage(Single perc)
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.UpdateSlidePercentage(perc = {perc}) called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.SlidePull))]
    public static void SlidePull()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.SlidePull() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.SlideLocked))]
    public static void SlideLocked()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.SlideLocked() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.SlideRelease))]
    public static void SlideRelease()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.SlideRelease() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.Fire))]
    public static void Fire()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.Fire() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.CeaseFire))]
    public static void CeaseFire()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.CeaseFire() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.PullCartridge))]
    public static void PullCartridge()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.PullCartridge() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.EjectCartridge))]
    public static void EjectCartridge()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.EjectCartridge() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.UpdateArt))]
    public static void UpdateArt()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.UpdateArt() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.CompleteSlidePull))]
    public static void CompleteSlidePull()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.CompleteSlidePull() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.CompleteSlideReturn))]
    public static void CompleteSlideReturn()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.CompleteSlideReturn() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.OnTriggerGripAttached))]
    public static void OnTriggerGripAttached(Hand hand)
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.OnTriggerGripAttached(hand = {hand}) called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.OnTriggerGripDetached))]
    public static void OnTriggerGripDetached(Hand hand)
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.OnTriggerGripDetached(hand = {hand}) called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun.HasMagazine))]
    public static void HasMagazine()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun.HasMagazine() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun._Awake_b__82_0))]
    public static void _Awake_b__82_0(Single perc)
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun._Awake_b__82_0(perc = {perc}) called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun._Awake_b__82_1))]
    public static void _Awake_b__82_1()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun._Awake_b__82_1() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun._Awake_b__82_2))]
    public static void _Awake_b__82_2()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun._Awake_b__82_2() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun._Awake_b__82_3))]
    public static void _Awake_b__82_3()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun._Awake_b__82_3() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun._UpdateMagazineArt_b__91_0))]
    public static void _UpdateMagazineArt_b__91_0(GameObject go)
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun._UpdateMagazineArt_b__91_0(go = {go}) called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun._UpdateMagazineArt_b__91_1))]
    public static void _UpdateMagazineArt_b__91_1(GameObject go)
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun._UpdateMagazineArt_b__91_1(go = {go}) called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun._UpdateMagazineArt_b__91_2))]
    public static void _UpdateMagazineArt_b__91_2(GameObject go)
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun._UpdateMagazineArt_b__91_2(go = {go}) called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun._OnFire_b__116_0))]
    public static void _OnFire_b__116_0(GameObject go)
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun._OnFire_b__116_0(go = {go}) called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Gun._EjectCartridge_b__128_0))]
    public static void _EjectCartridge_b__128_0(GameObject go)
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: Gun._EjectCartridge_b__128_0(go = {go}) called");
    }
    */
}