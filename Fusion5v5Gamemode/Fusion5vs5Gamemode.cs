using BoneLib.BoneMenu.Elements;
using LabFusion.Representation;
using LabFusion.SDK.Gamemodes;
using LabFusion.Senders;
using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Fusion5vs5Gamemode
{
    internal class Fusion5vs5Gamemode : Gamemode
    {
        public override string GamemodeCategory => "Snake1Byte's Gamemodes";

        public override string GamemodeName => "5 vs 5";

        public override bool PreventNewJoins => !enableLateJoining;

        private bool enableLateJoining = true;

        public override bool AutoHolsterOnDeath => false;

        // Debug
        public override bool DisableDevTools => !debug;
        public override bool DisableSpawnGun => !debug;
        public override bool DisableManualUnragdoll => !debug;

        private bool debug = true;

        // Metadata
        public const string DefaultPrefix = "Fusion5vs5";
        public const string TeamKey = DefaultPrefix + ".Team";
        public const string TeamScoreKey = TeamKey + ".Score";

        public const string PlayerKillsKey = DefaultPrefix + ".Kills";
        public const string PlayerDeathsKey = DefaultPrefix + ".Deaths";
        public const string PlayerAssistsKey = DefaultPrefix + ".Assists";

        // Music 
        public override bool MusicEnabled => enableMusic;
        private bool enableMusic = true;

        public static Fusion5vs5Gamemode Instance { get; private set; }

        public override void OnBoneMenuCreated(MenuCategory category)
        {
            //TODO Add custom settings for this gamemode

            category.CreateBoolElement("Enable late joining", Color.white, enableLateJoining, e =>
            {
                enableLateJoining = e;
            });

            category.CreateBoolElement("Enable round music", Color.white, enableMusic, e =>
            {
                enableMusic = e;
            });

            category.CreateBoolElement("Debug", Color.white, debug, e =>
            {
                debug = e;
            });
        }
        public override void OnGamemodeRegistered()
        {
            Instance = this;

            MultiplayerHooking.OnPlayerJoin += OnPlayerJoin;
            MultiplayerHooking.OnPlayerLeave += OnPlayerLeave;
            MultiplayerHooking.OnPlayerAction += OnPlayerAction;
        }

        private void OnPlayerAction(PlayerId playerId, PlayerActionType type, PlayerId otherPlayer)
        {
            throw new NotImplementedException();
        }

        private void OnPlayerLeave(PlayerId playerId)
        {
            throw new NotImplementedException();
        }

        private void OnPlayerJoin(PlayerId playerId)
        {
            throw new NotImplementedException();
        }

        public override void OnGamemodeUnregistered()
        {
            base.OnGamemodeUnregistered();

            if (Instance == this)
            {
                Instance = null;
            }

            MultiplayerHooking.OnPlayerJoin -= OnPlayerJoin;
            MultiplayerHooking.OnPlayerLeave -= OnPlayerLeave;
            MultiplayerHooking.OnPlayerAction -= OnPlayerAction;
        }

        public void EnableLateJoining(bool enabled)
        {
            enableLateJoining = enabled;
        }
    }
}
