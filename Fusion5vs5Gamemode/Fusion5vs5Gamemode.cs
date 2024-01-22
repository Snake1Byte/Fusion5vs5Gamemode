using System;
using System.Text;
using BoneLib.BoneMenu.Elements;
using Fusion5vs5Gamemode.SDK;
using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.SDK.Gamemodes;
using LabFusion.Utilities;
using MelonLoader;
using SLZ.Marrow.SceneStreaming;
using TMPro;
using UnityEngine;
using static Fusion5vs5Gamemode.Commons;

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
        public Server Server { get; private set; }
        private Fusion5vs5GamemodeTeams _DefendingTeam;
        private Fusion5vs5GamemodeTeams _LocalTeam;

        private MenuCategory _Menu;
        private FunctionElement _DefendingTeamSelection;
        private FunctionElement _AttackingTeamSelection;
        private IntElement _MaxRoundsSetting;
        private BoolElement _EnableHalfTimeSetting;
        private BoolElement _EnableLateJoiningSetting;
        private BoolElement _AllowAvatarChangingSetting;

        private const string _TEAM_TEMPLATE = "Join {0}";

        public override void OnBoneMenuCreated(MenuCategory category)
        {
            base.OnBoneMenuCreated(category);

            _Menu = category;

            //TODO Add custom settings for this gamemode 
            _DefendingTeamSelection =
                _Menu.CreateFunctionElement($"Join Defending Team", Color.white, RequestJoinDefenders);

            _AttackingTeamSelection =
                _Menu.CreateFunctionElement("Join Attacking Team", Color.white, RequestJoinAttackers);

            _MaxRoundsSetting = category.CreateIntElement("Maximum rounds", Color.white, 15, 1, 1, 1000000,
                i =>
                {
                    if (IsActive())
                        _MaxRoundsSetting.SetValue(Server.MaxRounds);
                });

            _EnableHalfTimeSetting = category.CreateBoolElement("Enable Half-Time", Color.white, true, b =>
            {
                if (IsActive())
                {
                    _EnableHalfTimeSetting.SetValue(Server.EnableHalftime);
                }
                else if (b)
                {
                    _MaxRoundsSetting.SetIncrement(2);
                    int maxRounds = _MaxRoundsSetting.GetValue();
                    if (maxRounds % 2 == 1)
                    {
                        _MaxRoundsSetting.SetValue(maxRounds + 1);
                    }
                }
                else
                {
                    _MaxRoundsSetting.SetIncrement(1);
                }
            });

            // TODO request changing these settings during a game to the server
            _EnableLateJoiningSetting = category.CreateBoolElement("Enable late joining", Color.white, true, b =>
            {
                if (IsActive())
                    _EnableLateJoiningSetting.SetValue(Server.EnableLateJoining);
            });

            _AllowAvatarChangingSetting = category.CreateBoolElement("Allow avatar changing", Color.white, true, b =>
            {
                if (IsActive())
                    _AllowAvatarChangingSetting.SetValue(Server.AllowAvatarChanging);
            });

            category.CreateBoolElement("Enable round music", Color.white, _EnableMusic, b => _EnableMusic = b);

            category.CreateBoolElement("Debug", Color.white, _Debug, e => _Debug = e);

            // Only show these while the game is running, until I add a better way to switch teams
            category.Elements.RemoveInstance(_DefendingTeamSelection);
            category.Elements.RemoveInstance(_AttackingTeamSelection);
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

            // TODO Import SDK descriptor

            _PreventNewJoins = !_EnableLateJoiningSetting.GetValue();
            if (NetworkInfo.IsServer)
            {
                Server = new Server(
                    this,
                    _DefendingTeam,
                    _MaxRoundsSetting.GetValue(),
                    _EnableHalfTimeSetting.GetValue(),
                    _PreventNewJoins,
                    _AllowAvatarChangingSetting.GetValue()
                );
            }

            _Menu.Elements.Insert(0, _DefendingTeamSelection);
            _Menu.Elements.Insert(0, _AttackingTeamSelection);
            _Menu.Elements.RemoveInstance(_EnableHalfTimeSetting);
            _Menu.Elements.RemoveInstance(_MaxRoundsSetting);

            FusionPlayer.SetPlayerVitality(1.0f);
            SceneStreamer.Reload();
        }

        protected override void OnStopGamemode()
        {
            base.OnStopGamemode();
            MelonLogger.Msg("5vs5 Mode: OnStopGamemode Called.");

            if (NetworkInfo.IsServer)
            {
                Server.Dispose();
            }

            _Menu.Elements.RemoveInstance(_DefendingTeamSelection);
            _Menu.Elements.RemoveInstance(_AttackingTeamSelection);
            _Menu.Elements.Insert(0, _EnableHalfTimeSetting);
            _Menu.Elements.Insert(0, _MaxRoundsSetting);

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
            else if (key.StartsWith(Commons.Metadata.TeamNameKey))
            {
                string _teamId = key.Split('.')[3];
                Fusion5vs5GamemodeTeams team = (Fusion5vs5GamemodeTeams)Enum.Parse(typeof(Fusion5vs5Gamemode), _teamId);
                TeamRepresentation rep = new TeamRepresentation { Team = team, DisplayName = value };
                OnTeamNameChanged(rep);
            }
            else if (key.StartsWith(Commons.Metadata.TeamKey))
            {
                string _player = key.Split('.')[2];
                PlayerId player = GetPlayerFromValue(_player);
                OnTeamChanged(player, value);
            }
            else if (key.Equals(Commons.Metadata.RoundNumberKey))
            {
                int newScore = int.Parse(value);
                OnRoundNumberChanged(newScore);
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
                string _team = eventName.Split('.')[2];
                PlayerId player = GetPlayerFromValue(_player);
                Fusion5vs5GamemodeTeams team = (Fusion5vs5GamemodeTeams)Enum.Parse(typeof(Fusion5vs5Gamemode), _team);

                RevivePlayer(player, team);
            } else if (eventName.StartsWith(Events.RespawnPlayer))
            {
                string _player = eventName.Split('.')[1];
                string _team = eventName.Split('.')[2];
                PlayerId player = GetPlayerFromValue(_player);
                Fusion5vs5GamemodeTeams team = (Fusion5vs5GamemodeTeams)Enum.Parse(typeof(Fusion5vs5Gamemode), _team);

                RespawnPlayer(player, team);
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

        private void OnStateChanged(GameStates state)
        {
            MelonLogger.Msg($"New game state {state}.");

            switch (state)
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
        }

        private void StartWarmupPhase()
        {
            // TODO decide how to implement team switching
            Notify("Warmup begun", "Select a team from <UI component>");
        }

        private void StartBuyPhase()
        {
            // TODO decide how to implement weapon buying
            Notify("Buy Phase", "Buy weapons from <UI component>");
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

        private void StartRoundEndPhase()
        {
            Notify("Round over", $"{winner.TeamName} wins.");
        }

        private void StartMatchHalfPhase()
        {
            Notify("Switching sides", $"Switching to {switchedTo.TeamName}.");
        }

        private void StartMatchEndPhase()
        {
            Notify("Match over", $"{totalWinner.TeamName} wins the match.");
        }

        private void RequestJoinDefenders()
        {
            Fusion5vs5GamemodeTeams team = _DefendingTeam;
            RequestJoinTeam(team);
        }

        private void RequestJoinAttackers()
        {
            Fusion5vs5GamemodeTeams team =
                _DefendingTeam == Fusion5vs5GamemodeTeams.Terrorists
                    ? Fusion5vs5GamemodeTeams.CounterTerrorists
                    : Fusion5vs5GamemodeTeams.Terrorists;
            RequestJoinTeam(team);
        }

        private void RequestJoinTeam(Fusion5vs5GamemodeTeams team)
        {
            PlayerId player = PlayerIdManager.LocalId;
            string request = $"{ClientRequest.ChangeTeams}.{player?.LongId}.{team.ToString()}";
            RequestToServer(request);
        }

        private void OnTeamChanged(PlayerId player, string teamId)
        {
            
            _LocalTeam = 
            // TODO Implement UI changes
        }

        private void OnTeamNameChanged(TeamRepresentation rep)
        {
            // TODO Implement UI changes
        }

        private void OnRoundNumberChanged(int newScore)
        {
            // TODO Implement UI changes
        }

        private void RequestToServer(string request)
        {
            if (NetworkInfo.HasServer)
            {
                using (var writer = FusionWriter.Create())
                {
                    using (var data = Fusion5vs5ClientRequest.Create(request))
                    {
                        writer.Write(data);
                        using (var message = FusionMessage.ModuleCreate<Fusion5vs5ClientRequestHandler>(writer))
                        {
                            MessageSender.SendToServer(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }

        private void RevivePlayer(PlayerId player, Fusion5vs5GamemodeTeams team)
        {
            // TODO change avatar from spectator avatar, place player in spawn, give back interactability and visibility
        }

        private void KillPlayer(PlayerId player)
        {
            // TODO simply spawn ragdoll where the player avatar is without actually killing player
            SetSpectator(player);
        }
        
        private void RespawnPlayer(PlayerId player, Fusion5vs5GamemodeTeams team)
        {
            // TODO RevivePlayer would call this. RevivePlayer will first get the player out of spectator mode
        }

        private void SetSpectator(PlayerId player)
        {
            // TODO change to spectator avatar, remove interactibility and visibility
        }

        public void ChangeTeamName(bool attackers, string name)
        {
            if (attackers)
            {
                _AttackingTeamSelection.SetName(string.Format(_TEAM_TEMPLATE, name));
            }
            else
            {
                _DefendingTeamSelection.SetName(string.Format(_TEAM_TEMPLATE, name));
            }
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

        // IServerOperations Implementation

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
    }
}