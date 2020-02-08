﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild.Archetypes
{
    public class ArrowsongMinstrel
    {
        static public BlueprintArchetype archetype;
        static public BlueprintSpellbook spellbook;

        static public BlueprintBuff precise_minstrel_no_cover_buff;

        static public BlueprintFeature precise_minstrel;
        static public BlueprintFeature arrowsong_strike;
        static public BlueprintFeature arcane_achery;
        static public BlueprintFeature weapon_proficiency;

        static LibraryScriptableObject library => Main.library;


        internal static void create()
        {
            var bard_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");
            createSpellbook();
            createPreciseMinstrel();
            createArrowSongStrike();
            createArcaneArchery();
            createWeaponProficiency();

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "ArrowsongMinstrelArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Arrowsong Minstrel");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Arrowsong minstrels combine the elven traditions of archery, song, and spellcasting into a seamless harmony of dazzling magical effects.");
            });
            Helpers.SetField(archetype, "m_ParentClass", bard_class);
            library.AddAsset(archetype, "");
            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, library.Get<BlueprintFeature>("fa3d3b2211a51994785d85e753f612d3")), //bard proficiencies
                                                          Helpers.LevelEntry(2, bard_class.Progression.LevelEntries[1].Features), //well versed and versatile
                                                          Helpers.LevelEntry(3, bard_class.Progression.LevelEntries[2].Features), //inspire competence
                                                          Helpers.LevelEntry(6, library.Get<BlueprintFeature>("ddaec3a5845bc7d4191792529b687d65")), //fascinate
                                                          Helpers.LevelEntry(7, library.Get<BlueprintFeature>("6d3fcfab6d935754c918eb0e004b5ef7")), //inspire competence
                                                          Helpers.LevelEntry(8, library.Get<BlueprintFeature>("1d48ab2bded57a74dad8af3da07d313a")), //dirge of doom
                                                          Helpers.LevelEntry(11, library.Get<BlueprintFeature>("6d3fcfab6d935754c918eb0e004b5ef7")), //inspire competence
                                                          Helpers.LevelEntry(12, library.Get<BlueprintFeature>("546698146e02d1e4ea00581a3ea7fe58")), //soothing performance
                                                          Helpers.LevelEntry(14, library.Get<BlueprintFeature>("cfd8940869a304f4aa9077415f93febe")), //frightening tune
                                                          Helpers.LevelEntry(15, library.Get<BlueprintFeature>("6d3fcfab6d935754c918eb0e004b5ef7")), //inspire competence
                                                          Helpers.LevelEntry(19, library.Get<BlueprintFeature>("6d3fcfab6d935754c918eb0e004b5ef7")), //inspire competence
                                                       };
            archetype.ReplaceSpellbook = spellbook;
            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, weapon_proficiency),
                                                       Helpers.LevelEntry(1, arcane_achery),
                                                       Helpers.LevelEntry(2, precise_minstrel),
                                                       Helpers.LevelEntry(6, arrowsong_strike)};

            bard_class.Progression.UIDeterminatorsGroup = bard_class.Progression.UIDeterminatorsGroup.AddToArray(weapon_proficiency);
            bard_class.Progression.UIGroups = bard_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(arcane_achery, precise_minstrel, arrowsong_strike));
            bard_class.Archetypes = bard_class.Archetypes.AddToArray(archetype);


            archetype.ReplaceStartingEquipment = true;
            var starting_items = library.Get<BlueprintArchetype>("44388c01eb4a29d4d90a25cc0574320d").StartingItems; //from eldritch archer
            //replace scroll of snowball with cure light wounds
            archetype.StartingItems = starting_items.RemoveFromArray(library.Get<BlueprintItemEquipmentUsable>("66fc961f9c39ae94fb87a79adc87212e")).AddToArray(library.Get<BlueprintItemEquipmentUsable>("cd635d5720937b044a354dba17abad8d"));
            addToPrestigeClasses();
        }


        static void addToPrestigeClasses()
        {
            var bard_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");

            //add check against arrowsong minstrel archetype for exisitng bard spellbooks
            var selections_to_fix = new BlueprintFeatureSelection[] {Common.EldritchKnightSpellbookSelection,
                                                                     Common.ArcaneTricksterSelection,
                                                                     Common.MysticTheurgeArcaneSpellbookSelection,
                                                                     Common.DragonDiscipleSpellbookSelection,
                                                                    };
            foreach (var s in selections_to_fix)
            {
                foreach (var f in s.AllFeatures)
                {
                    if (f.GetComponents<PrerequisiteClassSpellLevel>().Where(c => c.CharacterClass == bard_class).Count() > 0)
                    {
                        f.AddComponent(Common.prerequisiteNoArchetype(bard_class, archetype));
                    }
                }
            }


            Common.addReplaceSpellbook(Common.EldritchKnightSpellbookSelection, spellbook, "EldritchKnightArrowsongMinstrel",
                                       Common.createPrerequisiteClassSpellLevel(bard_class, 3),
                                       Common.createPrerequisiteArchetypeLevel(bard_class, archetype, 1));

            Common.addReplaceSpellbook(Common.ArcaneTricksterSelection, spellbook, "ArcaneTricksterArrowsongMinstrel",
                                       Common.createPrerequisiteClassSpellLevel(bard_class, 2),
                                       Common.createPrerequisiteArchetypeLevel(bard_class, archetype, 1));

            Common.addReplaceSpellbook(Common.MysticTheurgeArcaneSpellbookSelection, spellbook, "MysticTheurgeArrowosngMinstrel",
                                       Common.createPrerequisiteClassSpellLevel(bard_class, 2),
                                       Common.createPrerequisiteArchetypeLevel(bard_class, archetype, 1));

            Common.addReplaceSpellbook(Common.DragonDiscipleSpellbookSelection, spellbook, "DragonDiscipleArrowosngMinstrel",
                                       Common.createPrerequisiteClassSpellLevel(bard_class, 1),
                                       Common.createPrerequisiteArchetypeLevel(bard_class, archetype, 1));
        }


        static void createWeaponProficiency()
        {
            weapon_proficiency = library.CopyAndAdd<BlueprintFeature>("fa3d3b2211a51994785d85e753f612d3", "ArrowsongMinstrelProficiencies", "");
            weapon_proficiency.ReplaceComponent<AddProficiencies>(a => a.WeaponProficiencies = new WeaponCategory[] { WeaponCategory.Shortbow, WeaponCategory.Longbow });
            weapon_proficiency.SetNameDescriptionIcon("Arrowsong Minstrel Proficiencies",
                                                      "An Arrowsong minstrel is proficient with longbows, but not the longsword and rapier.",
                                                      library.Get<BlueprintAbility>("3e9d1119d43d07c4c8ba9ebfd1671952").Icon);//gravity bow
        }

        static void createArcaneArchery()
        {
            var bard_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");
            var icon = library.Get<BlueprintAbility>("9a46dfd390f943647ab4395fc997936d").Icon; //acid arrow

            arcane_achery = Helpers.CreateFeature("ArrowsongMinstrelArcaneArcheryFeature",
                                                  "Arcane Archery",
                                                  "An Arrowsong minstrel’s skill at ranged martial arts allows her to learn to cast a number of powerful, offensive spells that would otherwise be unavailable to her.\n"
                                                  + "Arrowsong Minstrel adds following spells to her spell list: acid arrow, flame arrow, hurricane bow, greater magic weapon, magic weapon, protection from arrows, true strike, and sorcerer/wizard spells of the evocation school. An Arrowsong minstrel must still select these spells as spells known before she can cast them.\n"
                                                  + "In addition, for the purpose of meeting the requirements of combat feats and prestige classes, an Arrowsong minstrel treats her bard level as her base attack bonus (in addition to base attack bonuses gained from other classes and Hit Dice).\n"
                                                  + "An Arrowsong minstrel casts one fewer spell of each level than normal. If this reduces the number to 0, she can cast spells of that level only if her Charisma score allows bonus spells of that level.",
                                                  "",
                                                  icon,
                                                  FeatureGroup.None,
                                                  Common.createReplace34BabWithClassLevel(bard_class)
                                                  );
        }


        static void createArrowSongStrike()
        {
            var bard_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");
            var add_spell_combat = Helpers.Create<AddMagusMechanicPart>(); //needed for unit_part magus creation (no actual ability though)
            Helpers.SetField(add_spell_combat, "m_Feature", 1);
            Helpers.SetField(add_spell_combat, "m_MagusClass", bard_class);

            var add_eldritch_archer = Helpers.Create<AddMagusMechanicPart>();
            Helpers.SetField(add_eldritch_archer, "m_Feature", 5);

            var add_spellstrike = Helpers.Create<AddMagusMechanicPart>();
            Helpers.SetField(add_spellstrike, "m_Feature", 2);

            var spellstrike = library.CopyAndAdd<BlueprintActivatableAbility>("e958891ef90f7e142a941c06c811181e", "ArrowsongMinstrelSpellstrike", "");
            spellstrike.SetName("Arrowsong Strike");
            spellstrike.SetDescription("At 6th level, an Arrowsong minstrel can use spellstrike (as per the magus class feature) to cast a single-target ranged touch attack spell and deliver it through a ranged weapon attack.");
            spellstrike.SetIcon(NewSpells.flame_arrow.Icon);
            arrowsong_strike = Common.ActivatableAbilityToFeature(spellstrike, false);
            arrowsong_strike.AddComponents(add_spell_combat, add_eldritch_archer, add_spellstrike);
        }


        static void createPreciseMinstrel()
        {
            var precise_shot = library.Get<BlueprintFeature>("8f3d1e6b4be006f4d896081f2f889665");

            precise_minstrel = Helpers.CreateFeature("PreciseMinstrelFeature",
                                                     "Precise Minstrel",
                                                     "At 2nd level, an Arrowsong minstrel gains Precise Shot as a bonus feat. In addition, any creature that is affected by any of the Arrowsong minstrel’s bardic performance does not provide soft cover to enemies against her ranged attacks with a bow.",
                                                     "",
                                                     precise_shot.Icon,
                                                     FeatureGroup.None,
                                                     Helpers.CreateAddFact(precise_shot)
                                                     );
        }


        static void createSpellbook()
        {
            var bard_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");
            var wizard_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");
            spellbook = Helpers.Create<BlueprintSpellbook>();
            spellbook.name = "ArrowsongMinstrelSpellbook";
            library.AddAsset(spellbook, "");
            spellbook.Name = Helpers.CreateString("ArrowsongMinstrelSpellbook.Name", "Arrowsong Minstrel");
            spellbook.SpellsPerDay = Common.increaseNumSpellsCast("ArrowsongMinstrellSpellPerDay", "", bard_class.Spellbook.SpellsPerDay, -1);
            spellbook.SpellsKnown = bard_class.Spellbook.SpellsKnown;
            spellbook.Spontaneous = true;
            spellbook.IsArcane = true;
            spellbook.AllSpellsKnown = false;
            spellbook.CanCopyScrolls = false;
            spellbook.CastingAttribute = StatType.Charisma;
            spellbook.CharacterClass = bard_class;
            spellbook.CasterLevelModifier = 0;
            spellbook.CantripsType = bard_class.Spellbook.CantripsType;

            spellbook.SpellList = Helpers.Create<BlueprintSpellList>();
            spellbook.SpellList.name = "ArrowsongMinstrelSpellList";
            library.AddAsset(spellbook.SpellList, "");
            spellbook.SpellList.SpellsByLevel = new SpellLevelList[10];

            for (int i = 0; i < spellbook.SpellList.SpellsByLevel.Length; i++)
            {
                spellbook.SpellList.SpellsByLevel[i] = new SpellLevelList(i);
            }
            spellbook.SpellList.SpellsByLevel[0].SpellLevel = 0;

            BlueprintAbility[] extra_spells = new BlueprintAbility[]
            {
                 library.Get<BlueprintAbility>("3e9d1119d43d07c4c8ba9ebfd1671952"), //gravity bow
                 library.Get<BlueprintAbility>("2c38da66e5a599347ac95b3294acbe00"), //true strike
                 NewSpells.magic_weapon,
                 NewSpells.magic_weapon_greater,
                 library.Get<BlueprintAbility>("c28de1f98a3f432448e52e5d47c73208"), //protection from arrows
                 library.Get<BlueprintAbility>("9a46dfd390f943647ab4395fc997936d"), //acid arrow
                 NewSpells.flame_arrow
            };
            //add ranger spells      
            foreach (var spell_level_list in bard_class.Spellbook.SpellList.SpellsByLevel)
            {
                int sp_level = spell_level_list.SpellLevel;
                foreach (var spell in spell_level_list.Spells)
                {
                    if (!spell.IsInSpellList(spellbook.SpellList))
                    {
                        ExtensionMethods.AddToSpellList(spell, spellbook.SpellList, sp_level);
                    }
                }
            }
            //add wizard spells 
            foreach (var spell_level_list in wizard_class.Spellbook.SpellList.SpellsByLevel)
            {
                int sp_level = spell_level_list.SpellLevel;
                if (sp_level > 6)
                {
                    continue;
                }

                foreach (var spell in spell_level_list.Spells)
                {
                    if (spell.School != SpellSchool.Evocation && !extra_spells.Contains(spell))
                    {
                        continue;
                    }
                    if (!spell.IsInSpellList(spellbook.SpellList))
                    {
                        ExtensionMethods.AddToSpellList(spell, spellbook.SpellList, sp_level);
                    }
                }
            }

        }
    }
}
