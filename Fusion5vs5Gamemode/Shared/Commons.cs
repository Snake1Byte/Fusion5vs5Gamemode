using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using BoneLib;
using Fusion5vs5Gamemode.SDK;
using Fusion5vs5Gamemode.Utilities;
using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.SDK.Gamemodes;
using MelonLoader;
using SLZ.Props.Weapons;

namespace Fusion5vs5Gamemode.Shared
{
    public static class Commons
    {
        public static class Metadata
        {
            // Metadata
            public const string DefaultPrefix = "Fusion5vs5";
            public const string TeamKey = DefaultPrefix + ".Team";
            public const string TeamScoreKey = DefaultPrefix + ".Score";
            public const string PlayerKillsKey = DefaultPrefix + ".Kills";
            public const string PlayerDeathsKey = DefaultPrefix + ".Deaths";
            public const string PlayerAssistsKey = DefaultPrefix + ".Assists";
            public const string RoundNumberKey = DefaultPrefix + ".RoundNumber";
        }

        public static class Events
        {
            public const string PlayerKilledPlayer = "PlayerKilledPlayer";
            public const string PlayerSuicide = "PlayerSuicide";
            public const string KillPlayer = "KillPlayer";
            public const string RevivePlayer = "RevivePlayer";
            public const string ReviveAndFreezePlayer = "ReviveAndFreezePlayer";
            public const string RespawnPlayer = "RespawnPlayer";
            public const string SetSpectator = "SetSpectator";
            public const string Freeze = "Freeze";
            public const string UnFreeze = "UnFreeze";
            public const string TeamWonRound = "TeamWonRound";
            public const string TeamWonGame = "TeamWonGame";
            public const string GameTie = "GameTie";
            public const string Fusion5vs5Started = "Fusion5vs5Started";
            public const string Fusion5vs5Aborted = "Fusion5vs5Aborted";
            public const string Fusion5vs5Over = "Fusion5vs5Over";
            public const string NewGameState = "NewGameState";
            public const string PlayerLeft = "PlayerLeft";
            public const string PlayerSpectates = "PlayerSpectates";
            public const string SpawnPointAssigned = "SpawnPointAssigned";
            public const string BuyTimeOver = "BuyTimeOver";
            public const string BuyTimeStart = "BuyTimeStart";
        }

        public static class ClientRequest
        {
            public const string ChangeTeams = "ChangeTeams";
            public const string JoinSpectator = "JoinSpectator";
            public const string BuyItem = "BuyItem";
            public const string BuyZoneEntered = "BuyZoneEntered";
            public const string BuyZoneExited = "BuyZoneExited";
        }

        static Commons()
        {
#if DEBUG
            builder.Append("==================================================================\n");
#endif
        }

        public const string SpectatorAvatar = CommonBarcodes.Avatars.PolyBlank;

        public static string GetTeamMemberKey(PlayerId id)
        {
            Log(id);
            return $"{Metadata.TeamKey}.{id.LongId}";
        }

        public static string GetTeamScoreKey(Fusion5vs5GamemodeTeams team)
        {
            Log(team);
            return $"{Metadata.TeamScoreKey}.{team.ToString()}";
        }

        public static Fusion5vs5GamemodeTeams GetTeamFromValue(string value)
        {
            Log(value);
            return (Fusion5vs5GamemodeTeams)Enum.Parse(typeof(Fusion5vs5GamemodeTeams), value);
        }

        public static string GetPlayerKillsKey(PlayerId killer)
        {
            Log(killer);
            return $"{Metadata.PlayerKillsKey}.{killer?.LongId}";
        }

        public static string GetPlayerAssistsKey(PlayerId assister)
        {
            Log(assister);
            return $"{Metadata.PlayerAssistsKey}.{assister?.LongId}";
        }

        public static string GetPlayerDeathsKey(PlayerId killed)
        {
            Log(killed);
            return $"{Metadata.PlayerDeathsKey}.{killed?.LongId}";
        }

        public static PlayerId GetPlayerFromValue(string player)
        {
            Log(player);
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

        public static int GetPlayerKills(FusionDictionary<string, string> metadata, PlayerId killer)
        {
            Log(metadata, killer);
            metadata.TryGetValue(GetPlayerKillsKey(killer), out string killerScore);
            return int.Parse(killerScore);
        }

        public static int GetPlayerDeaths(FusionDictionary<string, string> metadata, PlayerId killed)
        {
            Log(metadata, killed);
            metadata.TryGetValue(GetPlayerDeathsKey(killed), out string deathScore);
            return int.Parse(deathScore);
        }

        public static int GetRoundNumber(FusionDictionary<string, string> metadata)
        {
            Log(metadata);
            metadata.TryGetValue(Metadata.RoundNumberKey, out string roundNumber);
            return int.Parse(roundNumber);
        }

        public static void RotateGunPerpendicular(Gun gun, SerializedTransform forwardTransform)
        {
            
        }
        
        public static StringBuilder builder = new StringBuilder();

        public static void Log(params object[] parameters)
        {
#if DEBUG
            try
            {
                StackFrame frame = new StackFrame(1);
                MethodBase method = frame.GetMethod();
                if (method == null)
                {
                    return;
                }

                DateTime currentTime = DateTime.Now;
                string formattedTime = $"[{currentTime:yyyy/MM/dd HH:mm:ss.fff}]";
                builder.Append(formattedTime);
                builder.Append("\t");

                builder.Append(method.DeclaringType.FullName + " ");
                int i = 0;
                builder.Append(method.Name);
                if (method.GetParameters().Length > 0)
                {
                    builder.Append("(");
                    foreach (var parameter in method.GetParameters())
                    {
                        if (i > 0)
                        {
                            builder.Append(", ");
                        }

                        if (parameters != null)
                        {
                            builder.Append(parameter);
                            builder.Append(" = ");
                            if (parameters[i] is string)
                            {
                                builder.Append("\"");
                                builder.Append(parameters[i]);
                                builder.Append("\"");
                            }
                            else if (parameters[i] is Team t)
                            {
                                builder.Append(t.TeamName);
                            }
                            else if (parameters[i] is PlayerId p)
                            {
                                builder.Append(p.LongId);
                            }
                            else if (parameters[i] is RadialMenu.RadialSubMenu r)
                            {
                                builder.Append($"{(r.Parent == null ? "" : $"{r.Parent.Name}/")}{r.Name}");
                            }
                            else
                            {
                                builder.Append(parameters[i]);
                            }

                            ++i;
                        }
                    }

                    builder.Append(")");
                    builder.Append("\n");
                }
                else
                {
                    builder.Append("()");
                    builder.Append("\n");
                }

                Dump(builder.ToString(), @"D:\Windows User\Desktop\Fusion5vs5GamemodeDump.txt");
                builder.Clear();
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Exception during Log(): {e}");
            }
#endif
        }

        public static void Dump(string content, string path)
        {
#if DEBUG
            string filePath = path;
            try
            {
                if (!File.Exists(filePath))
                {
                    using (StreamWriter sw = File.CreateText(filePath))
                    {
                        sw.Write(content);
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(filePath))
                    {
                        sw.Write(content);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not dump stack trace: {ex.Message}");
            }
#endif
        }
    }
}