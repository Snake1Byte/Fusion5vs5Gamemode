using System;
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
            public const string KillPlayer = "KillPlayer";
            public const string RevivePlayer = "RevivePlayer";
            public const string RespawnPlayer = "RespawnPlayer";
            public const string Fusion5vs5Started = "Fusion5vs5Loaded";
            public const string Fusion5vs5Aborted = "Fusion5vs5Aborted";
            public const string Fusion5vs5Over = "Game Over";
            public const string NewGameState = "NewGameState";
        }

        public static class ClientRequest
        {
            public const string ChangeTeams = "ChangeTeams";
            public const string Buy = "Buy";
        }

        public static string GetTeamMemberKey(PlayerId id)
        {
            return $"{Metadata.TeamKey}.{id.LongId}";
        }

        public static string GetTeamScoreKey(Team team)
        {
            return $"{Metadata.TeamScoreKey}.{team?.TeamName}";
        }

        public static string GetPlayerKillsKey(PlayerId killer)
        {
            return $"{Metadata.PlayerKillsKey}.{killer?.LongId}";
        }

        public static string GetPlayerAssistsKey(PlayerId assister)
        {
            return $"{Metadata.PlayerAssistsKey}.{assister?.LongId}";
        }

        public static string GetPlayerDeathsKey(PlayerId killed)
        {
            return $"{Metadata.PlayerDeathsKey}.{killed?.LongId}";
        }
        
        public static string GetTeamNameKey(Team team)
        {
            return $"{Metadata.TeamNameKey}.{team?.TeamName}";
        }
        
        public static PlayerId GetPlayerFromValue(string player)
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
                    
        public static Team GetTeamFromValue(Team[] teams, string value)
        {
            foreach (var team in teams)
            {
                if (team.TeamName.Equals(value))
                {
                    return team;
                }
            }
            return null;
        }                 
        
        public static int GetPlayerKills(FusionDictionary<string, string> metadata, PlayerId killer)
        {
            metadata.TryGetValue(GetPlayerKillsKey(killer), out string killerScore);
            return int.Parse(killerScore);
        }

        public static int GetPlayerDeaths(FusionDictionary<string, string> metadata, PlayerId killed)
        {
            metadata.TryGetValue(GetPlayerDeathsKey(killed), out string deathScore);
            return int.Parse(deathScore);
        }
        
        public static int GetRoundNumber(FusionDictionary<string, string> metadata)
        {
            metadata.TryGetValue(Metadata.RoundNumberKey, out string roundNumber);
            return int.Parse(roundNumber);
        }
    }
}