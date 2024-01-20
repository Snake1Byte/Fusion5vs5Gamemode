using LabFusion.SDK.Gamemodes;
using MelonLoader;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoneLib;
using LabFusion.Network;
using LabFusion.Utilities;
using SLZ.Marrow.SceneStreaming;
using CommonBarcodes = LabFusion.Utilities.CommonBarcodes;

namespace Fusion5vs5Gamemode
{
    public class MainClass : MelonMod
    {
        public override void OnInitializeMelon()
        {
            //Patches.Patch();
            GamemodeRegistration.LoadGamemodes(Assembly.GetExecutingAssembly());
        }

        // For calling from within the UnityExplorer console
        public static void StartEverything()
        {
            MultiplayerHooking.OnStartServer += StartMap;
            NetworkHelper.StartServer();
        }

        private static void StartMap()
        {
            MultiplayerHooking.OnStartServer -= StartMap;
            MultiplayerHooking.OnMainSceneInitialized += StartGamemode;
            SceneStreamer.Load("Snek.csoffice.Level.Csoffice");
        }

        private static void StartGamemode()
        {
            if (FusionSceneManager.Level._barcode.Equals("Snek.csoffice.Level.Csoffice"))
            {
                MultiplayerHooking.OnMainSceneInitialized -= StartGamemode;
                Fusion5vs5Gamemode.Instance.StartGamemode();
            }
        }
    }
}