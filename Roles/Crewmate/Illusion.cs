using System.Collections.Generic;
using TOHX.Modules;
using static TOHX.Options;

namespace TOHX.Roles.Crewmate
{
    public static class Illusion
    {
        public static List<byte> playerIdList = new();
        private static readonly int Id = 17800;
        public static OptionItem VentCooldown;
        public static OptionItem VentDuration;
        public static bool IsEnable = false;

        public static void SetupOptions()
        {
            SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Illusion);
            VentCooldown = FloatOptionItem.Create(Id + 2, "IllusionVentCooldown", new(0f, 180f, 2.5f), 12.5f, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Illusion])
               .SetValueFormat(OptionFormat.Seconds);
            VentCooldown = FloatOptionItem.Create(Id + 3, "IllusionVentDuration", new(0f, 180f, 1), 10, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Illusion])
             .SetValueFormat(OptionFormat.Seconds);
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

        public static void OnEnterVent(PlayerControl pc)
        {
            pc.RPCPlayCustomSound("Teleport");
            pc.RpcRandomVentTeleport();
        }
    }
}
