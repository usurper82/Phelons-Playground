﻿using System;
using Trinity.Framework;
using Trinity.Framework.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buddy.Coroutines;
using Trinity.Framework.Objects;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;

namespace Trinity.Components.Coroutines
{
    public class EquipBehavior
    {
        public static EquipBehavior Instance => _instance ?? (_instance = (new EquipBehavior()));
        private static EquipBehavior _instance;

        public EquipBehavior()
        {
            _instance = this;
        }

        public EquipBehavior(Dictionary<Skill, Rune> skills, List<Passive> passives)
        {
            Skills = skills;
            Passives = passives;
        }

        public List<Passive> Passives { get; set; }
        public Dictionary<Skill, Rune> Skills { get; set; }

        public async Task<bool> Execute(Dictionary<Skill, Rune> skills, List<Passive> passives)
        {
            Skills = skills;
            Passives = passives;
            return await Execute();
        }

        private DateTime _lastSkillChange = DateTime.MinValue;

        public async Task<bool> Execute()
        {
            try
            {
                if (ZetaDia.Me == null || ZetaDia.Me.IsDead || ZetaDia.Me.IsGhosted || ZetaDia.Me.IsInCombat || ZetaDia.Me.IsInConversation || ZetaDia.Me.IsInBossEncounter || ZetaDia.Me.LoopingAnimationEndTime != 0)
                {
                    Core.Logger.Log("[Auto Skills] Cannot equip build right now");
                    return false;
                }

                if (DateTime.UtcNow.Subtract(_lastSkillChange).TotalMinutes < 1)
                    return false;

                _lastSkillChange = DateTime.UtcNow;

                if (Skills != null && Skills.Any())
                {
                    await Coroutine.Sleep(250);

                    // Only 'IsPrimary' flagged skills can be assigned to slots 0 and 1

                    KnownSkills = ZetaDia.Me.KnownSkills.ToList().DistinctBy(s => s.SNOPower).ToDictionary(s => s.SNOPower, v => v);

                    Core.Hotbar.Update();

                    var primarySkills = Skills.Where(s => s.Key.IsPrimary).Take(2).ToList();
                    if (primarySkills.Any())
                    {
                        for (int i = 0; i < Math.Min(primarySkills.Count, 2); i++)
                        {
                            var skillDefinition = primarySkills.ElementAtOrDefault(i);
                            if (Core.Hotbar.ActiveSkills.Any(s => s.Power == skillDefinition.Key.SNOPower && (int)s.Slot == i))
                            {
                                Core.Logger.Verbose("[Auto Skills] Skipping Skill (Already Equipped): {0} ({1}) - {2} in slot {3}",
                                    skillDefinition.Key.Name,
                                    (int)skillDefinition.Key.SNOPower,
                                    skillDefinition.Value.Name,
                                    i);

                                continue;
                            }

                            await EquipSkill(skillDefinition, i);

                            await Coroutine.Sleep(500);
                        }
                    }

                    var otherSkills = Skills.Except(primarySkills).ToList();
                    if (otherSkills.Any())
                    {
                        for (int i = 2; i < otherSkills.Count + 2; i++)
                        {
                            await EquipSkill(otherSkills.ElementAtOrDefault(i), i);
                            await Coroutine.Sleep(500);
                        }
                    }
                }

                if (Passives == null || !Passives.Any())
                    return false;

                var validPasives = Passives.Where(p => p.Class == ZetaDia.Me.ActorClass && Core.Player.Level >= p.RequiredLevel).ToList();
                var passivePowers = validPasives.Select(p => p.SNOPower).ToList();

                foreach (var passive in validPasives)
                {
                    Core.Logger.Verbose("[Auto Skills] Selecting Passive: {0} {1} ({2})", passive.Name, passive.SNOPower.ToString(), (int)passive.SNOPower);
                }

                await Coroutine.Sleep(250);

                switch (validPasives.Count)
                {
                    case 1:
                        ZetaDia.Me.SetTraits(passivePowers[0]);
                        break;

                    case 2:
                        ZetaDia.Me.SetTraits(passivePowers[0], passivePowers[1]);
                        break;

                    case 3:
                        ZetaDia.Me.SetTraits(passivePowers[0], passivePowers[1], passivePowers[2]);
                        break;

                    case 4:
                        ZetaDia.Me.SetTraits(passivePowers[0], passivePowers[1], passivePowers[2], passivePowers[3]);
                        break;
                }

                await Coroutine.Sleep(2000);
            }
            catch (Exception ex)
            {
                Core.Logger.Error("[Auto Skills] Exception in Build.EquipBuild(). {0}", ex);
            }

            return true;
        }

        public static Dictionary<int, ActiveSkillEntry> KnownSkills = new Dictionary<int, ActiveSkillEntry>();

        public static async Task<bool> EquipSkill(KeyValuePair<Skill, Rune> skill, int slot)
        {
            if (!ZetaDia.IsInGame || ZetaDia.Me == null || !ZetaDia.Me.IsValid || ZetaDia.Globals.IsLoadingWorld || ZetaDia.Globals.IsPlayingCutscene)
                return false;

            if (skill.Key == null || skill.Value == null || slot < 0)
                return false;

            Core.Logger.Verbose("[Auto Skills] Selecting Skill: {0} ({1}) - {2} in slot {3}",
                skill.Key.Name,
                (int)skill.Key.SNOPower,
                skill.Value.Name,
                slot);

            if (skill.Key.Class != ZetaDia.Me.ActorClass || skill.Value.Class != ZetaDia.Me.ActorClass)
            {
                Core.Logger.Error("[Auto Skills] Attempting to equip skill/rune for the wrong class will crash the game");
                return false;
            }

            var currentLevel = ZetaDia.Me.Level;

            //ActiveSkillEntry knownSkillRecord;
            //if (!KnownSkills.TryGetValue((int)skill.Key.SNOPower, out knownSkillRecord))
            //{
            //    var test = string.Format($"[Auto Skills] Skill is not known: {skill.Key.Name} CurrentLevel={ZetaDia.Me.Level} RequiredLevel={skill.Key.RequiredLevel}");
            //    Core.Logger.Error(test);
            //    return false;
            //}

            //if (currentLevel < knownSkillRecord.RequiredLevel)
            //{
            //    Core.Logger.Error("[Auto Skills] Skill {0} cannot be equipped until level {1}", skill.Key.Name, knownSkillRecord.RequiredLevel);
            //    return false;
            //}

            //if (skill.Value.RuneIndex == -1 && currentLevel < knownSkillRecord.RuneNoneRequiredLevel)
            //{
            //    Core.Logger.Error("[Auto Skills] Skill {0} with Rune {1} cannot be equipped until level {2}", skill.Key.Name, skill.Value.Name, knownSkillRecord.RuneNoneRequiredLevel);
            //    return false;
            //}

            //if (skill.Value.RuneIndex == 1 && currentLevel < knownSkillRecord.Rune1RequiredLevel)
            //{
            //    Core.Logger.Error("[Auto Skills] Skill {0} with Rune {1} cannot be equipped until level {2}", skill.Key.Name, skill.Value.Name, knownSkillRecord.RuneNoneRequiredLevel);
            //    return false;
            //}

            //if (skill.Value.RuneIndex == 2 && currentLevel < knownSkillRecord.Rune2RequiredLevel)
            //{
            //    Core.Logger.Error("[Auto Skills] Skill {0} with Rune {1} cannot be equipped until level {2}", skill.Key.Name, skill.Value.Name, knownSkillRecord.RuneNoneRequiredLevel);
            //    return false;
            //}

            //if (skill.Value.RuneIndex == 3 && currentLevel < knownSkillRecord.Rune3RequiredLevel)
            //{
            //    Core.Logger.Error("[Auto Skills] Skill {0} with Rune {1} cannot be equipped until level {2}", skill.Key.Name, skill.Value.Name, knownSkillRecord.RuneNoneRequiredLevel);
            //    return false;
            //}

            //if (skill.Value.RuneIndex == 4 && currentLevel < knownSkillRecord.Rune4RequiredLevel)
            //{
            //    Core.Logger.Error("[Auto Skills] Skill {0} with Rune {1} cannot be equipped until level {2}", skill.Key.Name, skill.Value.Name, knownSkillRecord.RuneNoneRequiredLevel);
            //    return false;
            //}

            //if (skill.Value.RuneIndex == 5 && currentLevel < knownSkillRecord.Rune5RequiredLevel)
            //{
            //    Core.Logger.Error("[Auto Skills] Skill {0} with Rune {1} cannot be equipped until level {2}", skill.Key.Name, skill.Value.Name, knownSkillRecord.RuneNoneRequiredLevel);
            //    return false;
            //}

            if (currentLevel < skill.Key.RequiredLevel)
            {
                Core.Logger.Error("[Auto Skills] Skill {0} cannot be equipped until level {1}", skill.Key.Name, skill.Key.RequiredLevel);
                return false;
            }

            //await Coroutine.Sleep(500);

            ZetaDia.Me.SetActiveSkill(skill.Key.SNOPower, skill.Value.RuneIndex, (HotbarSlot)slot);

            await Coroutine.Sleep(500);

            return true;// ZetaDia.Storage.PlayerDataManager.ActivePlayerData.GetActiveSkillBySlot((HotbarSlot)slot).Power == skill.Key.SNOPower;
        }
    }
}