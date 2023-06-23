using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Valve.Newtonsoft.Json;

/*
 * Expectation:
 * Have the game auto save after a user set amount of completed missions.
 * 
 * Reality:
 * This was a bitch, the first issue was figuring out how the game does autosaves, that took a few days by itself. After that it took figuring out how to do settings and create a persistant
 * file for settings, which then after that there was an issue where all the objectives in the mission I tested existed 3 times so the AutoSave() Method would be called three times in a row.
 * 
 * End Goal:
 * Allow for a settings file so the Auto Quick Save can happen after a user set amount of completed missions, more than likely limited from 1 to 5.
 * 
 * Future Plans:
 * 1. Create an Auto Save Menu with Save Selection
 * 
 * Changelog:
 * 06/22/20232 added time based saving and the option to disable the tutorial display.
*/

namespace AutoSavePoints
{
    public class AutoSavePointsSettings
    {
        public int savedUserMissionsAmount = 2;
        public int savedUserTimeBetweenSaves = 5;
        public bool savedUseMissionsCompletion = true;
        public bool savedUseTimePassed = true;
        public bool savedUserAlert = false;
    }

    public class AutoSavePoints : VTOLMOD
    {
        private int missionsCompletedAmount;
        private bool settingsChanged;
        private static AutoSavePointsSettings aspSettings;
        private UnityAction<int> savedUserMissionsAmount_changed;
        private UnityAction<int> savedUserTimeBetweenSaves_changed;
        private UnityAction<bool> savedUseMissionsCompletion_changed;
        private UnityAction<bool> savedUseTimePassed_changed;
        private UnityAction<bool> savedUserAlert_changed;
        List<MissionObjective> missionsList = new List<MissionObjective>();
        List<MissionObjective> missionsListUnique = new List<MissionObjective>();
        HashSet<MissionObjective> addedListeners = new HashSet<MissionObjective>();
        private static Settings modSettings;

        public override void ModLoaded()
        {
            VTOLAPI.SceneLoaded += SceneLoaded;

            aspSettings = new AutoSavePointsSettings();
            LoadSaveFile();

            savedUserMissionsAmount_changed += savedUserMissionsAmount_Settings;
            savedUseMissionsCompletion_changed += savedUseMissionsCompletion_Settings;
            savedUserTimeBetweenSaves_changed += savedUserTimeBetweenSaves_Settings;
            savedUseTimePassed_changed += savedUseTimePassed_Settings;
            savedUserAlert_changed += savedUserAlert_Settings;

            modSettings = new Settings(this);
            modSettings.CreateCustomLabel("Auto Save Points Settings: ");
            modSettings.CreateCustomLabel("");

            modSettings.CreateCustomLabel("This will determine how many missions to complete before an automatic ");
            modSettings.CreateCustomLabel("quicksave occurs");
            modSettings.CreateCustomLabel("");
            modSettings.CreateCustomLabel("Default = 2, Minimum = 1, Maximum = 5");
            modSettings.CreateIntSetting("Amount of Missions to Complete: ", savedUserMissionsAmount_changed, aspSettings.savedUserMissionsAmount, 1, 5);
            modSettings.CreateCustomLabel("");

            modSettings.CreateCustomLabel("This will determine if you want to use completed missions to auto save");
            modSettings.CreateCustomLabel("");
            modSettings.CreateCustomLabel("Default = True, True = Enabled : False = Disabled");
            modSettings.CreateBoolSetting("Use mission completion count to auto save: ", savedUseMissionsCompletion_changed, aspSettings.savedUseMissionsCompletion);
            modSettings.CreateCustomLabel("");

            modSettings.CreateCustomLabel("This will determine the time between saves, this does not affect mission auto");
            modSettings.CreateCustomLabel("saves");
            modSettings.CreateCustomLabel("");
            modSettings.CreateCustomLabel("Default = 5, Minimum = 1, Maximum = 9999");
            modSettings.CreateIntSetting("Amount of time between autosaves: ", savedUserTimeBetweenSaves_changed, aspSettings.savedUserTimeBetweenSaves, 1, 9999);
            modSettings.CreateCustomLabel("");

            modSettings.CreateCustomLabel("This will determine if you want to use time to auto save in minutes");
            modSettings.CreateCustomLabel("");
            modSettings.CreateCustomLabel("Default = True, True = Enabled : False = Disabled");
            modSettings.CreateBoolSetting("Use time to auto save: ", savedUseTimePassed_changed, aspSettings.savedUseTimePassed);
            modSettings.CreateCustomLabel("");

            modSettings.CreateCustomLabel("This will determine if you want it to alert you when the game is auto saved");
            modSettings.CreateCustomLabel("");
            modSettings.CreateCustomLabel("Default = False, True = Enabled : False = Disabled");
            modSettings.CreateBoolSetting("Alert when the game auto saves: ", savedUserAlert_changed, aspSettings.savedUserAlert);

            VTOLAPI.CreateSettingsMenu(modSettings);

            base.ModLoaded();
        }

        private void savedUserMissionsAmount_Settings(int savedUserMissionsAmount)
        {
            aspSettings.savedUserMissionsAmount = savedUserMissionsAmount;
            settingsChanged = true;
        }

        private void savedUseMissionsCompletion_Settings(bool savedUseMissionsCompletion)
        {
            aspSettings.savedUseMissionsCompletion = savedUseMissionsCompletion;
            settingsChanged = true;
        }

        private void savedUserTimeBetweenSaves_Settings(int savedUserTimeBetweenSaves)
        {
            aspSettings.savedUserTimeBetweenSaves = savedUserTimeBetweenSaves;
            settingsChanged = true;
        }

        private void savedUseTimePassed_Settings(bool savedUseTimePassed)
        {
            aspSettings.savedUseTimePassed = savedUseTimePassed;
            settingsChanged = true;
        }

        private void savedUserAlert_Settings(bool savedUserAlert)
        {
            aspSettings.savedUserAlert = savedUserAlert;
            settingsChanged = true;
        }

        private void CheckSaveFile()
        {
            if (settingsChanged)
            {
                SaveFile();
            }
            else
            {
                FlightLogger.Log("AutoSavePoints Logs: " + "Not saving file.");
            }
        }

        private void SaveFile()
        {
            string path = ModFolder;

            if (Directory.Exists(path))
            {
                File.WriteAllText(path + @"\aspsettings.json", JsonConvert.SerializeObject(aspSettings));
                settingsChanged = false;
            }
            else
            {
                FlightLogger.Log("AutoSavePoints Logs: " + "No save file found.");
            }
        }

        private void LoadSaveFile()
        {
            string path = ModFolder;

            if (Directory.Exists(path))
            {
                try
                {
                    string temporary = File.ReadAllText(path + @"\aspsettings.json");

                    aspSettings = JsonConvert.DeserializeObject<AutoSavePointsSettings>(temporary);
                    settingsChanged = false;
                }
                catch
                {
                    SaveFile();
                }
            }
            else
            {
                FlightLogger.Log("AutoSavePoints Logs: " + "Mod folder not found.");
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
                    objective.OnComplete.AddListener(() => MissionAutoSave());
                    addedListeners.Add(objective);
                }
            }

            missionsListUnique.Clear();
            missionsList.Clear();
        }

        private void MissionAutoSave()
        {
            missionsCompletedAmount++;

            if (missionsCompletedAmount == 0)
            {
                FlightLogger.Log("AutoSavePoints Logs: " + "LMAOXD you can't divide by 0 dawg.");
            }
            else if (missionsCompletedAmount % aspSettings.savedUserMissionsAmount == 0)
            {
                QuicksaveManager quickSaveInstance;
                quickSaveInstance = QuicksaveManager.instance;
                quickSaveInstance.Quicksave();
                if (aspSettings.savedUserAlert == true)
                {
                    TutorialLabel.instance.DisplayLabel("AutoSavePoints: " + "A Quick Save has been made by MissionAutoSave", null, 7);
                }
                missionsCompletedAmount = 0;
            }
            //else if (aspSettings.savedUserMissionsAmount == 4 && missionsCompletedAmount % 4 == 0)
            //{
            //    QuicksaveManager quickSaveInstance;
            //    quickSaveInstance = QuicksaveManager.instance;
            //    quickSaveInstance.Quicksave();
            //    missionsCompletedAmount = 0;
            //}
            //else if (aspSettings.savedUserMissionsAmount == 3 && missionsCompletedAmount % 3 == 0)
            //{
            //    QuicksaveManager quickSaveInstance;
            //    quickSaveInstance = QuicksaveManager.instance;
            //    quickSaveInstance.Quicksave();
            //    missionsCompletedAmount = 0;
            //}
            //else if (aspSettings.savedUserMissionsAmount == 2 && missionsCompletedAmount % 2 == 0)
            //{
            //    QuicksaveManager quickSaveInstance;
            //    quickSaveInstance = QuicksaveManager.instance;
            //    quickSaveInstance.Quicksave();
            //    missionsCompletedAmount = 0;
            //}
            //else if (aspSettings.savedUserMissionsAmount == 1 && missionsCompletedAmount % 1 == 0)
            //{
            //    QuicksaveManager quickSaveInstance;
            //    quickSaveInstance = QuicksaveManager.instance;
            //    quickSaveInstance.Quicksave();
            //    missionsCompletedAmount = 0;
            //}
        }

        private void TimeAutoSave()
        {
            QuicksaveManager quickSaveInstance;
            quickSaveInstance = QuicksaveManager.instance;
            quickSaveInstance.Quicksave();
            DateTime currentTime = DateTime.Now;
            if (aspSettings.savedUserAlert == true)
            {
                TutorialLabel.instance.DisplayLabel("AutoSavePoints: " + "A Quick Save has been made by TimeAutoSave", null, 7);
            }
        }

        private void SceneLoaded(VTOLScenes scene)
        {
            CheckSaveFile();

            CancelInvoke("AddMissions");

            CancelInvoke("TimeAutoSave");

            InvokeRepeating("AddMissions", 30, 30);

            InvokeRepeating("TimeAutoSave", aspSettings.savedUserTimeBetweenSaves * 60, aspSettings.savedUserTimeBetweenSaves * 60);

            missionsCompletedAmount = 0;

            missionsList.Clear();
        }
    }
}