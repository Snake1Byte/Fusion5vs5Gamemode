using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Fusion5vs5Gamemode.SDK;
using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.SDK.Gamemodes;
using MelonLoader;

namespace Fusion5vs5Gamemode
{
    public static class Commons
    {
        public static class Metadata
        {
            // Metadata
            public const string DefaultPrefix = "Fusion5vs5";
            public const string TeamKey = DefaultPrefix + ".Team";
            public const string TeamScoreKey = TeamKey + ".Score";
            public const string TeamNameKey = TeamKey + ".Name";
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
            public const string RespawnPlayer = "RespawnPlayer";
            public const string SetSpectator = "SetSpectator";
            public const string TeamWonRound = "TeamWonRound";
            public const string TeamWonGame = "TeamWonGame";
            public const string GameTie = "GameTie";
            public const string Fusion5vs5Started = "Fusion5vs5Started";
            public const string Fusion5vs5Aborted = "Fusion5vs5Aborted";
            public const string Fusion5vs5Over = "Fusion5vs5Over";
            public const string NewGameState = "NewGameState";
            public const string PlayerLeft = "PlayerLeft";
            public const string PlayerSpectates = "PlayerSpectates";
        }

        public static class ClientRequest
        {
            public const string ChangeTeams = "ChangeTeams";
            public const string JoinSpectator = "JoinSpectator";
            public const string Buy = "Buy";
        }

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

        public static StringBuilder builder = new StringBuilder();

        public static void LogCustom(string custom)
        {
            builder.Append(custom);
        }

        public static void Log(params object[] parameters)
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

            Dump();
        }

        public static void Dump()
        {
            string filePath = "D:\\Windows User\\Desktop\\Fusion5vs5GamemodeDump.txt";
            string contentToAppend = builder.ToString();
            try
            {
                if (!File.Exists(filePath))
                {
                    using (StreamWriter sw = File.CreateText(filePath))
                    {
                        sw.Write(contentToAppend);
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(filePath))
                    {
                        sw.Write(contentToAppend);
                    }
                }

                builder.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not dump stack trace: {ex.Message}");
            }
        }
    }
}