using AmongUs.GameOptions;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using static TOHE.Options;

namespace TOHE.Roles.Neutral;

public static class CovenLeader
{
    private static readonly int Id = 10350;
    public static List<byte> playerIdList = new();
    public static bool IsEnable = false;

    private static OptionItem ControlCooldown;
    public static OptionItem CanVent;
  //  private static OptionItem HasImpostorVision;

    public static void SetupCustomOption()
    {
        //CovenLeaderは1人固定
        SetupSingleRoleOptions(Id, TabGroup.CovenRoles, CustomRoles.CovenLeader, 1, zeroOne: false);
        ControlCooldown = FloatOptionItem.Create(Id + 12, "ControlCooldown", new(0f, 180f, 1f), 20f, TabGroup.CovenRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.CovenLeader])
            .SetValueFormat(OptionFormat.Seconds);
        CanVent = BooleanOptionItem.Create(Id + 11, "CanVent", false, TabGroup.CovenRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.CovenLeader]);
     //   HasImpostorVision = BooleanOptionItem.Create(Id + 13, "ImpostorVision", true, TabGroup.CovenRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.CovenLeader]);
    }
    public static void Init()
    {
        playerIdList = new();
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
    public static void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = ControlCooldown.GetFloat();
    public static void ApplyGameOptions(IGameOptions opt) => opt.SetVision(true);
    public static void CanUseVent(PlayerControl player)
    {
        bool CovenLeader_canUse = CanVent.GetBool();
        DestroyableSingleton<HudManager>.Instance.ImpostorVentButton.ToggleVisible(CovenLeader_canUse && !player.Data.IsDead);
        player.Data.Role.CanVent = CovenLeader_canUse;
    }
}
