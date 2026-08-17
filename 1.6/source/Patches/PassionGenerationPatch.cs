using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Limited_Passions.Patches
{
    [HarmonyPatch(typeof(PawnGenerator), "GenerateSkills")]
    public static class PassionGenerationPatch
    {
        private static void Postfix(Pawn pawn)
        {
            List<SkillRecord> skills = pawn.skills?.skills;
            if (skills == null || skills.Count == 0)
            {
                return;
            }

            HashSet<SkillDef> requiredSkills = null;
            HashSet<SkillDef> conflictingSkills = null;
            List<Trait> traits = pawn.story?.traits?.allTraits;
            if (traits != null)
            {
                foreach (Trait trait in traits)
                {
                    foreach (SkillRecord skill in skills)
                    {
                        if (trait.def.RequiresPassion(skill.def))
                        {
                            (requiredSkills ?? (requiredSkills = new HashSet<SkillDef>())).Add(skill.def);
                        }
                        if (trait.def.ConflictsWithPassion(skill.def))
                        {
                            (conflictingSkills ?? (conflictingSkills = new HashSet<SkillDef>())).Add(skill.def);
                        }
                    }
                }
            }

            HashSet<SkillDef> dropAllSkills = null;
            if (ModsConfig.BiotechActive && pawn.genes != null)
            {
                foreach (Gene gene in pawn.genes.GenesListForReading)
                {
                    if (gene.Active && gene.def.passionMod != null && gene.def.passionMod.modType == PassionMod.PassionModType.DropAll)
                    {
                        (dropAllSkills ?? (dropAllSkills = new HashSet<SkillDef>())).Add(gene.def.passionMod.skill);
                    }
                }
            }

            List<SkillRecord> retained = skills
                .Where(skill => !skill.TotallyDisabled
                    && (conflictingSkills == null || !conflictingSkills.Contains(skill.def))
                    && (dropAllSkills == null || !dropAllSkills.Contains(skill.def)))
                .OrderByDescending(skill => requiredSkills != null && requiredSkills.Contains(skill.def))
                .ThenByDescending(skill => skill.passion)
                .ThenBy(skill => Rand.Value)
                .Take(2)
                .ToList();

            foreach (SkillRecord skill in skills)
            {
                skill.passion = Passion.None;
            }

            if (retained.Count > 0)
            {
                retained[0].passion = Passion.Major;
            }
            if (retained.Count > 1)
            {
                retained[1].passion = Passion.Minor;
            }
        }
    }
}
