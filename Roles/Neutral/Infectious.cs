﻿using Hazel;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static TOHX.Options;
using static TOHX.Translator;

namespace TOHX.Roles.Neutral;

public static class Infectious
{
    private static readonly int Id = 12000;
    private static List<byte> playerIdList = new();
    public static bool IsEnable = false;

    public static OptionItem BiteCooldown;
   // public static OptionItem BiteCooldownIncrese;
    public static OptionItem BiteMax;
    public static OptionItem KnowTargetRole;
    public static OptionItem TargetKnowOtherTarget;
    public static OptionItem HasImpostorVision;
    public static OptionItem CanVent;
    public static OptionItem HideBittenRolesOnEject;
    

    private static int BiteLimit = new();

    public static void SetupCustomOption()
    {
        SetupSingleRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Infectious, 1, zeroOne: false);
        BiteCooldown = FloatOptionItem.Create(Id + 10, "InfectiousBiteCooldown", new(0f, 180f, 2.5f), 30f, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Infectious])
            .SetValueFormat(OptionFormat.Seconds);
     //   BiteCooldownIncrese = FloatOptionItem.Create(Id + 11, "InfectiousBiteCooldownIncrese", new(0f, 180f, 2.5f), 0f, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Infectious])
    //        .SetValueFormat(OptionFormat.Seconds);
        BiteMax = IntegerOptionItem.Create(Id + 12, "InfectiousBiteMax", new(1, 15, 1), 15, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Infectious])
            .SetValueFormat(OptionFormat.Times);
        KnowTargetRole = BooleanOptionItem.Create(Id + 13, "InfectiousKnowTargetRole", true, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Infectious]);
        TargetKnowOtherTarget = BooleanOptionItem.Create(Id + 14, "InfectiousTargetKnowOtherTarget", true, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Infectious]);
        HasImpostorVision = BooleanOptionItem.Create(Id + 15, "ImpostorVision", true, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Infectious]);
        CanVent = BooleanOptionItem.Create(Id + 17, "CanVent", true, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Infectious]);
     //   HideBittenRolesOnEject = BooleanOptionItem.Create(Id + 17, "HideBittenRolesOnEject", false, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Infectious]);        
    }
    public static void Init()
    {
        playerIdList = new();
        BiteLimit = new();
        IsEnable = false;
    }
    public static void Add(byte playerId)
    {
        playerIdList.Add(playerId);
        BiteLimit = BiteMax.GetInt();
        IsEnable = true;

        if (!AmongUsClient.Instance.AmHost) return;
        if (!Main.ResetCamPlayerList.Contains(playerId))
            Main.ResetCamPlayerList.Add(playerId);
    }
    private static void SendRPC()
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetInfectiousBiteLimit, SendOption.Reliable, -1);
        writer.Write(BiteLimit);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
    public static void ReceiveRPC(MessageReader reader)
    {
        BiteLimit = reader.ReadInt32();
    }
    public static void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = BiteCooldown.GetFloat();
    public static bool CanUseKillButton(PlayerControl player) => !player.Data.IsDead && BiteLimit >= 1;
    public static bool OnCheckMurder(PlayerControl killer, PlayerControl target)
    {
        if (target.Is(CustomRoles.Pestilence)) return true;
        if (target.Is(CustomRoles.Infectious)) return true;
        if (target.Is(CustomRoles.NSerialKiller)) return true;

        if (BiteLimit < 1) return false;
        if (CanBeBitten(target))
        {
            BiteLimit--;
            target.RpcSetCustomRole(CustomRoles.Infected);

            killer.Notify(Utils.ColorString(Utils.GetRoleColor(CustomRoles.Infectious), GetString("InfectiousBittenPlayer")));
            target.Notify(Utils.ColorString(Utils.GetRoleColor(CustomRoles.Infectious), GetString("BittenByInfectious")));
            Utils.NotifyRoles();

            killer.ResetKillCooldown();
            killer.SetKillCooldown();
            if (!Options.DisableShieldAnimations.GetBool()) killer.RpcGuardAndKill(target);
            target.RpcGuardAndKill(killer);
            target.RpcGuardAndKill(target);

            Logger.Info("设置职业:" + target?.Data?.PlayerName + " = " + target.GetCustomRole().ToString() + " + " + CustomRoles.Infected.ToString(), "Assign " + CustomRoles.Infected.ToString());
            if (BiteLimit < 0)
                HudManager.Instance.KillButton.OverrideText($"{GetString("KillButtonText")}");
            Logger.Info($"{killer.GetNameWithRole()} : 剩余{BiteLimit}次招募机会", "Infectious");
            return true;
        }
        if (!CanBeBitten(target) && !target.Is(CustomRoles.Infected))
        {
            killer.RpcMurderPlayerV3(target);
        }
        if (BiteLimit < 0)
            HudManager.Instance.KillButton.OverrideText($"{GetString("KillButtonText")}");
        killer.Notify(Utils.ColorString(Utils.GetRoleColor(CustomRoles.Infectious), GetString("InfectiousInvalidTarget")));
        Logger.Info($"{killer.GetNameWithRole()} : 剩余{BiteLimit}次招募机会", "Infectious");
        return false;
    }
    public static bool KnowRole(PlayerControl player, PlayerControl target)
    {
        if (player.Is(CustomRoles.Infected) && target.Is(CustomRoles.Infectious)) return true;
        if (KnowTargetRole.GetBool() && player.Is(CustomRoles.Infectious) && target.Is(CustomRoles.Infected)) return true;
        if (TargetKnowOtherTarget.GetBool() && player.Is(CustomRoles.Infected) && target.Is(CustomRoles.Infected)) return true;
        return false;
    }
    public static string GetBiteLimit() => Utils.ColorString(BiteLimit >= 1 ? Utils.GetRoleColor(CustomRoles.Infectious).ShadeColor(0.25f) : Color.gray, $"({BiteLimit})");
    public static bool CanBeBitten(this PlayerControl pc)
    {
        return pc != null && (pc.GetCustomRole().IsCrewmate() || pc.GetCustomRole().IsImpostor() || pc.GetCustomRole().IsNK()) && !pc.Is(CustomRoles.Infected) && !pc.Is(CustomRoles.Admired) && !pc.Is(CustomRoles.Loyal) && !pc.Is(CustomRoles.Succubus) && !pc.Is(CustomRoles.Infectious) && !pc.Is(CustomRoles.Virus)
        && !(
            false
            );
    }
}
