using LabFusion.SDK.Gamemodes;
using MelonLoader;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion5vs5Gamemode
{
    public class MainClass : MelonMod
    {
        public override void OnInitializeMelon()
        {
            GamemodeRegistration.LoadGamemodes(Assembly.GetExecutingAssembly());
        }
    }
}
