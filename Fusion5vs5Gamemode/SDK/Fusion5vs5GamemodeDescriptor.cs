using System;
using System.Linq;
using LabFusion.MarrowIntegration;
using SLZ.Marrow.Data;
using SLZ.Marrow.Warehouse;
using UnityEngine;

#if MELONLOADER
using MelonLoader;
using LabFusion.SDK.Gamemodes;
using UnhollowerBaseLib.Attributes;
using LabFusion.Utilities;
#endif

namespace Fusion5vs5Gamemode.SDK
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/Gamemodes/5vs5 Gamemode/Fusion5vs5GamemodeDescriptor")]
    [DisallowMultipleComponent]
#endif
    public class Fusion5vs5GamemodeDescriptor : FusionMarrowBehaviour
    {
        public Fusion5vs5GamemodeTeams DefendingTeam = Fusion5vs5GamemodeTeams.Terrorists;
        public string CounterTerroristTeamName = "Sabrelake";
        public string TerroristTeamName = "Lava Gang";
#if UNITY_EDITOR
        [Space(20)]
#endif
        public AvatarCrate DefaultAvatar;
        
#if MELONLOADER
        public Fusion5vs5GamemodeDescriptor(IntPtr intPtr) : base(intPtr)
        {
        }
        
        public static readonly FusionComponentCache<GameObject, Fusion5vs5GamemodeDescriptor> Cache =
            new FusionComponentCache<GameObject, Fusion5vs5GamemodeDescriptor>();

        private void Awake()
        {
            Cache.Add(gameObject, this);
        }

        private void OnDestroy()
        {
            Cache.Remove(gameObject);
        }


#else
        public override string Comment =>
            "A proxy script for triggering and configuring Team Deathmatch in your map.\n" +
            "You can use UltEvents or UnityEvents to trigger these functions. (ex. LifeCycleEvent that calls SetRoundLength).\n" +
            "Most settings can be configured, such as round length, team names, logos, etc.\n" +
            "The gamemode can also be started and stopped from here.";
#endif
    }
    
    public enum Fusion5vs5GamemodeTeams
    {
        CounterTerrorists,
        Terrorists
    }
}