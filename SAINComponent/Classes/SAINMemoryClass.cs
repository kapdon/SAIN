﻿using EFT;
using EFT.Interactive;
using SAIN.SAINComponent.Classes.Decision;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes
{
    public class SAINMemoryClass : SAINBase, ISAINClass
    {
        public SAINMemoryClass(SAINComponentClass sain) : base(sain)
        {
            Decisions = new DecisionWrapper(sain);
        }

        public Action<ETagStatus> HealthStatusChanged { get; set; }

        public void Init() 
        {
        }

        public void Update()
        {
            if (UpdateHealthTimer < Time.time)
            {
                UpdateHealthTimer = Time.time + 0.5f;

                var oldStatus = HealthStatus;
                HealthStatus = Player.HealthStatus;
                if (HealthStatus != oldStatus)
                {
                    HealthStatusChanged?.Invoke(HealthStatus);
                }
            }
            if (_checkIndoorsTime < Time.time)
            {
                _checkIndoorsTime = Time.time + 1f;
                IsIndoors = !Physics.Raycast(BotOwner.LookSensor._headPoint, Vector3.up, 10f, LayerMaskClass.HighPolyWithTerrainMask);
            }
        }

        public void Dispose()
        {
        }

        public bool IsIndoors { get; private set; }
        private float _checkIndoorsTime;

        public Collider BotZoneCollider => BotZone?.Collider;
        public AIPlaceInfo BotZone => BotOwner.AIData.PlaceInfo;

        public List<Player> VisiblePlayers = new List<Player>();

        public List<string> VisiblePlayerIds = new List<string>();

        private float UpdateHealthTimer = 0f;

        public Vector3? ExfilPosition { get; set; }
        public ExfiltrationPoint ExfilPoint { get; set; }

        public bool Healthy => HealthStatus == ETagStatus.Healthy;
        public bool Injured => HealthStatus == ETagStatus.Injured;
        public bool BadlyInjured => HealthStatus == ETagStatus.BadlyInjured;
        public bool Dying => HealthStatus == ETagStatus.Dying;

        public ETagStatus HealthStatus { get; private set; }

        public Vector3 UnderFireFromPosition { get; set; }

        public DecisionWrapper Decisions { get; private set; }
    }
}