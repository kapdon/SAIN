﻿using SAIN.SAINComponent;
using System.Collections.Generic;
using System.Text;

namespace SAIN.BotController.Classes
{
    public class SquadPersonalityManager
    {
        public static ESquadPersonality GetSquadPersonality(Dictionary<string, SAINComponentClass> Members, out SquadPersonalitySettings settings)
        {
            GetMemberPersonalities(Members);
            IPersonality mostFrequentPersonality =  GetMostFrequentPersonality(PersonalityCounts, out int count);
            ESquadPersonality result = PickSquadPersonality(mostFrequentPersonality);
            settings = GetSquadSettings(result);
            return result;
        }

        private static void GetMemberPersonalities(Dictionary<string, SAINComponentClass> Members)
        {
            PersonalityCounts.Clear();
            MemberPersonalities.Clear();

            foreach (var member in Members.Values)
            {
                if (member?.Player != null && member.Player.HealthController.IsAlive)
                {
                    var personality = member.Info.Personality;
                    MemberPersonalities.Add(personality);
                    if (!PersonalityCounts.ContainsKey(personality))
                    {
                        PersonalityCounts.Add(personality, 1);
                    }
                    else
                    {
                        PersonalityCounts[personality]++;
                    }
                }
            }

            StringBuilder stringbuilder = new StringBuilder();
            foreach (var personality in PersonalityCounts)
            {
                stringbuilder.AppendLine($"[{personality.Key}] : [{personality.Value}]");
            }

            //Logger.LogAndNotifyInfo(stringbuilder.ToString());

        }

        private static IPersonality GetMostFrequentPersonality(Dictionary<IPersonality, int> PersonalityCounts, out int count)
        {
            count = 0;
            IPersonality mostFrequent = IPersonality.Normal;
            foreach (var personalityCount in PersonalityCounts)
            {
                if (personalityCount.Value > count)
                {
                    count = personalityCount.Value;
                    mostFrequent = personalityCount.Key;
                }
            }

            //Logger.LogAndNotifyInfo($"Most Frequent Personality [{mostFrequent}] : Count {count}");
            return mostFrequent;
        }

        private static ESquadPersonality PickSquadPersonality(IPersonality mostFrequentPersonality)
        {
            ESquadPersonality result = ESquadPersonality.None;
            switch (mostFrequentPersonality)
            {
                case IPersonality.GigaChad:
                case IPersonality.Chad:
                case IPersonality.Wreckless:
                    result = Helpers.EFTMath.RandomBool(66) ? ESquadPersonality.GigaChads : ESquadPersonality.Elite;
                    break;

                case IPersonality.Timmy:
                case IPersonality.Coward:
                    result = ESquadPersonality.TimmyTeam6;
                    break;

                case IPersonality.Rat:
                case IPersonality.SnappingTurtle:
                    result = ESquadPersonality.Rats;
                    break;

                default:
                    result = Helpers.EnumValues.GetEnum<ESquadPersonality>().PickRandom();
                    break;
            }

            //Logger.LogAndNotifyInfo($"Assigned Squad Personality of [{result}] because most frequent personality is [{mostFrequentPersonality}]");
            return result;
        }

        private static SquadPersonalitySettings GetSquadSettings(ESquadPersonality squadPersonality)
        {
            switch (squadPersonality)
            {
                case ESquadPersonality.Elite:
                    return CreateSettings(squadPersonality, 3, 5, 4);

                case ESquadPersonality.GigaChads:
                    return CreateSettings(squadPersonality, 5, 4, 5);

                case ESquadPersonality.Rats:
                    return CreateSettings(squadPersonality, 1, 2, 1);

                case ESquadPersonality.TimmyTeam6:
                    return CreateSettings(squadPersonality, 3, 1, 2);

                default:
                    return CreateSettings(squadPersonality, 3, 3, 3);
            }
        }

        private static SquadPersonalitySettings CreateSettings(ESquadPersonality squadPersonality, float vocalization, float coordination, float aggression)
        {
            if (!SquadSettings.ContainsKey(squadPersonality))
            {
                var settings = new SquadPersonalitySettings
                {
                    VocalizationLevel = vocalization,
                    CoordinationLevel = coordination,
                    AggressionLevel = aggression
                };
                SquadSettings.Add(squadPersonality, settings);
            }
            return SquadSettings[squadPersonality];
        }

        private static readonly List<IPersonality> MemberPersonalities = new List<IPersonality>();
        private static readonly Dictionary<IPersonality, int> PersonalityCounts = new Dictionary<IPersonality, int>();
        private static readonly Dictionary<ESquadPersonality, SquadPersonalitySettings> SquadSettings = new Dictionary<ESquadPersonality, SquadPersonalitySettings>();
    }
}
