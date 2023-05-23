using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.Events;
using Valve.Newtonsoft.Json;


/*
 * Expectation:
 * Have the game auto save after a user set amount of completed mission.
 * 
 * Reality:
 * This was a bitch, the first issue was figuring out how the game does autosaves, that took a few days by itself. After that it took figuring out how to do settings and create a persistant
 * file for settings, which then after that there was an issue where all the objectives in the mission I tested existed 3 times so the AutoSave() Method would be called three times in a row.
 * 
 * End Goal:
 * Allow for a settings file so the Auto Quick Save can happen after a user set amount of completed missions, more than likely limited from 1 to 5.
*/
namespace AutoSavePoints

{
    public class AutoSavePointsSettings
    {
        public int savedUserMissionsAmount = 2;
    }

    public class AutoSavePoints : VTOLMOD
    {
        private int missionsCompletedAmount;
        private bool settingsChanged;
        private static AutoSavePointsSettings aspSettings;
        public UnityAction<int> savedUserMissionsAmount_changed;
        List<MissionObjective> missionsList = new List<MissionObjective>();
        List<MissionObjective> missionsListUnique = new List<MissionObjective>();
        HashSet<MissionObjective> addedListeners = new HashSet<MissionObjective>();
        private static Settings modSettings;

        // This method is run once, when the Mod Loader is done initialising this game object
        public override void ModLoaded()
        {
            //This is an event the VTOLAPI calls when the game is done loading a scene
            VTOLAPI.SceneLoaded += SceneLoaded;
            base.ModLoaded();

            aspSettings = new AutoSavePointsSettings();
            LoadSaveFile();

            savedUserMissionsAmount_changed += savedUserMissionsAmount_Settings;
            modSettings = new Settings(this);
            modSettings.CreateCustomLabel("This will determine how many missions to complete before ");
            modSettings.CreateCustomLabel("an automatic quicksave occurs");
            modSettings.CreateCustomLabel("Default = 2, Minimum = 1, Maximum = 5");
            modSettings.CreateIntSetting("Amount of Missions to Complete:", savedUserMissionsAmount_changed, aspSettings.savedUserMissionsAmount, 1, 5);
            VTOLAPI.CreateSettingsMenu(modSettings);
        }

        private void savedUserMissionsAmount_Settings(int amount)
        {
            FlightLogger.Log("Sanity Check: " + "ChangedValue has run." + amount.ToString());
            aspSettings.savedUserMissionsAmount = amount;
            settingsChanged = true;
        }

        private void CheckSaveFile()
        {
            FlightLogger.Log("Sanity Check: " + "CheckSaveFile has run.");
            if (settingsChanged)
            {
                FlightLogger.Log("Sanity Check: " + "Saving to file.");
                SaveFile();
            }
            else
            {
                FlightLogger.Log("Sanity Check: " + "Not saving file.");
            }
        }

        private void SaveFile()
        {
            FlightLogger.Log("Sanity Check: " + "SaveFile has run.");
            string path = ModFolder;

            if (Directory.Exists(path))
            {
                FlightLogger.Log("Sanity Check: " + "Checking save file existance.");
                File.WriteAllText(path + @"\settings.json", JsonConvert.SerializeObject(aspSettings));
                settingsChanged = false;
            }
            else
            {
                FlightLogger.Log("Sanity Check: " + "No save file found.");
            }
        }

        private void LoadSaveFile()
        {
            FlightLogger.Log("Sanity Check: " + "LoadSaveFile has run.");
            string path = ModFolder;

            if (Directory.Exists(path))
            {
                FlightLogger.Log("Sanity Check: " + "Checking if the save file exists.");

                try
                {
                    FlightLogger.Log("Sanity Check: " + "Attempting to read the user save file.");
                    string temporary = File.ReadAllText(path + @"\settings.json");

                    aspSettings = JsonConvert.DeserializeObject<AutoSavePointsSettings>(temporary);
                    settingsChanged = false;
                }
                catch
                {
                    FlightLogger.Log("Sanity Check: " + "No save file found, running SaveFile.");
                    SaveFile();
                }
            }
            else
            {
                FlightLogger.Log("Sanity Check: " + "Mod folder not found.");
            }
        }

        private void AddMissions()
        {
            missionsList = new List<MissionObjective>(FindObjectsOfType<MissionObjective>());
            SetListeners();
        }

        private void SetListeners()
        {
            missionsListUnique = missionsList.Distinct().ToList();

            foreach (MissionObjective objective in missionsListUnique)
            {
                if (!addedListeners.Contains(objective))
                {
                    objective.OnComplete.AddListener(() => AutoSave());
                    addedListeners.Add(objective);
                    FlightLogger.Log("Sanity Check (Added Listener): " + objective + " " + objective.OnComplete.HasListeners());
                }
            }

            missionsListUnique.Clear();
            missionsList.Clear();
        }


        private void AutoSave()
        {
            FlightLogger.Log("missionsCompletedAmount Before AutoSave: " + missionsCompletedAmount);
            missionsCompletedAmount++;

            if (missionsCompletedAmount == 0)
            {
                FlightLogger.Log("Sanity Check: " + "LMAOXD you can't divide by 0 dawg.");
            }
            else if (aspSettings.savedUserMissionsAmount == 5 && missionsCompletedAmount % 5 == 0)
            {
                FlightLogger.Log("Sanity Check: " + "AutoSave has run. 5");
                FlightLogger.Log("missionsCompletedAmount 5: " + missionsCompletedAmount);
                QuicksaveManager quickSaveInstance;
                quickSaveInstance = QuicksaveManager.instance;
                quickSaveInstance.Quicksave();
                TutorialLabel.instance.DisplayLabel("The game was Quick Saved by Auto Save Points. 5", null, 10);
                missionsCompletedAmount = 0;
            }
            else if (aspSettings.savedUserMissionsAmount == 4 && missionsCompletedAmount % 4 == 0)
            {
                FlightLogger.Log("Sanity Check: " + "AutoSave has run. 4");
                FlightLogger.Log("missionsCompletedAmount 4: " + missionsCompletedAmount);
                QuicksaveManager quickSaveInstance;
                quickSaveInstance = QuicksaveManager.instance;
                quickSaveInstance.Quicksave();
                TutorialLabel.instance.DisplayLabel("The game was Quick Saved by Auto Save Points. 4", null, 10);
                missionsCompletedAmount = 0;
            }
            else if (aspSettings.savedUserMissionsAmount == 3 && missionsCompletedAmount % 3 == 0)
            {
                FlightLogger.Log("Sanity Check: " + "AutoSave has run. 3");
                FlightLogger.Log("missionsCompletedAmount 3: " + missionsCompletedAmount);
                QuicksaveManager quickSaveInstance;
                quickSaveInstance = QuicksaveManager.instance;
                quickSaveInstance.Quicksave();
                TutorialLabel.instance.DisplayLabel("The game was Quick Saved by Auto Save Points. 3", null, 10);
                missionsCompletedAmount = 0;
            }
            else if (aspSettings.savedUserMissionsAmount == 2 && missionsCompletedAmount % 2 == 0)
            {
                FlightLogger.Log("Sanity Check: " + "AutoSave has run. 2");
                FlightLogger.Log("missionsCompletedAmount 2: " + missionsCompletedAmount);
                QuicksaveManager quickSaveInstance;
                quickSaveInstance = QuicksaveManager.instance;
                quickSaveInstance.Quicksave();
                TutorialLabel.instance.DisplayLabel("The game was Quick Saved by Auto Save Points. 2", null, 10);
                missionsCompletedAmount = 0;
            }
            else if (aspSettings.savedUserMissionsAmount == 1 && missionsCompletedAmount % 1 == 0)
            {
                FlightLogger.Log("Sanity Check: " + "AutoSave has run. 1");
                FlightLogger.Log("missionsCompletedAmount 1: " + missionsCompletedAmount);
                QuicksaveManager quickSaveInstance;
                quickSaveInstance = QuicksaveManager.instance;
                quickSaveInstance.Quicksave();
                TutorialLabel.instance.DisplayLabel("The game was Quick Saved by Auto Save Points. 1", null, 10);
                missionsCompletedAmount = 0;
            }

            FlightLogger.Log("missionsCompletedAmount After AutoSave: " + missionsCompletedAmount);
        }

        private void SceneLoaded(VTOLScenes scene)
        {
            CheckSaveFile();

            InvokeRepeating("AddMissions", 30, 30);

            missionsCompletedAmount = 0;

            missionsList.Clear();
        }
    }
}