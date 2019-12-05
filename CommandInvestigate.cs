using Rocket.API;
using Rocket.API.Extensions;
using Rocket.Unturned.Chat;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerInfoLibrary
{
    public class CommandInvestigate : IRocketCommand
    {
        internal static readonly string syntax = "<\"Player name\" | SteamID> [page]";
        internal static readonly string help = "Returns info for players matching the search quarry.";
        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public AllowedCaller AllowedCaller
        {
            get { return AllowedCaller.Both; }
        }

        public string Help
        {
            get { return help; }
        }

        public string Name
        {
            get { return "investigate"; }
        }

        public List<string> Permissions
        {
            get { return new List<string> { "PlayerInfoLib.Ivestigate" }; }
        }

        public string Syntax
        {
            get { return syntax; }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, PlayerInfoLib.Instance.Translate("investigate_help"));
            }
            else if (command.Length > 2)
            {
                UnturnedChat.Say(caller, PlayerInfoLib.Instance.Translate("too_many_parameters"));
                return;
            }
            var totalRecods = 1u;
            uint? page = 1;
            var perPage = caller is ConsolePlayer ? 10u : 4u;
            var pInfo = new List<PlayerData>();
            uint start;
            if (command.Length == 2)
            {
                page = command.GetUInt32Parameter(1);
                if (page == null || page == 0)
                {
                    UnturnedChat.Say(caller, PlayerInfoLib.Instance.Translate("invalid_page"));
                    return;
                }
            }
            // Is what is entered in the command a SteamID64 number?
            if (command[0].IsCSteamID(out var cSteamID))
            {
                var pData = PlayerInfoLib.Database.QueryById(cSteamID);
                if (pData.IsValid())
                    pInfo.Add(pData);
            }
            else
            {
                pInfo = PlayerInfoLib.Database.QueryByName(command[0], Parser.checkIP(command[0]) ? QueryType.IP : QueryType.Both, out totalRecods, true, (uint)page, perPage);
            }
            if (pInfo.Count == 0)
            {
                UnturnedChat.Say(caller, "No players found by that name.");
                return;
            }
            start = ((uint)page - 1) * perPage;
            UnturnedChat.Say(caller, PlayerInfoLib.Instance.Translate("number_of_records_found", totalRecods, command[0], page, Math.Ceiling(totalRecods / (float)perPage)), Color.red);
            foreach (var pData in pInfo)
            {
                start++;
                if (pData.IsLocal())
                {
                    UnturnedChat.Say(caller, string.Format("{0}: {1} [{2}] ({3}), IP: {4}, Local: {5}, IsVip: {6}", start, caller is ConsolePlayer ? pData.CharacterName : pData.CharacterName.Truncate(12), caller is ConsolePlayer ? pData.SteamName : pData.SteamName.Truncate(12), pData.SteamID, pData.IP, pData.IsLocal(), pData.IsVip()), Color.yellow);
                    UnturnedChat.Say(caller, string.Format("Seen: {0}, TT: {1}, Cleaned:{2}:{3}", pData.LastLoginLocal, pData.TotalPlayTime.FormatTotalTime(), pData.CleanedBuildables, pData.CleanedPlayerData), Color.yellow);
                }
                else
                {
                    UnturnedChat.Say(caller, string.Format("{0}: {1} [{2}] ({3}), IP: {4}, Local: {5}", start, caller is ConsolePlayer ? pData.CharacterName : pData.CharacterName.Truncate(12), caller is ConsolePlayer ? pData.SteamName : pData.SteamName.Truncate(12), pData.SteamID, pData.IP, pData.IsLocal()), Color.yellow);
                    UnturnedChat.Say(caller, string.Format("Seen: {0}, TT: {1}, on: {2}:{3}", pData.LastLoginLocal, pData.TotalPlayTime.FormatTotalTime(), pData.LastServerID, pData.LastServerName), Color.yellow);
                }
            }
        }
    }
}
