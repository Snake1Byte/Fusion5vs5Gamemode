using LabFusion.SDK.Gamemodes;
using MelonLoader;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoneLib;
using Fusion5vs5Gamemode.SDK;
using LabFusion.Network;
using LabFusion.SDK.Modules;
using LabFusion.Utilities;
using SLZ.Marrow.SceneStreaming;
using CommonBarcodes = LabFusion.Utilities.CommonBarcodes;

namespace Fusion5vs5Gamemode
{
    public class MainClass : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Commons.LogCustom("==================================================================\n");
            FieldInjector.SerialisationHandler.Inject<Fusion5vs5GamemodeDescriptor>();
            FieldInjector.SerialisationHandler.Inject<Invoke5vs5UltEvent>();
            ModuleHandler.LoadModule(Assembly.GetExecutingAssembly());
            GamemodeRegistration.LoadGamemodes(Assembly.GetExecutingAssembly());
        }

        public const string Name = "Fusion5vs5Gamemode";
        public const string Version = "0.0.1";
        public const string Author = "Snake1Byte";

    }
}