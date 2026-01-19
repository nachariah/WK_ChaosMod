using ChaosMod.Events;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChaosMod.Patches
{
    [HarmonyPatch(typeof(CL_GameManager), "Start")]
    public static class CL_GameManager_Start_Patch
    {
        private static void Postfix()
        {
            Main.instance.GameStart();
            TimeManager.Reset();
            if (EntityHolder.buddy == null)
                EntityHolder.SetVariables();
        }
    }
    [HarmonyPatch(typeof(ENT_Player), "OnControllerColliderHit")]
    public static class ENT_Player_Hit_Patch
    {
        [HarmonyPostfix]
        private static void Postfix(ENT_Player __instance, ControllerColliderHit hit)
        {
            if (hit.gameObject.GetComponent<PirateAI>() != null)
            {
                CL_GameManager.DeathType pirateDeath = new CL_GameManager.DeathType();
                pirateDeath.deathText = "DEAD MEN TELL NO TALES";
                CL_GameManager.gMan.deathTypes[0] = pirateDeath;
                EventManager.PlayAudio((AudioClip)EventManager.prefabs["ShipCollide"], 0.85f);
                __instance.Kill();
            }
            else if (hit.gameObject.GetComponent<TrainAI>() != null)
            {
                CL_GameManager.DeathType spiceDeath = new CL_GameManager.DeathType();
                spiceDeath.deathText = "TOO MUCH OLD SPICE";
                CL_GameManager.gMan.deathTypes[0] = spiceDeath;
                EventManager.PlayAudio((AudioClip)EventManager.prefabs["TrainHit"], 0.85f, 0.9f);
                __instance.Kill();
            }
        }
    }
}
