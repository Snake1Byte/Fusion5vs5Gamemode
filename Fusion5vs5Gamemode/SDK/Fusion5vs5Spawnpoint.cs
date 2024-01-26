using System;
using Fusion5vs5Gamemode.SDK.Internal;
using UnityEditor;
using UnityEngine;

#if MELONLOADER
using LabFusion.Utilities;
#endif

namespace Fusion5vs5Gamemode.SDK
{
#if UNITY_EDITOR
    [AddComponentMenu("Fusion 5vs5 Gamemode/Fusion5vs5Spawnpoint")]
    [DisallowMultipleComponent]
#endif
    public class Fusion5vs5Spawnpoint : Fusion5vs5GamemodeBehaviour
    {
        public Fusion5vs5GamemodeTeams Team = Fusion5vs5GamemodeTeams.CounterTerrorists;
        
#if MELONLOADER
        public Fusion5vs5Spawnpoint(IntPtr intPtr) : base(intPtr)
        {
        }
        
        public static readonly FusionComponentCache<GameObject, Fusion5vs5Spawnpoint> Cache =
            new FusionComponentCache<GameObject, Fusion5vs5Spawnpoint>();

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
            "A script mandatory for making your map compatible with Fusion5vs5Gamemode. This script designates one of the 10 spawnpoints that are required to be on this map. " +
            "Select which team this Spawnpoint is for from the dropdown below and make sure to place this Spawnpoint inside of a BuyZone script's Box collider, since you want " +
            "the members of a team to be able to buy weapons after they spawned during Buy Phase. After you're done, don't forget to add this Spawnpoint to the Fusion5vs5Descriptor script.";
#endif
    }
}