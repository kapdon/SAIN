﻿using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using HarmonyLib;
using Interpolation;
using SAIN.Components.Helpers;
using SAIN.Helpers;
using SAIN.Preset.GlobalSettings.Categories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static EFT.Interactive.BetterPropagationGroups;

using GClass2869 = Operation1;

namespace SAIN.Patches.Hearing
{
    public class HearingSensorPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotHearingSensor), "method_0");
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ____botOwner)
        {
            if (SAINPlugin.BotController?.GetSAIN(____botOwner, out _) == true)
            {
                return false;
            }
            return true;
        }
    }

    public class TryPlayShootSoundPatch : ModulePatch
    {
        private static PropertyInfo AIFlareEnabled;

        protected override MethodBase GetTargetMethod()
        {
            AIFlareEnabled = AccessTools.Property(typeof(AIData), "Boolean_0");
            return AccessTools.Method(typeof(AIData), "TryPlayShootSound");
        }

        [PatchPrefix]
        public static bool PatchPrefix(AIData __instance)
        {
            AIFlareEnabled.SetValue(__instance, true);
            return false;
        }
    }

    public class OnMakingShotPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), "OnMakingShot");
        }

        [PatchPrefix]
        public static void PatchPrefix(Player __instance)
        {
            var botController = SAINPlugin.BotController;
            if (botController != null)
            {
                botController.StartCoroutine(botController.PlayShootSoundCoroutine(__instance));
            }
        }
    }

    public class SoundClipNameCheckerPatch : ModulePatch
    {
        private static MethodInfo _Player;
        private static FieldInfo _PlayerBridge;

        protected override MethodBase GetTargetMethod()
        {
            _PlayerBridge = AccessTools.Field(typeof(BaseSoundPlayer), "playersBridge");
            _Player = AccessTools.PropertyGetter(_PlayerBridge.FieldType, "iPlayer");
            return AccessTools.Method(typeof(BaseSoundPlayer), "SoundEventHandler");
        }

        [PatchPrefix]
        public static void PatchPrefix(string soundName, BaseSoundPlayer __instance)
        {
            if (SAINPlugin.BotController != null)
            {
                object playerBridge = _PlayerBridge.GetValue(__instance);
                Player player = _Player.Invoke(playerBridge, null) as Player;
                SAINSoundTypeHandler.AISoundFileChecker(soundName, player);
            }
        }
    }

    public class LootingSoundPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), "method_39");
        }

        [PatchPostfix]
        public static void PatchPostfix(ref Player __instance, int count)
        {
            if (count > 0
                && __instance != null
                && SAINPlugin.BotController != null)
            {
                SAINPlugin.BotController.AISoundPlayed?.Invoke(SAINSoundType.Looting, __instance, 50f);
            }
        }
    }

    public class TurnSoundPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), "method_47");
        }

        [PatchPrefix]
        public static void PatchPrefix(ref Player __instance, ref float ____lastTimeTurnSound, ref float ___maxLengthTurnSound)
        {
            if (Time.time - ____lastTimeTurnSound >= ___maxLengthTurnSound && SAINPlugin.BotController != null)
            {
                SAINPlugin.BotController.AISoundPlayed?.Invoke(SAINSoundType.FootStep, __instance, 50f);
            }
        }
    }

    public class ProneSoundPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), "PlaySoundBank");
        }

        [PatchPrefix]
        public static void PatchPrefix(ref Player __instance, ref string soundBank)
        {
            if (soundBank == "Prone"
                && __instance.SinceLastStep >= 0.5f
                && SAINPlugin.BotController?.AISoundPlayed != null
                && __instance.CheckSurface())
            {
                SAINPlugin.BotController.AISoundPlayed?.Invoke(SAINSoundType.Prone, __instance, 40f);
            }
        }
    }

    public class AimSoundPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), "method_46");
        }

        [PatchPrefix]
        public static void PatchPrefix(float volume, Player __instance)
        {
            if (SAINPlugin.BotController != null)
            {
                SAINPlugin.BotController.AISoundPlayed?.Invoke(SAINSoundType.Aim, __instance, 30f * volume);
            }
        }
    }

    public class SetInHandsGrenadePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), "SetInHands", 
                new[] { typeof(GrenadeClass), typeof(Callback<IThrowableCallback>) });
        }

        [PatchPrefix]
        public static void PatchPrefix(Player __instance)
        {
            if (SAINPlugin.BotController != null)
            {
                SAINPlugin.BotController.AISoundPlayed?.Invoke(SAINSoundType.GrenadeDraw, __instance, 30f);
            }
        }
    }

    public class SetInHandsFoodPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), "SetInHands", 
                new[] { typeof(FoodClass), typeof(float), typeof(int), typeof(Callback<IMedsController>) });
        }

        [PatchPrefix]
        public static void PatchPrefix(Player __instance)
        {
            if (SAINPlugin.BotController != null)
            {
                SAINPlugin.BotController.AISoundPlayed?.Invoke(SAINSoundType.Food, __instance, 20f);
            }
        }
    }

    public class SetInHandsMedsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), "SetInHands", 
                new[] { typeof(MedsClass), typeof(EBodyPart), typeof(int), typeof(Callback<IMedsController>) });
        }

        [PatchPrefix]
        public static void PatchPrefix(Player __instance)
        {
            if (SAINPlugin.BotController != null)
            {
                SAINPlugin.BotController.AISoundPlayed?.Invoke(SAINSoundType.Heal, __instance, 30f);
            }
        }
    }
}