using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using ChaosMod.UI;
using ChaosMod.Events;
using System.Collections.Generic;
using System.Linq;

namespace ChaosMod
{
    [BepInPlugin(pluginGuid, "ChaosMod", pluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string pluginGuid = "nachariah.whiteknuckle.chaosmod";
        public const string pluginVersion = "1.2.0";

        public static Dictionary<int,float> difficultyTimers = new Dictionary<int,float>();

        void Awake()
        {
            Logger.LogInfo("[ChaosMod - Awake] Patching...");
            Harmony harmony = new Harmony(pluginGuid);
            harmony.PatchAll();
            EventManager.LoadBundle();
            EventManager.FillList();
            SceneManager.sceneLoaded += OnSceneLoaded;

            difficultyTimers.Add(0, 20f);
            difficultyTimers.Add(1, 10f);
            difficultyTimers.Add(2, 5f);
            difficultyTimers.Add(3, 2f);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Game-Main")
            {
                CommandConsole.hasCheated = true;
                CL_GameManager.gamemode.allowAchievements = false;
                CL_GameManager.gamemode.allowCheatedScores = false;
                Main.GameStart();
            }
            else if (scene.name == "Main-Menu")
            {
                ChaosUI.LoadMenuMenu();
            }
        }
    }
    public static class Main
    {
        private static bool active = false;
        public static bool hardMode = false;
        private static float timeMax = 10f;
        private static float timeLeft;

        public static int pauseThreshold = 0;

        private static float timeSinceStart = 0f;

        public static void MainUpdate()
        {
            if (!active || pauseThreshold > 0) return;

            float deltaTime = Time.deltaTime;

            if (timeSinceStart < 5f)
            {
                timeSinceStart += deltaTime;
                deltaTime *= timeSinceStart / 5f;
            }

            timeLeft -= deltaTime;

            if (hardMode && !ChaosSettings.customTimer)
                timeLeft -= deltaTime;

            if (timeLeft < 0)
            {
                EventManager.RandomEvent();
                timeLeft = timeMax;
            }

            ChaosUI.instance.SetTimer(timeLeft / timeMax);
        }
        private static void StartChaos()
        {
            EventManager.FillList();
            EventManager.eventsOnCooldown.Clear();
            hardMode = CL_GameManager.IsHardmode();

            if (ChaosSettings.customTimer)
                timeMax = ChaosSettings.customTimerValue;
            else
                if (Plugin.difficultyTimers.TryGetValue(ChaosSettings.difficulty,out float value))
                    timeMax = value;
                else
                    timeMax = 10f;

            timeLeft = timeMax;
            ChaosUI.ShowUI();
            pauseThreshold = 0;
            timeSinceStart = 0f;
            active = true;
        }
        public static void GameStart()
        {
            if (active)
                active = false;
            if (SceneManager.GetActiveScene().name == "Game-Main")
                StartChaos();
        }
        public static void AddPause(bool b = true)
        {
            if (b)
                pauseThreshold++;
            else
                pauseThreshold--;
            Debug.Log("[ChaosMod - AddPause] Add: "+b.ToString()+" | Pause Count: "+pauseThreshold);
            if (pauseThreshold != 0)
            {
                timeLeft = timeMax;
                timeSinceStart = 0;
                ChaosUI.instance.SetTimer(0);
                ChaosUI.instance.RemoveAllEntries();
            }
        }
    }

    public static class ChaosSettings
    {
        public static int difficulty = 1;
        public static bool customTimer = false;
        public static float customTimerValue = 10f;
        public static float loggerYOffset = 0f;
        public static Dictionary<string, bool> eventEnabled = new Dictionary<string, bool>();
        public static void Load()
        {
            difficulty = PlayerPrefs.GetInt("Chaos_Difficulty", 1);
            customTimer = PlayerPrefs.GetInt("Chaos_CustomTimer", 0) == 1;
            customTimerValue = PlayerPrefs.GetFloat("Chaos_CustomTimerValue", 10f);
            loggerYOffset = PlayerPrefs.GetFloat("Chaos_LoggerYOffset", 0f);
            foreach (var key in eventEnabled.Keys.ToList())
            {
                eventEnabled[key] = PlayerPrefs.GetInt("Chaos_Event_" + key, 1) == 1;
            }
        }

        public static void Save()
        {
            PlayerPrefs.SetInt("Chaos_Difficulty", difficulty);
            PlayerPrefs.SetInt("Chaos_CustomTimer", customTimer ? 1 : 0);
            PlayerPrefs.SetFloat("Chaos_CustomTimerValue", customTimerValue);
            PlayerPrefs.SetFloat("Chaos_LoggerYOffset", loggerYOffset);
            foreach (var kv in eventEnabled)
            {
                PlayerPrefs.SetInt("Chaos_Event_" + kv.Key, kv.Value ? 1 : 0);
            }

            PlayerPrefs.Save();
        }
    }
    public class ForceMonitor : MonoBehaviour
    {
        private bool active = false;
        public UT_PlayerForceMover RelatedMover;
        public void Initialize(UT_PlayerForceMover mover)
        {
            RelatedMover = mover;

            if (mover.active)
            {
                active = true;
                Main.AddPause(true);
            }
        }
        void OnEnable()
        {
            if (RelatedMover && RelatedMover.active)
            {
                active = true;
                Main.AddPause(true);
            }
        }
        void OnDisable()
        {
            if (RelatedMover && RelatedMover.active)
            {
                active = false;
                Main.AddPause(false);
            }
        }
        public void ChangeState(bool b)
        {
            if (active != b)
            {
                active = b;
                Main.AddPause(b);
            }
        }
    }
    public class TakeOverMonitor : MonoBehaviour
    {
        private bool active = false;
        public UT_PlayerTakeOver RelatedMover;
        public void Initialize(UT_PlayerTakeOver mover)
        {
            RelatedMover = mover;

            if (mover.active)
            {
                active = true;
                Main.AddPause(true);
            }
        }
        void OnEnable()
        {
            if (RelatedMover && RelatedMover.active)
            {
                active = true;
                Main.AddPause(true);
            }
        }
        void OnDisable()
        {
            if (RelatedMover && RelatedMover.active)
            {
                active = false;
                Main.AddPause(false);
            }
        }
        public void ChangeState(bool b)
        {
            if (active != b)
            {
                active = b;
                Main.AddPause(b);
            }
        }
    }
}
