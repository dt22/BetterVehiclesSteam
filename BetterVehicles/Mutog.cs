using Base.AI;
using Base.AI.Defs;
using Base.Core;
using Base.Defs;
using Base.Entities.Abilities;
using Base.Entities.Effects;
using Base.Entities.Statuses;
using Base.Levels;
using Base.UI;
using Base.Utils.Maths;
using Code.PhoenixPoint.Tactical.Entities.Equipments;
using PhoenixPoint.Common.Core;
using PhoenixPoint.Common.Entities;
using PhoenixPoint.Common.Entities.Characters;
using PhoenixPoint.Common.Entities.Equipments;
using PhoenixPoint.Common.Entities.GameTags;
using PhoenixPoint.Common.Entities.GameTagsTypes;
using PhoenixPoint.Common.Entities.Items;
using PhoenixPoint.Common.UI;
using PhoenixPoint.Geoscape.Entities.DifficultySystem;
using PhoenixPoint.Geoscape.Entities.Research;
using PhoenixPoint.Geoscape.Events.Eventus;
using PhoenixPoint.Tactical;
using PhoenixPoint.Tactical.AI;
using PhoenixPoint.Tactical.AI.Actions;
using PhoenixPoint.Tactical.AI.Considerations;
using PhoenixPoint.Tactical.AI.TargetGenerators;
using PhoenixPoint.Tactical.Entities;
using PhoenixPoint.Tactical.Entities.Abilities;
using PhoenixPoint.Tactical.Entities.Animations;
using PhoenixPoint.Tactical.Entities.DamageKeywords;
using PhoenixPoint.Tactical.Entities.Effects;
using PhoenixPoint.Tactical.Entities.Effects.DamageTypes;
using PhoenixPoint.Tactical.Entities.Equipments;
using PhoenixPoint.Tactical.Entities.Statuses;
using PhoenixPoint.Tactical.Entities.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BetterVehicles
{
    internal class Mutog
    {
        private static readonly DefRepository Repo = BetterVehiclesMain.Repo;
        private static readonly SharedData Shared = BetterVehiclesMain.Shared;       
        public static void Change_Mutog()
        {
            DefRepository Repo = GameUtl.GameComponent<DefRepository>();
            SharedData Shared = GameUtl.GameComponent<SharedData>();
            BetterVehiclesConfig Config = (BetterVehiclesConfig)BetterVehiclesMain.Main.Config;
            WeaponDef mutogVenomHead = Repo.GetAllDefs<WeaponDef>().FirstOrDefault(a => a.name.Equals("Mutog_HeadPoison_WeaponDef"));
            WeaponDef mutogSlashTail = Repo.GetAllDefs<WeaponDef>().FirstOrDefault(a => a.name.Equals("Mutog_Tail_Bladed_WeaponDef"));
            WeaponDef mutogBashTail = Repo.GetAllDefs<WeaponDef>().FirstOrDefault(a => a.name.Equals("Mutog_Tail_Bladed_WeaponDef"));
            TacticalItemDef mutogAgileLegs = Repo.GetAllDefs<TacticalItemDef>().FirstOrDefault(a => a.name.Equals("Mutog_RearRightLeg_Agile_BodyPartDef"));
            BashAbilityDef headButt = Repo.GetAllDefs<BashAbilityDef>().FirstOrDefault(a => a.name.Equals("Mutog_HeadBash_BashAbilityDef"));
            BashAbilityDef LeftPawBash = Repo.GetAllDefs<BashAbilityDef>().FirstOrDefault(a => a.name.Equals("Mutog_LeftPawBash_AbilityDef"));
            BashAbilityDef RightPawBash = Repo.GetAllDefs<BashAbilityDef>().FirstOrDefault(a => a.name.Equals("Mutog_RightPawBash_AbilityDef"));


            if (Config.TurnOnMutogChanges == true)
            {
                headButt.DamagePayload.ActorMultiplier = 1.1335f;

                mutogBashTail.DamagePayload.DamageKeywords[0].Value = 80;

                foreach(BashAbilityDef pawBash in Repo.GetAllDefs<BashAbilityDef>().Where(a => (a.name.Contains("Mutog") && a.name.Contains("PawBash"))))
                {
                    pawBash.DamagePayload.DamageKeywords[0].Value = 100;
                }

                mutogSlashTail.DamagePayload.DamageKeywords = new List<DamageKeywordPair>
                {
                    mutogSlashTail.DamagePayload.DamageKeywords[0],
                    mutogSlashTail.DamagePayload.DamageKeywords[1],
                    new DamageKeywordPair{DamageKeywordDef = Shared.SharedDamageKeywords.PiercingKeyword, Value = 20 },
                };

                mutogSlashTail.DamagePayload.DamageKeywords[0].Value = 90;

                mutogVenomHead.DamagePayload.DamageKeywords = new List<DamageKeywordPair>
                {
                    new DamageKeywordPair{DamageKeywordDef = Shared.SharedDamageKeywords.DamageKeyword, Value = 80 },
                    new DamageKeywordPair{DamageKeywordDef = Shared.SharedDamageKeywords.PoisonousKeyword, Value = 80 },
                    new DamageKeywordPair{DamageKeywordDef = Shared.SharedDamageKeywords.PiercingKeyword, Value = 60 },
                    new DamageKeywordPair{DamageKeywordDef = Shared.SharedDamageKeywords.ParalysingKeyword, Value = 8 },
                };
                //mutogVenomHead.UseAimIK = true;

                BodyPartAspectDef MAL = (BodyPartAspectDef)mutogAgileLegs.BodyPartAspectDef;
                MAL.Speed = 11;

                mutogAgileLegs.Abilities = new AbilityDef[]
                {
                    mutogAgileLegs.Abilities[0],
                    Repo.GetAllDefs<ApplyStatusAbilityDef>().FirstOrDefault(a => a.name.Equals("CloseQuarters_AbilityDef")),
                };

                FrenzyStatusDef frenzyStatusDef = Repo.GetAllDefs<FrenzyStatusDef>().FirstOrDefault((FrenzyStatusDef w) => w.name == "Mutog_Enraged_StatusDef");
                //frenzyStatusDef.SpeedCoefficient = 1f;
                HealthConditionStatusDef healthConditionStatusDef = Repo.GetAllDefs<HealthConditionStatusDef>().FirstOrDefault((HealthConditionStatusDef w) => w.name == "Mutog_HealthCondition_StatusDef");
                //healthConditionStatusDef.Thresholds[0].HealthRatioThreshold = 0.3f;

                //int usesPerTurn = 1;
                //float actionPointCost = 0.5f;
                //int num = 3;
                //int num2 = 10;
                //bool targetFirstDamagedBodypart = false;
                foreach (ItemDef itemDef in from ad in Repo.GetAllDefs<ItemDef>()
                                            where ad.name.Contains("Leg_Regenerating") && ad.name.Contains("BodyPartDef")
                                            select ad)
                {
                    itemDef.Armor = 25f;
                    itemDef.HitPoints = 160f;
                }
                TacticalActorDef tacticalActorDef = Repo.GetAllDefs<TacticalActorDef>().FirstOrDefault((TacticalActorDef ta) => ta.name.Equals("Mutog_ActorDef"));
                ApplyStatusAbilityDef item = Repo.GetAllDefs<ApplyStatusAbilityDef>().FirstOrDefault((ApplyStatusAbilityDef ad) => ad.name.Equals("Unbreakable_AbilityDef"));
                ApplyStatusAbilityDef applyStatusAbilityDef = Repo.GetAllDefs<ApplyStatusAbilityDef>().FirstOrDefault((ApplyStatusAbilityDef w) => w.name == "Mutog_Regeneration_AbilityDef");
                AbilityDef item2 = Repo.GetAllDefs<AbilityDef>().FirstOrDefault((AbilityDef w) => w.name == "GroundStomp_AbilityDef");
                AbilityDef item3 = Repo.GetAllDefs<AbilityDef>().FirstOrDefault((AbilityDef w) => w.name == "Mutog_Ram_AbilityDef");
                RamAbilityDef ramAbilityDef = Repo.GetAllDefs<RamAbilityDef>().FirstOrDefault((RamAbilityDef w) => w.name == "Mutog_Ram_AbilityDef");
                ItemDef itemDef2 = Repo.GetAllDefs<ItemDef>().FirstOrDefault((ItemDef w) => w.name.Contains("Mutog_RearRightLeg_Regenerating_BodyPartDef"));
                GooDamageMultiplierAbilityDef item4 = Repo.GetAllDefs<GooDamageMultiplierAbilityDef>().FirstOrDefault((GooDamageMultiplierAbilityDef w) => w.name == "GooImmunity_AbilityDef");
                DamageMultiplierAbilityDef item5 = Repo.GetAllDefs<DamageMultiplierAbilityDef>().FirstOrDefault((DamageMultiplierAbilityDef w) => w.name.Equals("AcidResistant_DamageMultiplierAbilityDef"));
                HealthChangeStatusDef healthChangeStatusDef = Repo.GetAllDefs<HealthChangeStatusDef>().FirstOrDefault((HealthChangeStatusDef w) => w.name.Equals("E_Status [Mutog_Regeneration_AbilityDef]"));
               // tacticalActorDef.ShouldStopMovingOnActorSpotted = false;
               // ramAbilityDef.UsesPerTurn = usesPerTurn;
               // ramAbilityDef.ActionPointCost = actionPointCost;
               // ramAbilityDef.WillPointCost = (float)num;
               // ramAbilityDef.RamForce = 600;
               // ramAbilityDef.ViewElementDef.DisplayName1 = new LocalizedTextBind("FURIOUS CHARGE", true);
               // ramAbilityDef.ViewElementDef.Description = new LocalizedTextBind("Charge forward ramming everything in the way for the cost of AP with distance", true);
               // healthChangeStatusDef.HealthChangeAmount = (float)num2;
               // healthChangeStatusDef.TargetFirstDamagedBodypart = targetFirstDamagedBodypart;
               // applyStatusAbilityDef.ViewElementDef.Description = new LocalizedTextBind("Each leg restores 10 Hit Points to all injured body parts.", true);
               // List<AbilityDef> list = itemDef2.Abilities.ToList<AbilityDef>();
               // list.Add(item3);
               // list.Add(item2);
               // list.Add(item5);
               // list.Add(item4);
               // list.Add(item);
               // itemDef2.Abilities = list.ToArray();
            }
        }
    }
}
