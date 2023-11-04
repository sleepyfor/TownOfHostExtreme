using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Epic.OnlineServices;

namespace TOHX.Roles.Crewmate
{
    public static class Captain
    {
        public static Dictionary<byte, int> AbilityUses = new();
        public static List<byte> playerIdList = new();
        public static OptionItem CaptainAbilityUses;
        public static OptionItem CaptainDies;
        public static bool IsEnable = false;
        public static int Id = 900600;

        public static void SetupCustomOption()
        {
            Options.SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Captain);
            CaptainAbilityUses = IntegerOptionItem.Create(Id + 10, "CaptainAbilityUses", new(1, 99, 1), 3, TabGroup.CrewmateRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Captain]);
            CaptainDies = BooleanOptionItem.Create(Id + 20, "CaptainCaptainDies", false, TabGroup.CrewmateRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Captain]);
        }

        public static void Init()
        {
            playerIdList = new();
            AbilityUses = new();
            IsEnable = false;
        }

        public static void Add(byte playerId)
        {
            playerIdList.Add(playerId);
            AbilityUses.Add(playerId, CaptainAbilityUses.GetInt());
            IsEnable = true;
        }
        public static string GetUses(byte playerId) => Utils.ColorString(Utils.GetRoleColor(CustomRoles.Sheriff).ShadeColor(0.25f), AbilityUses.TryGetValue(playerId, out var uses) ? $"({uses})" : "Invalid");

    }
}
