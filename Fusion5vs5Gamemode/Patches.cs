using System;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using SLZ.Combat;
using static PlayerDamageReceiver;
using System.Runtime.InteropServices;
using BoneLib.BoneMenu.Elements;
using SLZ.AI;
using SLZ.Marrow.Data;
using UnityEngine;


namespace Fusion5vs5Gamemode
{
    //[HarmonyPatch(typeof(Player_Health))]
    public static class Patches
    {
        
        public static void SetIncrement(this IntElement a, int b)
        {
            var type = a.GetType();
            var field = type.GetField("_increment", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(a, b);
            }
        }
        /*
        [StructLayout(LayoutKind.Sequential)]
        public struct Attack_
        {
            public float damage;

            public Vector3 normal;

            public Vector3 origin;

            public Vector3 direction;

            public bool backFacing;

            public int OrderInPool;

            public IntPtr collider;

            public AttackType attackType;

            public IntPtr proxy;
        }
        
        internal static unsafe class NativeUtilities
        {
            internal static IntPtr GetNativePtr<T>(string name)
            {
                return *(IntPtr*)(IntPtr)typeof(T).GetField(name, AccessTools.all).GetValue(null);
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
        
        public static void Patch()
        {
            PatchReceiveAttack();
        }
        public delegate void ReceiveAttackPatchDelegate(IntPtr instance, IntPtr attack, IntPtr method);
        // ReceiveAttack patching stuff
        private static ReceiveAttackPatchDelegate _original;

        private unsafe static void PatchReceiveAttack()
        {
            var tgtPtr = NativeUtilities.GetNativePtr<PlayerDamageReceiver>("NativeMethodInfoPtr_ReceiveAttack_Public_Virtual_Final_New_Void_Attack_0");
            var dstPtr = NativeUtilities.GetDestPtr<ReceiveAttackPatchDelegate>(ReceiveAttack);

            MelonUtils.NativeHookAttach((IntPtr)(&tgtPtr), dstPtr);
            _original = NativeUtilities.GetOriginal<ReceiveAttackPatchDelegate>(tgtPtr);
        }

        private static void ReceiveAttack(IntPtr instance, IntPtr attack, IntPtr method)
        {
            try
            {
                unsafe
                {
                    var receiver = new PlayerDamageReceiver(instance);
                    var rm = receiver.health._rigManager;

                    // Get the attack and its shooter
                    var _attack = *(Attack_*)attack;

                    TriggerRefProxy proxy = null;

                    if (_attack.proxy != IntPtr.Zero)
                        proxy = new TriggerRefProxy(_attack.proxy);
                    MelonLogger.Msg($"========================Damage was {_attack.damage}========================");
                }

                _original(instance, attack, method);
            }
            catch (Exception e)
            {
                MelonLogger.Msg($"Something happened :( {e}");
            }
        }

        public static void LogPropertiesAndFields(string a, object obj)
        {
            MelonLogger.Msg("Now logging: " + a);

            if (obj == null)
            {
                MelonLogger.Msg("... but it was was null.");
                return;
            }
            try
            { 
                Type type = obj.GetType();

                // Instance properties and fields
                PropertyInfo[] instanceProperties = type.GetProperties();
                FieldInfo[] instanceFields = type.GetFields();

                MelonLogger.Msg($"Instance Properties of {type.Name}:");
                foreach (var property in instanceProperties)
                {
                    object value = property.GetValue(obj);
                    if (value != null)
                        MelonLogger.Msg($"{property.Name}: {value}");
                }

                MelonLogger.Msg($"\nInstance Fields of {type.Name}:");
                foreach (var field in instanceFields)
                {
                    
                    object value = field.GetValue(obj);
                    if (value != null)
                        MelonLogger.Msg($"{field.Name}: {value}");
                }

                // Static properties and fields
                PropertyInfo[] staticProperties =
                    type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                FieldInfo[] staticFields =
                    type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                MelonLogger.Msg($"\nStatic Properties of {type.Name}:");
                foreach (var property in staticProperties)
                {
                    object value = property.GetValue(null); // null for static members
                    if (value != null)
                        MelonLogger.Msg($"{property.Name}: {value}");
                }

                MelonLogger.Msg($"\nStatic Fields of {type.Name}:");
                foreach (var field in staticFields)
                {
                    object value = field.GetValue(null); // null for static members
                    if (value != null)
                        MelonLogger.Msg($"{field.Name}: {value}");
                }
            }
            catch (Exception e)
            {
                MelonLogger.Msg($"Exception: {e}");
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Player_Health.OnReceivedDamage))]
        public static void OnReceivedDamage(Attack attack, BodyPart part)
        {
            MelonLogger.Msg($"Shot body part was {part}.");
        }*/
    }
}