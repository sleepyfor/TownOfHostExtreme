using HarmonyLib;
using Discord;
using System;
using AmongUs.Data;

namespace TOHX.Patches
{
    // Originally from Town of Us Rewritten, by Det
    [HarmonyPatch(typeof(ActivityManager), nameof(ActivityManager.UpdateActivity))]
    public class DiscordRPC
    {
        private static string lobbycode = "";
        private static string region = "";
        public static void Prefix([HarmonyArgument(0)] Activity activity)
        {
            var details = $"TOHX v{Main.PluginDisplayVersion}";
            activity.Details = details;
            activity.Name = "TOHX";

            try
            {
                if (activity.State != "In Menus")
                {
                    if (!DataManager.Settings.Gameplay.StreamerMode)
                    {
                        int maxSize = GameOptionsManager.Instance.currentNormalGameOptions.MaxPlayers;
                        if (GameStates.IsLobby)
                        {
                            lobbycode = GameStartManager.Instance.GameRoomNameCode.text;
                            region = ServerManager.Instance.CurrentRegion.Name;
                            if (region == "North America") region = "NA";
                            if (region == "Europe") region = "EU";
                            if (region == "Asia") region = "AS";
                        }

                        if (lobbycode != "" && region != "")
                        {
                            details = $"TOHX - {lobbycode} ({region})";
                        }

                        activity.Details = details;
                    }
                    else
                    {
                        details = $"TOHX v{Main.PluginDisplayVersion}";
                    }
                }
            }

            catch (ArgumentException ex)
            {
                Logger.Error("Error in updating discord rpc", "DiscordPatch");
                Logger.Exception(ex, "DiscordPatch");
                details = $"TOHX v{Main.PluginDisplayVersion}";
                activity.Details = details;
            }
        }
    }
}