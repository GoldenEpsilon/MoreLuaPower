using System;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;
using UnityEngine;

//The idea here is that we have an array of GenocideLenient stages that do not effect the visited world names count
//Since custom bosses by default dont increment the genocide counter things should work as expected
//Writing it this way also accounts for players who may have stopped their run in the middle and wanted to continue it later.

[HarmonyPatch(typeof(RunCtrl))]
[HarmonyPatch("GoToNextWorld")]
class MoreLuaPower_GenocideLeniency
{
    /*static bool Prefix(ref RunCtrl __instance, ref ZoneDot zoneDot) {
        Debug.Log((object)("GOING TO NEXT WORLD... " + zoneDot.worldName + " cur zone num : " + (object)__instance.currentRun.zoneNum + "  and the zone= " + (object)__instance.currentWorld.numZones + " BossExecs= " + (object)__instance.currentRun.bossExecutions + " unvis world nums:" + (object)__instance.currentRun.unvisitedWorldNames.Count));
        ++__instance.currentRun.worldTierNum;

        int count = 0;

        foreach(string worldName in __instance.currentRun.visitedWorldNames)
        {
            if (!LuaPowerData.GenocideLenientStages.Contains(worldName)) {
                count++;
            }
        }

        if (__instance.currentRun.shopkeeperKilled)
            ++count;
        if (__instance.currentRun.unvisitedWorldNames.Count > 0)
        {
            __instance.currentRun.visitedWorldNames.Add(zoneDot.worldName);
            __instance.currentRun.unvisitedWorldNames.Remove(zoneDot.worldName);
        }
        else if (S.I.BETA)
            zoneDot.worldName = "Beta";
        else if (__instance.currentRun.bossExecutions == 0)
        {
            zoneDot.worldName = "Pacifist";
            __instance.currentRun.pacifist = true;
        }
        else if (__instance.currentRun.bossExecutions < count)
        {
            zoneDot.worldName = "Normal";
            __instance.currentRun.neutral = true;
        }
        else if (__instance.currentRun.bossExecutions >= count)
        {
            __instance.currentRun.genocide = true;
            zoneDot.worldName = "Genocide";
        }
        __instance.progressBar.transform.SetParent(Camera.main.transform);
        __instance.currentRun.worldName = zoneDot.worldName;
        __instance.ResetWorld(zoneDot.worldName, -1);
        __instance.currentRun.zoneNum = 0;
        zoneDot = __instance.worldBar.currentZoneDots[0];

        return false;
    }*/
}