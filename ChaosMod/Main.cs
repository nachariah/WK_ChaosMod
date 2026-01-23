using BepInEx;
using HarmonyLib;
using UnityEngine;
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
        public const string pluginVersion = "1.1.0";

        void Awake()
        {
            Logger.LogInfo("[ChaosMod] Patching...");
            Harmony harmony = new Harmony(pluginGuid);
            harmony.PatchAll();
            EventManager.LoadBundle();
            EventManager.FillList();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Game-Main")
            {
                CommandConsole.hasCheated = true;
                CL_GameManager.gamemode.allowAchievements = false;
                CL_GameManager.gamemode.allowCheatedScores = false;
                Main.GameStart();
                if (EntityHolder.buddy == null)
                    EntityHolder.SetVariables();
            }
            else if (scene.name == "Main-Menu")
            {
                ChaosUI.CreateMainMenuText();
            }
        }
    }
    public static class Main
    {
        private static bool active = false;
        public static bool hardMode = false;
        private static float timeMax = 5f;
        private static float timeLeft;

        public static void MainUpdate()
        {
            if (!active) return;

            float deltaTime = Time.deltaTime;
            if (ChaosSettings.easyMode)
                deltaTime /= 2;

            if (hardMode)
                timeLeft -= deltaTime * 3;
            else
                timeLeft -= deltaTime;

            if (timeLeft < 0)
            {
                EventManager.RandomEvent();
                timeLeft = timeMax;
            }

            float fillValue = timeLeft / timeMax;

            ChaosUI.instance.SetTimer(fillValue);
        }
        private static void StartChaos()
        {
            hardMode = CL_GameManager.IsHardmode();
            timeLeft = timeMax;
            ChaosUI.ShowUI();
            active = true;
        }
        public static void GameStart()
        {
            if (active)
                active = false;
            if (SceneManager.GetActiveScene().name == "Game-Main")
                StartChaos();
        }
    }

    public static class ChaosSettings
    {
        public static bool easyMode;
        public static float loggerYOffset;

        public static Dictionary<string, bool> eventEnabled = new Dictionary<string, bool>();

        public static void Load()
        {
            easyMode = PlayerPrefs.GetInt("Chaos_EasyMode", 0) == 1;
            loggerYOffset = PlayerPrefs.GetFloat("Chaos_LoggerYOffset", 0f);

            foreach (var key in eventEnabled.Keys.ToList())
            {
                eventEnabled[key] = PlayerPrefs.GetInt("Chaos_Event_" + key, 1) == 1;
            }

            Main.hardMode = !easyMode;
        }

        public static void Save()
        {
            PlayerPrefs.SetInt("Chaos_EasyMode", easyMode ? 1 : 0);
            PlayerPrefs.SetFloat("Chaos_LoggerYOffset", loggerYOffset);

            foreach (var kv in eventEnabled)
            {
                PlayerPrefs.SetInt("Chaos_Event_" + kv.Key, kv.Value ? 1 : 0);
            }

            PlayerPrefs.Save();
        }
    }

}
