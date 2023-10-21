using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TOHX.Roles.Crewmate
{
    public static class Captain
    {
        public static OptionItem CaptainAbilityUses;
        public static OptionItem CaptainDies;
        public static int AbilityUses;
        public static int Id = 900600;

        public static void SetupCustomOption()
        {
            Options.SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Captain);
            CaptainAbilityUses = IntegerOptionItem.Create(Id + 10, "CaptainAbilityUses", new(1, 99, 1), 3, TabGroup.CrewmateRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Captain]);
            CaptainDies = BooleanOptionItem.Create(Id + 20, "CaptainCaptainDies", false, TabGroup.CrewmateRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Captain]);
        }

        public static void Init()
        {
            AbilityUses = CaptainAbilityUses.GetValue();
        }
        public static string GetUses() => Utils.ColorString(Utils.GetRoleColor(CustomRoles.Sheriff).ShadeColor(0.25f), $"{AbilityUses}");

    }
}
