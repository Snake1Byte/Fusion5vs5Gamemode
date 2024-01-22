using BoneLib.BoneMenu.Elements;
using LabFusion.Representation;
using LabFusion.SDK.Gamemodes;
using LabFusion.Senders;
using LabFusion.Utilities;
using static Fusion5vs5Gamemode.Commons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using BoneLib;
using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using MelonLoader;
using UnityEngine;
using TMPro;
using SLZ.Marrow.SceneStreaming;

namespace Fusion5vs5Gamemode
{
    public class Fusion5vs5Gamemode : Gamemode, IServerOperations
    {
        public override string GamemodeCategory => "Snake1Byte's Gamemodes";

        public override string GamemodeName => "5 vs 5";

        public override bool PreventNewJoins => _PreventNewJoins;
        private bool _PreventNewJoins = false;

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

        // Internal
        public static Fusion5vs5Gamemode Instance { get; private set; }
        public Fusion5vs5Gamemode.Server _Server { get; private set; }

        private MenuCategory menu;
        private BoneMenuTeamSelection unconfirmedTeamSelection = BoneMenuTeamSelection.TerroristTeam;
        private EnumElement<BoneMenuTeamSelection> teamSelectionMenuElement;
        private FunctionElement confirmTeamSelectionMenuElement;
        private IntElement maxRoundsSetting;
        private BoolElement enableHalfTimeSetting;
        private BoolElement enableLateJoiningSetting;
        private BoolElement allowAvatarChangingSetting;

        public override void OnBoneMenuCreated(MenuCategory category)
        {
            base.OnBoneMenuCreated(category);

            menu = category;

            //TODO Add custom settings for this gamemode
            teamSelectionMenuElement = menu.CreateEnumElement("Choose Team", Color.white, unconfirmedTeamSelection,
                team => unconfirmedTeamSelection = team);

            confirmTeamSelectionMenuElement =
                menu.CreateFunctionElement("Confirm", Color.white, BoneMenuConfirmTeamChange);

            maxRoundsSetting = category.CreateIntElement("Maximum rounds", Color.white, 15, 1, 1, 1000000,
                i =>
                {
                    if (IsActive())
                        maxRoundsSetting.SetValue(_Server.MaxRounds);
                });

            enableHalfTimeSetting = category.CreateBoolElement("Enable Half-Time", Color.white, true, b =>
            {
                if (IsActive())
                {
                    enableHalfTimeSetting.SetValue(_Server.EnableHalftime);
                }
                else if (b)
                {
                    maxRoundsSetting.SetIncrement(2);
                    int maxRounds = maxRoundsSetting.GetValue();
                    if (maxRounds % 2 == 1)
                    {
                        maxRoundsSetting.SetValue(maxRounds + 1);
                    }
                }
                else
                {
                    maxRoundsSetting.SetIncrement(1);
                }
            });

            enableLateJoiningSetting = category.CreateBoolElement("Enable late joining", Color.white, true, b =>
            {
                if (IsActive())
                    enableLateJoiningSetting.SetValue(_Server.EnableLateJoining);
            });

            allowAvatarChangingSetting = category.CreateBoolElement("Allow avatar changing", Color.white, true, b =>
            {
                if (IsActive())
                    allowAvatarChangingSetting.SetValue(_Server.AllowAvatarChanging);
            });

            category.CreateBoolElement("Enable round music", Color.white, _EnableMusic, b => _EnableMusic = b);

            category.CreateBoolElement("Debug", Color.white, _Debug, e => _Debug = e);

            // Only show these while the game is running, until I add a better way to switch teams
            category.Elements.RemoveInstance(teamSelectionMenuElement);
            category.Elements.RemoveInstance(confirmTeamSelectionMenuElement);
        }


        public override void OnGamemodeRegistered()
        {
            base.OnGamemodeRegistered();
            MelonLogger.Msg("5vs5 Mode: OnGameModeRegistered Called.");
            Instance = this;
        }

        public override void OnGamemodeUnregistered()
        {
            base.OnGamemodeUnregistered();
            MelonLogger.Msg("5vs5 Mode: OnGameModeUnRegistered Called.");
            if (Instance == this)
            {
                Instance = null;
            }
        }

        protected override void OnStartGamemode()
        {
            base.OnStartGamemode();
            MelonLogger.Msg("5vs5 Mode: OnStartGamemode Called.");

            _PreventNewJoins = !enableLateJoiningSetting.GetValue();
            if (NetworkInfo.IsServer)
            {
                _Server = new Server(
                    this,
                    maxRoundsSetting.GetValue(),
                    enableHalfTimeSetting.GetValue(),
                    _PreventNewJoins,
                    allowAvatarChangingSetting.GetValue()
                );
            }

            menu.Elements.Insert(0, confirmTeamSelectionMenuElement);
            menu.Elements.Insert(0, teamSelectionMenuElement);
            menu.Elements.RemoveInstance(enableHalfTimeSetting);
            menu.Elements.RemoveInstance(maxRoundsSetting);

            FusionPlayer.SetPlayerVitality(1.0f);
            SceneStreamer.Reload();
        }

        protected override void OnStopGamemode()
        {
            base.OnStopGamemode();
            MelonLogger.Msg("5vs5 Mode: OnStopGamemode Called.");

            if (NetworkInfo.IsServer)
            {
                _Server.Dispose();
            }

            menu.Elements.RemoveInstance(teamSelectionMenuElement);
            menu.Elements.RemoveInstance(confirmTeamSelectionMenuElement);
            menu.Elements.Insert(0, enableHalfTimeSetting);
            menu.Elements.Insert(0, maxRoundsSetting);

            FusionPlayer.ClearPlayerVitality();
            _DebugText = null;
        }

        protected override void OnMetadataChanged(string key, string value)
        {
            base.OnMetadataChanged(key, value);

            if (_Debug)
            {
                MelonLogger.Msg($"5vs5: OnMetadataChanged called: {key} {value}");
                UpdateDebugText();
            }

            if (key.StartsWith(Commons.Metadata.TeamNameKey))
            {
                string teamId = key.Split('.')[3];
                TeamRepresentation rep = new TeamRepresentation { TeamId = teamId, DisplayName = value };
                OnTeamNameChanged(rep);
            }

            if (key.StartsWith(Commons.Metadata.TeamKey))
            {
                string _player = key.Split('.')[2];
                PlayerId player = GetPlayerFromValue(_player);
                OnTeamChanged(player, value);
            }

            // TODO decide later whether this can just be an event trigger instead of a saved value
            if (key.Equals(Commons.Metadata.GameStateKey))
            {
                OnStateChanged((Fusion5vs5GameStates)int.Parse(value));
            }

            if (key.StartsWith(Commons.Metadata.PlayerDeathsKey))
            {
                string _player = key.Split('.')[2];
                int _value = int.Parse(value);
                // Make sure we didnt send this metadata to reset the values to 0 (game start)
                if (_value > 0)
                {
                    PlayerId player = GetPlayerFromValue(_player);

                    DetermineTeamWon(player);
                }
            }
        }

        protected override void OnEventTriggered(string eventName)
        {
            if (eventName.StartsWith(Events.KillPlayer))
            {
                string _player = eventName.Split('.')[1];
                PlayerId player = GetPlayerFromValue(_player);

                KillPlayer(player);
            }
            else if (eventName.StartsWith(Events.RevivePlayer))
            {
                string _player = eventName.Split('.')[1];
                PlayerId player = GetPlayerFromValue(_player);

                RevivePlayer(player);
            }
            else if (eventName.Equals(Events.Fusion5vs5Started))
            {
                if (_Debug)
                {
                    if (FusionSceneManager.Level._barcode.Equals("Snek.csoffice.Level.Csoffice"))
                    {
                        _DebugText = GameObject.Find("debugText");
                    }
                }
            }
            else if (eventName.Equals(Commons.Events.Fusion5vs5Aborted))
            {
                StopGamemode();
            }
            else if (eventName.Equals(Commons.Events.Fusion5vs5Over))
            {
                StopGamemode();
            }
        }

        protected override void OnUpdate()
        {
            if (_Debug)
            {
                _DebugTextUpdateTimePassed += Time.deltaTime;
                if (_DebugTextUpdateTimePassed >= 1)
                {
                    _DebugTextUpdateTimePassed -= 1;
                    UpdateDebugText();
                }
            }
        }

        private void OnTeamChanged(PlayerId player, string teamId)
        {
            // TODO Implement UI changes
        }

        private void OnTeamNameChanged(TeamRepresentation rep)
        {
            // TODO Implement UI changes
        }

        // TODO decide later whether we wanna let the client know about states or instead just invoke triggers
        private void OnStateChanged(Fusion5vs5GameStates state)
        {
            MelonLogger.Msg($"New game state {state}.");

            switch (state)
            {
                case Fusion5vs5GameStates.Warmup:
                    StartWarmupPhase();
                    break;
                case Fusion5vs5GameStates.BuyPhase:
                    StartBuyPhase();
                    break;
                case Fusion5vs5GameStates.PlayPhase:
                    StartPlayPhase();
                    break;
                case Fusion5vs5GameStates.RoundEndPhase:
                    StartRoundEndPhase();
                    break;
                case Fusion5vs5GameStates.MatchHalfPhase:
                    StartMatchHalfPhase();
                    break;
                case Fusion5vs5GameStates.MatchEndPhase:
                    StartMatchEndPhase();
                    break;
                default:
                    MelonLogger.Warning($"Could not execute the state {state}!");
                    break;
            }
        }

        private void StartMatchEndPhase(Team totalWinner)
        {
            Notify("Match over", $"{totalWinner.TeamName} wins the match.");
        }

        private void StartMatchHalfPhase(Team switchedTo)
        {
            Notify("Switching sides", $"Switching to {switchedTo.TeamName}.");
        }

        private void StartRoundEndPhase(Team winner)
        {
            Notify("Round over", $"{winner.TeamName} wins.");
        }

        private void StartPlayPhase()
        {
            Team team = GetTeam(PlayerIdManager.LocalId);
            if (team != null && team.Equals(CounterTerroristTeam))
            {
                // TODO change notification
                Notify("Round start", "Do counter terrorist stuff...");
            }
            else if (team.Equals(TerroristTeam))
            {
                Notify("Round start", "Do terrorist stuff...");
            }
        }

        private void StartBuyPhase()
        {
            IncrementRoundNumber();
            foreach (var team in Teams)
            {
                foreach (var player in team.Players)
                {
                    RevivePlayer(player);
                }
            }

            // TODO decide how to implement weapon buying
            Notify("Buy Phase", "Buy weapons from <UI component>");
        }

        private void RevivePlayer(PlayerId player)
        {
            SetPlayerState(player, Server.PlayerStates.Alive);
            // TODO change avatar from spectator avatar, place player in spawn, give back interactability and visibility
        }


        private void KillPlayer(PlayerId player)
        {
            SetPlayerState(player, Server.PlayerStates.Dead);
            // TODO simply spawn ragdoll where the player avatar is without actually killing player
            SetSpectator(player);
        }

        private void SetSpectator(PlayerId player)
        {
            // TODO change to spectator avatar, remove interactibility and visibility
        }

        private void StartWarmupPhase()
        {
            // TODO decide how to implement team switching
            Notify("Warmup begun", "Select a team from <UI component>");
        }

        // All dead, all dead...
        private void DetermineTeamWon(PlayerId killed)
        {
            Team losingTeam = GetTeam(killed);
            foreach (var player in losingTeam.Players)
            {
                if (GetPlayerState(player) == Server.PlayerStates.Alive)
                {
                    return;
                }
            }

            Team winnerTeam = losingTeam.Equals(CounterTerroristTeam) ? TerroristTeam : CounterTerroristTeam;
            TeamWonRound(winnerTeam);
        }

        private void BoneMenuConfirmTeamChange()
        {
            PlayerId player = PlayerIdManager.LocalId;
            Team selectedTeam = unconfirmedTeamSelection == TeamRepresentation.CounterTerroristTeam
                ? CounterTerroristTeam
                : TerroristTeam;
            ChangeTeam(player, selectedTeam);
        }

        private void Notify(string header, string body)
        {
            FusionNotification notif = new FusionNotification()
            {
                title = header,
                showTitleOnPopup = true,
                message = body,
                isMenuItem = false,
                isPopup = true,
            };
            FusionNotifier.Send(notif);
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
                foreach (var kvPair in Metadata)
                {
                    sb.Append(kvPair.Key + "\t\t");
                    sb.Append(kvPair.Value + "\n");
                }

                metadataText.text = sb.ToString();

                sb = new StringBuilder();
                foreach (var playerId in PlayerIdManager.PlayerIds)
                {
                    playerId.TryGetDisplayName(out string name);
                    sb.Append(name + "\t\t");
                    sb.Append(playerId.LongId + "\n");
                }

                playerListText.text = sb.ToString();
            }
        }

        public bool SetMetadata(string key, string value)
        {
            return TrySetMetadata(key, value);
        }

        public string GetMetadata(string key)
        {
            TryGetMetadata(key, out string value);
            return value;
        }

        public FusionDictionary<string, string> GetMetadata()
        {
            return Metadata;
        }

        public bool InvokeTrigger(string value)
        {
            return TryInvokeTrigger(value);
        }

        public struct TeamRepresentation
        {
            public string TeamId;
            public string DisplayName; // Name for the UI
        }

        public enum BoneMenuTeamSelection
        {
            CounterTerroristTeam = 0,
            TerroristTeam = 1
        }

        public class Server : IDisposable
        {
            // Settings
            private IServerOperations Operations { get; }
            public int MaxRounds { get; }
            public bool EnableHalftime { get; }
            public bool EnableLateJoining { get; }
            public bool AllowAvatarChanging { get; }

            // Teams
            public string CounterTerroristTeamName { get; private set; } = "Sabrelake";
            public string TerroristTeamName { get; private set; } = "Lava Gang";
            private Team CounterTerroristTeam { get; }
            private Team TerroristTeam { get; }

            private readonly Team[] Teams;

            // For defusing game mode, this would be Counter Terrorist Team. For hostage, this would be Terrorist Team.
            public Team
                DefendingTeam { get; set; } // Will be set from the SDK with the Fusion5vs5Descriptor component

            // Avatars
            public string DefaultAvatarBarcode { get; set; } = BoneLib.CommonBarcodes.Avatars.FordBL;

            // States
            private Fusion5vs5GameStates _state = Fusion5vs5GameStates.Unknown;
            public Fusion5vs5GameStates State => _state;
            private Timer gameTimer;
            private int timeElapsed = 0;
            public Dictionary<Fusion5vs5GameStates, int> TimeLimits { get; }

            private Dictionary<PlayerId, PlayerStates> playerStatesDict;

            public Server(IServerOperations operations, int maxRounds, bool enableHalfTime, bool enableLateJoining,
                bool allowAvatarChanging)
            {
                Operations = operations;

                MaxRounds = maxRounds;
                EnableHalftime = enableHalfTime;
                EnableLateJoining = enableLateJoining;
                AllowAvatarChanging = allowAvatarChanging;

                MultiplayerHooking.OnPlayerJoin += OnPlayerJoin;
                MultiplayerHooking.OnPlayerLeave += OnPlayerLeave;
                MultiplayerHooking.OnPlayerAction += OnPlayerAction;
                MultiplayerHooking.OnMainSceneInitialized += On5vs5Loaded;

                // TODO set logos
                Teams = new Team[2];
                CounterTerroristTeam = new Team(UnityEngine.Random.RandomRangeInt(10000000, 100000000).ToString(), 5);
                TerroristTeam = new Team(UnityEngine.Random.RandomRangeInt(10000000, 100000000).ToString(), 5);
                CounterTerroristTeam.SetDisplayName(CounterTerroristTeamName);
                TerroristTeam.SetDisplayName(TerroristTeamName);
                Teams[0] = CounterTerroristTeam;
                Teams[1] = TerroristTeam;

                TimeLimits = new Dictionary<Fusion5vs5GameStates, int>();
                TimeLimits.Add(Fusion5vs5GameStates.Warmup, 60);
                TimeLimits.Add(Fusion5vs5GameStates.BuyPhase, 15);
                TimeLimits.Add(Fusion5vs5GameStates.PlayPhase, 135);
                TimeLimits.Add(Fusion5vs5GameStates.RoundEndPhase, 10);
                TimeLimits.Add(Fusion5vs5GameStates.MatchHalfPhase, 15);
                TimeLimits.Add(Fusion5vs5GameStates.MatchEndPhase, 30);
            }

            // Callbacks

            public void On5vs5Loaded()
            {
                MelonLogger.Msg(
                    $"Scene {FusionSceneManager.Level.Title} has been loaded for 5vs5 Gamemode. Barcode {FusionSceneManager.Level._barcode}. Debug {_Debug}");
                MultiplayerHooking.OnMainSceneInitialized -= On5vs5Loaded;
                MultiplayerHooking.OnLoadingBegin += On5vs5Aborted;

                SetTeamName(CounterTerroristTeam, CounterTerroristTeamName);
                SetTeamName(TerroristTeam, TerroristTeamName);

                foreach (var team in Teams)
                {
                    SetTeamScore(team, 0);
                }

                SetRoundNumber(0);

                foreach (PlayerId player in PlayerIdManager.PlayerIds)
                {
                    InitializePlayer(player);
                }

                StartStateMachine();

                Operations.InvokeTrigger(Commons.Events.Fusion5vs5Started);
            }

            public void On5vs5Aborted()
            {
                MelonLogger.Msg(
                    "5vs5 Mode: A different scene has been loaded while 5vs5 Gamemode was running. Aborting gamemode.");
                MultiplayerHooking.OnLoadingBegin -= On5vs5Aborted;
                Operations.InvokeTrigger(Commons.Events.Fusion5vs5Aborted);
            }

            private void OnPlayerJoin(PlayerId playerId)
            {
                MelonLogger.Msg("5vs5 Mode: OnPlayerJoin Called.");

                InitializePlayer(playerId);

                playerId.TryGetDisplayName(out string name);
                Notify($"{name} joined.", "");
            }

            private void OnPlayerLeave(PlayerId playerId)
            {
                MelonLogger.Msg("5vs5 Mode: OnPlayerLeave Called.");

                GetTeam(playerId).RemovePlayer(playerId);
                playerStatesDict.Remove(playerId);
            }

            private void OnPlayerAction(PlayerId playerId, PlayerActionType type, PlayerId otherPlayer)
            {
                try
                {
                    playerId.TryGetDisplayName(out string name1);
                    otherPlayer.TryGetDisplayName(out string name2);
                    name1 = name1 ?? "null";
                    name2 = name2 ?? "null";
                    MelonLogger.Msg($"5vs5 Mode: OnPlayerAction Called: {name1} {type.ToString()}: {name2}");
                }
                catch
                {
                    MelonLogger.Msg($"5vs5 Mode: OnPlayerAction Called: {type.ToString()}");
                }

                if (NetworkInfo.IsServer)
                {
                    if (type == PlayerActionType.DYING_BY_OTHER_PLAYER)
                    {
                        PlayerKilled(otherPlayer, playerId, null);
                    }

                    if (type == PlayerActionType.DYING)
                    {
                        Suicide(playerId, null);
                    }
                }
            }

            public void ClientRequested(string request)
            {
                if (request.StartsWith(ClientRequest.ChangeTeams))
                {
                    string[] info = request.Split('.');
                    PlayerId player = GetPlayerFromValue(info[1]);
                    Team team = GetTeamFromValue(Teams, info[2]);
                    TeamChangeRequested(player, team);
                }
            }

            // Team

            public void TeamChangeRequested(PlayerId player, Team selectedTeam)
            {
                if (player == null || selectedTeam == null)
                {
                    MelonLogger.Warning("TeamChangeRequested(): at least one argument was null.");
                    return;
                }

                player.TryGetDisplayName(out var playerName);
                Team currentTeam = GetTeam(player);
                if (currentTeam != selectedTeam)
                {
                    if (currentTeam != null)
                    {
                        currentTeam.RemovePlayer(player);
                    }

                    selectedTeam.AddPlayer(player);
                    Operations.SetMetadata(GetTeamMemberKey(player), selectedTeam.TeamName);
                    MelonLogger.Msg($"Player {playerName} switched teams to {selectedTeam.TeamName}");

                    if (_state == Fusion5vs5GameStates.Warmup || _state == Fusion5vs5GameStates.BuyPhase)
                    {
                        Operations.InvokeTrigger($"{Events.RevivePlayer}.{player.LongId}");
                    }
                    else if (_state == Fusion5vs5GameStates.PlayPhase || _state == Fusion5vs5GameStates.RoundEndPhase)
                    {
                        Operations.InvokeTrigger($"{Events.KillPlayer}.{player.LongId}");
                    }
                }
            }

            private Team GetTeam(PlayerId playerId)
            {
                foreach (var team in Teams)
                {
                    foreach (var player in team.Players)
                    {
                        if (player.LongId == playerId.LongId)
                        {
                            return team;
                        }
                    }
                }

                return null;
            }

            private void SetTeamName(Team team, string teamName)
            {
                Operations.SetMetadata(GetTeamNameKey(team), teamName);
                team.SetDisplayName(teamName);
            }

            private void TeamWonRound(Team team)
            {
                SetTeamScore(team, GetTeamScore(team) + 1);
                NextState();
            }

            private void SetTeamScore(Team team, int teamScore)
            {
                Operations.SetMetadata(GetTeamScoreKey(team), teamScore.ToString());
            }

            private int GetTeamScore(Team team)
            {
                string teamScore = Operations.GetMetadata(GetTeamScoreKey(team));
                return int.Parse(teamScore);
            }

            // Can be set from the SDK for each map
            public void ChangeCounterTerroristTeamName(string name)
            {
                CounterTerroristTeamName = name;
            }

            // Can be set from the SDK for each map
            public void ChangeTerroristTeamName(string name)
            {
                TerroristTeamName = name;
            }

            // Player

            private void InitializePlayer(PlayerId player)
            {
                SetPlayerKills(player, 0);
                SetPlayerDeaths(player, 0);
                SetPlayerAssists(player, 0);

                SetPlayerState(player, Server.PlayerStates.Spectator);
            }

            private void PlayerKilled(PlayerId killer, PlayerId killed, object weapon)
            {
                SetPlayerKills(killer, GetPlayerKills(Operations.GetMetadata(), killer) + 1);
                SetPlayerDeaths(killed, GetPlayerDeaths(Operations.GetMetadata(), killed) + 1);

                SetPlayerState(killed, PlayerStates.Dead);
            }

            private void Suicide(PlayerId playerId, object weapon)
            {
                SetPlayerDeaths(playerId, GetPlayerDeaths(Operations.GetMetadata(), playerId) + 1);
                SetPlayerState(playerId, PlayerStates.Dead);
            }

            private void SetPlayerKills(PlayerId killer, int kills)
            {
                Operations.SetMetadata(GetPlayerKillsKey(killer), kills.ToString());
            }

            private void SetPlayerDeaths(PlayerId killed, int deaths)
            {
                Operations.SetMetadata(GetPlayerDeathsKey(killed), deaths.ToString());
            }

            private void SetPlayerAssists(PlayerId assister, int assists)
            {
                Operations.SetMetadata(GetPlayerAssistsKey(assister), assists.ToString());
            }

            private int GetPlayerAssists(PlayerId assister)
            {
                string assistsScore = Operations.GetMetadata(GetPlayerAssistsKey(assister));
                return int.Parse(assistsScore);
            }

            private void SetPlayerState(PlayerId playerId, PlayerStates state)
            {
                playerStatesDict.Remove(playerId);
                playerStatesDict.Add(playerId, state);
            }

            private PlayerStates GetPlayerState(PlayerId player)
            {
                playerStatesDict.TryGetValue(player, out PlayerStates playerState);
                return playerState;
            }

            // Internal

            private void StartStateMachine()
            {
                gameTimer = new Timer();
                gameTimer.AutoReset = false;
                gameTimer.Elapsed += (sender, args) => TimeElapsed();
                NextState();
            }

            private void TimeElapsed()
            {
                if (NetworkInfo.IsServer)
                {
                    if (_state == Fusion5vs5GameStates.PlayPhase)
                    {
                        // defending team wins due to time running out
                        TeamWonRound(DefendingTeam);
                    }
                    else
                    {
                        NextState();
                    }
                }

                if (_state == Fusion5vs5GameStates.MatchEndPhase)
                {
                    Operations.InvokeTrigger(Commons.Events.Fusion5vs5Over);
                }
            }

            // When calling NextState() from anywhere but the timer's Elapsed event, call this as the last thing, after changing scores and states
            private void NextState()
            {
                // In case anyone else calls NextState(), stop the timer manually
                gameTimer.Stop();
                Fusion5vs5GameStates oldState = _state;
                if (NetworkInfo.IsServer)
                {
                    // We update the old state to the next one and dispatch it to everyone else
                    Fusion5vs5GameStates nextState = Fusion5vs5GameStates.Unknown;
                    switch (oldState)
                    {
                        case Fusion5vs5GameStates.Unknown:
                            nextState = Fusion5vs5GameStates.Warmup;
                            break;
                        case Fusion5vs5GameStates.Warmup:
                            nextState = Fusion5vs5GameStates.BuyPhase;
                            break;
                        case Fusion5vs5GameStates.BuyPhase:
                            nextState = Fusion5vs5GameStates.PlayPhase;
                            break;
                        case Fusion5vs5GameStates.PlayPhase:
                            if (IsRoundNumberMaxedOut() || IsTeamScoreMaxedOut())
                            {
                                nextState = Fusion5vs5GameStates.MatchEndPhase;
                            }
                            else if (EnableHalftime && HalfOfRoundsPlayed())
                            {
                                nextState = Fusion5vs5GameStates.MatchHalfPhase;
                            }
                            else
                            {
                                nextState = Fusion5vs5GameStates.RoundEndPhase;
                            }

                            break;
                        case Fusion5vs5GameStates.RoundEndPhase:
                            nextState = Fusion5vs5GameStates.BuyPhase;
                            break;
                        case Fusion5vs5GameStates.MatchHalfPhase:
                            nextState = Fusion5vs5GameStates.BuyPhase;
                            break;
                    }
                    
                    if (TimeLimits.TryGetValue(nextState, out int timeLimit))
                    {
                        gameTimer.Interval = timeLimit;
                        gameTimer.Start();
                    }
                    else
                    {
                        MelonLogger.Warning($"Could not find a time limit for {nextState}!");
                    }

                    _state = nextState;

                    Operations.SetMetadata(Commons.Metadata.GameStateKey, ((int)nextState).ToString());
                }
            }

            private void IncrementRoundNumber()
            {
                SetRoundNumber(GetRoundNumber(Operations.GetMetadata()) + 1);
            }

            private void SetRoundNumber(int i)
            {
                Operations.SetMetadata(Commons.Metadata.RoundNumberKey, i.ToString());
            }

            private bool HalfOfRoundsPlayed()
            {
                string _roundNumber = Operations.GetMetadata(Commons.Metadata.RoundNumberKey);
                int roundNumber = int.Parse(_roundNumber);
                return roundNumber == MaxRounds / 2;
            }

            private bool IsTeamScoreMaxedOut()
            {
                int maxTeamScore = (int)((float)MaxRounds / 2) + 1;

                foreach (var team in Teams)
                {
                    int score = GetTeamScore(team);
                    if (score >= maxTeamScore)
                    {
                        return true;
                    }
                }

                return false;
            }

            private bool IsRoundNumberMaxedOut()
            {
                TryGetMetadata(Commons.RoundNumberKey, out string _roundNumber);
                int roundNumber = int.Parse(_roundNumber);

                return roundNumber == MaxRounds;
            }

            public void Dispose()
            {
                MultiplayerHooking.OnPlayerJoin -= OnPlayerJoin;
                MultiplayerHooking.OnPlayerLeave -= OnPlayerLeave;
                MultiplayerHooking.OnPlayerAction -= OnPlayerAction;
                MultiplayerHooking.OnLoadingBegin -= On5vs5Aborted;

                foreach (var team in Teams)
                {
                    foreach (var player in team.Players)
                    {
                        team.RemovePlayer(player);
                    }
                }

                if (gameTimer != null)
                {
                    gameTimer.Stop();
                    try
                    {
                        gameTimer.Dispose();
                    }
                    catch
                    {
                        MelonLogger.Warning("Could not dispose game timer.");
                    }
                }
            }

            public enum PlayerStates
            {
                Spectator = 0,
                Alive = 1,
                Dead = 2
            }
        }
    }
}