using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using TOHX;
using TOHX.Roles.Neutral;
using static TOHX.Translator;

namespace TOHX;

[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
class GameEndChecker
{
    private static GameEndPredicate predicate;
    public static bool Prefix()
    {
        if (!AmongUsClient.Instance.AmHost) return true;

        //Suspended if the game has already been determined to be over. (Translation by deepl.com)
        if (predicate == null) return false;

        //Suspended when in non-end-of-game mode and not in an abandoned village???? (Translation by deepl.com)
        if (Options.NoGameEnd.GetBool() && CustomWinnerHolder.WinnerTeam is not CustomWinner.Draw and not CustomWinner.Error) return false;

        //Default for abandoned villages???? (Translation by deepl.com)
        var reason = GameOverReason.ImpostorByKill;

        //Game end decision (Translation by deepl.com)
        predicate.CheckForEndGame(out reason);
        
        // SoloKombat
        if (Options.CurrentGameMode == CustomGameMode.SoloKombat)
        {
            if (CustomWinnerHolder.WinnerIds.Any() || CustomWinnerHolder.WinnerTeam != CustomWinner.Default)
            {
                ShipStatus.Instance.enabled = false;
                StartEndGame(reason);
                predicate = null;
            }
            return false;
        }

        //At the end of the game (Translation by deepl.com)
        if (CustomWinnerHolder.WinnerTeam != CustomWinner.Default)
        {
            //Camouflage forced removal?? (Translation by deepl.com)
            Main.AllPlayerControls.Do(pc => Camouflage.RpcSetSkin(pc, ForceRevert: true, RevertToDefault: true));

            if (reason == GameOverReason.ImpostorBySabotage && (CustomRoles.Jackal.RoleExist() || CustomRoles.Sidekick.RoleExist()) && Jackal.CanWinBySabotageWhenNoImpAlive.GetBool() && !Main.AllAlivePlayerControls.Any(x => x.GetCustomRole().IsImpostorTeam()))
            {
                reason = GameOverReason.ImpostorByKill;
                CustomWinnerHolder.WinnerIds.Clear();
                CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Jackal);
                CustomWinnerHolder.WinnerRoles.Add(CustomRoles.Jackal);
            }

            switch (CustomWinnerHolder.WinnerTeam)
            {
                case CustomWinner.Crewmate:
                    Main.AllPlayerControls
                        .Where(pc => pc.Is(CustomRoleTypes.Crewmate) && !pc.Is(CustomRoles.Madmate) && !pc.Is(CustomRoles.Rogue) && !pc.Is(CustomRoles.Charmed) && !pc.Is(CustomRoles.Recruit) && !pc.Is(CustomRoles.Infected) && !pc.Is(CustomRoles.Contagious) && !pc.Is(CustomRoles.EvilSpirit) && !pc.Is(CustomRoles.Recruit) || pc.Is(CustomRoles.Admired))
                        .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
                    break;
                case CustomWinner.Impostor:
                    Main.AllPlayerControls
                        .Where(pc => (pc.Is(CustomRoleTypes.Impostor) || pc.Is(CustomRoles.Madmate) || pc.Is(CustomRoles.Crewpostor) || pc.Is(CustomRoles.Parasite) || pc.Is(CustomRoles.Troll) || pc.Is(CustomRoles.Refugee) || pc.Is(CustomRoles.Convict)) && !pc.Is(CustomRoles.Rogue) && !pc.Is(CustomRoles.Charmed) && !pc.Is(CustomRoles.Recruit) && !pc.Is(CustomRoles.Infected) && !pc.Is(CustomRoles.Contagious) && !pc.Is(CustomRoles.EvilSpirit) && !pc.Is(CustomRoles.Recruit) && !pc.Is(CustomRoles.Admired))
                        .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
                    break;
                case CustomWinner.Succubus:
                    Main.AllPlayerControls
                        .Where(pc => pc.Is(CustomRoles.Succubus) || pc.Is(CustomRoles.Charmed) && !pc.Is(CustomRoles.Rogue) && !pc.Is(CustomRoles.Admired))
                        .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
                    break;
                case CustomWinner.CursedSoul:
                    Main.AllPlayerControls
                        .Where(pc => pc.Is(CustomRoles.CursedSoul) || pc.Is(CustomRoles.Soulless) && !pc.Is(CustomRoles.Rogue) && !pc.Is(CustomRoles.Admired))
                        .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
                    break;
                case CustomWinner.Infectious:
                    Main.AllPlayerControls
                        .Where(pc => pc.Is(CustomRoles.Infectious) || pc.Is(CustomRoles.Infected) && !pc.Is(CustomRoles.Rogue) && !pc.Is(CustomRoles.Admired))
                        .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
                    break;
                case CustomWinner.Virus:
                    Main.AllPlayerControls
                        .Where(pc => pc.Is(CustomRoles.Virus) || pc.Is(CustomRoles.Contagious) && !pc.Is(CustomRoles.Rogue) && !pc.Is(CustomRoles.Admired))
                        .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
                    break;
                case CustomWinner.Jackal:
                    Main.AllPlayerControls
                        .Where(pc => (pc.Is(CustomRoles.Jackal) || pc.Is(CustomRoles.Sidekick) || pc.Is(CustomRoles.Recruit)) && !pc.Is(CustomRoles.Infected) && !pc.Is(CustomRoles.Rogue) && !pc.Is(CustomRoles.Admired))
                        .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
                    break;
                case CustomWinner.Spiritcaller:
                    Main.AllPlayerControls
                        .Where(pc => (pc.Is(CustomRoles.Spiritcaller) || pc.Is(CustomRoles.EvilSpirit)))
                        .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
                    break;
                case CustomWinner.RuthlessRomantic:
                    foreach (var pc in Main.AllPlayerControls)
                    {
                        if (pc.Is(CustomRoles.RuthlessRomantic))
                        {
                            CustomWinnerHolder.WinnerIds.Add(Romantic.BetPlayer[pc.PlayerId]);
                            
                        }

                    }
                    break;
            }
            if (CustomWinnerHolder.WinnerTeam is not CustomWinner.Draw and not CustomWinner.None and not CustomWinner.Error)
            {

                //Stalker snatches victory?? (Translation by deepl.com)
                foreach (var pc in Main.AllPlayerControls)
                {
                    if (pc.Is(CustomRoles.DarkHide) && !pc.Data.IsDead
                        && ((CustomWinnerHolder.WinnerTeam == CustomWinner.Impostor && !reason.Equals(GameOverReason.ImpostorBySabotage)) || CustomWinnerHolder.WinnerTeam == CustomWinner.DarkHide
                        || (CustomWinnerHolder.WinnerTeam == CustomWinner.Crewmate && !reason.Equals(GameOverReason.HumansByTask) && (DarkHide.IsWinKill[pc.PlayerId] == true && DarkHide.SnatchesWin.GetBool()))))
                    {
                        CustomWinnerHolder.ResetAndSetWinner(CustomWinner.DarkHide);
                        CustomWinnerHolder.WinnerIds.Add(pc.PlayerId);
                    }
                }
                foreach (var pc in Main.AllPlayerControls)
                {
                    if (pc.Is(CustomRoles.Phantom) && pc.GetPlayerTaskState().IsTaskFinished && pc.Data.IsDead
                        && (((CustomWinnerHolder.WinnerTeam == CustomWinner.Impostor || CustomWinnerHolder.WinnerTeam == CustomWinner.Crewmate || CustomWinnerHolder.WinnerTeam == CustomWinner.Jackal || CustomWinnerHolder.WinnerTeam == CustomWinner.BloodKnight || CustomWinnerHolder.WinnerTeam == CustomWinner.SerialKiller || CustomWinnerHolder.WinnerTeam == CustomWinner.Juggernaut || CustomWinnerHolder.WinnerTeam == CustomWinner.PotionMaster || CustomWinnerHolder.WinnerTeam == CustomWinner.Poisoner || CustomWinnerHolder.WinnerTeam == CustomWinner.Succubus || CustomWinnerHolder.WinnerTeam == CustomWinner.Infectious  || CustomWinnerHolder.WinnerTeam == CustomWinner.Jinx || CustomWinnerHolder.WinnerTeam == CustomWinner.Virus || CustomWinnerHolder.WinnerTeam == CustomWinner.Arsonist || CustomWinnerHolder.WinnerTeam == CustomWinner.Pelican || CustomWinnerHolder.WinnerTeam == CustomWinner.Occultist || CustomWinnerHolder.WinnerTeam == CustomWinner.Shade || CustomWinnerHolder.WinnerTeam == CustomWinner.Agitater || CustomWinnerHolder.WinnerTeam == CustomWinner.Pestilence || CustomWinnerHolder.WinnerTeam == CustomWinner.Rogue || CustomWinnerHolder.WinnerTeam == CustomWinner.Spiritcaller) && (Options.PhantomSnatchesWin.GetBool()))))
                    {
                        reason = GameOverReason.ImpostorByKill;
                        CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Phantom);
                        CustomWinnerHolder.WinnerIds.Add(pc.PlayerId);
                    }
                }
                foreach (var pc in Main.AllPlayerControls)
                {
                    if (pc.Is(CustomRoles.Famine) && !pc.Data.IsDead
                        && (((CustomWinnerHolder.WinnerTeam == CustomWinner.Impostor || CustomWinnerHolder.WinnerTeam == CustomWinner.Crewmate || CustomWinnerHolder.WinnerTeam == CustomWinner.Jackal || CustomWinnerHolder.WinnerTeam == CustomWinner.BloodKnight || CustomWinnerHolder.WinnerTeam == CustomWinner.SerialKiller || CustomWinnerHolder.WinnerTeam == CustomWinner.Juggernaut || CustomWinnerHolder.WinnerTeam == CustomWinner.PotionMaster || CustomWinnerHolder.WinnerTeam == CustomWinner.Poisoner || CustomWinnerHolder.WinnerTeam == CustomWinner.Succubus || CustomWinnerHolder.WinnerTeam == CustomWinner.Infectious  || CustomWinnerHolder.WinnerTeam == CustomWinner.Jinx || CustomWinnerHolder.WinnerTeam == CustomWinner.Virus || CustomWinnerHolder.WinnerTeam == CustomWinner.Arsonist || CustomWinnerHolder.WinnerTeam == CustomWinner.Pelican || CustomWinnerHolder.WinnerTeam == CustomWinner.Occultist || CustomWinnerHolder.WinnerTeam == CustomWinner.Shade || CustomWinnerHolder.WinnerTeam == CustomWinner.Agitater || CustomWinnerHolder.WinnerTeam == CustomWinner.Pestilence || CustomWinnerHolder.WinnerTeam == CustomWinner.Rogue))))
                    {
                        reason = GameOverReason.ImpostorByKill;
                        CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Famine);
                        CustomWinnerHolder.WinnerIds.Add(pc.PlayerId);
                    }
                }
                foreach (var pc in Main.AllPlayerControls)
                {
                    if (pc.Is(CustomRoles.CursedSoul) && !pc.Data.IsDead
                        && (((CustomWinnerHolder.WinnerTeam == CustomWinner.Impostor || CustomWinnerHolder.WinnerTeam == CustomWinner.Crewmate || CustomWinnerHolder.WinnerTeam == CustomWinner.Jackal || CustomWinnerHolder.WinnerTeam == CustomWinner.BloodKnight || CustomWinnerHolder.WinnerTeam == CustomWinner.SerialKiller || CustomWinnerHolder.WinnerTeam == CustomWinner.Juggernaut || CustomWinnerHolder.WinnerTeam == CustomWinner.PotionMaster || CustomWinnerHolder.WinnerTeam == CustomWinner.Poisoner || CustomWinnerHolder.WinnerTeam == CustomWinner.Succubus || CustomWinnerHolder.WinnerTeam == CustomWinner.Infectious  || CustomWinnerHolder.WinnerTeam == CustomWinner.Jinx || CustomWinnerHolder.WinnerTeam == CustomWinner.Virus || CustomWinnerHolder.WinnerTeam == CustomWinner.Arsonist || CustomWinnerHolder.WinnerTeam == CustomWinner.Pelican || CustomWinnerHolder.WinnerTeam == CustomWinner.Occultist || CustomWinnerHolder.WinnerTeam == CustomWinner.Shade || CustomWinnerHolder.WinnerTeam == CustomWinner.Agitater || CustomWinnerHolder.WinnerTeam == CustomWinner.Pestilence || CustomWinnerHolder.WinnerTeam == CustomWinner.Rogue || CustomWinnerHolder.WinnerTeam == CustomWinner.Jester || CustomWinnerHolder.WinnerTeam == CustomWinner.Executioner))))
                    {
                        reason = GameOverReason.ImpostorByKill;
                        CustomWinnerHolder.ResetAndSetWinner(CustomWinner.CursedSoul);
                        CustomWinnerHolder.WinnerRoles.Add(CustomRoles.Soulless);
                        CustomWinnerHolder.WinnerIds.Add(pc.PlayerId);
                    }
                }

                //Egoist (Crew)
                if (CustomWinnerHolder.WinnerTeam == CustomWinner.Crewmate)
                {
                    foreach (var pc in Main.AllAlivePlayerControls.Where(x => x.GetCustomRole().IsCrewmate()))
                        if (pc.Is(CustomRoles.Egoist))
                        {
                            reason = GameOverReason.ImpostorByKill;
                            CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Egoist);
                            CustomWinnerHolder.WinnerRoles.Add(CustomRoles.Egoist);
                            CustomWinnerHolder.WinnerIds.Add(pc.PlayerId);
                        }
                }

                //Egoist (Imp)
                if (CustomWinnerHolder.WinnerTeam == CustomWinner.Impostor)
                {
                    foreach (var pc in Main.AllAlivePlayerControls.Where(x => x.GetCustomRole().IsImpostor()))
                        if (pc.Is(CustomRoles.Egoist))
                        {
                            reason = GameOverReason.ImpostorByKill;
                            CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Egoist);
                            CustomWinnerHolder.WinnerIds.Add(pc.PlayerId);
                        }
                }

                //God
                if (CustomRolesHelper.RoleExist(CustomRoles.God))
                {
                    CustomWinnerHolder.ResetAndSetWinner(CustomWinner.God);
                    Main.AllPlayerControls
                        .Where(p => p.Is(CustomRoles.God) && p.IsAlive())
                        .Do(p => CustomWinnerHolder.WinnerIds.Add(p.PlayerId));
                }

                //Lovers
                else if (CustomRolesHelper.RoleExist(CustomRoles.Lovers) && !reason.Equals(GameOverReason.HumansByTask))
                {
                    if (!(!Main.LoversPlayers.ToArray().All(p => p.IsAlive()) && Options.LoverSuicide.GetBool()))
                    {
                        if (CustomWinnerHolder.WinnerTeam is CustomWinner.Crewmate or CustomWinner.Impostor or CustomWinner.Jackal or CustomWinner.Pelican)
                        {
                            CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Lovers);
                            Main.AllPlayerControls
                                .Where(p => p.Is(CustomRoles.Lovers))
                                .Do(p => CustomWinnerHolder.WinnerIds.Add(p.PlayerId));
                        }
                    }
                }

                //Additional victories
                foreach (var pc in Main.AllPlayerControls)
                {
                    //Opportunist
                    if (pc.Is(CustomRoles.Opportunist) && pc.IsAlive())
                    {
                        CustomWinnerHolder.WinnerIds.Add(pc.PlayerId);
                        CustomWinnerHolder.AdditionalWinnerTeams.Add(AdditionalWinners.Opportunist);
                    }
                    //Shaman
                    if (pc.Is(CustomRoles.Shaman) && pc.IsAlive())
                    {
                        CustomWinnerHolder.WinnerIds.Add(pc.PlayerId);
                        CustomWinnerHolder.AdditionalWinnerTeams.Add(AdditionalWinners.Shaman);
                    }
                    //Witch
                    if (pc.Is(CustomRoles.NWitch) && pc.IsAlive() && CustomWinnerHolder.WinnerTeam != CustomWinner.Crewmate && CustomWinnerHolder.WinnerTeam != CustomWinner.Lovers)
                    {
                        CustomWinnerHolder.WinnerIds.Add(pc.PlayerId);
                        CustomWinnerHolder.AdditionalWinnerTeams.Add(AdditionalWinners.Witch);
                    }
                    if (pc.Is(CustomRoles.Pursuer) && pc.IsAlive() && CustomWinnerHolder.WinnerTeam != CustomWinner.Jester && CustomWinnerHolder.WinnerTeam != CustomWinner.Lovers && CustomWinnerHolder.WinnerTeam != CustomWinner.Terrorist && CustomWinnerHolder.WinnerTeam != CustomWinner.Executioner && CustomWinnerHolder.WinnerTeam != CustomWinner.Collector && CustomWinnerHolder.WinnerTeam != CustomWinner.Innocent && CustomWinnerHolder.WinnerTeam != CustomWinner.Youtuber)
                    {
                        CustomWinnerHolder.WinnerIds.Add(pc.PlayerId);
                        CustomWinnerHolder.AdditionalWinnerTeams.Add(AdditionalWinners.Pursuer);
                    }
                    //Sunnyboy
                    if (pc.Is(CustomRoles.Sunnyboy) && !pc.IsAlive())
                    {
                        CustomWinnerHolder.WinnerIds.Add(pc.PlayerId);
                        CustomWinnerHolder.AdditionalWinnerTeams.Add(AdditionalWinners.Sunnyboy);
                    }
                    if (pc.Is(CustomRoles.Maverick) && pc.IsAlive())
                    {
                        CustomWinnerHolder.WinnerIds.Add(pc.PlayerId);
                        CustomWinnerHolder.AdditionalWinnerTeams.Add(AdditionalWinners.Maverick);
                    }
                    if (!Options.PhantomSnatchesWin.GetBool())
                    {
                        if (pc.Is(CustomRoles.Phantom) && !pc.IsAlive() && pc.GetPlayerTaskState().IsTaskFinished)
                        {
                            CustomWinnerHolder.WinnerIds.Add(pc.PlayerId);
                            CustomWinnerHolder.AdditionalWinnerTeams.Add(AdditionalWinners.Phantom);
                        }
                    }
                    //自爆卡车来咯
                    if (pc.Is(CustomRoles.Provocateur) && Main.Provoked.TryGetValue(pc.PlayerId, out var tar))
                    {
                        if (!CustomWinnerHolder.WinnerIds.Contains(tar))
                        {
                            CustomWinnerHolder.WinnerIds.Add(pc.PlayerId);
                            CustomWinnerHolder.AdditionalWinnerTeams.Add(AdditionalWinners.Provocateur);
                        }
                    }
                }

                //Lovers follow winner
                if (CustomWinnerHolder.WinnerTeam is not CustomWinner.Lovers)
                {
                    foreach (var pc in Main.AllPlayerControls.Where(x => x.Is(CustomRoles.Lovers)))
                    {
                        if (CustomWinnerHolder.WinnerIds.Where(x => Utils.GetPlayerById(x).Is(CustomRoles.Lovers)).Any())
                        {
                            CustomWinnerHolder.WinnerIds.Add(pc.PlayerId);
                            CustomWinnerHolder.AdditionalWinnerTeams.Add(AdditionalWinners.Lovers);
                        }
                    }
                }
                

                //FFF
                if (CustomWinnerHolder.WinnerTeam != CustomWinner.Lovers && !CustomWinnerHolder.AdditionalWinnerTeams.Contains(AdditionalWinners.Lovers) && !CustomRolesHelper.RoleExist(CustomRoles.Lovers) && !CustomRolesHelper.RoleExist(CustomRoles.Ntr))
                {
                    foreach (var pc in Main.AllPlayerControls.Where(x => x.Is(CustomRoles.FFF)))
                    {
                        if (Main.AllPlayerControls.Where(x => (x.Is(CustomRoles.Lovers) || x.Is(CustomRoles.Ntr)) && x.GetRealKiller()?.PlayerId == pc.PlayerId).Any())
                        {
                            CustomWinnerHolder.WinnerIds.Add(pc.PlayerId);
                            CustomWinnerHolder.AdditionalWinnerTeams.Add(AdditionalWinners.FFF);
                        }
                    }
                }
                
                foreach (var pc in Main.AllPlayerControls.Where(x => x.Is(CustomRoles.Totocalcio)))
                {
                    if (Totocalcio.BetPlayer.TryGetValue(pc.PlayerId, out var betTarget) && (
                        CustomWinnerHolder.WinnerIds.Contains(betTarget) ||
                        (Main.PlayerStates.TryGetValue(betTarget, out var ps) && CustomWinnerHolder.WinnerRoles.Contains(ps.MainRole)
                        )))
                    {
                        CustomWinnerHolder.WinnerIds.Add(pc.PlayerId);
                        CustomWinnerHolder.AdditionalWinnerTeams.Add(AdditionalWinners.Totocalcio);
                    }
                }
                //Romantic win condition
                foreach (var pc in Main.AllPlayerControls.Where(x => x.Is(CustomRoles.Romantic)))
                {
                    if (Romantic.BetPlayer.TryGetValue(pc.PlayerId, out var betTarget) && (
                        CustomWinnerHolder.WinnerIds.Contains(betTarget) ||
                        (Main.PlayerStates.TryGetValue(betTarget, out var ps) && CustomWinnerHolder.WinnerRoles.Contains(ps.MainRole)
                        )))
                    {
                        CustomWinnerHolder.WinnerIds.Add(pc.PlayerId);
                        CustomWinnerHolder.AdditionalWinnerTeams.Add(AdditionalWinners.Romantic);
                    }
                }
                foreach (var pc in Main.AllPlayerControls.Where(x => x.Is(CustomRoles.RuthlessRomantic)))
                {
                    if (Romantic.BetPlayer.TryGetValue(pc.PlayerId, out var betTarget) && (
                        CustomWinnerHolder.WinnerIds.Contains(betTarget) ||
                        (Main.PlayerStates.TryGetValue(betTarget, out var ps) && CustomWinnerHolder.WinnerRoles.Contains(ps.MainRole)
                        )))
                    {
                        CustomWinnerHolder.WinnerIds.Add(pc.PlayerId);
                        CustomWinnerHolder.AdditionalWinnerTeams.Add(AdditionalWinners.RuthlessRomantic);
                    }
                }
                
                //Vengeful Romantic win condition
                foreach (var pc in Main.AllPlayerControls.Where(x => x.Is(CustomRoles.VengefulRomantic)))
                {
                    if (VengefulRomantic.hasKilledKiller)
                    {
                        CustomWinnerHolder.WinnerIds.Add(pc.PlayerId);
                        CustomWinnerHolder.WinnerIds.Add(Romantic.BetPlayer[pc.PlayerId]);
                        //if ((Romantic.BetPlayer.TryGetValue(pc.PlayerId, out var RomanticPartner)) && pc.PlayerId == RomanticPartner)
                        //    CustomWinnerHolder.WinnerIds.Add(RomanticPartner);
                        CustomWinnerHolder.AdditionalWinnerTeams.Add(AdditionalWinners.VengefulRomantic);
                    }
                }
                //Lawyer win cond
                foreach (var pc in Main.AllPlayerControls.Where(x => x.Is(CustomRoles.Lawyer)))
                {
                    if (Lawyer.Target.TryGetValue(pc.PlayerId, out var lawyertarget) && (
                        CustomWinnerHolder.WinnerIds.Contains(lawyertarget) ||
                        (Main.PlayerStates.TryGetValue(lawyertarget, out var ps) && CustomWinnerHolder.WinnerRoles.Contains(ps.MainRole)
                        )))
                    {
                        CustomWinnerHolder.WinnerIds.Add(pc.PlayerId);
                        CustomWinnerHolder.AdditionalWinnerTeams.Add(AdditionalWinners.Lawyer);
                    }
                }

                //Supplementing the Lovers' Victory List
                if (CustomWinnerHolder.WinnerTeam == CustomWinner.Lovers || CustomWinnerHolder.AdditionalWinnerTeams.Contains(AdditionalWinners.Lovers))
                {
                    Main.AllPlayerControls
                        .Where(p => p.Is(CustomRoles.Lovers) && !CustomWinnerHolder.WinnerIds.Contains(p.PlayerId))
                        .Do(p => CustomWinnerHolder.WinnerIds.Add(p.PlayerId));
                }

                //Neutral Win Together
                if (Options.NeutralWinTogether.GetBool() && !CustomWinnerHolder.WinnerIds.Where(x => Utils.GetPlayerById(x) != null && Utils.GetPlayerById(x).GetCustomRole().IsCrewmate() && Utils.GetPlayerById(x).GetCustomRole().IsImpostor() && Utils.GetPlayerById(x).GetCustomRole().IsCoven()).Any())
                {
                    foreach (var pc in Main.AllPlayerControls)
                        if (pc.GetCustomRole().IsNeutral() && !pc.GetCustomRole().IsCoven() && !CustomWinnerHolder.WinnerIds.Contains(pc.PlayerId) && !CustomWinnerHolder.WinnerRoles.Contains(pc.GetCustomRole()))
                            CustomWinnerHolder.WinnerIds.Add(pc.PlayerId);
                }
                else if (!Options.NeutralWinTogether.GetBool() && Options.NeutralRoleWinTogether.GetBool())
                {
                    foreach (var id in CustomWinnerHolder.WinnerIds)
                    {
                        var pc = Utils.GetPlayerById(id);
                        if (pc == null || !pc.GetCustomRole().IsNeutral()) continue;
                        if (pc.GetCustomRole().IsCoven()) continue;
                        foreach (var tar in Main.AllPlayerControls)
                            if (!CustomWinnerHolder.WinnerIds.Contains(tar.PlayerId) && tar.GetCustomRole() == pc.GetCustomRole())
                                CustomWinnerHolder.WinnerIds.Add(tar.PlayerId);
                    }
                }

            }
            ShipStatus.Instance.enabled = false;
            StartEndGame(reason);
            predicate = null;
        }
        return false;
    }
    public static void StartEndGame(GameOverReason reason)
    {
        var sender = new CustomRpcSender("EndGameSender", SendOption.Reliable, true);
        sender.StartMessage(-1); // 5: GameData
        MessageWriter writer = sender.stream;

        //ゴーストロール化
        List<byte> ReviveRequiredPlayerIds = new();
        var winner = CustomWinnerHolder.WinnerTeam;
        foreach (var pc in Main.AllPlayerControls)
        {
            if (winner == CustomWinner.Draw)
            {
                SetGhostRole(ToGhostImpostor: true);
                continue;
            }
            bool canWin = CustomWinnerHolder.WinnerIds.Contains(pc.PlayerId) ||
                    CustomWinnerHolder.WinnerRoles.Contains(pc.GetCustomRole());
            bool isCrewmateWin = reason.Equals(GameOverReason.HumansByVote) || reason.Equals(GameOverReason.HumansByTask);
            SetGhostRole(ToGhostImpostor: canWin ^ isCrewmateWin);

            void SetGhostRole(bool ToGhostImpostor)
            {
                if (!pc.Data.IsDead) ReviveRequiredPlayerIds.Add(pc.PlayerId);
                if (ToGhostImpostor)
                {
                    Logger.Info($"{pc.GetNameWithRole()}: ImpostorGhostに変更", "ResetRoleAndEndGame");
                    sender.StartRpc(pc.NetId, RpcCalls.SetRole)
                        .Write((ushort)RoleTypes.ImpostorGhost)
                        .EndRpc();
                    pc.SetRole(RoleTypes.ImpostorGhost);
                }
                else
                {
                    Logger.Info($"{pc.GetNameWithRole()}: CrewmateGhostに変更", "ResetRoleAndEndGame");
                    sender.StartRpc(pc.NetId, RpcCalls.SetRole)
                        .Write((ushort)RoleTypes.CrewmateGhost)
                        .EndRpc();
                    pc.SetRole(RoleTypes.Crewmate);
                }
            }
            SetEverythingUpPatch.LastWinsReason = winner is CustomWinner.Crewmate or CustomWinner.Impostor ? GetString($"GameOverReason.{reason}") : "";
        }

        // CustomWinnerHolderの情報の同期
        sender.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EndGame);
        CustomWinnerHolder.WriteTo(sender.stream);
        sender.EndRpc();

        // GameDataによる蘇生処理
        writer.StartMessage(1); // Data
        {
            writer.WritePacked(GameData.Instance.NetId); // NetId
            foreach (var info in GameData.Instance.AllPlayers)
            {
                if (ReviveRequiredPlayerIds.Contains(info.PlayerId))
                {
                    // 蘇生&メッセージ書き込み
                    info.IsDead = false;
                    writer.StartMessage(info.PlayerId);
                    info.Serialize(writer);
                    writer.EndMessage();
                }
            }
            writer.EndMessage();
        }

        sender.EndMessage();

        // バニラ側のゲーム終了RPC
        writer.StartMessage(8); //8: EndGame
        {
            writer.Write(AmongUsClient.Instance.GameId); //GameId
            writer.Write((byte)reason); //GameoverReason
            writer.Write(false); //showAd
        }
        writer.EndMessage();

        sender.SendMessage();
    }

    public static void SetPredicateToNormal() => predicate = new NormalGameEndPredicate();
    public static void SetPredicateToSoloKombat() => predicate = new SoloKombatGameEndPredicate();

    // ===== ゲーム終了条件 =====
    // 通常ゲーム用
    class NormalGameEndPredicate : GameEndPredicate
    {
        public override bool CheckForEndGame(out GameOverReason reason)
        {
            reason = GameOverReason.ImpostorByKill;
            if (CustomWinnerHolder.WinnerTeam != CustomWinner.Default) return false;
            if (CheckGameEndByLivingPlayers(out reason)) return true;
            if (CheckGameEndByTask(out reason)) return true;
            if (CheckGameEndBySabotage(out reason)) return true;

            return false;
        }

        public bool CheckGameEndByLivingPlayers(out GameOverReason reason)
        {
            reason = GameOverReason.ImpostorByKill;

            if (CustomRoles.Sunnyboy.RoleExist() && Main.AllAlivePlayerControls.Count() > 1) return false;
            var neutralRoleCounts = new Dictionary<CountTypes, int>();
            var apcList = Main.AllAlivePlayerControls.ToList();
            int dual = 0, impCount = 0, crewCount = 0;

            for (int i = 0; i < apcList.Count; i++)
            {
                PlayerControl pc = apcList[i];
                if (pc == null) continue;

                dual = pc.Is(CustomRoles.DualPersonality) ? 1 : 0;
                var countType = Main.PlayerStates[pc.PlayerId].countTypes;
                switch (countType)
                {
                    case CountTypes.OutOfGame:
                    case CountTypes.None:
                    case CountTypes.NWitch:
                        continue;
                    case CountTypes.Impostor:
                        impCount++;
                        impCount += dual;
                        break;
                    case CountTypes.Crew:
                        crewCount++;
                        crewCount += dual;
                        break;
                    default:
                        if (neutralRoleCounts.ContainsKey(countType))
                            neutralRoleCounts[countType]++;
                        else
                            neutralRoleCounts[countType] = 1;
                        neutralRoleCounts[countType] += dual;
                        break;
                }
            }

            int totalNKAlive = neutralRoleCounts.Sum(kvp => kvp.Value);

            if (Main.AllAlivePlayerControls.Any() && Main.AllAlivePlayerControls.All(p => p.Is(CustomRoles.Lovers))) // if lover is alive lover wins
            {
                reason = GameOverReason.ImpostorByKill;
                CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Lovers);
                return true;
            }

            else if (totalNKAlive == 0) // total number of nks alive 0
            {
                if (crewCount == 0 && impCount == 0) // Crew and Imp both 0, everyone is dead
                {
                    reason = GameOverReason.ImpostorByKill;
                    CustomWinnerHolder.ResetAndSetWinner(CustomWinner.None);
                }

                else if (crewCount <= impCount) // Crew less than or equal to Imps, Imp wins
                {
                    reason = GameOverReason.ImpostorByKill;
                    CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Impostor);
                }
                else if (impCount == 0) // Remaining Imps are 0, Crew wins (neutral is already dead)
                {
                    reason = GameOverReason.HumansByVote;
                    CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Crewmate);
                }
                return true;
            }

            else
            {
                if (impCount >= 1) return false; // Both Imp and NK are alive, the game must continue
                if (crewCount > totalNKAlive) return false; // Imps are dead, but Crew still outnumbers NK (the game must continue)
                else // Imps dead, Crew <= NK, Checking if All nk alive are in 1 team 
                {
                    var winners = neutralRoleCounts.Where(kvp => kvp.Value == totalNKAlive).ToList();
                    var winnnerLength = winners.Count;
                    if (winnnerLength == 1)
                    {
                        var winnerRole = winners[0].Key.GetNeutralCustomRoleFromCountType();
                        reason = GameOverReason.ImpostorByKill;
                        CustomWinnerHolder.ResetAndSetWinner(winnerRole.GetNeutralCustomWinnerFromRole());
                        CustomWinnerHolder.WinnerRoles.Add(winnerRole);
                    }
                    else if (winnnerLength == 0)
                    {
                        return false; // Not all alive neutrals were in one team
                    }
                    return true;
                }
            }
        }
    }
}

// 个人竞技模式用
class SoloKombatGameEndPredicate : GameEndPredicate
{
    public override bool CheckForEndGame(out GameOverReason reason)
    {
        reason = GameOverReason.ImpostorByKill;
        if (CustomWinnerHolder.WinnerIds.Any()) return false;
        if (CheckGameEndByLivingPlayers(out reason)) return true;
        return false;
    }

    public bool CheckGameEndByLivingPlayers(out GameOverReason reason)
    {
        reason = GameOverReason.ImpostorByKill;

        if (SoloKombatManager.RoundTime > 0) return false;

        var list = Main.AllPlayerControls.Where(x => !x.Is(CustomRoles.GM) && SoloKombatManager.GetRankOfScore(x.PlayerId) == 1);
        var winner = list.FirstOrDefault();

        CustomWinnerHolder.WinnerIds = new()
            {
                winner.PlayerId
            };

        Main.DoBlockNameChange = true;

        return true;
    }
}

public abstract class GameEndPredicate
{
    /// <summary>ゲームの終了条件をチェックし、CustomWinnerHolderに値を格納します。</summary>
    /// <params name="reason">バニラのゲーム終了処理に使用するGameOverReason</params>
    /// <returns>ゲーム終了の条件を満たしているかどうか</returns>
    public abstract bool CheckForEndGame(out GameOverReason reason);

    /// <summary>GameData.TotalTasksとCompletedTasksをもとにタスク勝利が可能かを判定します。</summary>
    public virtual bool CheckGameEndByTask(out GameOverReason reason)
    {
        reason = GameOverReason.ImpostorByKill;
        if (Options.DisableTaskWin.GetBool() || TaskState.InitialTotalTasks == 0) return false;

        if (GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
        {
            reason = GameOverReason.HumansByTask;
            CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Crewmate);
            return true;
        }
        return false;
    }
    /// <summary>ShipStatus.Systems内の要素をもとにサボタージュ勝利が可能かを判定します。</summary>
    public virtual bool CheckGameEndBySabotage(out GameOverReason reason)
    {
        reason = GameOverReason.ImpostorByKill;
        if (ShipStatus.Instance.Systems == null) return false;

        // TryGetValueは使用不可
        var systems = ShipStatus.Instance.Systems;
        LifeSuppSystemType LifeSupp;
        if (systems.ContainsKey(SystemTypes.LifeSupp) && // サボタージュ存在確認
            (LifeSupp = systems[SystemTypes.LifeSupp].TryCast<LifeSuppSystemType>()) != null && // キャスト可能確認
            LifeSupp.Countdown < 0f) // タイムアップ確認
        {
            // 酸素サボタージュ
            CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Impostor);
            reason = GameOverReason.ImpostorBySabotage;
            LifeSupp.Countdown = 10000f;
            return true;
        }

        ISystemType sys = null;
        if (systems.ContainsKey(SystemTypes.Reactor)) sys = systems[SystemTypes.Reactor];
        else if (systems.ContainsKey(SystemTypes.Laboratory)) sys = systems[SystemTypes.Laboratory];
        else if (systems.ContainsKey(SystemTypes.HeliSabotage)) sys = systems[SystemTypes.HeliSabotage];

        ICriticalSabotage critical;
        if (sys != null && // サボタージュ存在確認
            (critical = sys.TryCast<ICriticalSabotage>()) != null && // キャスト可能確認
            critical.Countdown < 0f) // タイムアップ確認
        {
            // リアクターサボタージュ
            CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Impostor);
            reason = GameOverReason.ImpostorBySabotage;
            critical.ClearSabotage();
            return true;
        }

        return false;
    }
}