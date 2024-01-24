using System;
using System.Linq;
using Fusion5vs5Gamemode.SDK.Internal;
using LabFusion.MarrowIntegration;
using SLZ.Marrow.Data;
using SLZ.Marrow.Warehouse;
using UnityEditor;
using UnityEngine;

#if MELONLOADER
using MelonLoader;
using LabFusion.SDK.Gamemodes;
using UnhollowerBaseLib.Attributes;
using LabFusion.Utilities;
#endif

namespace Fusion5vs5Gamemode.SDK
{
#if UNITY_EDITOR
    [AddComponentMenu("Fusion 5vs5 Gamemode/Fusion5vs5GamemodeDescriptor")]
    [DisallowMultipleComponent]
#endif
    public class Fusion5vs5GamemodeDescriptor : Fusion5vs5GamemodeBehaviour
    {
        public Fusion5vs5GamemodeTeams DefendingTeam = Fusion5vs5GamemodeTeams.CounterTerrorists;
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
            "A script mandatory for making your map compatible with Fusion5vs5Gamemode. This script it required to start the gaemmode on this map.\n" +
            "The Fusion5vs5Gamemode also has events that can trigger your custom UltEvents. To be able to listen to these events, add the \"Invoke5vs5UltEvent\" component to a GameObject.";
#endif
    }

    public enum Fusion5vs5GamemodeTeams
    {
        CounterTerrorists,
        Terrorists
    }
}