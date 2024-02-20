using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Fusion5vs5Gamemode.SDK;
using Fusion5vs5Gamemode.Shared;
using Fusion5vs5Gamemode.Utilities;
using Fusion5vs5Gamemode.Utilities.HarmonyPatches;
using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.SDK.Gamemodes;
using LabFusion.Senders;
using LabFusion.Utilities;
using MelonLoader;
using SLZ.Interaction;
using SLZ.Props.Weapons;
using SLZ.Rig;
using SLZ.SFX;
using UnityEngine;
using static Fusion5vs5Gamemode.Shared.Commons;

namespace Fusion5vs5Gamemode.Server
{
    public class Server : IDisposable
    {
        // Settings
        private IServerOperations Operations { get; }
        private int MaxRounds { get; }
        private bool EnableHalftime { get; }
        private bool EnableLateJoining { get; }
        private bool AllowAvatarChanging { get; }

        // Teams
        private Team CounterTerroristTeam { get; }
        private Team TerroristTeam { get; }
        private readonly Team[] _Teams;

        private Dictionary<PlayerId, SerializedTransform> _SpawnPoints =
            new Dictionary<PlayerId, SerializedTransform>();

        private readonly List<SerializedTransform> _CounterTerroristSpawnPoints;
        private readonly List<SerializedTransform> _TerroristSpawnPoints;

        // For defusing game mode, this would be Counter Terrorist Team. For hostage, this would be Terrorist Team.
        public Team
            DefendingTeam { get; } // Will be set from the SDK with the Fusion5vs5Descriptor component

        // States
        private Timer _GameTimer;
        private Dictionary<GameStates, int> TimeLimits { get; }
        private GameStates _State = GameStates.Unknown;
        private Timer _BuyTimer;
        private bool _IsBuyTime;

        private readonly Dictionary<PlayerId, PlayerStates> _PlayerStatesDict;
        private readonly List<PlayerId> _PlayersInBuyZone = new List<PlayerId>();

        public Server(IServerOperations operations,
            Fusion5vs5GamemodeTeams defendingTeam,
            List<SerializedTransform> counterTerroristSpawnPoints,
            List<SerializedTransform> terroristSpawnPoints,
            int maxRounds,
            bool enableHalfTime,
            bool enableLateJoining,
            bool allowAvatarChanging,
            Dictionary<GameStates, int> timeLimits)
        {
            Log(operations, defendingTeam, counterTerroristSpawnPoints, terroristSpawnPoints, maxRounds, enableHalfTime,
                enableLateJoining, allowAvatarChanging,
                timeLimits);
            Operations = operations;

            _CounterTerroristSpawnPoints = counterTerroristSpawnPoints;
            _TerroristSpawnPoints = terroristSpawnPoints;
            MaxRounds = maxRounds;
            EnableHalftime = enableHalfTime;
            EnableLateJoining = enableLateJoining;
            AllowAvatarChanging = allowAvatarChanging;

            MultiplayerHooking.OnPlayerJoin += OnPlayerJoin;
            MultiplayerHooking.OnPlayerLeave += OnPlayerLeave;
            MultiplayerHooking.OnPlayerAction += OnPlayerAction;
            MultiplayerHooking.OnMainSceneInitialized += On5vs5Loaded;

            // TODO set logos
            _Teams = new Team[2];
            CounterTerroristTeam = new Team(Fusion5vs5GamemodeTeams.CounterTerrorists.ToString(), 5);
            TerroristTeam = new Team(Fusion5vs5GamemodeTeams.Terrorists.ToString(), 5);
            _Teams[0] = CounterTerroristTeam;
            _Teams[1] = TerroristTeam;
            DefendingTeam = defendingTeam == Fusion5vs5GamemodeTeams.Terrorists ? TerroristTeam : CounterTerroristTeam;

            TimeLimits = timeLimits;
            _PlayerStatesDict = new Dictionary<PlayerId, PlayerStates>();
        }

        // Callbacks

        public void On5vs5Loaded()
        {
            Log();
            MelonLogger.Msg(
                $"Scene {FusionSceneManager.Level.Title} has been loaded for 5vs5 Gamemode. Barcode {FusionSceneManager.Level._barcode}.");
            MultiplayerHooking.OnMainSceneInitialized -= On5vs5Loaded;
            MultiplayerHooking.OnLoadingBegin += On5vs5Aborted;

            foreach (var team in _Teams)
            {
                SetTeamScore(team, 0);
            }

            SetRoundNumber(0);

            foreach (PlayerId player in PlayerIdManager.PlayerIds)
            {
                InitializePlayer(player);
            }

            Operations.InvokeTrigger(Events.Fusion5vs5Started);

            StartStateMachine();
        }

        public void On5vs5Aborted()
        {
            Log();
            MelonLogger.Msg(
                "5vs5 Mode: A different scene has been loaded while 5vs5 Gamemode was running. Aborting gamemode.");
            MultiplayerHooking.OnLoadingBegin -= On5vs5Aborted;
            Operations.InvokeTrigger(Events.Fusion5vs5Aborted);
        }

        private void OnPlayerJoin(PlayerId playerId)
        {
            Log(playerId);
            MelonLogger.Msg("5vs5 Mode: OnPlayerJoin Called.");

            InitializePlayer(playerId);
        }

        private void OnPlayerLeave(PlayerId playerId)
        {
            Log(playerId);
            MelonLogger.Msg("5vs5 Mode: OnPlayerLeave Called.");

            Team team = GetTeam(playerId);
            team.Players.Remove(playerId);
            _PlayerStatesDict.Remove(playerId);
            Operations.InvokeTrigger($"{Events.PlayerLeft}.{playerId.LongId}.{team.TeamName}");
        }

        private void OnPlayerAction(PlayerId playerId, PlayerActionType type, PlayerId otherPlayer)
        {
            Log(playerId, type, otherPlayer);
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
                else if (type == PlayerActionType.DYING)
                {
                    Suicide(playerId, null);
                }
                else if (type == PlayerActionType.DEATH)
                {
                    DyingAnimationCompleted(playerId);
                }
            }
        }

        public void OnClientRequested(string request)
        {
            Log(request);
            if (request.StartsWith(ClientRequest.ChangeTeams))
            {
                string[] info = request.Split('.');
                PlayerId player = GetPlayerFromValue(info[1]);
                Team team = GetTeamFromValue(info[2]);
                TeamChangeRequested(player, team);
            }
            else if (request.StartsWith(ClientRequest.JoinSpectator))
            {
                string[] info = request.Split('.');
                PlayerId player = GetPlayerFromValue(info[1]);
                Team team = GetTeam(player);
                if (team == null)
                    return;
                _SpawnPoints.Remove(player);
                team.Players.Remove(player);
                _PlayersInBuyZone.Remove(player);
                SetPlayerState(player, PlayerStates.Spectator);
                DetermineTeamWon(player, team);
                Operations.InvokeTrigger($"{Events.PlayerSpectates}.{player.LongId}");
            }
            else if (request.StartsWith(ClientRequest.BuyZoneEntered))
            {
                string[] info = request.Split('.');
                PlayerId player = GetPlayerFromValue(info[1]);
                OnPlayerEnteredBuyZone(player);
            }
            else if (request.StartsWith(ClientRequest.BuyZoneExited))
            {
                string[] info = request.Split('.');
                PlayerId player = GetPlayerFromValue(info[1]);
                OnPlayerExitedBuyZone(player);
            }
            else if (request.StartsWith(ClientRequest.BuyItem))
            {
                string[] info = request.Split('.');
                PlayerId player = GetPlayerFromValue(info[1]);
                string barcode = string.Join(".", info.Skip(2));
                BuyItemRequested(player, info[2]);
            }
        }

        private void OnTimeElapsed()
        {
            Log();
            NextState();
        }

        // Team

        public void TeamChangeRequested(PlayerId player, Team selectedTeam)
        {
            Log(player, selectedTeam);
            if (player == null || selectedTeam == null)
            {
#if DEBUG
                MelonLogger.Warning("TeamChangeRequested(): at least one argument was null.");
#endif
                return;
            }

            player.TryGetDisplayName(out var playerName);

            if (_State == GameStates.MatchHalfPhase || _State == GameStates.MatchEndPhase)
            {
#if DEBUG
                MelonLogger.Warning(
                    $"Player {playerName} tried to switch teams during MatchHalfPhase/MatchEndPhase, aborting.");
#endif
                return;
            }

            Team currentTeam = GetTeam(player);
            if (currentTeam != selectedTeam)
            {
                var spawnPoints = selectedTeam.Equals(CounterTerroristTeam)
                    ? _CounterTerroristSpawnPoints
                    : _TerroristSpawnPoints;
                SerializedTransform? newSpawnPoint = AssignSpawnPoint(player, spawnPoints);
                if (newSpawnPoint == null)
                {
#if DEBUG
                    MelonLogger.Warning("TeamChangeRequested(): no free spawn points available for this team.");
#endif
                    return;
                }

                if (currentTeam != null)
                {
                    currentTeam.Players.Remove(player);
                }

                selectedTeam.Players.Add(player);
                Team oldTeam = currentTeam;
                Operations.SetMetadata(GetTeamMemberKey(player), selectedTeam.TeamName);
                MelonLogger.Msg($"Player {playerName} switched teams to {selectedTeam.TeamName}");

                if (_State == GameStates.Warmup)
                {
                    if (GetPlayerState(player) == PlayerStates.Spectator)
                    {
                        SetPlayerState(player, PlayerStates.Alive);
                        RevivePlayer(player);
                    }
                    else
                    {
                        // Already inside of a team
                        RespawnPlayer(player);
                    }
                }
                else if (_State == GameStates.BuyPhase)
                {
                    if (GetPlayerState(player) == PlayerStates.Spectator)
                    {
                        SetPlayerState(player, PlayerStates.Alive);
                        ReviveAndFreezePlayer(player);
                    }
                    else
                    {
                        RespawnPlayer(player);
                    }
                }
                else if (_State == GameStates.PlayPhase || _State == GameStates.RoundEndPhase)
                {
                    PlayerStates state = GetPlayerState(player);
                    if (state == PlayerStates.Spectator)
                    {
                        SetPlayerState(player, PlayerStates.Dead);
                    }
                    else if (state == PlayerStates.Alive)
                    {
                        SetPlayerState(player, PlayerStates.Dead);
                        KillPlayer(player);
                    }

                    // Team used to be empty, now finish the round
                    if (_State == GameStates.PlayPhase && selectedTeam.Players.Count == 1)
                    {
                        Team otherTeam = selectedTeam.Equals(CounterTerroristTeam)
                            ? TerroristTeam
                            : CounterTerroristTeam;
                        IncrementTeamScore(otherTeam);
                        Operations.InvokeTrigger($"{Events.TeamWonRound}.{otherTeam.TeamName}");
                        NextState();
                    }
                }

                DetermineTeamWon(player, oldTeam);
            }
        }

        private SerializedTransform? AssignSpawnPoint(PlayerId player, List<SerializedTransform> spawnPoints)
        {
            Log(player, spawnPoints);
            foreach (SerializedTransform spawnPoint in spawnPoints)
            {
                if (!_SpawnPoints.TryGetValue(player, out var transform) || !spawnPoints.Contains(transform))
                {
                    // Found a free spawn point for the player to assign to
                    _SpawnPoints.Remove(player);
                    _SpawnPoints.Add(player, spawnPoint);
                    Vector3 pos = spawnPoint.position.ToUnityVector3();
                    Vector3 rot = spawnPoint.rotation.ToUnityQuaternion().eulerAngles;
                    Operations.InvokeTrigger(
                        $"{Events.SpawnPointAssigned}.{player.LongId}.{pos.x},{pos.y},{pos.z},{rot.x},{rot.y},{rot.z}");
                    return spawnPoint;
                }
            }

            return null;
        }

        private Team GetTeam(PlayerId playerId)
        {
            Log(playerId);
            foreach (var team in _Teams)
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

        private Team GetTeamFromValue(string value)
        {
            Log(value);
            return TerroristTeam.TeamName.Equals(value) ? TerroristTeam : CounterTerroristTeam;
        }

        private void IncrementTeamScore(Team team)
        {
            Log(team);
            SetTeamScore(team, GetTeamScore(team) + 1);
        }

        private void SetTeamScore(Team team, int teamScore)
        {
            Log(team, teamScore);
            Operations.SetMetadata(GetTeamScoreKey(Commons.GetTeamFromValue(team.TeamName)), teamScore.ToString());
        }

        private int GetTeamScore(Team team)
        {
            Log(team);
            string teamScore = Operations.GetMetadata(GetTeamScoreKey(Commons.GetTeamFromValue(team.TeamName)));
            return int.Parse(teamScore);
        }

        // All dead, all dead...
        private void DetermineTeamWon(PlayerId killed)
        {
            Log(killed);

            Team losingTeam = GetTeam(killed);
            DetermineTeamWon(killed, losingTeam);
        }

        private void DetermineTeamWon(PlayerId player, Team losingTeam)
        {
            Log(player, losingTeam);
            if (_State != GameStates.PlayPhase)
                return;

            // Not part of any team
            if (losingTeam == null)
                return;

            foreach (var _player in losingTeam.Players)
            {
                if (GetPlayerState(_player) == PlayerStates.Alive)
                    return;
            }

            Team winnerTeam = losingTeam.Equals(CounterTerroristTeam) ? TerroristTeam : CounterTerroristTeam;
            IncrementTeamScore(winnerTeam);
            Operations.InvokeTrigger($"{Events.TeamWonRound}.{winnerTeam.TeamName}");
            NextState();
        }

        private void SwapTeams()
        {
            Log();
            List<PlayerId> toTransfer = new List<PlayerId>(TerroristTeam.Players);
            TerroristTeam.Players.Clear();
            TerroristTeam.Players.AddRange(CounterTerroristTeam.Players);
            CounterTerroristTeam.Players.Clear();
            CounterTerroristTeam.Players.AddRange(toTransfer);

            // Transfer points
            int score = GetTeamScore(CounterTerroristTeam);
            SetTeamScore(CounterTerroristTeam, GetTeamScore(TerroristTeam));
            SetTeamScore(TerroristTeam, score);

            _SpawnPoints.Clear();
            foreach (var team in _Teams)
            {
                List<SerializedTransform> spawnPoints = team.Equals(CounterTerroristTeam)
                    ? _CounterTerroristSpawnPoints
                    : _TerroristSpawnPoints;
                foreach (var player in team.Players)
                {
                    AssignSpawnPoint(player, spawnPoints);
                }
            }
        }

        // Player

        private void InitializePlayer(PlayerId player)
        {
            Log(player);

            ResetScore(player);

            SetPlayerState(player, PlayerStates.Spectator);
            Operations.InvokeTrigger($"{Events.SetSpectator}.{player.LongId}");
        }

        private void ResetScore(PlayerId player)
        {
            Log(player);
            SetPlayerKills(player, 0);
            SetPlayerDeaths(player, 0);
            SetPlayerAssists(player, 0);
        }

        private void PlayerKilled(PlayerId killer, PlayerId killed, object weapon)
        {
            Log(killer, killed, weapon);

            if (GetPlayerState(killer) != PlayerStates.Alive || GetPlayerState(killed) != PlayerStates.Alive)
                return;

            SetPlayerKills(killer, GetPlayerKills(Operations.Metadata, killer) + 1);
            SetPlayerDeaths(killed, GetPlayerDeaths(Operations.Metadata, killed) + 1);
            SetPlayerState(killed, PlayerStates.Dead);

            Operations.InvokeTrigger($"{Events.PlayerKilledPlayer}.{killer.LongId}.{killed.LongId}");

            if (_State == GameStates.PlayPhase || _State == GameStates.BuyPhase)
            {
                DetermineTeamWon(killed);
            }
        }

        private void Suicide(PlayerId playerId, object weapon)
        {
            Log(playerId, weapon);

            if (GetPlayerState(playerId) != PlayerStates.Alive)
                return;

            SetPlayerDeaths(playerId, GetPlayerDeaths(Operations.Metadata, playerId) + 1);
            SetPlayerState(playerId, PlayerStates.Dead);

            Operations.InvokeTrigger($"{Events.PlayerSuicide}.{playerId.LongId}");

            if (_State == GameStates.PlayPhase || _State == GameStates.BuyPhase)
            {
                DetermineTeamWon(playerId);
            }
        }

        private void DyingAnimationCompleted(PlayerId playerId)
        {
            Log(playerId);
            if (_State == GameStates.PlayPhase || _State == GameStates.RoundEndPhase)
                Operations.InvokeTrigger($"{Events.SetSpectator}.{playerId.LongId}");
        }

        private void SetPlayerKills(PlayerId killer, int kills)
        {
            Log(killer, kills);
            Operations.SetMetadata(GetPlayerKillsKey(killer), kills.ToString());
        }

        private void SetPlayerDeaths(PlayerId killed, int deaths)
        {
            Log(killed, deaths);
            Operations.SetMetadata(GetPlayerDeathsKey(killed), deaths.ToString());
        }

        private void SetPlayerAssists(PlayerId assister, int assists)
        {
            Log(assister, assists);
            Operations.SetMetadata(GetPlayerAssistsKey(assister), assists.ToString());
        }

        private int GetPlayerAssists(PlayerId assister)
        {
            Log(assister);
            string assistsScore = Operations.GetMetadata(GetPlayerAssistsKey(assister));
            return int.Parse(assistsScore);
        }

        private void SetPlayerState(PlayerId playerId, PlayerStates state)
        {
            Log(playerId, state);
            _PlayerStatesDict.Remove(playerId);
            _PlayerStatesDict.Add(playerId, state);
        }

        private PlayerStates GetPlayerState(PlayerId player)
        {
            Log(player);
            _PlayerStatesDict.TryGetValue(player, out PlayerStates playerState);
            return playerState;
        }

        private void RevivePlayer(PlayerId player)
        {
            Log(player);
            SetPlayerState(player, PlayerStates.Alive);
            Operations.InvokeTrigger($"{Events.RevivePlayer}.{player.LongId}");
        }

        // Two time-sensitive methods who's order may not be swapped
        private void ReviveAndFreezePlayer(PlayerId player)
        {
            Log(player);
            SetPlayerState(player, PlayerStates.Alive);
            Operations.InvokeTrigger($"{Events.ReviveAndFreezePlayer}.{player.LongId}");
        }

        private void KillPlayer(PlayerId player)
        {
            Log(player);
            Operations.InvokeTrigger($"{Events.KillPlayer}.{player.LongId}");
        }

        private void RespawnPlayer(PlayerId player)
        {
            Log(player);
            Operations.InvokeTrigger($"{Events.RespawnPlayer}.{player.LongId}");
        }

        private void FreezePlayer(PlayerId player)
        {
            Log(player);
            Operations.InvokeTrigger($"{Events.Freeze}.{player.LongId}");
        }

        private void UnFreezePlayer(PlayerId player)
        {
            Log(player);
            Operations.InvokeTrigger($"{Events.UnFreeze}.{player.LongId}");
        }

        private void FreezeAllPlayers()
        {
            Log();
            foreach (var team in _Teams)
            {
                foreach (var player in team.Players)
                {
                    bool ok = _PlayerStatesDict.TryGetValue(player, out PlayerStates playerState);
                    if (ok)
                    {
                        if (playerState != PlayerStates.Spectator)
                        {
                            FreezePlayer(player);
                        }
                    }
                }
            }
        }

        private void UnFreezeAllPlayers()
        {
            Log();
            foreach (var team in _Teams)
            {
                foreach (var player in team.Players)
                {
                    bool ok = _PlayerStatesDict.TryGetValue(player, out PlayerStates playerState);
                    if (ok)
                    {
                        if (playerState != PlayerStates.Spectator)
                        {
                            UnFreezePlayer(player);
                        }
                    }
                }
            }
        }

        private void BuyItemRequested(PlayerId player, string barcode)
        {
            Log(player, barcode);
            MelonLogger.Msg("BuyItemRequested() called.");
            _PlayerStatesDict.TryGetValue(player, out PlayerStates state);
            if (!_PlayersInBuyZone.Contains(player) || !_IsBuyTime || state != PlayerStates.Alive)
            {
                return;
            }

            RigReferenceCollection rigReferences;
            if (player.IsSelf)
            {
                rigReferences = RigData.RigReferences;
            }
            else
            {
                PlayerRepManager.TryGetPlayerRep(player.SmallId, out var rep);
                rigReferences = rep.RigReferences;
            }

            if (rigReferences == null)
            {
                player.TryGetDisplayName(out string name);
                MelonLogger.Warning(
                    $"Could not find RigReferenceCollection for player {name ?? $"with ID {player.LongId.ToString()}"}.");
                return;
            }

            RigManager rm = rigReferences.RigManager;
            Transform headTransform = rm.physicsRig.m_pelvis;
            SerializedTransform finalTransform = new SerializedTransform(headTransform.position + headTransform.forward,
                headTransform.rotation);
            SpawnResponseMessagePatches.OnSpawnFinished += PlaceItemInInventory;
            PooleeUtilities.RequestSpawn(barcode, finalTransform);
            MelonLogger.Msg("BuyItemRequested(): passed all checks.");

            void PlaceItemInInventory(byte owner, string spawnedBarcode, GameObject spawnedGo)
            {
                Log(owner, spawnedBarcode, spawnedGo);
                MelonLogger.Msg("PlaceItemInInventory(): called.");
                if (barcode != spawnedBarcode)
                {
                    return;
                }

                SpawnResponseMessagePatches.OnSpawnFinished -= PlaceItemInInventory;

                WeaponSlot weaponSlot = spawnedGo.GetComponentInChildren<WeaponSlot>();
                if (weaponSlot == null)
                {
                    return;
                }

                InteractableHost host = spawnedGo.GetComponentInChildren<InteractableHost>();
                if (host == null)
                {
                    return;
                }

                foreach (var slot in rigReferences.RigSlots)
                {
                    if (slot._slottedWeapon == null && (slot.slotType & weaponSlot.slotType) != 0)
                    {
                        slot.InsertInSlot(host);
                        return;
                    }
                }

                MelonLogger.Msg("PlaceItemInInventory(): passed all checks.");
            }
        }

        private void OnPlayerEnteredBuyZone(PlayerId player)
        {
            Log(player);
            if (!_PlayersInBuyZone.Contains(player))
            {
                _PlayersInBuyZone.Add(player);
            }
        }

        private void OnPlayerExitedBuyZone(PlayerId player)
        {
            Log(player);
            _PlayerStatesDict.TryGetValue(player, out var state);
            if (_State == GameStates.BuyPhase && state == PlayerStates.Alive)
            {
                RespawnPlayer(player);
            }
            else
            {
                _PlayersInBuyZone.Remove(player);
            }
        }

        // Internal

        private void StartStateMachine()
        {
            Log();
            _GameTimer = new Timer();
            _GameTimer.AutoReset = false;
            _GameTimer.Elapsed += (sender, args) => OnTimeElapsed();

            _BuyTimer = new Timer();
            _BuyTimer.AutoReset = false;
            _BuyTimer.Elapsed += (sender, args) => BuyTimeOver();
            NextState();
        }

        // When calling NextState() from anywhere but the timer's Elapsed event, call this as the last thing, after changing scores, round numbers, etc.

        private void NextState()
        {
            Log();
            OnStateEnd(_State);
            // In case anyone else calls NextState(), stop the timer manually
            _GameTimer.Stop();
            GameStates oldState = _State;
            // We update the old state to the next one and dispatch it to everyone else
            GameStates nextState = GameStates.Unknown;
            switch (oldState)
            {
                case GameStates.Unknown:
                    nextState = GameStates.Warmup;
                    break;
                case GameStates.Warmup:
                    nextState = GameStates.BuyPhase;
                    break;
                case GameStates.BuyPhase:
                    nextState = GameStates.PlayPhase;
                    break;
                case GameStates.PlayPhase:
                    if (EnableHalftime && HalfOfRoundsPlayed())
                    {
                        nextState = GameStates.MatchHalfPhase;
                    }
                    else if (IsRoundNumberMaxedOut() || IsTeamScoreMaxedOut())
                    {
                        nextState = GameStates.MatchEndPhase;
                    }
                    else
                    {
                        nextState = GameStates.RoundEndPhase;
                    }

                    break;
                case GameStates.RoundEndPhase:
                    nextState = GameStates.BuyPhase;
                    break;
                case GameStates.MatchHalfPhase:
                    nextState = GameStates.BuyPhase;
                    break;
            }

            // This means the game is over
            if (nextState == GameStates.Unknown)
                return;

            if (TimeLimits.TryGetValue(nextState, out int timeLimit))
            {
                _GameTimer.Interval = timeLimit * 1000;
                _GameTimer.Start();
            }
            else
            {
                MelonLogger.Warning($"Could not find a time limit for {nextState}!");
            }

            _State = nextState;

            OnStateChanged(_State);
            Operations.InvokeTrigger($"{Events.NewGameState}.{nextState.ToString()}");
        }

        private void OnStateChanged(GameStates newState)
        {
            Log(newState);
            switch (newState)
            {
                case GameStates.Unknown:
                    break;
                case GameStates.Warmup:
                    BuyTimeStart();
                    break;
                case GameStates.BuyPhase:
                    IncrementRoundNumber();

                    BuyTimeStart(40);

                    foreach (var team in _Teams)
                    {
                        foreach (var player in team.Players)
                        {
                            bool ok = _PlayerStatesDict.TryGetValue(player, out PlayerStates playerState);
                            if (ok)
                            {
                                if (playerState == PlayerStates.Dead)
                                {
                                    ReviveAndFreezePlayer(player);
                                }
                                else if (playerState == PlayerStates.Alive)
                                {
                                    RespawnPlayer(player);
                                    FreezePlayer(player);
                                }
                            }
                        }
                    }

                    break;
                case GameStates.PlayPhase:
                    UnFreezeAllPlayers();

                    break;
                case GameStates.RoundEndPhase:
                    break;
                case GameStates.MatchHalfPhase:
                    FreezeAllPlayers();
                    SwapTeams();
                    break;
                case GameStates.MatchEndPhase:
                    FreezeAllPlayers();

                    int tScore = GetTeamScore(TerroristTeam);
                    int cScore = GetTeamScore(CounterTerroristTeam);
                    if (tScore == cScore)
                    {
                        Operations.InvokeTrigger(Events.GameTie);
                    }
                    else if (tScore > cScore)
                    {
                        Operations.InvokeTrigger($"{Events.TeamWonGame}.{TerroristTeam.TeamName}");
                    }
                    else
                    {
                        Operations.InvokeTrigger($"{Events.TeamWonGame}.{CounterTerroristTeam.TeamName}");
                    }

                    break;
            }
        }

        private void OnStateEnd(GameStates state)
        {
            Log(state);
            switch (state)
            {
                case GameStates.Unknown:
                    break;
                case GameStates.Warmup:
                    foreach (var player in PlayerIdManager.PlayerIds)
                        ResetScore(player);
                    break;
                case GameStates.BuyPhase:
                    break;
                case GameStates.PlayPhase:
                    // defending team wins due to time running out
                    IncrementTeamScore(DefendingTeam);
                    Operations.InvokeTrigger($"{Events.TeamWonRound}.{DefendingTeam.TeamName}");

                    BuyTimeOver();
                    break;
                case GameStates.RoundEndPhase:
                    break;
                case GameStates.MatchHalfPhase:
                    break;
                case GameStates.MatchEndPhase:
                    Operations.InvokeTrigger(Events.Fusion5vs5Over);
                    return;
            }
        }

        private void IncrementRoundNumber()
        {
            Log();
            SetRoundNumber(GetRoundNumber(Operations.Metadata) + 1);
        }

        private void SetRoundNumber(int i)
        {
            Log(i);
            Operations.SetMetadata(Metadata.RoundNumberKey, i.ToString());
        }

        private bool HalfOfRoundsPlayed()
        {
            Log();
            string _roundNumber = Operations.GetMetadata(Metadata.RoundNumberKey);
            int roundNumber = int.Parse(_roundNumber);
            return roundNumber == MaxRounds / 2;
        }

        private bool IsTeamScoreMaxedOut()
        {
            Log();
            int maxTeamScore = MaxRounds / 2 + 1;

            foreach (var team in _Teams)
            {
                int score = GetTeamScore(team);
                MelonLogger.Msg($"IsTeamScoreMaxedOut(): {team.TeamName}'s score = {score}");
                if (score == maxTeamScore)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsRoundNumberMaxedOut()
        {
            Log();
            string _roundNumber = Operations.GetMetadata(Metadata.RoundNumberKey);
            int roundNumber = int.Parse(_roundNumber);

            return roundNumber == MaxRounds;
        }

        private void BuyTimeStart()
        {
            Log();
            if (!_IsBuyTime)
            {
                _IsBuyTime = true;
                Operations.InvokeTrigger(Events.BuyTimeStart);
            }
        }

        private void BuyTimeStart(int seconds)
        {
            Log(seconds);
            _BuyTimer.Interval = seconds * 1000;
            _BuyTimer.Start();

            BuyTimeStart();
        }

        private void BuyTimeOver()
        {
            Log();
            if (_BuyTimer.Enabled)
                _BuyTimer.Stop();

            if (_IsBuyTime)
            {
                _IsBuyTime = false;
                Operations.InvokeTrigger(Events.BuyTimeOver);
            }
        }

        public void Dispose()
        {
            Log();
            try
            {
                MultiplayerHooking.OnPlayerJoin -= OnPlayerJoin;
                MultiplayerHooking.OnPlayerLeave -= OnPlayerLeave;
                MultiplayerHooking.OnPlayerAction -= OnPlayerAction;
                MultiplayerHooking.OnLoadingBegin -= On5vs5Aborted;

                foreach (var team in _Teams)
                {
                    team.Players.Clear();
                }
            }
            finally
            {
                if (_GameTimer != null)
                {
                    _GameTimer.Stop();
                    try
                    {
                        _GameTimer.Dispose();
                    }
                    catch
                    {
                        MelonLogger.Warning("Could not dispose game timer.");
                    }
                }

                if (_BuyTimer != null)
                {
                    _BuyTimer.Stop();
                    try
                    {
                        _BuyTimer.Dispose();
                    }
                    catch
                    {
                        MelonLogger.Warning("Could not dispose buy timer.");
                    }
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