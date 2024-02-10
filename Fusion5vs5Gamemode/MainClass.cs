using System.Reflection;
using FieldInjector;
using Fusion5vs5Gamemode.SDK;
using Fusion5vs5Gamemode.Shared;
using Fusion5vs5Gamemode.Utilities;
using Fusion5vs5Gamemode.Utilities.HarmonyPatches;
using LabFusion.SDK.Gamemodes;
using LabFusion.SDK.Modules;
using MelonLoader;

namespace Fusion5vs5Gamemode
{
    public class MainClass : MelonMod
    {
        public override void OnInitializeMelon()
        {
            SerialisationHandler.Inject<Fusion5vs5GamemodeDescriptor>();
            SerialisationHandler.Inject<Invoke5vs5UltEvent>();
            ModuleHandler.LoadModule(Assembly.GetExecutingAssembly());
            GamemodeRegistration.LoadGamemodes(Assembly.GetExecutingAssembly());
            ImpactPropertiesPatches.Patch();
        }

        public const string Name = "Fusion5vs5Gamemode";
        public const string Version = "0.0.1";
        public const string Author = "Snake1Byte";
    }
}