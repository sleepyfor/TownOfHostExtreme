using System.Collections.Generic;
using TOHX.Modules;
using static TOHX.Options;

namespace TOHX.Roles.Crewmate
{
    public static class Illusion
    {
        private static readonly int Id = 17800;
        public static OptionItem VentCooldown;
        public static OptionItem VentDuration;
        public static bool IsEnable = false;

        public static void SetupOptions()
        {
            SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Illusion);
            VentCooldown = FloatOptionItem.Create(Id + 2, "IllusionVentCooldown", new(0f, 180f, 2.5f), 12.5f, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Illusion])
               .SetValueFormat(OptionFormat.Seconds);
            VentDuration = FloatOptionItem.Create(Id + 3, "IllusionVentDuration", new(0f, 180f, 1), 10, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Illusion])
             .SetValueFormat(OptionFormat.Seconds);
        }

        public static void Init()
        {
            IsEnable = false;
        }
        public static void Add(byte playerId)
        {
            IsEnable = true;
        }

        public static void OnExitVent(PlayerControl pc)
        {
            pc.RPCPlayCustomSound("Teleport");
            pc.RpcRandomVentTeleport();
        }
    }
}
