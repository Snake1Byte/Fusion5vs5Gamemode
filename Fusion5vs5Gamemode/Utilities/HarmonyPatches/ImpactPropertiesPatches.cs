using System;
using HarmonyLib;
using LabFusion.NativeStructs;
using LabFusion.Patching;
using MelonLoader;
using SLZ.AI;
using SLZ.Combat;
// ReSharper disable Unity.IncorrectMonoBehaviourInstantiation

namespace Fusion5vs5Gamemode.Utilities.HarmonyPatches;

[HarmonyPatch(typeof(ImpactProperties))]
public static class ImpactPropertiesPatches
{
    public static Action<ImpactProperties, Attack_>? OnAttackReceived;
#if DEBUG
    private static int _Counter = 0;
#endif

    public static void Patch()
    {
        PatchReceiveAttack();
    }

    // Note: this is taken from BONELAB Fusion: https://github.com/Lakatrazz/BONELAB-Fusion/blob/6241505268fc22bb6aeb3182268441a7ab99b279/Core/src/Patching/Patches/Attacks/ImpactPropertiesPatches.cs#L32, partial commit SHA 6241505
    private static ReceiveAttackPatchDelegate? _Original;

    private static unsafe void PatchReceiveAttack()
    {
        var tgtPtr =
            NativeUtilities.GetNativePtr<ImpactProperties>(
                "NativeMethodInfoPtr_ReceiveAttack_Public_Virtual_Final_New_Void_Attack_0");
        var dstPtr = NativeUtilities.GetDestPtr<ReceiveAttackPatchDelegate>(ReceiveAttack);

        MelonUtils.NativeHookAttach((IntPtr)(&tgtPtr), dstPtr);
        _Original = NativeUtilities.GetOriginal<ReceiveAttackPatchDelegate>(tgtPtr);
    }

    private static void ReceiveAttack(IntPtr instance, IntPtr attack, IntPtr method)
    {
        try
        {
            ImpactProperties receiver;
            Attack_ _attack = new Attack_();
            TriggerRefProxy proxy;
            receiver = new ImpactProperties(instance);
            unsafe
            {
                _attack = *(Attack_*)attack;
            }

            proxy = new TriggerRefProxy(_attack.proxy);
#if DEBUG
            _Counter++;
            MelonLogger.Msg(
                $"{_Counter}: Called ImpactProperties.ReceiveAttack(attack: damage = {_attack.damage}, direction = {_attack.direction}, normal = {_attack.normal}, origin = {_attack.origin}, proxy = {proxy.GetInstanceID()} {proxy})");
#endif
            BoneLib.SafeActions.InvokeActionSafe(OnAttackReceived, receiver, _attack);
            _Original(instance, attack, method);
        }
        catch (Exception e)
        {
#if DEBUG
            MelonLogger.Msg($"Exception {e} fired in ImpactProperties.ReceiveAttack() HarmonyPatch.");
#endif
        }
    }

    /*

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ImpactProperties.Awake))]
    public static void Awake()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: ImpactProperties.Awake() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ImpactProperties.StaticFix))]
    public static void StaticFix()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: ImpactProperties.StaticFix() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ImpactProperties.OnDestroy))]
    public static void OnDestroy()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: ImpactProperties.OnDestroy() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ImpactProperties.UpdateMaterial))]
    public static void UpdateMaterial()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: ImpactProperties.UpdateMaterial() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ImpactProperties.FindColliders))]
    public static void FindColliders(bool childColliders)
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: ImpactProperties.FindColliders(childColliders = {childColliders}) called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ImpactProperties.FindRenderer))]
    public static void FindRenderer()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: ImpactProperties.FindRenderer() called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ImpactProperties.SetRenderer))]
    public static void SetRenderer(Renderer renderer)
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: ImpactProperties.SetRenderer(renderer = {renderer}) called");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ImpactProperties.GetMegaPascals))]
    public static void GetMegaPascals()
    {
        Counter++;
        MelonLogger.Msg($"{Counter}: ImpactProperties.GetMegaPascals() called");
    }
    */
}