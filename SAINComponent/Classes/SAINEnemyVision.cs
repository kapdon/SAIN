﻿using EFT;
using UnityEngine;

namespace SAIN.SAINComponent.Classes
{
    public class SAINEnemyVision : EnemyBase
    {
        public SAINEnemyVision(SAINEnemy enemy) : base(enemy)
        {
        }

        public void Update(bool isCurrentEnemy)
        {
            if (Enemy == null || BotOwner == null || BotOwner.Settings?.Current == null || EnemyPlayer == null)
            {
                return;
            }

            float timeToAdd;
            bool performanceMode = SAINPlugin.LoadedPreset.GlobalSettings.General.PerformanceMode;
            if (!isCurrentEnemy && Enemy.IsAI)
            {
                timeToAdd = performanceMode ? 4f : 2f;
            }
            else if (performanceMode)
            {
                timeToAdd = isCurrentEnemy ? 0.15f : 1f;
            }
            else
            {
                timeToAdd = isCurrentEnemy ? 0.1f : 0.5f;
            }

            bool visible = false;
            bool canshoot = false;

            if (CheckLosTimer + timeToAdd < Time.time)
            {
                CheckLosTimer = Time.time;
                InLineOfSight = CheckLineOfSight(true, !isCurrentEnemy);
            }

            var enemyInfo = EnemyInfo;
            if (enemyInfo?.IsVisible == true && InLineOfSight)
            {
                visible = true;
            }
            if (enemyInfo?.CanShoot == true)
            {
                canshoot = true;
            }

            UpdateVisible(visible);
            UpdateCanShoot(canshoot);
        }

        public bool FirstContactOccured { get; private set; }
        public bool ShallReportRepeatContact { get; set; }
        public bool ShallReportLostVisual { get; set; }

        private bool CheckLineOfSight(bool noDistRestrictions = false, bool simpleCheck = false)
        {
            if (Enemy == null || BotOwner == null || BotOwner.Settings?.Current == null || EnemyPlayer == null)
            {
                return false;
            }
            if (SAINPlugin.DebugMode && EnemyPlayer.IsYourPlayer)
            {
                //Logger.LogInfo($"EnemyDistance [{Enemy.RealDistance}] Vision Distance [{BotOwner.Settings.Current.CurrentVisibleDistance}]");
            }
            if (noDistRestrictions || Enemy.RealDistance <= BotOwner.Settings.Current.CurrentVisibleDistance)
            {
                if (simpleCheck)
                {
                    if (SAIN.SightChecker != null)
                    {
                        return SAIN.SightChecker.SimpleSightCheck(Enemy.EnemyChestPosition, BotOwner.LookSensor._headPoint);
                    }
                    else
                    {
                        Logger.LogError("SightChecker is null");
                        Vector3 headPos = BotOwner.LookSensor._headPoint;
                        Vector3 direction = Enemy.EnemyChestPosition - headPos;
                        return !Physics.Raycast(headPos, direction, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask);
                    }
                }
                else
                {
                    if (SAIN.SightChecker != null)
                    {
                        return SAIN.SightChecker.CheckLineOfSight(EnemyPlayer);
                    }
                    else
                    {
                        Logger.LogError("SightChecker is null");
                        foreach (var part in EnemyPlayer.MainParts.Values)
                        {
                            Vector3 headPos = BotOwner.LookSensor._headPoint;
                            Vector3 direction = part.Position - headPos;
                            if (!Physics.Raycast(headPos, direction, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private const float _repeatContactMinSeenTime = 12f;
        private const float _lostContactMinSeenTime = 12f;

        public void UpdateVisible(bool visible)
        {
            bool wasVisible = IsVisible;
            IsVisible = visible;

            if (IsVisible)
            {
                if (!wasVisible)
                {
                    VisibleStartTime = Time.time;
                }
                if (!wasVisible 
                    && TimeSinceSeen >= _repeatContactMinSeenTime)
                {
                    ShallReportRepeatContact = true;
                }
                if (!Seen)
                {
                    FirstContactOccured = true;
                    TimeFirstSeen = Time.time;
                    Seen = true;
                }
                TimeLastSeen = Time.time;
                LastSeenPosition = EnemyPerson.Position;
                Enemy.UpdateKnownPosition(EnemyPerson.Position, false, true);
            }

            if (!IsVisible)
            {
                if (Seen 
                    && TimeSinceSeen > _lostContactMinSeenTime 
                    && _nextReportLostVisualTime < Time.time)
                {
                    _nextReportLostVisualTime = Time.time + 20f;
                    ShallReportLostVisual = true;
                }
                VisibleStartTime = -1f;
            }

            if (IsVisible != wasVisible)
            {
                LastChangeVisionTime = Time.time;
            }
        }

        private float _nextReportLostVisualTime;

        private void CheckForAimingDelay()
        {

        }

        public void UpdateCanShoot(bool value)
        {
            CanShoot = value;
        }

        public bool InLineOfSight { get; private set; }
        public bool IsVisible { get; private set; }
        public bool CanShoot { get; private set; }
        public Vector3? LastSeenPosition { get; set; }
        public float VisibleStartTime { get; private set; }
        public float TimeSinceSeen => Seen ? Time.time - TimeLastSeen : -1f;
        public bool Seen { get; private set; }
        public float TimeFirstSeen { get; private set; }
        public float TimeLastSeen { get; private set; }
        public float LastChangeVisionTime { get; private set; }

        private float CheckLosTimer;
    }
}