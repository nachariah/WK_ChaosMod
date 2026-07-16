using ChaosMod.Events;
using ChaosMod.UI;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

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
    [HarmonyPatch(typeof(UT_PlayerForceMover), "Start")]
    public static class UT_PlayerForceMover_Start_Patch
    {
        [HarmonyPostfix]
        private static void Postfix(UT_PlayerForceMover __instance)
        {
            ForceMonitor mon = __instance.gameObject.AddComponent<ForceMonitor>();
            mon.Initialize(__instance);
        }
    }
    [HarmonyPatch(typeof(UT_PlayerForceMover), "SetActive")]
    public static class UT_PlayerForceMover_SetActive_Patch
    {
        [HarmonyPostfix]
        private static void Postfix(UT_PlayerForceMover __instance,bool b)
        {
            ForceMonitor monitor = __instance.GetComponent<ForceMonitor>();
            if (monitor)
            {
                monitor.ChangeState(b);
            }
        }
    }
    [HarmonyPatch(typeof(UT_PlayerTakeOver), "Start")]
    public static class UT_PlayerTakeOver_Start_Patch
    {
        [HarmonyPostfix]
        private static void Postfix(UT_PlayerTakeOver __instance)
        {
            TakeOverMonitor mon = __instance.gameObject.AddComponent<TakeOverMonitor>();
            mon.Initialize(__instance);
        }
    }
    [HarmonyPatch(typeof(UT_PlayerTakeOver), "SetActive")]
    public static class UT_PlayerTakeOver_SetActive_Patch
    {
        [HarmonyPostfix]
        private static void Postfix(UT_PlayerTakeOver __instance, bool b)
        {
            TakeOverMonitor monitor = __instance.GetComponent<TakeOverMonitor>();
            if (monitor)
            {
                monitor.ChangeState(b);
            }
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
                EventManager.PlayAudio((AudioClip)EventManager.prefabs["ShipCollide"], 0.5f, 1f, AudioUtils.GetEffectsMixer());
                Damageable.DamageInfo info = Damageable.DamageInfo.CreateDamageInfo(1f, "Ghost Ship", new List<string>(), null);
                __instance.Kill(info.type, info);
            }
            else if (hit.gameObject.GetComponent<TrainAI>() != null)
            {
                CL_GameManager.DeathType spiceDeath = new CL_GameManager.DeathType();
                spiceDeath.deathText = "TOO MUCH OLD SPICE";
                CL_GameManager.gMan.deathTypes[0] = spiceDeath;
                EventManager.PlayAudio((AudioClip)EventManager.prefabs["TrainHit"], 0.4f, 0.5f, AudioUtils.GetEffectsMixer());
                Damageable.DamageInfo info = Damageable.DamageInfo.CreateDamageInfo(1f, "Terry Crews", new List<string>(), null);
                __instance.Kill(info.type, info);
            }
            else if (hit.gameObject.GetComponent<ShrekAI>() != null)
            {
                __instance.AddForce((__instance.transform.position - hit.transform.position).normalized * 20);
                if (__instance.health - 1f <= 0f)
                {
                    CL_GameManager.DeathType shrekDeath = new CL_GameManager.DeathType();
                    shrekDeath.deathText = "SHREKT";
                    CL_GameManager.gMan.deathTypes[0] = shrekDeath;
                    EventManager.PlayAudio((AudioClip)EventManager.prefabs["ShipCollide"], 0.45f, 0.9f, AudioUtils.GetEffectsMixer());
                    Damageable.DamageInfo info = Damageable.DamageInfo.CreateDamageInfo(1f, "Shrek", new List<string>(), null);
                    __instance.Kill(info.type,info);
                } else
                    __instance.Damage(Damageable.DamageInfo.CreateDamageInfo(1f, "Shrek", new List<string>(), null));
            }
        }
    }
    [HarmonyPatch(typeof(CL_GameManager), "Win")]
    public static class CL_GameManager_Win_Patch
    {
        [HarmonyPrefix]
        private static void Prefix(CL_GameManager __instance)
        {
            ChaosUI.SetEndScreens();
        }
    }
    [HarmonyPatch(typeof(CL_GameManager), "Die")]
    public static class CL_GameManager_Die_Patch
    {
        [HarmonyPostfix]
        private static void Postfix(CL_GameManager __instance)
        {
            ChaosUI.SetEndScreens();
        }
    }
}
