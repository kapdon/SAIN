﻿using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using SAIN.Preset;
using SAIN.Components;
using SAIN.Plugin;
using SAIN.Preset.BotSettings.SAINSettings;
using System;
using System.Reflection;
using UnityEngine;
using Comfort.Common;
using SAIN.SAINComponent.Classes;
using SAIN.Helpers;
using System.Collections.Generic;
using static UnityEngine.EventSystems.EventTrigger;
using UnityEngine.UIElements;
using EFT.InventoryLogic;

namespace SAIN.Patches.Vision
{
    public class Math
    {
        public static float CalcVisSpeed(float dist, SAINSettingsClass preset)
        {
            float result = 1f;
            if (dist >= preset.Look.CloseFarThresh)
            {
                result *= preset.Look.FarVisionSpeed;
            }
            else
            {
                result *= preset.Look.CloseVisionSpeed;
            }
            result *= preset.Look.VisionSpeedModifier;

            return Mathf.Round(result * 100f) / 100f;
        }
    }

    public class AIVisionUpdateLimitPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(LookSensor), "CheckAllEnemies");
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ____botOwner, GClass522 lookAll)
        {
            if (____botOwner != null)
            {
                if (!LookUpdates.ContainsKey(____botOwner.ProfileId))
                {
                    LookUpdates.Add(____botOwner.ProfileId, new AIVisionInfo(____botOwner));
                }
                LookUpdates[____botOwner.ProfileId].CheckEnemies(lookAll);
            }
            return false;
        }

        public static Dictionary<string, AIVisionInfo> LookUpdates = new Dictionary<string, AIVisionInfo>();

        public class AIVisionInfo
        {
            public AIVisionInfo(BotOwner bot)
            {
                BotOwner = bot;
            }

            public void CheckEnemies(GClass522 lookAll)
            {
                float time = Time.time;
                var enemyInfos = this.BotOwner.EnemiesController.EnemyInfos.Values;
                this._cacheToLookEnemyInfos.AddRange(enemyInfos);
                foreach (EnemyInfo enemyInfo in this._cacheToLookEnemyInfos)
                {
                    try
                    {
                        IPlayer person = enemyInfo.Person;
                        if (!NextUpdateTimes.ContainsKey(person.ProfileId))
                        {
                            NextUpdateTimes.Add(person.ProfileId, 0f);
                        }

                        float timeAdd;
                        if (!person.IsAI)
                        {
                            timeAdd = 0.1f;
                        }
                        else
                        {
                            if (BotOwner.Memory.GoalEnemy == enemyInfo)
                            {
                                timeAdd = 0.2f;
                            }
                            else
                            {
                                timeAdd = 0.33f;
                            }
                        }

                        if (NextUpdateTimes[person.ProfileId] + timeAdd < time)
                        {
                            NextUpdateTimes[person.ProfileId] = time;
                            enemyInfo.CheckLookEnemy(lookAll);
                        }
                    }
                    catch (Exception e) 
                    {
                        Logger.LogError(e);
                        BotOwner.EnemiesController.EnemyInfos.Remove(enemyInfo.Person);
                    }
                }
                this._cacheToLookEnemyInfos.Clear();
            }

            private readonly BotOwner BotOwner;
            public Dictionary<string, float> NextUpdateTimes = new Dictionary<string, float>();
            private readonly List<EnemyInfo> _cacheToLookEnemyInfos = new List<EnemyInfo>();
        }
    }

    public class WeatherTimeVisibleDistancePatch : ModulePatch
    {
        private static PropertyInfo _clearVisibleDistProperty;
        private static PropertyInfo _visibleDistProperty;
        private static PropertyInfo _HourServerProperty;

        protected override MethodBase GetTargetMethod()
        {
            _clearVisibleDistProperty = typeof(LookSensor).GetProperty("ClearVisibleDist");
            _visibleDistProperty = typeof(LookSensor).GetProperty("VisibleDist");
            _HourServerProperty = typeof(LookSensor).GetProperty("HourServer");

            return AccessTools.Method(typeof(LookSensor), "method_2");
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ____botOwner, ref float ____nextUpdateVisibleDist)
        {
            if (____nextUpdateVisibleDist < Time.time)
            {
                float timeMod = 1f;
                float weatherMod = 1f;

                // Checks to make sure a date and time is present
                if (____botOwner.GameDateTime != null)
                {
                    DateTime dateTime = SAINPlugin.BotController.TimeVision.GameDateTime;
                    timeMod = SAINPlugin.BotController.TimeVision.TimeOfDayVisibility;
                    // Modify the Rounding of the "HourServer" property to the hour from the DateTime object
                    _HourServerProperty.SetValue(____botOwner.LookSensor, (int)((short)dateTime.Hour));
                }
                if (SAINPlugin.BotController != null)
                {
                    weatherMod = SAINPlugin.BotController.WeatherVision.VisibilityNum;
                    weatherMod = Mathf.Clamp(weatherMod, 0.5f, 1f);
                }

                float currentVisionDistance = ____botOwner.Settings.Current.CurrentVisibleDistance;

                // Sets a minimum cap based on weather conditions to avoid bots having too low of a vision Distance while at peace in bad weather
                float currentVisionDistanceCapped = Mathf.Clamp(currentVisionDistance * weatherMod, 80f, currentVisionDistance);

                // Applies SeenTime Modifier to the final vision Distance results
                float finalVisionDistance = currentVisionDistanceCapped * timeMod;

                _clearVisibleDistProperty.SetValue(____botOwner.LookSensor, finalVisionDistance);

                finalVisionDistance = ____botOwner.NightVision.UpdateVision(finalVisionDistance);
                finalVisionDistance = ____botOwner.BotLight.UpdateLightEnable(finalVisionDistance);
                _visibleDistProperty.SetValue(____botOwner.LookSensor, finalVisionDistance);

                ____nextUpdateVisibleDist = Time.time + (____botOwner.FlashGrenade.IsFlashed ? 3 : 20);
            }
            // Not sure what this does, but its new, so adding it here since this patch replaces the old.
            ____botOwner.BotLight.UpdateStrope();
            return false;
        }
    }

    public class NoAIESPPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetMethod("IsEnemyLookingAtMe", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(IPlayer) }, null);
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }

    public class EnemyPartDataVisionPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(AIData), "FlarePower");
        }

        [PatchPrefix]
        public static void PatchPrefix(ref float visibleCoef, BotOwner owner)
        {
        }
    }

    public class VisionSpeedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(EnemyInfo), "method_5");
        }

        [PatchPostfix]
        public static void PatchPostfix(BifacialTransform BotTransform, BifacialTransform enemy, ref float __result, EnemyInfo __instance)
        {
            float dist = (BotTransform.position - enemy.position).magnitude;
            float weatherModifier = SAINPlugin.BotController.WeatherVision.VisibilityNum;
            float inverseWeatherModifier = Mathf.Sqrt(2f - weatherModifier);

            WildSpawnType wildSpawnType = __instance.Owner.Profile.Info.Settings.Role;
            if (PresetHandler.LoadedPreset.BotSettings.SAINSettings.TryGetValue(wildSpawnType, out var botType))
            {
                BotDifficulty diff = __instance.Owner.Profile.Info.Settings.BotDifficulty;
                __result *= Math.CalcVisSpeed(dist, botType.Settings[diff]);
            }

            var person = __instance?.Person;
            if (person != null)
            {
                Player player = EFTInfo.GetPlayer(__instance.Person.ProfileId);
                if (player != null)
                {
                    var gearInfo = SAINGearInfoHandler.GetGearInfo(player);
                    if (gearInfo != null)
                    {
                        __result *= gearInfo.GetGainSightModifierFromGear(__instance.Distance);
                    }
                    // if player is using suppressed weapon, and has shot recently, don't increase vis speed as much.
                    bool suppressedFlare = false;
                    if (player.HandsController.Item is Weapon weapon)
                    {
                        suppressedFlare = person.AIData.GetFlare && gearInfo?.GetWeaponInfo(weapon)?.HasSuppressor == true;
                    }

                    if (!person.AIData.GetFlare || suppressedFlare)
                    {
                        __result *= inverseWeatherModifier;
                    }

                    if (player.IsSprintEnabled)
                    {
                        __result *= 0.66f;
                    }

                    float elevationDifference = enemy.position.y - BotTransform.position.y;
                    if (elevationDifference > 2f)
                    {
                        __result *= 1.33f;
                    }
                    if (elevationDifference < -2f)
                    {
                        __result *= 0.85f;
                    }
                    if (!player.IsAI)
                    {
                        __result *= SAINNotLooking.GetVisionSpeedIncrease(__instance.Owner);
                    }

                    if (SAINPlugin.GetSAIN(__instance.Owner, out var sain, nameof(VisionSpeedPatch)))
                    {
                        SAINEnemy sainEnemy = sain.EnemyController.GetEnemy(player.ProfileId);
                        if (sainEnemy?.EnemyStatus.FlareEnabled == true && sainEnemy.Heard)
                        {
                            __result *= 0.9f;
                        }
                    }
                }
            }
        }
    }

    public class VisionDistancePosePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(EnemyInfo), "CheckVisibility");
        }

        [PatchPrefix]
        public static void PatchPrefix(ref float addVisibility, EnemyInfo __instance)
        {
            Player player = EFTInfo.GetPlayer(__instance?.Person?.ProfileId);
            if (player != null)
            {
                // Increase or decrease vis distance based on pose and if sprinting.
                float visibility = SAINVisionClass.GetVisibilityModifier(player);

                var gearInfo = SAINGearInfoHandler.GetGearInfo(player);
                if (gearInfo != null)
                {
                    visibility *= gearInfo.GetGainSightModifierFromGear(__instance.Distance);
                }

                if (SAINPlugin.GetSAIN(__instance.Owner, out var sain, nameof(VisionDistancePosePatch)))
                {
                    SAINEnemy sainEnemy = sain.EnemyController.GetEnemy(player.ProfileId);
                    if (sainEnemy?.EnemyStatus.FlareEnabled == true)
                    {
                        visibility *= 1.25f;
                    }
                    if (player.AIData.GetFlare && sainEnemy?.Heard == true)
                    {
                        // if player shot a weapon recently
                        // if player is using suppressed weapon, and has shot recently, don't increase vis distance as much.
                        bool suppressedFlare = false;
                        if (player.HandsController.Item is Weapon weapon)
                        {
                            var weaponInfo = SAINGearInfoHandler.GetGearInfo(player);
                            suppressedFlare = weaponInfo?.GetWeaponInfo(weapon)?.HasSuppressor == true;
                        }

                        // increase visiblity
                        visibility *= suppressedFlare ? 1.1f : 1.25f;
                    }
                }
                if (player.IsSprintEnabled)
                {
                    visibility *= 1.25f;
                }

                float defaultVisDist = __instance.Owner.LookSensor.VisibleDist;
                float visionDist = (defaultVisDist * visibility) - defaultVisDist;

                addVisibility += visionDist;
            }
        }
    }

    public class CheckFlashlightPatch : ModulePatch
    {
        private static FieldInfo _tacticalModesField;
        private static MethodInfo _UsingLight;

        protected override MethodBase GetTargetMethod()
        {
            _UsingLight = AccessTools.PropertySetter(typeof(AIData), "UsingLight");

            _tacticalModesField = AccessTools.Field(typeof(TacticalComboVisualController), "list_0");

            return AccessTools.Method(typeof(Player.FirearmController), "SetLightsState");
        }

        [PatchPostfix]
        public static void PatchPostfix(ref Player ____player)
        {
            if (____player.gameObject.TryGetComponent<SAINFlashLightComponent>(out var component))
            {
                component.CheckDevice(____player, _tacticalModesField);
                if (!component.WhiteLight && !component.Laser)
                {
                    _UsingLight.Invoke(____player.AIData, new object[] { false });
                }
            }
        }
    }
}