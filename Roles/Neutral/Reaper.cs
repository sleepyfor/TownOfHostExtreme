using System.Collections.Generic;
using System.Linq;
using Hazel;
using static GameData;
using static TOHX.Options;
using static TOHX.Translator;

namespace TOHX.Roles.Neutral;
public static class Reaper
{
    public static Dictionary<byte, byte> TargetPlayer = new();
    public static List<byte> playerIdList = new();
    public static OptionItem TargetCooldown;
    private static readonly int Id = 17700;
    public static bool IsEnable = false;

    public static void SetupOptions()
    {
        SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Reaper);
        TargetCooldown = FloatOptionItem.Create(Id + 2, "ReaperTargetCooldown", new(0f, 180f, 2.5f), 8f, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Reaper])
           .SetValueFormat(OptionFormat.Seconds);
    }

    public static void Init()
    {
        playerIdList = new();
        TargetPlayer = new();
        IsEnable = false;
    }
    public static void Add(byte playerId)
    {
        playerIdList.Add(playerId);
        IsEnable = true;

        if (!AmongUsClient.Instance.AmHost) return;
        if (!Main.ResetCamPlayerList.Contains(playerId))
            Main.ResetCamPlayerList.Add(playerId);
    }

    public static void SendRPC(byte reaper, byte target = 0x73, string Progress = "")
    {
        MessageWriter writer;
        switch (Progress)
        {
            case "SetTarget":
                writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetReaperTarget, SendOption.Reliable);
                writer.Write(reaper);
                writer.Write(target);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                break;
            case "WinCheck":
                if (CustomWinnerHolder.WinnerTeam != CustomWinner.Default) break;
                CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Reaper);
                CustomWinnerHolder.WinnerIds.Add(reaper);
                CustomWinnerHolder.WinnerIds.Add(target);
                break;
        }
    }
    public static void ReceiveRPC(MessageReader reader, bool SetTarget)
    {
        if (SetTarget)
        {
            byte Reaper = reader.ReadByte();
            byte Target = reader.ReadByte();
            TargetPlayer[Reaper] = Target;
        }
        else
            TargetPlayer.Remove(reader.ReadByte());
    }

    public static void OnPressKillButton(PlayerControl killer, PlayerControl target)
    {
        TargetPlayer.Clear();
        if (!TargetPlayer.ContainsValue(target.PlayerId)) TargetPlayer.Add(killer.PlayerId, target.PlayerId);
    }

    public static bool OnVoteEnd(PlayerControl reaper, PlayerInfo exiled, bool DecidedWinner)
    {
        foreach (var kvp in TargetPlayer.Where(Reaper => Reaper.Value == exiled.PlayerId))
        {
            var target = Utils.GetPlayerById(kvp.Key);
            if (target == null || !reaper.IsAlive() || target.Data.Disconnected || reaper.Data.Disconnected) continue;
            ReaperWin(reaper, target, DecidedWinner);
            return true;
        }
        return false;
    }

    public static void ReaperWin(PlayerControl reaper, PlayerControl target, bool DecidedWinner)
    {
        if (!DecidedWinner)
            SendRPC(reaper.PlayerId, Progress: "WinCheck");
        else if (reaper.Is(CustomRoles.Reaper))
        {
            CustomWinnerHolder.AdditionalWinnerTeams.Add(AdditionalWinners.Reaper);
            CustomWinnerHolder.WinnerIds.Add(reaper.PlayerId);
            CustomWinnerHolder.WinnerIds.Add(target.PlayerId);
        }
    }

    public static void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = TargetCooldown.GetFloat();

    public static string GetTargetText(PlayerControl target)
    {
        if (GameStates.IsMeeting) return "";
        if (TargetPlayer.TryGetValue(target.PlayerId, out var targetId))
            return $"Current Target: {Utils.GetPlayerById(targetId).name}";
        return "Select a target!";
    }
}
