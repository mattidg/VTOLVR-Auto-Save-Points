using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI.Extensions;


/*
Expectation:
Allow for auto quick saving after every set amount of completed objectives, the player will be able to edit after how many missions the mod will auto quick save the game for them.

Reality:

*/
namespace AutoSavePoints
{
    public class Main : VTOLMOD
    {
        float missionCompletionCountUsed = MissionObjectivePatch.missionCompletionCount;
        float newMissionCompletionCheck = 0;

        // This method is run once, when the Mod Loader is done initialising this game object
        public override void ModLoaded()
        {
            HarmonyInstance harmonyInstance = HarmonyInstance.Create("Auto_Quicksave");
            try
            {
                harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch
            {
                TutorialLabel.instance.DisplayLabel("The mod was unable to load correctly.", null, 7);
            }

            //This is an event the VTOLAPI calls when the game is done loading a scene
            base.ModLoaded();
            Debug.Log("Mod has been loaded");
        }

        public void QuicksaveManager()
        {

        }

        //This method is called every frame by Unity. Here you'll probably put most of your code
        void Update()
        {
            if(newMissionCompletionCheck != missionCompletionCountUsed)
            {
                if(newMissionCompletionCheck == 1)
                {
                    var save = new QuicksaveManager();

                    save.Quicksave();

                    Debug.Log("Game has been saved");
                }
            }
            else
            {

            }
        }
    }
}