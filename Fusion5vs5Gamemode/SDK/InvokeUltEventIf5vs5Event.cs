using System;
using LabFusion.MarrowIntegration;
using LabFusion.Utilities;
using MelonLoader;
using SLZ.UI;
using UnityEngine;
using UltEvents;

namespace Fusion5vs5Gamemode.SDK
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/UltEvents/5vs5 Gamemode/5vs5 Invoke Ult Event")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UltEventHolder))]
#endif
    public class InvokeUltEventIf5vs5Event : FusionMarrowBehaviour
    {
        public Fusion5vs5GamemodeUltEvents Event;

        public enum Fusion5vs5GamemodeUltEvents
        {
            CounterTerroristTeamJoined,
            TerroristTeamJoined,
            CounterTerroristTeamScored,
            TerroristTeamJoinedScored,
            NewRoundStarted,
            PlayerKilledAnotherPlayer,
            PlayerSuicide,
            WarmupPhaseStarted,
            BuyPhaseStarted,
            PlayPhaseStarted,
            RoundEndPhaseStarted,
            MatchHalfPhaseStarted,
            MatchEndPhaseStarted
        }

#if MELONLOADER

        public InvokeUltEventIf5vs5Event(IntPtr intPtr) : base(intPtr)
        {
        }

        public static readonly FusionComponentCache<GameObject, InvokeUltEventIf5vs5Event> Cache =
            new FusionComponentCache<GameObject, InvokeUltEventIf5vs5Event>();

        private void Awake()
        {
            Cache.Add(gameObject, this);
        }

        private void OnDestroy()
        {
            Cache.Remove(gameObject);
        }

        public void Invoke()
        {
            var holder = GetComponent<UltEventHolder>();
            MelonLogger.Msg($"{Event.ToString()} invoked.");

            if (holder != null)
                holder.Invoke();
        }
#else
        public override string Comment => "The UltEventHolder attached to this GameObject will be executed whenever the event that can be selected from the dropdown below is triggered.";
#endif
    }
}