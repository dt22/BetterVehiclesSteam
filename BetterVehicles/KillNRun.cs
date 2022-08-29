using Base.Cameras.ExecutionNodes;
using Base.Core;
using Base.Defs;
using Base.Entities.Abilities;
using Base.Entities.Effects;
using Base.Entities.Statuses;
using Base.UI;
using Code.PhoenixPoint.Tactical.Entities.Equipments;
using HarmonyLib;
using I2.Loc;
using PhoenixPoint.Common.Core;
using PhoenixPoint.Common.Entities;
using PhoenixPoint.Common.Entities.Equipments;
using PhoenixPoint.Common.Entities.GameTags;
using PhoenixPoint.Common.Entities.GameTagsTypes;
using PhoenixPoint.Common.UI;
using PhoenixPoint.Tactical.Cameras.Filters;
using PhoenixPoint.Tactical.Entities.Abilities;
using PhoenixPoint.Tactical.Entities.Animations;
using PhoenixPoint.Tactical.Entities.Equipments;
using PhoenixPoint.Tactical.Entities.Statuses;
using PhoenixPoint.Tactical.Entities.Weapons;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BetterVehicles
{
    internal class KillNRun
    {
        private static readonly DefRepository Repo = BetterVehiclesMain.Repo;
        private static readonly SharedData Shared = BetterVehiclesMain.Shared;
        public static void Change_EP()
        {
            DefRepository Repo = GameUtl.GameComponent<DefRepository>();
            GroundVehicleModuleDef experimentalExhaust = Repo.GetAllDefs<GroundVehicleModuleDef>().FirstOrDefault(a => a.name.Equals("KS_Kaos_Buggy_Experimental_Exhaust_System_Engine_GroundVehicleModuleDef"));
            string skillName = "KillAndRunVehicle_AbilityDef";        

            // Source to clone from for main ability: Inspire
            ApplyStatusAbilityDef inspireAbility = Repo.GetAllDefs<ApplyStatusAbilityDef>().FirstOrDefault(a => a.name.Equals("Inspire_AbilityDef"));

            // Create Neccessary RuntimeDefs
            ApplyStatusAbilityDef killAndRunAbility = Helper.CreateDefFromClone(
                inspireAbility,
                "af77ed60-254d-4be6-adf8-91ca972d1e39",
                skillName);
            AbilityCharacterProgressionDef progression = Helper.CreateDefFromClone(
                inspireAbility.CharacterProgressionData,
                "52bdb6ed-7544-4d2a-af4f-eb199ab68fb0",
                skillName);
            TacticalAbilityViewElementDef viewElement = Helper.CreateDefFromClone(
                inspireAbility.ViewElementDef,
                "861dd580-f206-4e21-98c3-c846a6071f03",
                skillName);
            OnActorDeathEffectStatusDef onActorDeathEffectStatus = Helper.CreateDefFromClone(
                inspireAbility.StatusDef as OnActorDeathEffectStatusDef,
                "1f5d9d57-a777-43a2-8026-1755a66fd4b2",
                "E_KillListenerStatus [" + skillName + "]");
            RepositionAbilityDef dashAbility = Helper.CreateDefFromClone( // Create an own Dash ability from standard Dash
                Repo.GetAllDefs<RepositionAbilityDef>().FirstOrDefault(r => r.name.Equals("Dash_AbilityDef")),
                "6a35bee7-3201-4333-b0e3-00ffdc0fd025",
                "KillAndRun_Dash_AbilityDef");
            TacticalTargetingDataDef dashTargetingData = Helper.CreateDefFromClone( // ... and clone its targeting data
                Repo.GetAllDefs<TacticalTargetingDataDef>().FirstOrDefault(t => t.name.Equals("E_TargetingData [Dash_AbilityDef]")),
                "503e2edc-4c31-4762-b8ca-fd1a7f60af8a",
                "KillAndRun_Dash_AbilityDef");
            StatusRemoverEffectDef statusRemoverEffect = Helper.CreateDefFromClone( // Borrow effect from Manual Control
                Repo.GetAllDefs<StatusRemoverEffectDef>().FirstOrDefault(a => a.name.Equals("E_RemoveStandBy [ManualControlStatus]")),
                "60275a1e-6caf-48c1-b24c-cc9e33a103e2",
                "E_StatusRemoverEffect [" + skillName + "]");
            AddAbilityStatusDef addAbiltyStatus = Helper.CreateDefFromClone( // Borrow status from Deplay Beacon (final mission)
                Repo.GetAllDefs<AddAbilityStatusDef>().FirstOrDefault(a => a.name.Equals("E_AddAbilityStatus [DeployBeacon_StatusDef]")),
                "519423f6-b41a-4409-a48f-b5113efe61ac",
                skillName);
            MultiStatusDef multiStatus = Helper.CreateDefFromClone( // Borrow multi status from Rapid Clearance
                Repo.GetAllDefs<MultiStatusDef>().FirstOrDefault(m => m.name.Equals("E_MultiStatus [RapidClearance_AbilityDef]")),
                "d8af0b40-94f9-4c2a-a841-796469998d86",
                skillName);
            FirstMatchExecutionDef cameraAbility = Helper.CreateDefFromClone(
                Repo.GetAllDefs<FirstMatchExecutionDef>().FirstOrDefault(bd => bd.name.Equals("E_DashCameraAbility [NoDieCamerasTacticalCameraDirectorDef]")),
                "20f5659c-890a-4f29-9968-07ea67b04c6b",
                "E_KnR_Dash_CameraAbility [NoDieCamerasTacticalCameraDirectorDef]");
            cameraAbility.FilterDef = Helper.CreateDefFromClone(
                Repo.GetAllDefs<TacCameraAbilityFilterDef>().FirstOrDefault(c => c.name.Equals("E_DashAbilityFilter [NoDieCamerasTacticalCameraDirectorDef]")),
                "64ba51e9-c67b-4e5e-ad61-315e7f796ffa",
                "E_KnR_Dash_CameraAbilityFilter [NoDieCamerasTacticalCameraDirectorDef]");
            (cameraAbility.FilterDef as TacCameraAbilityFilterDef).TacticalAbilityDef = dashAbility;

            // Add new KnR Dash ability to animation action handler for dash (same animation)
            foreach (TacActorSimpleAbilityAnimActionDef def in Repo.GetAllDefs<TacActorSimpleAbilityAnimActionDef>().Where(b => b.name.Contains("Dash")))
            {
                if (!def.AbilityDefs.Contains(dashAbility))
                {
                    def.AbilityDefs = def.AbilityDefs.Append(dashAbility).ToArray();
                }
            }

            // Set fields
            killAndRunAbility.CharacterProgressionData = progression;
            killAndRunAbility.ViewElementDef = viewElement;
            killAndRunAbility.SkillTags = new SkillTagDef[0];
            killAndRunAbility.StatusDef = multiStatus;
            killAndRunAbility.StatusApplicationTrigger = StatusApplicationTrigger.StartTurn;

            viewElement.DisplayName1 = new LocalizedTextBind("KILL'N'RUN", true);
            viewElement.Description = new LocalizedTextBind("Once per turn, take a free move after killing an enemy.", true);
            
           
            viewElement.ShowInStatusScreen = true;
            viewElement.HideFromPassives = true;

            dashAbility.TargetingDataDef = dashTargetingData;
            dashAbility.TargetingDataDef.Origin.Range = 14.0f;

            dashAbility.ViewElementDef = Helper.CreateDefFromClone(
                inspireAbility.ViewElementDef,
                "2e5aaf9d-21b3-4857-8e98-6df883654506",
                "KillAndRun_Dash_AbilityDef");
            dashAbility.ViewElementDef.DisplayName1 = viewElement.DisplayName1;
            dashAbility.ViewElementDef.Description = viewElement.Description;
            
            dashAbility.ViewElementDef.ShowInStatusScreen = false;
            dashAbility.ViewElementDef.HideFromPassives = true;
            dashAbility.ViewElementDef.ShouldFlash = true;

            dashAbility.SuppressAutoStandBy = true;
            dashAbility.DisablingStatuses = new StatusDef[] { onActorDeathEffectStatus };
            dashAbility.UsesPerTurn = 1;
            dashAbility.ActionPointCost = 0.0f;
            dashAbility.WillPointCost = 0.0f;
            dashAbility.SamePositionIsValidTarget = true;
            dashAbility.AmountOfMovementToUseAsRange = -1.0f;

            multiStatus.Statuses = new StatusDef[] { onActorDeathEffectStatus, addAbiltyStatus };

            onActorDeathEffectStatus.EffectName = "KnR_KillTriggerListener";
            onActorDeathEffectStatus.Visuals = viewElement;
            onActorDeathEffectStatus.VisibleOnPassiveBar = true;
            onActorDeathEffectStatus.DurationTurns = 0;
            onActorDeathEffectStatus.EffectDef = statusRemoverEffect;

            statusRemoverEffect.StatusToRemove = "KnR_KillTriggerListener";

            addAbiltyStatus.DurationTurns = 0;
            addAbiltyStatus.SingleInstance = true;
            addAbiltyStatus.AbilityDef = dashAbility;

            experimentalExhaust.Abilities = new AbilityDef[]
            {
                Repo.GetAllDefs<AbilityDef>().FirstOrDefault(a => a.name.Equals("KillAndRunVehicle_AbilityDef")),
            };
        }
        public static void BV()
        {
            BetterVehiclesConfig Config = (BetterVehiclesConfig)BetterVehiclesMain.Main.Config;
            GroundVehicleWeaponDef ArmadilloFT = Repo.GetAllDefs<GroundVehicleWeaponDef>().FirstOrDefault(a => a.name.Equals("NJ_Armadillo_Mephistopheles_GroundVehicleWeaponDef"));
            GroundVehicleWeaponDef ArmadilloPurgatory = Repo.GetAllDefs<GroundVehicleWeaponDef>().FirstOrDefault(a => a.name.Equals("NJ_Armadillo_Purgatory_GroundVehicleWeaponDef"));
            GroundVehicleWeaponDef ArmadilloGaussTurret = Repo.GetAllDefs<GroundVehicleWeaponDef>().FirstOrDefault(a => a.name.Equals("NJ_Armadillo_Gauss_Turret_GroundVehicleWeaponDef"));

            GroundVehicleWeaponDef Taurus2 = Repo.GetAllDefs<GroundVehicleWeaponDef>().FirstOrDefault(a => a.name.Equals("PX_Scarab_Taurus_GroundVehicleWeaponDef"));

            WeaponDef fullStop = Repo.GetAllDefs<WeaponDef>().FirstOrDefault(a => a.name.Equals("KS_Buggy_Fullstop_WeaponDef"));
            WeaponDef screamer = Repo.GetAllDefs<WeaponDef>().FirstOrDefault(a => a.name.Equals("KS_Buggy_Screamer_WeaponDef"));
            WeaponDef vishnu = Repo.GetAllDefs<WeaponDef>().FirstOrDefault(a => a.name.Equals("KS_Buggy_The_Vishnu_Gun_Cannon_WeaponDef"));
            WeaponDef fullStopMiniGun = Repo.GetAllDefs<WeaponDef>().FirstOrDefault(a => a.name.Equals("KS_Buggy_Minigun_Fullstop_WeaponDef"));
            WeaponDef screamerMiniGun = Repo.GetAllDefs<WeaponDef>().FirstOrDefault(a => a.name.Equals("KS_Buggy_Minigun_Screamer_WeaponDef"));
            WeaponDef vishnuMiniGun = Repo.GetAllDefs<WeaponDef>().FirstOrDefault(a => a.name.Equals("KS_Buggy_Minigun_Vishnu_WeaponDef"));

            GroundVehicleModuleDef revisedArmor = Repo.GetAllDefs<GroundVehicleModuleDef>().FirstOrDefault(a => a.name.Equals("KS_Kaos_Buggy_Revised_Armor_Plating_Hull_GroundVehicleModuleDef"));
            GroundVehicleModuleDef spikedArmor = Repo.GetAllDefs<GroundVehicleModuleDef>().FirstOrDefault(a => a.name.Equals("KS_Kaos_Buggy_Spiked_Armor_Plating_Hull_GroundVehicleModuleDef"));
            GroundVehicleModuleDef experimentalExhaust = Repo.GetAllDefs<GroundVehicleModuleDef>().FirstOrDefault(a => a.name.Equals("KS_Kaos_Buggy_Experimental_Exhaust_System_Engine_GroundVehicleModuleDef"));
            GroundVehicleModuleDef reinforcedPlating = Repo.GetAllDefs<GroundVehicleModuleDef>().FirstOrDefault(a => a.name.Equals("NJ_Armadillo_Reinforced_Plating_Hull_GroundVehicleModuleDef"));
            GroundVehicleModuleDef lightAlloy = Repo.GetAllDefs<GroundVehicleModuleDef>().FirstOrDefault(a => a.name.Equals("NJ_Armadillo_Lightweight_Alloy_Plating_Hull_GroundVehicleModuleDef"));
            GroundVehicleModuleDef superCharger = Repo.GetAllDefs<GroundVehicleModuleDef>().FirstOrDefault(a => a.name.Equals("NJ_Armadillo_Supercharger_GroundVehicleModuleDef"));
            GroundVehicleModuleDef cargoRacks = Repo.GetAllDefs<GroundVehicleModuleDef>().FirstOrDefault(a => a.name.Equals("PX_Scarab_Reinforced_Cargo_Racks_GroundVehicleModuleDef"));
            GroundVehicleModuleDef improvedChasis = Repo.GetAllDefs<GroundVehicleModuleDef>().FirstOrDefault(a => a.name.Equals("SY_Aspida_Improved_Chassis_GroundVehicleModuleDef"));

            GroundVehicleModuleDef vishnuModule = Repo.GetAllDefs<GroundVehicleModuleDef>().FirstOrDefault(a => a.name.Equals("KS_Buggy_The_Vishnu_Gun_GroundVehicleModuleDef"));

            TacticalItemDef revisedLeftTire = Repo.GetAllDefs<TacticalItemDef>().FirstOrDefault(a => a.name.Equals("KS_Kaos_Buggy_Revised_Armor_Plating_LeftFrontTyre_BodyPartDef"));
            TacticalItemDef revisedRightTire = Repo.GetAllDefs<TacticalItemDef>().FirstOrDefault(a => a.name.Equals("KS_Kaos_Buggy_Revised_Armor_Plating_RightFrontTyre_BodyPartDef"));
            TacticalItemDef spikedLeftFrontTire = Repo.GetAllDefs<TacticalItemDef>().FirstOrDefault(a => a.name.Equals("KS_Kaos_Buggy_Spiked_Armor_LeftFrontTyre_BodyPartDef"));
            TacticalItemDef spikedLeftBackTire = Repo.GetAllDefs<TacticalItemDef>().FirstOrDefault(a => a.name.Equals("KS_Kaos_Buggy_Spiked_Armor_LeftBackTyre_BodyPartDef"));
            TacticalItemDef spikedRightFrontTire = Repo.GetAllDefs<TacticalItemDef>().FirstOrDefault(a => a.name.Equals("KS_Kaos_Buggy_Spiked_Armor_RightFrontTyre_BodyPartDef"));

            TacticalItemDef KaosBuggyTop = Repo.GetAllDefs<TacticalItemDef>().FirstOrDefault(a => a.name.Equals("KS_Kaos_Buggy_Top_BodyPartDef"));
            TacticalItemDef KaosBuggyFront = Repo.GetAllDefs<TacticalItemDef>().FirstOrDefault(a => a.name.Equals("KS_Kaos_Buggy_Front_BodyPartDef"));
            TacticalItemDef KaosBuggyLeft = Repo.GetAllDefs<TacticalItemDef>().FirstOrDefault(a => a.name.Equals("KS_Kaos_Buggy_Left_BodyPartDef"));
            TacticalItemDef KaosBuggyRight = Repo.GetAllDefs<TacticalItemDef>().FirstOrDefault(a => a.name.Equals("KS_Kaos_Buggy_Right_BodyPartDef"));
            TacticalItemDef KaosBuggyBack = Repo.GetAllDefs<TacticalItemDef>().FirstOrDefault(a => a.name.Equals("KS_Kaos_Buggy_Back_BodyPartDef"));

            TacticalItemDef KaosBuggyRearTyre = Repo.GetAllDefs<TacticalItemDef>().FirstOrDefault(a => a.name.Equals("KS_Kaos_Buggy_RearTyre_BodyPartDef"));
            TacticalItemDef KaosBuggyFrontLeftTyre = Repo.GetAllDefs<TacticalItemDef>().FirstOrDefault(a => a.name.Equals("KS_Kaos_Buggy_LeftFrontTyre_BodyPartDef"));
            TacticalItemDef KaosBuggyFrontRightTyre = Repo.GetAllDefs<TacticalItemDef>().FirstOrDefault(a => a.name.Equals("KS_Kaos_Buggy_RightFrontTyre_BodyPartDef"));

            KaosBuggyTop.HitPoints = 220;
            KaosBuggyFront.HitPoints = 280;
            KaosBuggyFront.Armor = 40;
            KaosBuggyLeft.HitPoints = 200;
            KaosBuggyLeft.Armor = 20;
            KaosBuggyRight.HitPoints = 200;
            KaosBuggyRight.Armor = 20;
            KaosBuggyBack.HitPoints = 200;
            KaosBuggyBack.Armor = 20;

            KaosBuggyRearTyre.HitPoints = 180;
            KaosBuggyRearTyre.Armor = 10;
            KaosBuggyRearTyre.BodyPartAspectDef.Speed = 13;
            KaosBuggyFrontLeftTyre.HitPoints = 180;
            KaosBuggyFrontLeftTyre.Armor = 10;
            KaosBuggyFrontRightTyre.HitPoints = 180;
            KaosBuggyFrontRightTyre.Armor = 10;

            vishnuModule.BodyPartAspectDef.Endurance = 1;
            KaosBuggyTop.BodyPartAspectDef.Endurance = 19;
            KaosBuggyFront.BodyPartAspectDef.Endurance = 20;
            KaosBuggyLeft.BodyPartAspectDef.Endurance = 18;
            KaosBuggyRight.BodyPartAspectDef.Endurance = 18;
            KaosBuggyBack.BodyPartAspectDef.Endurance = 19;

            foreach (GroundVehicleWeaponDef groundvehiclweapon in Repo.GetAllDefs<GroundVehicleWeaponDef>().Where(a => a.name.Contains("GroundVehicleWeaponDef")))
            {
                groundvehiclweapon.HitPoints *= 2;
                groundvehiclweapon.Armor *= 2;
            }

            foreach (WeaponDef groundvehiclweapon in Repo.GetAllDefs<WeaponDef>().Where(a => a.name.Contains("KS_Buggy_Minigun_") || a.name.Equals(fullStop.name) ||
             a.name.Equals(screamer.name) || a.name.Equals(vishnu.name)))
            {
                groundvehiclweapon.HitPoints *= 2;
                groundvehiclweapon.Armor *= 2;
            }

            foreach (WeaponDef groundvehiclweapon in Repo.GetAllDefs<WeaponDef>().Where(a => a.name.Contains("KS_Buggy_Minigun_")))
            {
                groundvehiclweapon.DamagePayload.AutoFireShotCount = 6;
                groundvehiclweapon.ChargesMax = 60;
            }

            fullStop.ChargesMax = 3;
            Taurus2.ChargesMax = 8;
            ArmadilloFT.ChargesMax = 10;
            ArmadilloPurgatory.ChargesMax = 6;
            revisedLeftTire.BodyPartAspectDef.Speed = 0;
            revisedRightTire.BodyPartAspectDef.Speed = 0;
            if (Config.FixText == true)
            {
                string text10 = "DOES NOT ADD ARMOR, adds +250 HP and +20 Armor to Wheels";
                revisedArmor.ViewElementDef.Description = new LocalizedTextBind(text10, true);
                spikedArmor.ViewElementDef.Description = new LocalizedTextBind(text10, true);
                BetterVehiclesMain.ModifiedLocalizationTerms.Add(text10);
            }
            spikedLeftBackTire.BodyPartAspectDef.Speed = 0;
            spikedLeftFrontTire.BodyPartAspectDef.Speed = 0;
            spikedRightFrontTire.BodyPartAspectDef.Speed = 0;



            BodyPartAspectDef LWA = (BodyPartAspectDef)lightAlloy.BodyPartAspectDef;
            LWA.StatModifications[0].Value = 6;

            BodyPartAspectDef IC = (BodyPartAspectDef)improvedChasis.BodyPartAspectDef;
            IC.Speed = 4;

            BodyPartAspectDef CR = (BodyPartAspectDef)cargoRacks.BodyPartAspectDef;
            CR.StatModifications[0].Value = 9;

            BodyPartAspectDef RPBPAD = (BodyPartAspectDef)reinforcedPlating.BodyPartAspectDef;
            RPBPAD.StatModifications = new ItemStatModification[]
            {
                new ItemStatModification()
                {
                    TargetStat = StatModificationTarget.UnitsInside,
                    Modification = StatModificationType.Add,
                    Value = 1,
                },
            };

            ArmadilloGaussTurret.ChargesMax = 96;
            ArmadilloGaussTurret.DamagePayload.DamageKeywords[1].Value = 2;
            ArmadilloGaussTurret.SpreadDegrees = 1.8f;

            superCharger.Abilities = new AbilityDef[]
            {
                Repo.GetAllDefs<AbilityDef>().FirstOrDefault(a => a.name.Equals("GooImmunity_AbilityDef")),
            };
        }
        [HarmonyPatch(typeof(LocalizationManager), "TryGetTranslation")]
        public static class LocalizationManager_TryGetTranslation_Patch
        {
            // Token: 0x060000CA RID: 202 RVA: 0x0000ABF2 File Offset: 0x00008DF2
            public static bool Prepare()
            {
                BetterVehiclesConfig Config = (BetterVehiclesConfig)BetterVehiclesMain.Main.Config;
                return Config.FixText;
            }

            // Token: 0x060000CB RID: 203 RVA: 0x0000B0B4 File Offset: 0x000092B4
            public static void Postfix(bool __result, string Term, ref string Translation)
            {
                try
                {
                    if (!__result)
                    {
                        if (!string.IsNullOrEmpty(Term) && BetterVehiclesMain.ModifiedLocalizationTerms.Contains(Term))
                        {
                            Translation = Term;
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }
        [HarmonyPatch(typeof(TacticalAbility), "get_ShouldDisplay")]
        internal static class BC_TacticalAbility_get_ShouldDisplay_Patch
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051")]
            private static void Postfix(TacticalAbility __instance, ref bool __result)
            {
                // Check if instance is KnR ability
                if (__instance.TacticalAbilityDef.name.Equals("KillAndRun_Dash_AbilityDef"))
                {
                    //  Set return value __result = true when ability is not disabled => show
                    __result = __instance.GetDisabledState() == AbilityDisabledState.NotDisabled;
                }
            }
        }
    }
}
