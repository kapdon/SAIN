﻿using UnityEngine;

namespace SAIN.SAINComponent.Classes
{
    public enum EEnemyAction
    {
        None = 0,
        Reloading = 1,
        HasGrenade = 2,
        Healing = 3,
        UsingSurgery = 4,
        TryingToExtract = 5,
        Looting = 6,
    }

    public class SAINEnemyStatus : EnemyBase
    {
        public EEnemyAction EnemyAction
        {
            get
            {
                if (EnemyIsReloading)
                {
                    return EEnemyAction.Reloading;
                }
                else if (EnemyHasGrenadeOut)
                {
                    return EEnemyAction.HasGrenade;
                }
                else if (EnemyIsHealing)
                {
                    return EEnemyAction.Healing;
                }
                else
                {
                    return EEnemyAction.None;
                }
            }
            set
            {
                switch (value)
                {
                    case EEnemyAction.None:
                        break;

                    case EEnemyAction.Reloading:
                        EnemyIsReloading = true;
                        break;

                    case EEnemyAction.HasGrenade:
                        EnemyHasGrenadeOut = true;
                        break;

                    case EEnemyAction.Healing:
                        EnemyIsHealing = true;
                        break;
                }
            }
        }

        public SAINEnemyStatus(SAINEnemy enemy) : base(enemy)
        {
        }

        public bool FlareEnabled
        {
            get
            {
                if (Enemy.LastKnownPosition != null
                    && (Enemy.LastKnownPosition.Value - EnemyPlayer.Position).sqrMagnitude < _maxDistFromPosFlareEnabled * _maxDistFromPosFlareEnabled)
                {
                    return true;
                }
                return false;
            }
        }

        private const float _maxDistFromPosFlareEnabled = 10f;

        public bool EnemyLookingAtMe
        {
            get
            {
                if (_nextCheckEnemyLookTime < Time.time)
                {
                    _nextCheckEnemyLookTime = Time.time + 0.2f;
                    Vector3 directionToBot = (SAIN.Position - EnemyPosition).normalized;
                    Vector3 enemyLookDirection = EnemyPerson.Transform.LookDirection.normalized;
                    float dot = Vector3.Dot(directionToBot, enemyLookDirection);
                    _enemyLookAtMe = dot >= 0.9f;
                }
                return _enemyLookAtMe;
            }
        }

        private bool _enemyLookAtMe;
        private float _nextCheckEnemyLookTime;

        public bool SearchStarted
        {
            get
            {
                return _searchStarted.Value;
            }
            set
            {
                if (value)
                {
                    TimeSearchLastStarted = Time.time;
                }
                _searchStarted.Value = value;
            }
        }

        public int NumberOfSearchesStarted { get; set; }
        public float TimeSearchLastStarted { get; private set; }
        public float TimeSinceSearchLastStarted => Time.time - TimeSearchLastStarted;

        private readonly ExpirableBool _searchStarted = new ExpirableBool(300f, 0.85f, 1.15f);

        public bool EnemyIsSuppressed
        {
            get
            {
                return _enemyIsSuppressed.Value;
            }
            set
            {
                _enemyIsSuppressed.Value = value;
            }
        }

        private readonly ExpirableBool _enemyIsSuppressed = new ExpirableBool(4f, 0.85f, 1.15f);

        public bool ShotAtMeRecently
        {
            get
            {
                return _enemyShotAtMe.Value;
            }
            set
            {
                _enemyShotAtMe.Value = value;
            }
        }

        private readonly ExpirableBool _enemyShotAtMe = new ExpirableBool(30f, 0.75f, 1.25f);

        public bool EnemyIsReloading
        {
            get
            {
                return _enemyIsReloading.Value;
            }
            set
            {
                _enemyIsReloading.Value = value;
            }
        }

        private readonly ExpirableBool _enemyIsHealing = new ExpirableBool(4f, 0.75f, 1.25f);

        public bool EnemyHasGrenadeOut
        {
            get
            {
                return _enemyHasGrenade.Value;
            }
            set
            {
                _enemyHasGrenade.Value = value;
            }
        }

        private readonly ExpirableBool _enemyHasGrenade = new ExpirableBool(4f, 0.75f, 1.25f);

        public bool EnemyIsHealing
        {
            get
            {
                return _enemyIsHealing.Value;
            }
            set
            {
                _enemyIsHealing.Value = value;
            }
        }

        private readonly ExpirableBool _enemyIsReloading = new ExpirableBool(4f, 0.75f, 1.25f);
    }
}