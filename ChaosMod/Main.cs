using BepInEx;
using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using ChaosMod.UI;
using ChaosMod.Events;

namespace ChaosMod
{
    [BepInPlugin(pluginGuid, "ChaosMod", pluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string pluginGuid = "nachariah.whiteknuckle.chaosmod";
        public const string pluginVersion = "1.0.2";

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

            if (hardMode)
                timeLeft -= Time.deltaTime * 3;
            else
                timeLeft -= Time.deltaTime;

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
}
