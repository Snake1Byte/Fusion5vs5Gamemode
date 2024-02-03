using System.Reflection;
using BoneLib;
using FieldInjector;
using Fusion5vs5Gamemode.SDK;
using LabFusion.Data;
using LabFusion.SDK.Gamemodes;
using LabFusion.SDK.Modules;
using LabFusion.Utilities;
using MelonLoader;
using SLZ.Rig;
using SLZ.UI;
using UnityEngine;

namespace Fusion5vs5Gamemode
{
    public class MainClass : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Commons.LogCustom("==================================================================\n");
            SerialisationHandler.Inject<Fusion5vs5GamemodeDescriptor>();
            SerialisationHandler.Inject<Invoke5vs5UltEvent>();
            ModuleHandler.LoadModule(Assembly.GetExecutingAssembly());
            GamemodeRegistration.LoadGamemodes(Assembly.GetExecutingAssembly());
            RadialMenu.Initialize();
        }

        public const string Name = "Fusion5vs5Gamemode";
        public const string Version = "0.0.1";
        public const string Author = "Snake1Byte";
    }
}