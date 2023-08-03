﻿using Newtonsoft.Json;
using SAIN.SAINPreset.Attributes;
using System.ComponentModel;
using System.Reflection;

namespace SAIN.BotSettings.Categories
{
    public class SAINAimingSettings
    {
        public bool FasterCQBReactions = true;
        public float FasterCQBReactionsDistance = 30f;
        public float FasterCQBReactionsMinimum = 0.15f;
        public float AccuracySpreadMulti = 1f;

        [Name("Aiming Upgrade By Time")]
        [Description(null)]
        [DefaultValue(0.8f)]
        [Minimum(0.1f)]
        [Maximum(0.95f)]
        [Rounding(100)]
        public float MAX_AIMING_UPGRADE_BY_TIME = 0.8f;

        [Name("Max Aim Time")]
        [Description(null)]
        [DefaultValue(2f)]
        [Minimum(0.1f)]
        [Maximum(5f)]
        [Rounding(10)]
        public float MAX_AIM_TIME = 2f;

        [Name("Aim Type")]
        [Description(null)]
        [DefaultValue(4)]
        [Minimum(1)]
        [Maximum(6)]
        public int AIMING_TYPE = 4;

        [Name("Frieldly Fire Spherecast Size")]
        [Description(null)]
        [DefaultValue(0.15f)]
        [Minimum(0f)]
        [Maximum(0.5f)]
        [Rounding(100)]
        public float SHPERE_FRIENDY_FIRE_SIZE = 0.15f;

        [DefaultValue(1)]
        [IsHidden(true)]
        public int RECALC_MUST_TIME = 1;

        [DefaultValue(1)]
        [IsHidden(true)]
        public int RECALC_MUST_TIME_MIN = 1;

        [DefaultValue(2)]
        [IsHidden(true)]
        public int RECALC_MUST_TIME_MAX = 2;

        [Name("Hit Reaction Recovery Time")]
        [Description("How much time it takes to recover a bot's aim when they get hit by a bullet")]
        [DefaultValue(0.5f)]
        [Minimum(0.1f)]
        [Maximum(0.95f)]
        [Rounding(100)]
        public float BASE_HIT_AFFECTION_DELAY_SEC = 0.5f;

        [Name("Minimum Hit Reaction Angle")]
        [Description("How much to kick a bot's aim when they get hit by a bullet")]
        [DefaultValue(3f)]
        [Minimum(0f)]
        [Maximum(25f)]
        [Rounding(10)]
        public float BASE_HIT_AFFECTION_MIN_ANG = 3f;

        [Name("Maximum Hit Reaction Angle")]
        [Description("How much to kick a bot's aim when they get hit by a bullet")]
        [DefaultValue(5f)]
        [Minimum(0f)]
        [Maximum(25f)]
        [Rounding(10)]
        public float BASE_HIT_AFFECTION_MAX_ANG = 5f;
    }
}