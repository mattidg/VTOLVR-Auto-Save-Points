using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI.Extensions;


/*
 * Expectation:
 * Have the game auto save after a completed mission.
 * 
 * Reality:
 * Giant pain in the ass to figure out the correct syntax for the method calls to using things already created in VTOL VR. I at first thought I should Harmony Patch the SaveLoadMenu
 * but I don't actually have to do that, I can call the method by just having a listener for the Unity event.
 * 
 * End Goal:
 * Allow for a settings file so the Auto Quick Save can happen after a user set amount of completed missions, more than likely limited from 1 to 5.
*/
namespace AutoSavePoints

{
    public class Main : VTOLMOD
    {
        // This method is run once, when the Mod Loader is done initialising this game object
        public override void ModLoaded()
        {
            //This is an event the VTOLAPI calls when the game is done loading a scene
            VTOLAPI.SceneLoaded += SceneLoaded;
            base.ModLoaded();
        }

        private void getObjectives()
        {
            MissionObjective[] objectives = FindObjectsOfType<MissionObjective>();

            foreach (MissionObjective objective in objectives)
            {
                objective.OnComplete.AddListener(() => objectiveComplete(objective));
            }

        }

        private void objectiveComplete(MissionObjective objective)
        {
            SaveLoadMenu quickSave = new SaveLoadMenu();

            quickSave.SaveGame();

            TutorialLabel.instance.DisplayLabel("Game has been quick saved by AutoSavePoints", null, 7);
        }

        //This method is called every frame by Unity. Here you'll probably put most of your code
        void Update()
        {
            
        }

        //This method is like update but it's framerate independent. This means it gets called at a set time interval instead of every frame. This is useful for physics calculations
        void FixedUpdate()
        {

        }

        //This function is called every time a scene is loaded. this behaviour is defined in Awake().
        private void SceneLoaded(VTOLScenes scene)
        {
            //If you want something to happen in only one (or more) scenes, this is where you define it.

            //For example, lets say you're making a mod which only does something in the ready room and the loading scene. This is how your code could look:
            switch (scene)
            {
                case VTOLScenes.ReadyRoom:
                    //Add your ready room code here
                    break;
                case VTOLScenes.LoadingScene:
                    //Add your loading scene code here
                    break;
            }
        }
    }
}