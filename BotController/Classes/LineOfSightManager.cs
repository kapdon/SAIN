using BepInEx.Logging;
using Comfort.Common;
using EFT;
using SAIN.Helpers;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes.Decision;
using SAIN.SAINComponent.Classes.Talk;
using SAIN.SAINComponent.Classes.WeaponFunction;
using SAIN.SAINComponent.Classes.Mover;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.SubComponents;
using System.Collections;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace SAIN.Components
{
    public class LineOfSightComponent : MonoBehaviour
    {
        private SAINBotControllerComponent BotController;

        public void Awake()
        {
            BotController = GetComponent<SAINBotControllerComponent>();
        }

        public void Update()
        {

        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            BotList.Clear();
        }

        private IEnumerator CheckLineOfSightForBots()
        {
            
            yield return null;
        }

        private readonly List<SAINComponentClass> BotList = new List<SAINComponentClass>();

    }

    public class LineOfSightManager : SAINControl
    {
        public void Update()
        {
            Frames++;
            if (Frames >= 10)
            {
                Frames = 0;
                if (Bots != null && Bots.Count > 0)
                {
                    foreach (var bot in Bots)
                    {
                        if (bot.Value != null)
                        {
                            TempBotList.Add(bot.Value);
                        }
                    }

                    GlobalRaycastJob();

                    TempBotList.Clear();
                }
            }
        }

        private Vector3 HeadPos(Player player)
        {
            return player.MainParts[BodyPartType.head].Position;
        }

        private Vector3 BodyPos(Player player)
        {
            return player.MainParts[BodyPartType.body].Position;
        }

        private void GlobalRaycastJob()
        {
            int total = TempBotList.Count * Players.Count;

            NativeArray<SpherecastCommand> allSpherecastCommands = new NativeArray<SpherecastCommand>(
                total,
                Allocator.TempJob
            );
            NativeArray<RaycastHit> allRaycastHits = new NativeArray<RaycastHit>(
                total,
                Allocator.TempJob
            );

            total = 0;
            for (int i = 0; i < TempBotList.Count; i++)
            {
                var bot = TempBotList[i];
                Vector3 head = HeadPos(bot.BotOwner.GetPlayer);

                for (int j = 0; j < Players.Count; j++)
                {
                    Vector3 target = BodyPos(Players[j]);
                    Vector3 direction = target - head;
                    float max = 300f;
                    //float max = bot.BotOwner.Settings.Current.CurrentVisibleDistance;
                    float rayDistance = Mathf.Clamp(direction.magnitude, 0f, max);

                    allSpherecastCommands[total] = new SpherecastCommand(
                        head,
                        SpherecastRadius,
                        direction.normalized,
                        rayDistance,
                        SightLayers
                    );
                    total++;
                }
            }

            JobHandle spherecastJob = SpherecastCommand.ScheduleBatch(
                allSpherecastCommands,
                allRaycastHits,
                MinJobSize
            );

            spherecastJob.Complete();
            total = 0;

            for (int i = 0; i < TempBotList.Count; i++)
            {
                var visPlayers = TempBotList[i].Memory.VisiblePlayers;
                var idList = TempBotList[i].Memory.VisiblePlayerIds;
                visPlayers.Clear();
                for (int j = 0; j < Players.Count; j++)
                {
                    Player player = Players[j];
                    if (allRaycastHits[total].collider == null && player != null && player.HealthController.IsAlive)
                    {
                        visPlayers.Add(player);
                        string id = player.ProfileId;
                        if (!idList.Contains(id))
                        {
                            idList.Add(id);
                        }
                    }
                    total++;
                }
            }

            allSpherecastCommands.Dispose();
            allRaycastHits.Dispose();
        }

        private readonly float SpherecastRadius = 0.025f;
        private LayerMask SightLayers => LayerMaskClass.HighPolyWithTerrainMaskAI;
        private readonly int MinJobSize = 6;
        private List<Player> Players => EFTInfo.AlivePlayers;
        private readonly List<SAINComponentClass> TempBotList = new List<SAINComponentClass>();
        private int Frames = 0;
    }
}