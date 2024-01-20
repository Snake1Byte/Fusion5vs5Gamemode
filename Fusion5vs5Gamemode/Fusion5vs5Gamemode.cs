using BoneLib.BoneMenu.Elements;
using LabFusion.Representation;
using LabFusion.SDK.Gamemodes;
using LabFusion.Senders;
using LabFusion.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using LabFusion.Extensions;
using LabFusion.Network;
using MelonLoader;
using UnityEngine;
using TMPro;
using SLZ.Marrow.SceneStreaming;

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

        private float debugTextUpdateTimePassed = 0;
        private GameObject debugText = null;

        private bool debug = true;

        // Metadata
        public const string DefaultPrefix = "Fusion5vs5";
        public const string TeamKey = DefaultPrefix + ".Team";
        public const string TeamScoreKey = TeamKey + ".Score";

        public const string PlayerKillsKey = DefaultPrefix + ".Kills";
        public const string PlayerDeathsKey = DefaultPrefix + ".Deaths";
        public const string PlayerAssistsKey = DefaultPrefix + ".Assists";

        public const string GameStateKey = DefaultPrefix + ".State";
        public const string RoundNumberKey = DefaultPrefix + ".RoundNumber";

        // Music 
        public override bool MusicEnabled => enableMusic;
        private bool enableMusic = true;

        // Teams
        public const string CounterTerroristTeamName = "Sabrelake";
        public const string TerroristTeamName = "Lava Gang";
        private Team[] Teams = new Team[2];

        // Avatars
        private bool allowAvatarChanging = true;
        public string DefaultAvatarBarcode { get; set; } = BoneLib.CommonBarcodes.Avatars.FordBL;
        public override bool DisableManualUnragdoll => false;

        // Gamemode specific
        public int MaxRounds { get; set; } = 15;

        private bool _enableHalftime = false;
        public bool EnableHalftime => _enableHalftime;

        // Internal
        public static Fusion5vs5Gamemode Instance { get; private set; }

        public override bool AutoStopOnSceneLoad => false;

        private MenuCategory menu;
        private BoneMenuTeams unconfirmedTeamSelection = BoneMenuTeams.TerroristTeam;
        private EnumElement<BoneMenuTeams> teamSelectionMenuElement;
        private FunctionElement confirmTeamSelectionMenuElement;
        private IntElement maxRoundsSetting;

        private Fusion5vs5GameStates _state = Fusion5vs5GameStates.Unknown;
        public Fusion5vs5GameStates State => _state;

        private Timer gameTimer;
        private int timeEllapsed = 0;
        private Dictionary<Fusion5vs5GameStates, int> timeLimits;

        public override void OnBoneMenuCreated(MenuCategory category)
        {
            base.OnBoneMenuCreated(category);

            menu = category;

            //TODO Add custom settings for this gamemode

            maxRoundsSetting = category.CreateIntElement("Maximum rounds", Color.white, MaxRounds, 1, 1, 1000000,
                i =>
                {
                    if (IsActive())
                        MaxRounds = i;
                    else
                        maxRoundsSetting.SetValue(MaxRounds);
                });

            category.CreateBoolElement("Enable Half-Time", Color.white, _enableHalftime, b =>
            {
                SetHalftimeEnabled(b);
                if (b)
                {
                    int maxRounds = maxRoundsSetting.GetValue();
                    if (maxRounds % 2 == 1)
                    {
                        maxRoundsSetting.SetValue(maxRounds + 1);
                        MaxRounds = maxRounds + 1;
                    }
                }
            });

            category.CreateBoolElement("Enable late joining", Color.white, enableLateJoining, EnableLateJoining);

            category.CreateBoolElement("Allow avatar changing", Color.white, allowAvatarChanging, AllowAvatarChanging);

            category.CreateBoolElement("Enable round music", Color.white, enableMusic, EnableMusic);

            category.CreateBoolElement("Debug", Color.white, debug, e => { debug = e; });
        }

        public override void OnGamemodeRegistered()
        {
            base.OnGamemodeRegistered();

            MelonLogger.Msg("5vs5 Mode: OnGameModeRegistered Called.");
            Instance = this;

            Initialize();
        }

        public override void OnGamemodeUnregistered()
        {
            base.OnGamemodeUnregistered();

            MelonLogger.Msg("5vs5 Mode: OnGameModeUnRegistered Called.");
            base.OnGamemodeUnregistered();

            if (Instance == this)
            {
                Instance = null;
            }
        }

        protected override void OnStartGamemode()
        {
            base.OnStartGamemode();

            MultiplayerHooking.OnPlayerJoin += OnPlayerJoin;
            MultiplayerHooking.OnPlayerLeave += OnPlayerLeave;
            MultiplayerHooking.OnPlayerAction += OnPlayerAction;

            if (teamSelectionMenuElement == null || confirmTeamSelectionMenuElement == null)
            {
                teamSelectionMenuElement = menu.CreateEnumElement("Choose Team", Color.white, unconfirmedTeamSelection,
                    team => unconfirmedTeamSelection = team);
                confirmTeamSelectionMenuElement =
                    menu.CreateFunctionElement("Confirm", Color.white, BoneMenuConfirmTeamChange);
            }
            else
            {
                menu.Elements.Add(teamSelectionMenuElement);
                menu.Elements.Add(confirmTeamSelectionMenuElement);
            }

            MelonLogger.Msg("5vs5 Mode: OnStartGamemode Called.");
            MultiplayerHooking.OnMainSceneInitialized += OnFirstTimeLevelLoad;
            SceneStreamer.Reload();
            FusionPlayer.SetPlayerVitality(1.0f);
        }

        protected override void OnStopGamemode()
        {
            base.OnStopGamemode();
            MelonLogger.Msg("5vs5 Mode: OnStopGamemode Called.");

            MultiplayerHooking.OnPlayerJoin -= OnPlayerJoin;
            MultiplayerHooking.OnPlayerLeave -= OnPlayerLeave;
            MultiplayerHooking.OnPlayerAction -= OnPlayerAction;

            menu.Elements.RemoveInstance(teamSelectionMenuElement);
            menu.Elements.RemoveInstance(confirmTeamSelectionMenuElement);

            MultiplayerHooking.OnLoadingBegin -= OnLevelLoadedAgain;
            FusionPlayer.ClearPlayerVitality();

            foreach (var team in Teams)
            {
                foreach (var player in team.Players)
                {
                    team.RemovePlayer(player);
                }
            }

            debugText = null;

            _state = Fusion5vs5GameStates.Unknown;

            if (gameTimer != null)
            {
                gameTimer.Stop();
                try
                {
                    gameTimer.Dispose();
                }
                catch
                {
                    MelonLogger.Msg("Could not dispose gameTimer.");
                }
            }

            timeEllapsed = 0;
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

        private void OnPlayerLeave(PlayerId playerId)
        {
            MelonLogger.Msg("5vs5 Mode: OnPlayerLeave Called.");
        }

        private void OnPlayerJoin(PlayerId playerId)
        {
            MelonLogger.Msg("5vs5 Mode: OnPlayerJoin Called.");
        }

        protected override void OnMetadataChanged(string key, string value)
        {
            base.OnMetadataChanged(key, value);

            if (debug)
            {
                MelonLogger.Msg($"5vs5: OnMetadataChanged called: {key} {value}");
                UpdateDebugText();
            }

            if (key.StartsWith(TeamKey))
            {
                string player = key.Split('.')[2];
                ChangeTeamLocal(GetPlayerFromValue(player), GetTeamFromValue(value));
            }

            if (key.Equals(GameStateKey))
            {
                ChangeStateLocal((Fusion5vs5GameStates)int.Parse(value));
            }
        }

        protected override void OnUpdate()
        {
            if (debug)
            {
                debugTextUpdateTimePassed += Time.deltaTime;
                if (debugTextUpdateTimePassed >= 1)
                {
                    debugTextUpdateTimePassed -= 1;
                    UpdateDebugText();
                }
            }
        }

        private void Initialize()
        {
            Team counterTerroristTeam = new Team(CounterTerroristTeamName, 5);
            Team terroristTeam = new Team(TerroristTeamName, 5);
            // TODO set logos
            Teams[0] = counterTerroristTeam;
            Teams[1] = terroristTeam;
            timeLimits.Add(Fusion5vs5GameStates.Warmup, 60);
            timeLimits.Add(Fusion5vs5GameStates.BuyPhase, 15);
            timeLimits.Add(Fusion5vs5GameStates.PlayPhase, 135);
            timeLimits.Add(Fusion5vs5GameStates.RoundEndPhase, 10);
            timeLimits.Add(Fusion5vs5GameStates.MatchHalfPhase, 15);
            timeLimits.Add(Fusion5vs5GameStates.MatchEndPhase, 30);
        }

        private void TeamWonRound(Team team)
        {
            SetTeamScore(team, GetTeamScore(team) + 1);
        }

        private void SetTeamScore(Team team, int teamScore)
        {
            TrySetMetadata(GetTeamScoreKey(team), teamScore.ToString());
        }

        private int GetTeamScore(Team team)
        {
            TryGetMetadata(GetTeamScoreKey(team), out string teamScore);
            return int.Parse(teamScore);
        }

        private string GetTeamScoreKey(Team team)
        {
            return $"{TeamScoreKey}.{team?.TeamName}";
        }

        private void PlayerKilled(PlayerId killer, PlayerId killed, object weapon)
        {
            SetPlayerKills(killer, GetPlayerKills(killer) + 1);
            SetPlayerDeaths(killed, GetPlayerDeaths(killed) + 1);

            DetermineRoundEnd();
        }

        private void Suicide(PlayerId playerId, object weapon)
        {
            SetPlayerDeaths(playerId, GetPlayerDeaths(playerId) + 1);

            DetermineRoundEnd();
        }

        private void SetPlayerKills(PlayerId killer, int kills)
        {
            TrySetMetadata(GetPlayerKillsKey(killer), kills.ToString());
        }

        private int GetPlayerKills(PlayerId killer)
        {
            TryGetMetadata(GetPlayerKillsKey(killer), out string killerScore);
            return int.Parse(killerScore);
        }

        private string GetPlayerKillsKey(PlayerId killer)
        {
            return $"{PlayerKillsKey}.{killer?.LongId}";
        }

        private void SetPlayerDeaths(PlayerId killed, int deaths)
        {
            TrySetMetadata(GetPlayerDeathsKey(killed), deaths.ToString());
        }

        private int GetPlayerDeaths(PlayerId killed)
        {
            TryGetMetadata(GetPlayerDeathsKey(killed), out string deathScore);
            return int.Parse(deathScore);
        }

        private string GetPlayerDeathsKey(PlayerId killed)
        {
            return $"{PlayerDeathsKey}.{killed?.LongId}";
        }

        private void SetPlayerAssists(PlayerId assister, int assists)
        {
            TrySetMetadata(GetPlayerAssistsKey(assister), assists.ToString());
        }

        private int GetPlayerAssists(PlayerId assister)
        {
            TryGetMetadata(GetPlayerAssistsKey(assister), out string assistsScore);
            return int.Parse(assistsScore);
        }

        private string GetPlayerAssistsKey(PlayerId assister)
        {
            return $"{PlayerAssistsKey}.{assister?.LongId}";
        }

        private Team GetTeamFromValue(string value)
        {
            foreach (var team in Teams)
            {
                if (team.TeamName.Equals(value))
                {
                    return team;
                }
            }

            return null;
        }

        private PlayerId GetPlayerFromValue(string player)
        {
            ulong _playerId = ulong.Parse(player);
            foreach (var playerId in PlayerIdManager.PlayerIds)
            {
                if (playerId.LongId == _playerId)
                {
                    return playerId;
                }
            }

            return null;
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

        private bool ChangeTeamLocal(PlayerId player, Team selectedTeam)
        {
            if (player == null || selectedTeam == null)
            {
                MelonLogger.Warning("GetTeam(): at least one argument was null.");
                return false;
            }

            player.TryGetDisplayName(out var playerName);
            Team currentTeam = GetTeam(player);
            if (currentTeam != selectedTeam)
            {
                if (currentTeam != null)
                {
                    currentTeam.RemovePlayer(player);
                }
                else
                {
                    InitializePlayer(player);
                }

                selectedTeam.AddPlayer(player);

                MelonLogger.Msg($"Player {playerName} switched teams to {selectedTeam.TeamName}");
                return true;
            }

            return false;
        }

        private bool ChangeTeam(PlayerId player, Team selectedTeam)
        {
            if (ChangeTeamLocal(player, selectedTeam))
            {
                // Broadcast it to everyone else
                TrySetMetadata(GetTeamMemberKey(player), selectedTeam.TeamName);
                return true;
            }

            return false;
        }

        private void InitializePlayer(PlayerId player)
        {
            SetPlayerKills(player, 0);
            SetPlayerDeaths(player, 0);
            SetPlayerAssists(player, 0);
        }

        private string GetTeamMemberKey(PlayerId id)
        {
            return $"{TeamKey}.{id.LongId}";
        }

        public void OnFirstTimeLevelLoad()
        {
            MelonLogger.Msg(
                $"Scene {FusionSceneManager.Level.Title} has been loaded for 5vs5 Gamemode. Barcode {FusionSceneManager.Level._barcode}. Debug {debug}");
            MultiplayerHooking.OnMainSceneInitialized -= OnFirstTimeLevelLoad;
            MultiplayerHooking.OnLoadingBegin += OnLevelLoadedAgain;

            if (debug)
            {
                if (FusionSceneManager.Level._barcode.Equals("Snek.csoffice.Level.Csoffice"))
                {
                    debugText = GameObject.Find("debugText");
                }
            }

            foreach (var team in Teams)
            {
                SetTeamScore(team, 0);
            }

            TrySetMetadata(RoundNumberKey, "0");

            StartStateMachine();
        }

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
                    // terrorists win due to time running out
                    TeamWonRound(Teams[1]);
                }

                NextState();
            }

            if (_state == Fusion5vs5GameStates.MatchEndPhase)
            {
                StopGamemode();
            }
        }

        // When calling NextState() from anywhere but the timer's Elapsed event, call this as the last thing, after changing scores and states
        private void NextState()
        {
            // In case anyone else calls NextState(), stop the timer manually
            gameTimer.Stop();

            if (NetworkInfo.IsServer)
            {
                // We update the old state to the next one and dispatch it to everyone else
                Fusion5vs5GameStates nextState = Fusion5vs5GameStates.Unknown;
                switch (_state)
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

                        break;
                    case Fusion5vs5GameStates.RoundEndPhase:
                        nextState = Fusion5vs5GameStates.BuyPhase;
                        break;
                    case Fusion5vs5GameStates.MatchHalfPhase:
                        nextState = Fusion5vs5GameStates.BuyPhase;
                        break;
                }

                TrySetMetadata(GameStateKey, ((int)nextState).ToString());
            }
        }

        private void ChangeStateLocal(Fusion5vs5GameStates state)
        {
            if (_state == state)
            {
                MelonLogger.Warning($"Tried to change to same state {state}. Aborting.");
                return;
            }

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

            if (timeLimits.TryGetValue(state, out int timeLimit))
            {
                gameTimer.Interval = timeLimit;
                gameTimer.Start();
            }
            else
            {
                MelonLogger.Warning($"Could not find a time limit for {state}!");
            }

            _state = state;
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
            if (team.Equals(Teams[0]))
            {
                // TODO change notification
                Notify("Round start", "Do counter terrorist stuff...");
            }
            else if (team.Equals(Teams[1]))
            {
                Notify("Round start", "Do terrorist stuff...");
            }
        }

        private void StartBuyPhase()
        {
            // TODO decide how to implement weapon buying
            Notify("Buy Phase", "Buy weapons from <UI component>");
        }

        private void StartWarmupPhase()
        {
            // TODO decide how to implement team switching
            Notify("Warmup begun", "Select a team from <UI component>");
        }

        // All dead, all dead...
        private void DetermineRoundEnd(PlayerId killed)
        {
            Team team = GetTeam(killed);
            foreach (var player in team.Players)
            {
                PlayerStatesDict.TryGetValue(player, out PlayerState playerState);
                if (playerState == PlayerState.ALIVE)
                {
                    return;
                }
            }

            // TeamWonRound(());
        }

        public void OnLevelLoadedAgain()
        {
            MelonLogger.Msg(
                $"5vs5 Mode: A different scene has been loaded while 5vs5 Gamemode was running. Aborting gamemode.");
            MultiplayerHooking.OnLoadingBegin -= OnLevelLoadedAgain;
            StopGamemode();
        }

        private void BoneMenuConfirmTeamChange()
        {
            PlayerId player = PlayerIdManager.LocalId;
            Team selectedTeam = Teams[(int)unconfirmedTeamSelection];
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

        public void SetHalftimeEnabled(bool enable)
        {
            _enableHalftime = enable;
        }

        public void EnableLateJoining(bool enabled)
        {
            enableLateJoining = enabled;
        }

        public void AllowAvatarChanging(bool allow)
        {
            allowAvatarChanging = allow;
        }

        public void EnableMusic(bool enabled)
        {
            enableMusic = enabled;
        }

        private bool HalfOfRoundsPlayed()
        {
            TryGetMetadata(RoundNumberKey, out string _roundNumber);
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
            TryGetMetadata(RoundNumberKey, out string _roundNumber);
            int roundNumber = int.Parse(_roundNumber);

            return roundNumber == MaxRounds;
        }

        private void UpdateDebugText()
        {
            if (debugText != null)
            {
                TextMeshProUGUI metadataText = debugText.transform.Find("MetadataText").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI playerListText =
                    debugText.transform.Find("PlayerListText").GetComponent<TextMeshProUGUI>();

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

        public enum BoneMenuTeams
        {
            CounterTerroristTeam = 0,
            TerroristTeam = 1
        }

        public enum Fusion5vs5GameStates
        {
            Unknown = 0,
            Warmup = 1,
            BuyPhase = 2,
            PlayPhase = 3,
            RoundEndPhase = 4,
            MatchHalfPhase = 5,
            MatchEndPhase = 6
        }
    }
}