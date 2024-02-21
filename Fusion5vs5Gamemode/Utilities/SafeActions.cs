using System;
using System.Linq;
using System.Reflection;
using MelonLoader;

namespace Fusion5vs5Gamemode.Utilities;

// From BoneLib, extended to take more than 2 parameters
public class SafeActions
{
    public static void InvokeActionSafe<T1, T2, T3>(Action<T1, T2, T3>? action, T1 param1, T2 param2, T3 param3)
    {
        if (action == null) return;
        foreach (Delegate invoker in action.GetInvocationList())
        {
            try
            {
                Action<T1, T2, T3> call = (Action<T1, T2, T3>)invoker;
                call(param1, param2, param3);
            }
            catch (Exception ex)
            {
                Type? declaringType = invoker.GetMethodInfo().DeclaringType;
                if (declaringType != null)
                {
                    string asm = declaringType.Assembly.FullName;
                    MelonMod? mod = MelonMod.RegisteredMelons.FirstOrDefault(i => i.MelonAssembly.Assembly.FullName == asm);

                    MelonLogger.Error("Exception while invoking hook callback!");
                    mod?.LoggerInstance.Error(ex.ToString());
                }
            }
        }
    }
        
    public static void InvokeActionSafe<T1, T2, T3, T4>(Action<T1, T2, T3, T4>? action, T1 param1, T2 param2, T3 param3, T4 param4)
    {
        if (action == null) return;
        foreach (Delegate invoker in action.GetInvocationList())
        {
            try
            {
                Action<T1, T2, T3, T4> call = (Action<T1, T2, T3, T4>)invoker;
                call(param1, param2, param3, param4);
            }
            catch (Exception ex)
            {
                Type? declaringType = invoker.GetMethodInfo().DeclaringType;
                if (declaringType != null)
                {
                    string asm = declaringType.Assembly.FullName;
                    MelonMod? mod = MelonMod.RegisteredMelons.FirstOrDefault(i => i.MelonAssembly.Assembly.FullName == asm);

                    MelonLogger.Error("Exception while invoking hook callback!");
                    mod?.LoggerInstance.Error(ex.ToString());
                }
            }
        }
    }
        
    public static void InvokeActionSafe<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5>? action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
    {
        if (action == null) return;
        foreach (Delegate invoker in action.GetInvocationList())
        {
            try
            {
                Action<T1, T2, T3, T4, T5> call = (Action<T1, T2, T3, T4, T5>)invoker;
                call(param1, param2, param3, param4, param5);
            }
            catch (Exception ex)
            {
                Type? declaringType = invoker.GetMethodInfo().DeclaringType;
                if (declaringType != null)
                {
                    string asm = declaringType.Assembly.FullName;
                    MelonMod? mod = MelonMod.RegisteredMelons.FirstOrDefault(i => i.MelonAssembly.Assembly.FullName == asm);

                    MelonLogger.Error("Exception while invoking hook callback!");
                    mod?.LoggerInstance.Error(ex.ToString());
                }
            }
        }
    }
}