using System.Reflection;
using FieldInjector;
using Fusion5vs5Gamemode.Client.Combat;
using Fusion5vs5Gamemode.SDK;
using Fusion5vs5Gamemode.Utilities.HarmonyPatches;
using LabFusion.SDK.Gamemodes;
using LabFusion.SDK.Modules;
using MelonLoader;

namespace Fusion5vs5Gamemode;

public class MainClass : MelonMod
{

    public static Assembly Assembly = Assembly.GetExecutingAssembly();
    public override void OnInitializeMelon()
    {
        SerialisationHandler.Inject<Fusion5vs5GamemodeDescriptor>();
        SerialisationHandler.Inject<Invoke5vs5UltEvent>();
        ModuleHandler.LoadModule(Assembly.GetExecutingAssembly());
        GamemodeRegistration.LoadGamemodes(Assembly.GetExecutingAssembly());
        ImpactPropertiesPatches.Patch();
    }

    public const string NAME = "Fusion5vs5Gamemode";
    public const string VERSION = "0.0.1";
    public const string AUTHOR = "Snake1Byte";
}