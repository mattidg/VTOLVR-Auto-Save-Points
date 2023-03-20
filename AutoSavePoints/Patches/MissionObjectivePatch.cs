using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AutoSavePoints
{
    [HarmonyPatch(typeof(MissionObjective), "CompleteObjective")]
    
    public class MissionObjectivePatch
    {
        public static float missionCompletionCount
        {
            get; set;
        }

        public void Postfix()
        {
            missionCompletionCount++;

            Debug.Log("Mission completetion count has been incremented.");
        }
    }
}
