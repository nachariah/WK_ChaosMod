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
    public class Main : BaseUnityPlugin
    {
        public const string pluginGuid = "nachariah.whiteknuckle.chaosmod";
        public const string pluginVersion = "1.0.0";

        public static Main instance;

        private bool active = false;
        public bool hardMode = false;
        private float timeMax = 5f;
        private float timeLeft;

        void Awake()
        {
            Logger.LogInfo("[ChaosMod] Patching...");
            DontDestroyOnLoad(gameObject);
            instance = this;
            timeLeft = timeMax;
            Harmony harmony = new Harmony(pluginGuid);
            harmony.PatchAll();
            EventManager.LoadBundle();
            EventManager.FillList();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        void Update()
        {
            if (!active) return;

            if (hardMode)
                timeLeft -= Time.deltaTime * 3 / TimeManager.currentSpeed;
            else
                timeLeft -= Time.deltaTime / TimeManager.currentSpeed;

            if (timeLeft < 0)
            {
                EventManager.RandomEvent();
                timeLeft = timeMax;
            }

            float fillValue = timeLeft / timeMax;

            ChaosUI.instance.SetTimer(fillValue);
        }
        private void StartChaos()
        {
            hardMode = CL_GameManager.IsHardmode();
            timeLeft = timeMax;
            ChaosUI.ShowUI();
            active = true;
        }
        public void GameStart()
        {
            if (active)
                active = false;
            if (SceneManager.GetActiveScene().name == "Game-Main")
                instance.StartChaos();
        }
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Game-Main")
            {
                CommandConsole.hasCheated = true;
                CL_GameManager.gamemode.allowAchievements = false;
                CL_GameManager.gamemode.allowCheatedScores = false;
            }
        }
    }
}
