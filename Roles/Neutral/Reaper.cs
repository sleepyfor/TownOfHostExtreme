using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using TOHX.Modules;
using static GameData;
using static TOHX.Options;
using static TOHX.Translator;
using static UnityEngine.GraphicsBuffer;

namespace TOHX.Roles.Neutral;
public static class Reaper
{
    public static Dictionary<byte, byte> TargetPlayer = new();
    public static Dictionary<byte, bool> HasTarget = new();
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
            byte ExecutionerId = reader.ReadByte();
            byte TargetId = reader.ReadByte();
            TargetPlayer[ExecutionerId] = TargetId;
        }
        else
            TargetPlayer.Remove(reader.ReadByte());
    }

    public static bool OnCheckMurder(PlayerControl killer, PlayerControl target)
    {
        if (killer.PlayerId == target.PlayerId) return true;
        if (TargetPlayer.TryGetValue(killer.PlayerId, out var tar) && tar == target.PlayerId) return false;
        if (!HasTarget.TryGetValue(killer.PlayerId, out var hasTarget) && hasTarget) return false;
        HasTarget[killer.PlayerId] = true;
        if (TargetPlayer.TryGetValue(killer.PlayerId, out var originalTarget) && Utils.GetPlayerById(originalTarget) != null)
            Utils.NotifyRoles(SpecifySeer: Utils.GetPlayerById(originalTarget));
        TargetPlayer.Remove(killer.PlayerId);
        TargetPlayer[killer.PlayerId] = target.PlayerId;
        target.RpcSetCustomRole(CustomRoles.Reaped);
        SendRPC(killer.PlayerId);
        SendRPC(target.PlayerId);
        killer.ResetKillCooldown();
        killer.SetKillCooldown();
        killer.Notify(GetString("ReaperTargetPlayer") + Utils.GetPlayerById(TargetPlayer[killer.PlayerId]).name);
        return false;
    }

    public static bool OnVoteEnd(PlayerControl executioner, PlayerInfo exiled, bool DecidedWinner)
    {
        var temp = exiled.PlayerName;
        if (Utils.GetPlayerById(exiled.PlayerId).Is(CustomRoles.Reaped))
        {
            exiled.PlayerName = temp;
            CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Reaper);
            CustomWinnerHolder.WinnerIds.Add(exiled.PlayerId);
            CustomWinnerHolder.WinnerIds.Add(executioner.PlayerId);
        }
        return true;
    }

    public static void ReaperWin(PlayerControl reaper, PlayerControl target, bool DecidedWinner)
    {
        Main.AllPlayerControls
                         .Where(pc => pc.Is(CustomRoles.Succubus) || pc.Is(CustomRoles.Charmed) && !pc.Is(CustomRoles.Rogue) && !pc.Is(CustomRoles.Admired))
                         .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
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
