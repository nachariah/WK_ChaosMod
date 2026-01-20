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
    [HarmonyPatch(typeof(ENT_Player), "Update")]
    public static class ENT_Player_Update_Patch
    {
        [HarmonyPostfix]
        private static void Postfix(ENT_Player __instance)
        {
            Main.MainUpdate();
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
                EventManager.PlayAudio((AudioClip)EventManager.prefabs["TrainHit"], 0.75f, 0.9f);
                __instance.Kill();
            }
        }
    }
}
