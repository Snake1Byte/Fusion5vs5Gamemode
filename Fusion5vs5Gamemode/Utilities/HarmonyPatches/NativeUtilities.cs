using System;
using System.Runtime.InteropServices;
using HarmonyLib;

namespace Fusion5vs5Gamemode.Utilities.HarmonyPatches;

// Note: this is taken from BONELAB Fusion: https://github.com/Lakatrazz/BONELAB-Fusion/blob/6241505268fc22bb6aeb3182268441a7ab99b279/Core/src/Utilities/Internal/NativeUtilities.cs, partial commit SHA 6241505
internal static unsafe class NativeUtilities
{
    internal static IntPtr GetNativePtr<T>(string name)
    {
        return *(IntPtr*)(IntPtr)typeof(T).GetField(name, AccessTools.all)!.GetValue(null);
    }

    internal static IntPtr GetDestPtr<TDelegate>(TDelegate destination) where TDelegate : Delegate
    {
        return destination.Method.MethodHandle.GetFunctionPointer();
    }

    internal static TDelegate GetOriginal<TDelegate>(IntPtr nativePtr)
    {
        return Marshal.GetDelegateForFunctionPointer<TDelegate>(nativePtr);
    }
}