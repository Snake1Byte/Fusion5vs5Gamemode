using System;
using Fusion5vs5Gamemode.SDK.Internal;
using UnityEngine;

#if MELONLOADER
using LabFusion.Utilities;
#endif

namespace Fusion5vs5Gamemode.SDK
{
#if UNITY_EDITOR
    [AddComponentMenu("Fusion 5vs5 Gamemode/BuyZone")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
#endif
    public class BuyZone : Fusion5vs5GamemodeBehaviour
    {
        public Fusion5vs5GamemodeTeams Team = Fusion5vs5GamemodeTeams.CounterTerrorists;

#if MELONLOADER
        public BuyZone(IntPtr intPtr) : base(intPtr)
        {
        }

        public static readonly FusionComponentCache<GameObject, BuyZone> Cache =
            new FusionComponentCache<GameObject, BuyZone>();

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
            "A script mandatory for making your map compatible with Fusion5vs5Gamemode. This script defines the zones where the Team selected in the dropdown below will be able to buy weapons in. "+
            "This is required since we don't want either team to be able to buy weapons from anywhere within the map, but rather within a small area inside of the spawnpoints of either teams. "+
            "Use the Box collider provided with this script to define the buy zone for the selected team. When you're done, don't forget to add this component to the Fusion5vs5Descriptor script.";
#endif
    }
}