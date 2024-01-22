using System;
using System.Collections.Generic;
using System.Timers;
using Fusion5vs5Gamemode.SDK;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.SDK.Gamemodes;
using LabFusion.Senders;
using LabFusion.Utilities;
using MelonLoader;
using static Fusion5vs5Gamemode.Commons;
using CommonBarcodes = BoneLib.CommonBarcodes;
using Random = UnityEngine.Random;

namespace Fusion5vs5Gamemode
{
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

        private readonly Team[] _Teams;

        // For defusing game mode, this would be Counter Terrorist Team. For hostage, this would be Terrorist Team.
        public Team
            DefendingTeam { get; set; } // Will be set from the SDK with the Fusion5vs5Descriptor component

        // Avatars
        public string DefaultAvatarBarcode { get; set; } = CommonBarcodes.Avatars.FordBL;

        // States
        private GameStates _State = GameStates.Unknown;
        public GameStates State => _State;
        private Timer _GameTimer;
        private int _TimeElapsed = 0;
        public Dictionary<GameStates, int> TimeLimits { get; }

        private Dictionary<PlayerId, PlayerStates> playerStatesDict;

        public Server(IServerOperations operations, Fusion5vs5GamemodeTeams defendingTeam, int maxRounds,
            bool enableHalfTime, bool enableLateJoining,
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
            _Teams = new Team[2];
            CounterTerroristTeam = new Team(Fusion5vs5GamemodeTeams.CounterTerrorists.ToString(), 5);
            TerroristTeam = new Team(Fusion5vs5GamemodeTeams.Terrorists.ToString(), 5);
            _Teams[0] = CounterTerroristTeam;
            _Teams[1] = TerroristTeam;
            DefendingTeam = defendingTeam == Fusion5vs5GamemodeTeams.Terrorists ? TerroristTeam : CounterTerroristTeam;

            TimeLimits = new Dictionary<GameStates, int>();
            TimeLimits.Add(GameStates.Warmup, 60);
            TimeLimits.Add(GameStates.BuyPhase, 15);
            TimeLimits.Add(GameStates.PlayPhase, 135);
            TimeLimits.Add(GameStates.RoundEndPhase, 10);
            TimeLimits.Add(GameStates.MatchHalfPhase, 15);
            TimeLimits.Add(GameStates.MatchEndPhase, 30);
        }

        // Callbacks

        public void On5vs5Loaded()
        {
            MelonLogger.Msg(
                $"Scene {FusionSceneManager.Level.Title} has been loaded for 5vs5 Gamemode. Barcode {FusionSceneManager.Level._barcode}.");
            MultiplayerHooking.OnMainSceneInitialized -= On5vs5Loaded;
            MultiplayerHooking.OnLoadingBegin += On5vs5Aborted;

            SetTeamName(CounterTerroristTeam, CounterTerroristTeamName);
            SetTeamName(TerroristTeam, TerroristTeamName);

            foreach (var team in _Teams)
            {
                SetTeamScore(team, 0);
            }

            SetRoundNumber(0);

            foreach (PlayerId player in PlayerIdManager.PlayerIds)
            {
                InitializePlayer(player);
            }

            StartStateMachine();

            Operations.InvokeTrigger(Events.Fusion5vs5Started);
        }

        public void On5vs5Aborted()
        {
            MelonLogger.Msg(
                "5vs5 Mode: A different scene has been loaded while 5vs5 Gamemode was running. Aborting gamemode.");
            MultiplayerHooking.OnLoadingBegin -= On5vs5Aborted;
            Operations.InvokeTrigger(Events.Fusion5vs5Aborted);
        }

        private void OnPlayerJoin(PlayerId playerId)
        {
            MelonLogger.Msg("5vs5 Mode: OnPlayerJoin Called.");

            InitializePlayer(playerId);

            playerId.TryGetDisplayName(out string name);
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
                Team team = GetTeamFromValue(info[2]);
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

                if (_State == GameStates.Warmup || _State == GameStates.BuyPhase)
                {
                    SetPlayerState(player, PlayerStates.Alive);
                    RevivePlayer(player);
                }
                else if (_State == GameStates.PlayPhase || _State == GameStates.RoundEndPhase)
                {
                    SetPlayerState(player, PlayerStates.Dead);
                    KillPlayer(player);
                }
            }
        }

        private Team GetTeam(PlayerId playerId)
        {
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
            return TerroristTeam.TeamName.Equals(value) ? TerroristTeam : CounterTerroristTeam;
        }

        private void SetTeamName(Team team, string teamName)
        {
            Operations.SetMetadata(GetTeamNameKey(team), teamName);
            team.SetDisplayName(teamName);
        }

        private void IncrementTeamScore(Team team)
        {
            SetTeamScore(team, GetTeamScore(team) + 1);
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

        // All dead, all dead...
        private void DetermineTeamWon(PlayerId killed)
        {
            Team losingTeam = GetTeam(killed);
            foreach (var player in losingTeam.Players)
            {
                if (GetPlayerState(player) == PlayerStates.Alive)
                {
                    return;
                }
            }

            Team winnerTeam = losingTeam.Equals(CounterTerroristTeam) ? TerroristTeam : CounterTerroristTeam;
            IncrementTeamScore(winnerTeam);
            NextState();
        }

        // Player

        private void InitializePlayer(PlayerId player)
        {
            SetPlayerKills(player, 0);
            SetPlayerDeaths(player, 0);
            SetPlayerAssists(player, 0);

            SetPlayerState(player, PlayerStates.Spectator);
        }

        private void PlayerKilled(PlayerId killer, PlayerId killed, object weapon)
        {
            SetPlayerKills(killer, GetPlayerKills(Operations.GetMetadata(), killer) + 1);
            SetPlayerDeaths(killed, GetPlayerDeaths(Operations.GetMetadata(), killed) + 1);

            SetPlayerState(killed, PlayerStates.Dead);

            DetermineTeamWon(killed);
        }

        private void Suicide(PlayerId playerId, object weapon)
        {
            SetPlayerDeaths(playerId, GetPlayerDeaths(Operations.GetMetadata(), playerId) + 1);
            SetPlayerState(playerId, PlayerStates.Dead);
            DetermineTeamWon(playerId);
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

        private void RevivePlayer(PlayerId player)
        {
            Team team = GetTeam(player);
            Operations.InvokeTrigger($"{Events.RevivePlayer}.{player.LongId}.{team.TeamName}");
        }

        private void KillPlayer(PlayerId player)
        {
            Operations.InvokeTrigger($"{Events.KillPlayer}.{player.LongId}");
        }
        
        private void RespawnPlayer(PlayerId player)
        {
            Team team = GetTeam(player);
            Operations.InvokeTrigger($"{Events.RespawnPlayer}.{player.LongId}.{team.TeamName}");
        }
        
        // Internal

        private void StartStateMachine()
        {
            _GameTimer = new Timer();
            _GameTimer.AutoReset = false;
            _GameTimer.Elapsed += (sender, args) => TimeElapsed();
            NextState();
        }

        private void TimeElapsed()
        {
            if (NetworkInfo.IsServer)
            {
                if (_State == GameStates.MatchEndPhase)
                {
                    Operations.InvokeTrigger(Events.Fusion5vs5Over);
                    return;
                }

                if (_State == GameStates.PlayPhase)
                {
                    // defending team wins due to time running out
                    IncrementTeamScore(DefendingTeam);
                }

                NextState();
            }
        }

        // When calling NextState() from anywhere but the timer's Elapsed event, call this as the last thing, after changing scores, round numbers, etc.
        private void NextState()
        {
            OnStateEnd(_State);
            // In case anyone else calls NextState(), stop the timer manually
            _GameTimer.Stop();
            GameStates oldState = _State;
            if (NetworkInfo.IsServer)
            {
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
                        if (IsRoundNumberMaxedOut() || IsTeamScoreMaxedOut())
                        {
                            nextState = GameStates.MatchEndPhase;
                        }
                        else if (EnableHalftime && HalfOfRoundsPlayed())
                        {
                            nextState = GameStates.MatchHalfPhase;
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

                if (TimeLimits.TryGetValue(nextState, out int timeLimit))
                {
                    _GameTimer.Interval = timeLimit;
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
        }

        private void OnStateChanged(GameStates newState)
        {
            switch (newState)
            {
                case GameStates.Unknown:
                    break;
                case GameStates.Warmup:
                    break;
                case GameStates.BuyPhase:
                    IncrementRoundNumber();
                    foreach (var team in _Teams)
                    {
                        foreach (var player in team.Players)
                        {
                            bool ok = playerStatesDict.TryGetValue(player, out PlayerStates playerState);
                            if (ok)
                            {
                                if (playerState == PlayerStates.Dead)
                                {
                                    RevivePlayer(player);
                                }
                                else if (playerState == PlayerStates.Alive)
                                {
                                    RespawnPlayer(player);
                                }
                            }
                        }
                    }

                    break;
                case GameStates.PlayPhase:
                    break;
                case GameStates.RoundEndPhase:
                    break;
                case GameStates.MatchHalfPhase:
                    break;
                case GameStates.MatchEndPhase:
                    break;
            }
        }

        private void OnStateEnd(GameStates state)
        {
            switch (state)
            {
                case GameStates.Unknown:
                    break;
                case GameStates.Warmup:
                    break;
                case GameStates.BuyPhase:
                    break;
                case GameStates.PlayPhase:
                    break;
                case GameStates.RoundEndPhase:
                    break;
                case GameStates.MatchHalfPhase:
                    break;
                case GameStates.MatchEndPhase:
                    break;
            }
        }

        private void IncrementRoundNumber()
        {
            SetRoundNumber(GetRoundNumber(Operations.GetMetadata()) + 1);
        }

        private void SetRoundNumber(int i)
        {
            Operations.SetMetadata(Metadata.RoundNumberKey, i.ToString());
        }

        private bool HalfOfRoundsPlayed()
        {
            string _roundNumber = Operations.GetMetadata(Metadata.RoundNumberKey);
            int roundNumber = int.Parse(_roundNumber);
            return roundNumber == MaxRounds / 2;
        }

        private bool IsTeamScoreMaxedOut()
        {
            int maxTeamScore = (int)((float)MaxRounds / 2) + 1;

            foreach (var team in _Teams)
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
            string _roundNumber = Operations.GetMetadata(Metadata.RoundNumberKey);
            int roundNumber = int.Parse(_roundNumber);

            return roundNumber == MaxRounds;
        }

        public void Dispose()
        {
            MultiplayerHooking.OnPlayerJoin -= OnPlayerJoin;
            MultiplayerHooking.OnPlayerLeave -= OnPlayerLeave;
            MultiplayerHooking.OnPlayerAction -= OnPlayerAction;
            MultiplayerHooking.OnLoadingBegin -= On5vs5Aborted;

            foreach (var team in _Teams)
            {
                foreach (var player in team.Players)
                {
                    team.RemovePlayer(player);
                }
            }

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
        }

        private enum PlayerStates
        {
            Spectator = 0,
            Alive = 1,
            Dead = 2
        }
    }
}