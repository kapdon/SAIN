﻿using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.SubComponents;
using SAIN.SAINComponent;
using System.Text;
using UnityEngine;
using SAIN.SAINComponent.SubComponents.CoverFinder;
using SAIN.Layers.Combat.Solo;
using UnityEngine.AI;
using SAIN.Helpers;

namespace SAIN.Layers.Combat.Run
{
    internal class GetUnstuckAction : SAINAction
    {
        public GetUnstuckAction(BotOwner bot) : base(bot, nameof(GetUnstuckAction))
        {
        }

        public override void Update()
        {
            SAIN.Mover.SetTargetPose(1f);
            SAIN.Mover.SetTargetMoveSpeed(1f);
            SAIN.Steering.LookToMovingDirection();

            Vector3? unstuckDestination = null;
            var coverPoints = SAIN.Cover.CoverPoints;
            if (coverPoints.Count > 0)
            {
                for (int i = 0; i < coverPoints.Count; i++)
                {
                    var cover = coverPoints[i];
                    NavMeshPath path = new NavMeshPath();
                    if (NavMesh.CalculatePath(cover.Position, SAIN.Position, -1, path))
                    {
                        unstuckDestination = new Vector3?(path.corners[path.corners.Length - 1]);
                        break;
                    }
                }
            }

            if (unstuckDestination == null)
            {
                var preVaultPosition = SAIN.BotStuck.PreVaultPositions;
                for (int i = 0; i < preVaultPosition.Count; i++)
                {
                    var pos = preVaultPosition[i];
                    NavMeshPath path = new NavMeshPath();
                    if (NavMesh.CalculatePath(pos, SAIN.Position, -1, path))
                    {
                        unstuckDestination = new Vector3?(path.corners[path.corners.Length - 1]);
                        break;
                    }
                    else
                    {
                        unstuckDestination = new Vector3?(pos);
                        break;
                    }
                }
            }

            if (unstuckDestination != null)
            {
                BotOwner.Mover.GoToByWay(new Vector3[] { SAIN.Position, unstuckDestination.Value }, -1f);
            }
        }

        public override void Start()
        {
            SAIN.Mover.StopMove();
        }

        public override void Stop()
        {
        }

        public override void BuildDebugText(StringBuilder stringBuilder)
        {

        }
    }
}