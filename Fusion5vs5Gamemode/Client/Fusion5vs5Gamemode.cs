using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using BoneLib.BoneMenu.Elements;
using Fusion5vs5Gamemode.SDK;
using Fusion5vs5Gamemode.Server;
using Fusion5vs5Gamemode.Shared;
using Fusion5vs5Gamemode.Utilities;
using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.SDK.Gamemodes;
using LabFusion.Utilities;
using MelonLoader;
using SLZ.Bonelab;
using SLZ.Marrow.SceneStreaming;
using SLZ.Rig;
using SwipezGamemodeLib.Spawning;
using SwipezGamemodeLib.Utilities;
using TMPro;
using UnityEngine;
using static Fusion5vs5Gamemode.Shared.Commons;
using static Fusion5vs5Gamemode.Shared.Fusion5vs5CustomModule;

namespace Fusion5vs5Gamemode.Client
{
    public class Fusion5vs5Gamemode : Gamemode
    {
        public override string GamemodeCategory => "Snake1Byte's Gamemodes";

        public override string GamemodeName => "5 vs 5";

        public override bool PreventNewJoins => EnableLateJoining;

        public override bool AutoHolsterOnDeath => false;
        public override bool DisableManualUnragdoll => true;
        public override bool AutoStopOnSceneLoad => false;

        // Debug
        public override bool DisableDevTools => !_Debug;
        public override bool DisableSpawnGun => !_Debug;

        private float _DebugTextUpdateTimePassed = 0;
        private GameObject _DebugText = null;
        private bool _Debug = true;

        // Music 
        public override bool MusicEnabled => _EnableMusic;
        private bool _EnableMusic = true;

        // UI
        private Timer _UITimer;
        private int _CurrentTimeLimit;
        private int _ElapsedTime = 0;

        // Internal
        public static Fusion5vs5Gamemode Instance { get; private set; }
        private Fusion5vs5GamemodeDescriptor _Descriptor;
        public Server.Server Server { get; private set; }
        private Fusion5vs5GamemodeTeams _DefendingTeam;
        private string _CounterTerroristTeamName;
        private string _TerroristTeamName;

        private Fusion5vs5GamemodeTeams _LocalTeam;
        private SpawnPointRepresentation _LocalSpawnPoint;
        private float _LocalPlayerVelocity;
        private string _LastLocalAvatar;
        private bool _LocalPlayerFrozen;


        private GameStates _State = GameStates.Unknown;

        private MenuCategory _Menu;
        private IntElement _MaxRoundsSetting;
        private BoolElement _EnableHalfTimeSetting;
        private BoolElement _EnableLateJoiningSetting;
        private BoolElement _AllowAvatarChangingSetting;

        // Settings
        public int MaxRounds { get; private set; } = 15;
        public bool HalfTimeEnabled { get; private set; } = true;
        public bool EnableLateJoining { get; private set; } = true;
        public bool AllowAvatarChanging { get; private set; } = true;

        public Dictionary<GameStates, int> TimeLimits { get; } = new Dictionary<GameStates, int>();

        public override void OnBoneMenuCreated(MenuCategory category)
        {
            Log(category);
            base.OnBoneMenuCreated(category);

            _Menu = category;

            _MaxRoundsSetting = category.CreateIntElement("Maximum rounds", Color.white, MaxRounds, 1, 1, 1000000,
                i =>
                {
                    if (IsActive())
                        _MaxRoundsSetting.SetValue(MaxRounds);
                    else
                    {
                        int maxRounds = _MaxRoundsSetting.GetValue();
                        if (maxRounds % 2 == 1 && maxRounds > MaxRounds)
                        {
                            _MaxRoundsSetting.SetValue(maxRounds + 1);
                        }
                        else if (maxRounds % 2 == 1 && maxRounds < MaxRounds)
                        {
                            _MaxRoundsSetting.SetValue(maxRounds - 1);
                        }

                        MaxRounds = _MaxRoundsSetting.GetValue();
                    }
                });

            _EnableHalfTimeSetting = category.CreateBoolElement("Enable Half-Time", Color.white, HalfTimeEnabled, b =>
            {
                if (IsActive())
                {
                    _EnableHalfTimeSetting.SetValue(HalfTimeEnabled);
                }
                else if (b)
                {
                    _MaxRoundsSetting.SetIncrement(2);
                    int maxRounds = _MaxRoundsSetting.GetValue();
                    if (maxRounds % 2 == 1)
                    {
                        _MaxRoundsSetting.SetValue(maxRounds + 1);
                    }

                    MaxRounds = _MaxRoundsSetting.GetValue();
                }
                else
                {
                    _MaxRoundsSetting.SetIncrement(1);
                }
            });

            _EnableLateJoiningSetting = category.CreateBoolElement("Enable late joining", Color.white,
                EnableLateJoining, b =>
                {
                    if (IsActive())
                        _EnableLateJoiningSetting.SetValue(EnableLateJoining);
                    else
                        EnableLateJoining = b;
                });

            _AllowAvatarChangingSetting = category.CreateBoolElement("Allow avatar changing", Color.white,
                AllowAvatarChanging, b =>
                {
                    if (IsActive())
                        _AllowAvatarChangingSetting.SetValue(AllowAvatarChanging);
                });

            category.CreateBoolElement("Enable round music", Color.white, _EnableMusic, b => _EnableMusic = b);

            category.CreateBoolElement("Debug", Color.white, _Debug, e => _Debug = e);
        }

        public override void OnGamemodeRegistered()
        {
            Log();
            base.OnGamemodeRegistered();
            MelonLogger.Msg("5vs5 Mode: OnGameModeRegistered Called.");
            Instance = this;

            TimeLimits.Add(GameStates.Warmup, 60);
            TimeLimits.Add(GameStates.BuyPhase, 15);
            TimeLimits.Add(GameStates.PlayPhase, 135);
            TimeLimits.Add(GameStates.RoundEndPhase, 10);
            TimeLimits.Add(GameStates.MatchHalfPhase, 15);
            TimeLimits.Add(GameStates.MatchEndPhase, 30);
        }

        public override void OnGamemodeUnregistered()
        {
            Log();
            base.OnGamemodeUnregistered();
            MelonLogger.Msg("5vs5 Mode: OnGameModeUnRegistered Called.");
            if (Instance == this)
            {
                Instance = null;
            }
        }

        protected override void OnStartGamemode()
        {
            Log();
            base.OnStartGamemode();
            MelonLogger.Msg("5vs5 Mode: OnStartGamemode Called.");
            _Menu.Elements.RemoveInstance(_EnableHalfTimeSetting);
            _Menu.Elements.RemoveInstance(_MaxRoundsSetting);

            Fusion5vs5GamemodeDescriptor descriptor = null;
            foreach (var firstComponent in Fusion5vs5GamemodeDescriptor.Cache.Components)
            {
                descriptor = firstComponent;
                break;
            }

            if (descriptor == null)
            {
                Notify("Map does not support game mode!",
                    "The current map does not have a Fusion5vs5GamemodeDescriptor script attached to it.",
                    10f);
                StopGamemode();
                return;
            }

            _Descriptor = descriptor;

            Collider counterTerroristBuyZoneGo = _Descriptor.CounterTerroristBuyZone;
            Collider terroristBuyZoneGo = _Descriptor.TerroristBuyZone;

            if (counterTerroristBuyZoneGo == null || terroristBuyZoneGo == null)
            {
                Notify("Map does not support game mode!", "The current map does not have a buy zone for each team.",
                    10f);
                StopGamemode();
                return;
            }

            if (_Descriptor.CounterTerroristSpawnPoints.Count < 5 || _Descriptor.CounterTerroristSpawnPoints.Count < 5)
            {
                Notify("Map does not support game mode!",
                    "The current map does not have a minimum of 10 Spawn Points for 5vs5.",
                    10f);
                StopGamemode();
                return;
            }

            _UITimer = new Timer();
            _UITimer.AutoReset = true;
            _UITimer.Interval = 1000;
            _UITimer.Elapsed += (a, b) => UpdateUITimer();

#pragma warning disable CS0472
            if (_Descriptor.DefendingTeam == null)
#pragma warning restore CS0472
            {
                _Descriptor.DefendingTeam = Fusion5vs5GamemodeDescriptor.Defaults.DefendingTeam;
            }

            _DefendingTeam = _Descriptor.DefendingTeam;

            List<SpawnPointRepresentation> counterTerroristSpawnPoints = new List<SpawnPointRepresentation>();
            List<SpawnPointRepresentation> terroristSpawnPoints = new List<SpawnPointRepresentation>();
            foreach (var spawnPoint in _Descriptor.CounterTerroristSpawnPoints)
            {
                SpawnPointRepresentation t = new SpawnPointRepresentation();
                t.position = spawnPoint.position;
                t.eulerAngles = spawnPoint.eulerAngles;
                counterTerroristSpawnPoints.Add(t);
            }

            foreach (var spawnPoint in _Descriptor.TerroristSpawnPoints)
            {
                SpawnPointRepresentation t = new SpawnPointRepresentation();
                t.position = spawnPoint.position;
                t.eulerAngles = spawnPoint.eulerAngles;
                terroristSpawnPoints.Add(t);
            }

            if (NetworkInfo.IsServer)
            {
                Server = new Server.Server(
                    new ServerOperationsImpl(this),
                    _DefendingTeam,
                    counterTerroristSpawnPoints,
                    terroristSpawnPoints,
                    MaxRounds,
                    HalfTimeEnabled,
                    PreventNewJoins,
                    AllowAvatarChanging,
                    TimeLimits
                );
            }

            _LastLocalAvatar = RigData.GetAvatarBarcode();

            FusionPlayer.SetPlayerVitality(1.0f);

            SceneStreamer.Reload();
        }

        protected override void OnStopGamemode()
        {
            Log();
            base.OnStopGamemode();
            MelonLogger.Msg("5vs5 Mode: OnStopGamemode Called.");

            UnFreeze();

            if (NetworkInfo.IsServer && Server != null)
            {
                Server.Dispose();
            }

            _Menu.Elements.Insert(0, _EnableHalfTimeSetting);
            _Menu.Elements.Insert(0, _MaxRoundsSetting);

            try
            {
                GameObject.Destroy(_Descriptor.TerroristBuyZone.gameObject.GetComponent<TriggerLasers>());
                GameObject.Destroy(_Descriptor.CounterTerroristBuyZone.gameObject.GetComponent<TriggerLasers>());
            }
            catch
            {
                // ignored
            }

            TriggerLasersEvents.OnTriggerEntered -= OnTriggerEntered;
            TriggerLasersEvents.OnTriggerExited -= OnTriggerExited;

            BuyMenu.OnBuyMenuItemClicked -= OnBuyMenuItemClicked;

            TeamSelectionMenu.RemoveTeamsMenu();
            TeamSelectionMenu.OnDefendersSelected -= RequestJoinDefenders;
            TeamSelectionMenu.OnAttackersSelected -= RequestJoinAttackers;
            TeamSelectionMenu.OnSpectatorsSelected -= RequestJoinSpectator;

            FusionPlayer.ClearPlayerVitality();

            _Descriptor = null;
            _CounterTerroristTeamName = null;
            _TerroristTeamName = null;
            _LocalPlayerVelocity = 0;
            _LastLocalAvatar = null;
            _State = GameStates.Unknown;

            _DebugText = null;
            Dump();
        }

        private void OnFusion5vs5Started()
        {
            if (_Debug)
            {
                if (FusionSceneManager.Level._barcode.Equals("Snek.csoffice.Level.Csoffice"))
                {
                    _DebugText = GameObject.Find("debugText");
                }
            }

            foreach (var firstComponent in Fusion5vs5GamemodeDescriptor.Cache.Components)
            {
                _Descriptor = firstComponent;
                break;
            }

            if (String.IsNullOrEmpty(_Descriptor.CounterTerroristTeamName))
            {
                _Descriptor.CounterTerroristTeamName = Fusion5vs5GamemodeDescriptor.Defaults.CounterTerroristTeamName;
            }

            if (String.IsNullOrEmpty(_Descriptor.TerroristTeamName))
            {
                _Descriptor.TerroristTeamName = Fusion5vs5GamemodeDescriptor.Defaults.TerroristTeamName;
            }

            if (_Descriptor.DefaultAvatar == null)
            {
                _Descriptor.DefaultAvatar = Fusion5vs5GamemodeDescriptor.Defaults.DefaultAvatar;
            }

            _CounterTerroristTeamName = _Descriptor.CounterTerroristTeamName;
            _TerroristTeamName = _Descriptor.TerroristTeamName;

            TriggerLasers CTBuyZoneTrigger =
                _Descriptor.CounterTerroristBuyZone.gameObject.AddComponent<TriggerLasers>();
            CTBuyZoneTrigger.LayerFilter = 134217728;
            CTBuyZoneTrigger.onlyTriggerOnPlayer = true;

            TriggerLasers TBuyZoneTrigger = _Descriptor.TerroristBuyZone.gameObject.AddComponent<TriggerLasers>();
            TBuyZoneTrigger.LayerFilter = 134217728;
            TBuyZoneTrigger.onlyTriggerOnPlayer = true;

            TriggerLasersEvents.OnTriggerEntered += OnTriggerEntered;
            TriggerLasersEvents.OnTriggerExited += OnTriggerExited;

            BuyMenu.OnBuyMenuItemClicked += OnBuyMenuItemClicked;

            TeamSelectionMenu.AddTeamsMenu();
            TeamSelectionMenu.OnDefendersSelected += RequestJoinDefenders;
            TeamSelectionMenu.OnAttackersSelected += RequestJoinAttackers;
            TeamSelectionMenu.OnSpectatorsSelected += RequestJoinSpectator;
        }

        protected override void OnMetadataChanged(string key, string value)
        {
            Log(key, value);
            base.OnMetadataChanged(key, value);

            if (_Debug)
            {
                MelonLogger.Msg($"5vs5: OnMetadataChanged called: {key} {value}");
                UpdateDebugText();
            }

            if (key.StartsWith(Commons.Metadata.TeamKey))
            {
                string _player = key.Split('.')[2];
                PlayerId player = GetPlayerFromValue(_player);
                Fusion5vs5GamemodeTeams _team = GetTeamFromValue(value);
                TeamRepresentation team = GetTeamRepresentation(_team);
                OnTeamChanged(player, team);
            }
            else if (key.Equals(Commons.Metadata.RoundNumberKey))
            {
                int newScore = int.Parse(value);
                OnRoundNumberChanged(newScore);
            }
        }

        protected override void OnEventTriggered(string eventName)
        {
            Log(eventName);
            if (eventName.StartsWith(Events.PlayerKilledPlayer))
            {
                string _killer = eventName.Split('.')[1];
                string _killed = eventName.Split('.')[2];
                PlayerId killer = GetPlayerFromValue(_killer);
                PlayerId killed = GetPlayerFromValue(_killed);
                killer.TryGetDisplayName(out string killerName);
                killed.TryGetDisplayName(out string killedName);
                if (killed.IsSelf && _State != GameStates.Warmup)
                {
                    RigManager rm = RigData.RigReferences.RigManager;
                    SetFusionSpawnPoint(rm.physicsRig.m_pelvis);
                }

                SDKIntegration.InvokePlayerKilledAnotherPlayer(killerName, killedName, killed.IsSelf);
            }
            else if (eventName.StartsWith(Events.PlayerSuicide))
            {
                string _player = eventName.Split('.')[1];
                PlayerId player = GetPlayerFromValue(_player);
                player.TryGetDisplayName(out string playerName);
                if (player.IsSelf && _State != GameStates.Warmup)
                {
                    RigManager rm = RigData.RigReferences.RigManager;
                    SetFusionSpawnPoint(rm.physicsRig.m_pelvis);
                }

                SDKIntegration.InvokePlayerSuicide(playerName, player.IsSelf);
            }
            else if (eventName.StartsWith(Events.KillPlayer))
            {
                string _player = eventName.Split('.')[1];
                PlayerId player = GetPlayerFromValue(_player);

                if (player.IsSelf)
                {
                    KillPlayer();
                }
            }
            else if (eventName.StartsWith(Events.RevivePlayer))
            {
                string _player = eventName.Split('.')[1];
                PlayerId player = GetPlayerFromValue(_player);
                if (player.IsSelf)
                {
                    RevivePlayer();
                }
            }
            else if (eventName.StartsWith(Events.RespawnPlayer))
            {
                string _player = eventName.Split('.')[1];
                PlayerId player = GetPlayerFromValue(_player);
                if (player.IsSelf)
                {
                    RespawnPlayer();
                }
            }
            else if (eventName.StartsWith(Events.SetSpectator))
            {
                string _player = eventName.Split('.')[1];
                PlayerId player = GetPlayerFromValue(_player);
                if (player.IsSelf)
                {
                    SetSpectator();
                }
            }
            else if (eventName.StartsWith(Events.Freeze))
            {
                string _player = eventName.Split('.')[1];
                PlayerId player = GetPlayerFromValue(_player);
                if (player.IsSelf)
                {
                    Freeze();
                }
            }
            else if (eventName.StartsWith(Events.UnFreeze))
            {
                string _player = eventName.Split('.')[1];
                PlayerId player = GetPlayerFromValue(_player);
                if (player.IsSelf)
                {
                    UnFreeze();
                }
            }
            else if (eventName.StartsWith(Events.TeamWonRound))
            {
                Fusion5vs5GamemodeTeams _team = GetTeamFromValue(eventName.Split('.')[1]);
                TeamRepresentation team = GetTeamRepresentation(_team);
                OnTeamWonRound(team);
            }
            else if (eventName.StartsWith(Events.TeamWonGame))
            {
                Fusion5vs5GamemodeTeams _team = GetTeamFromValue(eventName.Split('.')[1]);
                TeamRepresentation team = GetTeamRepresentation(_team);
                OnTeamWonGame(team);
            }
            else if (eventName.Equals(Events.GameTie))
            {
                OnGameTie();
            }
            else if (eventName.Equals(Events.Fusion5vs5Started))
            {
                OnFusion5vs5Started();
            }
            else if (eventName.Equals(Events.Fusion5vs5Aborted))
            {
                StopGamemode();
            }
            else if (eventName.Equals(Events.Fusion5vs5Over))
            {
                StopGamemode();
            }
            else if (eventName.StartsWith(Events.NewGameState))
            {
                string _newState = eventName.Split('.')[1];
                GameStates newState = (GameStates)Enum.Parse(typeof(GameStates), _newState);
                OnStateChanged(newState);
            }
            else if (eventName.StartsWith(Events.PlayerLeft))
            {
                string _player = eventName.Split('.')[1];
                string _team = eventName.Split('.')[2];
                PlayerId player = GetPlayerFromValue(_player);
                Fusion5vs5GamemodeTeams team = GetTeamFromValue(_team);

                OnTeamRemoved(player, team);
            }
            else if (eventName.StartsWith(Events.PlayerSpectates))
            {
                string _player = eventName.Split('.')[1];
                PlayerId player = GetPlayerFromValue(_player);
                if (player.IsSelf)
                {
                    SetSpectator();
                    Notify("Joined Spectators", "You can join a team from <UI component>"); // TODO 
                }
            }
            else if (eventName.StartsWith(Events.SpawnPointAssigned))
            {
                string _player = eventName.Split('.')[1];
                PlayerId player = GetPlayerFromValue(_player);
                if (player.IsSelf)
                {
                    Regex regex = new Regex(Regex.Escape($"SpawnPointAssigned.{_player}."));
                    string _transform = regex.Replace(eventName, "", 1);
                    string[] split = _transform.Split(',');
                    Vector3 pos = new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]));
                    Vector3 rot = new Vector3(float.Parse(split[3]), float.Parse(split[4]), float.Parse(split[5]));
                    SpawnPointRepresentation spawnPoint = new SpawnPointRepresentation();
                    spawnPoint.position = pos;
                    spawnPoint.eulerAngles = rot;
                    _LocalSpawnPoint = new SpawnPointRepresentation
                        { position = spawnPoint.position, eulerAngles = spawnPoint.eulerAngles };

                    SetFusionSpawnPoint(pos, rot);
                }
            }

            UpdateDebugText(eventName);
        }

        protected override void OnUpdate()
        {
            if (_Debug)
            {
                _DebugTextUpdateTimePassed += Time.deltaTime;
                if (_DebugTextUpdateTimePassed >= 10)
                {
                    _DebugTextUpdateTimePassed -= 10;
                    UpdateDebugText();
                }
            }
        }

        private void OnStateChanged(GameStates state)
        {
            MelonLogger.Msg($"New game state {state}.");

            _UITimer.Stop();
            _ElapsedTime = 0;
            _CurrentTimeLimit = TimeLimits[state];
            _UITimer.Start();

            _State = state;

            switch (_State)
            {
                case GameStates.Warmup:
                    StartWarmupPhase();
                    break;
                case GameStates.BuyPhase:
                    StartBuyPhase();
                    break;
                case GameStates.PlayPhase:
                    StartPlayPhase();
                    break;
                case GameStates.RoundEndPhase:
                    StartRoundEndPhase();
                    break;
                case GameStates.MatchHalfPhase:
                    StartMatchHalfPhase();
                    break;
                case GameStates.MatchEndPhase:
                    StartMatchEndPhase();
                    break;
                default:
                    MelonLogger.Warning($"Could not execute the state {state}!");
                    break;
            }

            Log(_State);
        }

        private void StartWarmupPhase()
        {
            Log();
            // TODO decide how to implement team switching
            Notify("Warmup begun", "You're a spectator. Select a team from UI component", 10f);

            SDKIntegration.InvokeWarmupPhaseStarted();
        }

        private void StartBuyPhase()
        {
            Log();
            // TODO decide how to implement weapon buying
            Notify("Buy Phase", "Buy weapons from <UI component>");

            SDKIntegration.InvokeBuyPhaseStarted();
        }

        private void StartPlayPhase()
        {
            Log();

            if (_LocalTeam == _DefendingTeam)
            {
                // TODO change notification
                Notify("Round start", "Do defending team stuff...");
            }
            else
            {
                Notify("Round start", "Do attacking team stuff...");
            }

            SDKIntegration.InvokePlayPhaseStarted();
        }

        private void StartRoundEndPhase()
        {
            Log();

            SDKIntegration.InvokeRoundEndPhaseStarted();
        }

        private void StartMatchHalfPhase()
        {
            Log();

            Freeze();

            Fusion5vs5GamemodeTeams team = _LocalTeam == Fusion5vs5GamemodeTeams.Terrorists
                ? Fusion5vs5GamemodeTeams.CounterTerrorists
                : Fusion5vs5GamemodeTeams.Terrorists;
            Notify("Switching sides", $"Switching to team {GetTeamDisplayName(team)}.");

            OnSwapTeams();

            SDKIntegration.InvokeMatchHalfPhaseStarted();
        }

        private void StartMatchEndPhase()
        {
            Log();

            Freeze();

            SDKIntegration.InvokeMatchEndPhaseStarted();
        }

        private void RequestJoinDefenders()
        {
            Log();
            Fusion5vs5GamemodeTeams team = _DefendingTeam;
            RequestJoinTeam(team);
        }

        private void RequestJoinAttackers()
        {
            Log();
            Fusion5vs5GamemodeTeams team =
                _DefendingTeam == Fusion5vs5GamemodeTeams.Terrorists
                    ? Fusion5vs5GamemodeTeams.CounterTerrorists
                    : Fusion5vs5GamemodeTeams.Terrorists;
            RequestJoinTeam(team);
        }

        private void RequestJoinTeam(Fusion5vs5GamemodeTeams team)
        {
            Log(team);
            PlayerId player = PlayerIdManager.LocalId;
            string request = $"{ClientRequest.ChangeTeams}.{player?.LongId}.{team.ToString()}";
            RequestToServer(request);
        }

        private void RequestJoinSpectator()
        {
            Log();
            PlayerId player = PlayerIdManager.LocalId;
            string request = $"{ClientRequest.JoinSpectator}.{player?.LongId}";
            RequestToServer(request);
        }

        private void OnTeamChanged(PlayerId player, TeamRepresentation team)
        {
            Log(player, team);
            if (player.IsSelf)
            {
                _LocalTeam = team.Team;
                Notify($"Joined {team.DisplayName}", "");
            }

            // TODO Implement UI changes

            player.TryGetDisplayName(out string name);
            if (team.Team == Fusion5vs5GamemodeTeams.CounterTerrorists)
            {
                SDKIntegration.InvokeCounterTerroristTeamJoined(name, player.IsSelf);
            }
            else
            {
                SDKIntegration.InvokeTerroristTeamJoined(name, player.IsSelf);
            }
        }

        private void OnTeamRemoved(PlayerId player, Fusion5vs5GamemodeTeams team)
        {
            Log(player, team);
            // TODO Implement UI changes
        }

        private void OnTeamWonRound(TeamRepresentation team)
        {
            Log(team);
            // TODO Implement UI changes
            Notify("Round over.", $"Team {team.DisplayName} wins.");

            Metadata.TryGetValue(GetTeamScoreKey(team.Team), out string value);
            int totalScore = int.Parse(value);
            if (team.Team == Fusion5vs5GamemodeTeams.CounterTerrorists)
            {
                SDKIntegration.InvokeCounterTerroristTeamScored(totalScore,
                    _LocalTeam == Fusion5vs5GamemodeTeams.CounterTerrorists);
            }
            else
            {
                SDKIntegration.InvokeTerroristTeamScored(totalScore, _LocalTeam == Fusion5vs5GamemodeTeams.Terrorists);
            }
        }

        private void OnTeamWonGame(TeamRepresentation team)
        {
            Log(team);
            Notify("Game over.", $"Team {team.DisplayName} wins.");
            // TODO implement UI
        }

        private void OnGameTie()
        {
            Log();
            Notify("Game over.", "Game tied.");
            // TODO implement UI
        }

        private void OnSwapTeams()
        {
            Log();
            // TODO Implement UI changes
        }

        private void OnRoundNumberChanged(int newScore)
        {
            Log(newScore);
            // TODO Implement UI changes

            SDKIntegration.InvokeNewRoundStarted(newScore);
        }

        private TeamRepresentation GetTeamRepresentation(Fusion5vs5GamemodeTeams team)
        {
            Log(team);
            return new TeamRepresentation
                { Team = team, DisplayName = GetTeamDisplayName(team) };
        }

        private string GetTeamDisplayName(Fusion5vs5GamemodeTeams team)
        {
            Log(team);
            if (team == Fusion5vs5GamemodeTeams.Terrorists)
            {
                return _TerroristTeamName;
            }

            return _CounterTerroristTeamName;
        }

        /// <summary>
        /// Spawns a ragdoll of the local player's avatar where they stand, without actually subtracting their score and
        /// enters the player into the spectating avatar, where they then are taken away their visibility and
        /// interactability, as described in <see cref="SetSpectator"/>. Typically called when player changes their team
        /// mid-round, outside of the buy phase or warmup phase.
        /// </summary>
        private void KillPlayer()
        {
            Log();
            SpawnRagdoll();
            SetSpectator();
        }

        /// <summary>
        /// Used to respawn the local dead player inside of their spawn point. Usually called once the BuyPhase begins.
        /// Dead players leave their spectator avatar, get placed inside the their spawn point and are given back
        /// their visibility to other players as well as their interactability with the world.
        /// </summary>
        private void RevivePlayer()
        {
            Log();
            // TODO give back interactability and visibility
            FusionPlayerExtended.worldInteractable = true;
            FusionPlayerExtended.canSendDamage = true;
            FusionPlayer.ClearAvatarOverride();
            RigManager rm = RigData.RigReferences.RigManager;
            rm.SwapAvatarCrate(_LastLocalAvatar, true);
            RespawnPlayer();
        }

        /// <summary>
        /// Enters the local player into the spectator avatar, where they're then taken away of their visibility from
        /// other players and their interactability with the world. Typically called by other methods such as
        /// <see cref="KillPlayer"/>, or after the player has left any team and joined the spectators.
        /// </summary>
        private void SetSpectator()
        {
            Log();
            // TODO remove interactability and visibility
            FusionPlayerExtended.worldInteractable = false;
            FusionPlayerExtended.canSendDamage = false;
            string avatarBarcode = RigData.GetAvatarBarcode();
            if (!avatarBarcode.Equals(SpectatorAvatar))
            {
                _LastLocalAvatar = avatarBarcode;
            }

            MelonLogger.Msg($"_LastLocalAvatar changed to {_LastLocalAvatar}.");
            FusionPlayer.SetAvatarOverride(SpectatorAvatar);
        }

        /// <summary>
        /// Simply teleports the local player into their team's spawnpoint. Typically used to re-position all players once
        /// the round ends and buy phase starts. Dead players however wont be respawned, they will be revived with
        /// <see cref="RevivePlayer"/>. Using <see cref="RespawnPlayer"/> with dead players leads to undefined behaviour.
        /// </summary>
        private void RespawnPlayer()
        {
            Log();
            FusionPlayer.Teleport(_LocalSpawnPoint.position, _LocalSpawnPoint.eulerAngles);
        }

        /// <summary>
        /// Uses the SwipezGamemodeLib to spawn a ragdoll of the local player's avatar where they stand and removes the
        /// ragdoll after three seconds.
        /// </summary>
        private void SpawnRagdoll()
        {
            RigManager rm = RigData.RigReferences.RigManager;
            Transform transform = rm.physicsRig.m_pelvis;
            SpawnManager.SpawnRagdoll(_LastLocalAvatar, transform.position, transform.rotation,
                (_rm) =>
                {
                    Timer despawnTimer = new Timer();
                    despawnTimer.Interval = 5000;
                    despawnTimer.AutoReset = false;
                    despawnTimer.Elapsed += (s, e) =>
                    {
                        GameObject.Destroy(_rm.gameObject);
                        despawnTimer.Dispose();
                    };
                    despawnTimer.Start();
                });
        }

        /// <summary>
        /// Uses LabFusion's API to set a new multiplayer spawn point using a <see cref="Transform"/>.
        /// </summary>
        /// <param name="transform"><see cref="Transform"/> object where the new spawn point will be</param>
        private void SetFusionSpawnPoint(Transform transform)
        {
            Log(transform);
            FusionPlayer.SetSpawnPoints(transform);
        }

        /// <summary>
        /// Uses LabFusion's API to set a new multiplayer spawn point using a <see cref="Vector3"/> as position and
        /// eulerAngles, spawns a new GameObject to get ahold of a <see cref="Transform"/> object and feeds that into
        /// <see cref="FusionPlayer.SetSpawnPoints"/>.
        /// </summary>
        /// <param name="pos"><see cref="Vector3"/> world space position of the new spawn point</param>
        /// <param name="rot"><see cref="Vector3"/> eulerAngles of the new spawn point</param>
        private void SetFusionSpawnPoint(Vector3 pos, Vector3 rot)
        {
            Log(pos, rot);
            GameObject go = GameObject.Find("Fusion 5vs5 Spawn Point");
            if (go == null)
            {
                go = new GameObject("Fusion 5vs5 Spawn Point");
            }

            go.transform.position = pos;
            go.transform.localEulerAngles = rot;
            FusionPlayer.SetSpawnPoints(go.transform);
        }

        private void Freeze()
        {
            Log();
            if (!_LocalPlayerFrozen)
            {
                RemapRig rig = RigData.RigReferences.RigManager.remapHeptaRig;
                rig.jumpEnabled = false;
                _LocalPlayerVelocity = rig.maxVelocity;
                rig.maxVelocity = 0.001f;

                _LocalPlayerFrozen = true;
            }
        }

        private void UnFreeze()
        {
            Log();
            RemapRig rig = RigData.RigReferences.RigManager.remapHeptaRig;
            rig.jumpEnabled = true;
            rig.maxVelocity = _LocalPlayerVelocity;

            _LocalPlayerFrozen = false;
        }


        private void OnBuyMenuItemClicked(string barcode)
        {
            RequestToServer($"{ClientRequest.BuyItem}.{PlayerIdManager.LocalId.LongId}.{barcode}");
        }

        private void OnBuyZoneEntered()
        {
            Log();
            MelonLogger.Msg("Buy Zone entered.");
            BuyMenu.AddBuyMenu();
            RequestToServer($"{ClientRequest.BuyZoneEntered}.{PlayerIdManager.LocalId.LongId}");
        }

        private void OnBuyZoneExited()
        {
            Log();
            MelonLogger.Msg("Buy Zone exited.");
            BuyMenu.RemoveBuyMenu();
            RequestToServer($"{ClientRequest.BuyZoneExited}.{PlayerIdManager.LocalId.LongId}");
        }

        private void OnTriggerEntered(TriggerLasers obj)
        {
            Log(obj);

            TriggerLasers ctTrigger = _Descriptor.CounterTerroristBuyZone.gameObject.GetComponent<TriggerLasers>();
            TriggerLasers tTrigger = _Descriptor.TerroristBuyZone.gameObject.GetComponent<TriggerLasers>();
            if (ctTrigger != null && ctTrigger.GetInstanceID() == obj.GetInstanceID())
            {
                // Prevent a NullReferenceException inside of TriggerLasers component
                ctTrigger.obj_SpecificTrigger = ctTrigger.gameObject;
            }

            if (tTrigger != null && tTrigger.GetInstanceID() == obj.GetInstanceID())
            {
                tTrigger.obj_SpecificTrigger = tTrigger.gameObject;
            }

            if (_LocalTeam == Fusion5vs5GamemodeTeams.CounterTerrorists)
            {
                if (ctTrigger != null && ctTrigger.GetInstanceID() == obj.GetInstanceID())
                {
                    OnBuyZoneEntered();
                }
            }
            else if (_LocalTeam == Fusion5vs5GamemodeTeams.Terrorists)
            {
                if (tTrigger != null && tTrigger.GetInstanceID() == obj.GetInstanceID())
                {
                    tTrigger.obj_SpecificTrigger = _Descriptor.TerroristBuyZone.gameObject;
                    OnBuyZoneEntered();
                }
            }
        }

        private void OnTriggerExited(TriggerLasers obj)
        {
            Log(obj);
            if (_LocalTeam == Fusion5vs5GamemodeTeams.CounterTerrorists)
            {
                TriggerLasers trigger = _Descriptor.CounterTerroristBuyZone.gameObject.GetComponent<TriggerLasers>();
                if (trigger != null && trigger.GetInstanceID() == obj.GetInstanceID())
                {
                    OnBuyZoneExited();
                }
            }
            else
            {
                TriggerLasers trigger = _Descriptor.TerroristBuyZone.gameObject.GetComponent<TriggerLasers>();
                if (trigger != null && trigger.GetInstanceID() == obj.GetInstanceID())
                {
                    OnBuyZoneExited();
                }
            }
        }

        private void Notify(string header, string body, float popupLength = 2f)
        {
            Log(header, body, popupLength);
            FusionNotification notif = new FusionNotification()
            {
                title = header,
                showTitleOnPopup = true,
                message = body,
                isMenuItem = false,
                isPopup = true,
                popupLength = popupLength
            };
            FusionNotifier.Send(notif);
        }

        private void UpdateUITimer()
        {
            ++_ElapsedTime;

            UpdateDebugText();
        }

        private void UpdateDebugText()
        {
            if (_DebugText != null)
            {
                TextMeshProUGUI metadataText =
                    _DebugText.transform.Find("MetadataText").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI playerListText =
                    _DebugText.transform.Find("PlayerListText").GetComponent<TextMeshProUGUI>();

                StringBuilder sb = new StringBuilder();
                sb.Append("Metadata:\n");
                foreach (var kvPair in Metadata)
                {
                    sb.Append(kvPair.Key + "\t\t");
                    sb.Append(kvPair.Value + "\n");
                }

                metadataText.text = sb.ToString();

                sb = new StringBuilder();
                sb.Append("Player List:");

                int timeLeft = _CurrentTimeLimit - _ElapsedTime;
                sb.Append("\t");
                if (timeLeft >= 0)
                {
                    int minutes = timeLeft / 60;
                    int seconds = timeLeft - 60 * minutes;
                    string time = $"{minutes}:{seconds:00}";
                    sb.Append(time);
                }
                else
                {
                    sb.Append("0:00");
                }

                sb.Append("\n");
                foreach (var playerId in PlayerIdManager.PlayerIds)
                {
                    playerId.TryGetDisplayName(out string name);
                    sb.Append(name + "\t\t");
                    sb.Append(playerId.LongId + "\n");
                }

                playerListText.text = sb.ToString();
            }
        }

        private void UpdateDebugText(string eventTriggerName)
        {
            if (_DebugText == null)
            {
                return;
            }
            
            TextMeshProUGUI eventTriggerText =
                _DebugText.transform.Find("EventTriggerText").GetComponent<TextMeshProUGUI>();
            eventTriggerText.text = eventTriggerText.text + "\n" + eventTriggerName;
        }
    }
}