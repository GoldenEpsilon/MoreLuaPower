using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml;
using System.Linq;
using UnityEngine;

using MoonSharp.Interpreter;
using System;
using UnityEngine.UI;
using I2.Loc;

[HarmonyPatch(typeof(XMLReader), nameof(XMLReader.XMLtoZoneData))]
public static class ZoneXMLPatch
{
    //Prevents custom zonetypes from being filled in automatically.
    public static bool Prefix(XMLReader __instance, World worldn, ref int stageNum, ZoneType zoneType)
    {
        S.I.spCtrl.enemyToSpawn.Clear();
        S.I.spCtrl.bossesToSpawn.Clear();
        S.I.spCtrl.enemySpawnX.Clear();
        S.I.spCtrl.enemySpawnY.Clear();
        S.I.spCtrl.enemyChance.Clear();
        return CustomZoneUtil.defaults.Contains(zoneType);
    }

}


[HarmonyPatch]
public static class CustomZoneMiscPatches
{
    //Triggers zone initialization event
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SpawnCtrl), nameof(SpawnCtrl.SpawnZoneC))]
    public static bool SpawnCustomZone(SpawnCtrl __instance, ZoneType zoneType)
    {
        CustomZoneUtil.TriggerTypeZoneEvent(zoneType, "Init");
        return true;
    }

    //Used for detecting when a boss is defeated
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Boss), nameof(Boss.DownC))]
    public static bool DownBossZoneEvent(Boss __instance)
    {
        if (S.I.batCtrl.perfectBattle)
        {
            CustomZoneUtil.TriggerTypeZoneEvent(__instance.runCtrl.currentZoneDot.type, "PerfectBoss");
        }
        CustomZoneUtil.TriggerTypeZoneEvent(__instance.runCtrl.currentZoneDot.type, "Downed");
        return true;
    }

    //Used for detecting when a boss is spared
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Boss), nameof(Boss.Spare))]
    public static bool SpareBossZoneEvent(Boss __instance)
    {
        CustomZoneUtil.TriggerTypeZoneEvent(__instance.runCtrl.currentZoneDot.type, "Spare");
        return true;
    }

    //Used for detecting the end of a battle (aswell as boss executions)
    [HarmonyPrefix]
    [HarmonyPatch(typeof(BC), nameof(BC.EndBattle))]
    public static bool EndBattlePrefix(BC __instance)
    {
        if (__instance.perfectBattle)
        {
            CustomZoneUtil.TriggerTypeZoneEvent(__instance.runCtrl.currentZoneDot.type, "PerfectBattle");
        }
        CustomZoneUtil.TriggerTypeZoneEvent(__instance.runCtrl.currentZoneDot.type, "EndBattle");
        return true;
    }

    //Ensures that world dots earlier on the map work.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(RunCtrl), nameof(RunCtrl.GoToNextZone))]
    static bool Prefix(RunCtrl __instance, ZoneDot zoneDot)
    {
        if (zoneDot.type == ZoneType.World)
        {
            if (CustomWorldGenerator.refreshWorldDots.Contains(zoneDot))
            {
                __instance.currentRun.unvisitedWorldNames.Add(__instance.currentWorld.nameString);
            }
            __instance.currentRun.zoneNum = 10000;
        }
        return true;
    }

}

//Ensures usage of Custom Campaigns and Custom World Generation
[HarmonyPatch]
public static class WorldBarPatches
{
    //Run custom world generation.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(WorldBar), nameof(WorldBar.GenerateWorldBar))]
    public static bool WorldGenerationPrefix(WorldBar __instance, ref int numSteps)
    {
        if (numSteps == -666)
        {
            numSteps = __instance.runCtrl.currentWorld.numZones;
            return true;
        }
        World world = __instance.runCtrl.currentWorld;
        if ((world.nameString == "Genocide" || world.nameString == "Pacfifist" || world.nameString == "Normal") && __instance.runCtrl != null && __instance.runCtrl.currentRun != null)
        {
            __instance.runCtrl.currentRun.unvisitedWorldNames.Clear();
        }
        var gen = new CustomWorldGenerator(__instance);
        gen.Generate();
        return false;
    }

    //Slight safety.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(WorldBar), nameof(WorldBar.Open))]
    public static bool OpenPrefix(WorldBar __instance)
    {
        return __instance.runCtrl.currentZoneDot != null;
    }

    //Custom Campaign Start code aswell as run start zone event.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(RunCtrl), nameof(RunCtrl.CreateNewRun))]
    public static bool CreateRunPrefix(RunCtrl __instance, int zoneNum, int worldTierNum, bool campaign, string seed = "")
    {
        CustomZoneUtil.TriggerZoneEvent("Start");
        var heroID = __instance.ctrl.currentHeroObj.beingID;
        CustomZoneUtil.currentCampaign = CustomZoneUtil.customCampaignCharacters.ContainsKey(heroID) ? CustomZoneUtil.customCampaignCharacters[heroID] : "Default";
        if (CustomZoneUtil.customCampaignCharacters.ContainsKey(heroID) && CustomZoneUtil.currentCampaign != "Default")
        {
            __instance.currentRun = new Run("Run");
            __instance.currentRun.beingID = heroID;
            __instance.currentRun.animName = __instance.ctrl.currentHeroObj.animName;
            if (!string.IsNullOrEmpty(seed))
            {
                __instance.currentRun.seed = seed;
                __instance.currentRun.seedWasPredefined = true;
            }
            else if (__instance.useRandomSeed)
                __instance.currentRun.seed = Mathf.Abs(Environment.TickCount).ToString();
            else if (__instance.testSeed != null)
                __instance.currentRun.seed = __instance.testSeed;
            __instance.pseudoRandom = new System.Random(__instance.currentRun.seed.GetHashCode());
            __instance.pseudoRandomWorld = new System.Random(__instance.currentRun.seed.GetHashCode());
            __instance.worldBar.seedText.text = ScriptLocalization.UI.Worldbar_Seed + " " + __instance.currentRun.seed;
            CustomZoneUtil.GenerateCampaignWorlds();
            __instance.currentRun.zoneNum = zoneNum;
            __instance.currentRun.worldTierNum = worldTierNum;
            __instance.currentRun.hellPassNum = __instance.currentHellPassNum;
            __instance.currentRun.hellPasses = new List<int>((IEnumerable<int>)__instance.currentHellPasses);
            __instance.idCtrl.heroNameText.text = __instance.ctrl.currentHeroObj.localizedName;
            __instance.idCtrl.heroLevelText.text = string.Format(ScriptLocalization.UI.TopNav_LevelShort + " {0}", (object)1);
            if (__instance.heCtrl.gameMode == GameMode.CoOp)
            {
                __instance.currentRun.coOp = true;
            }

            __instance.ctrl.deCtrl.deckScreen.ResetValues();
            return false;
        }
        return true;
    }

    //Loop event and ensure custom campaigns.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(RunCtrl), nameof(RunCtrl.LoopRun))]
    public static bool LoopRunPrefix(RunCtrl __instance)
    {
        CustomZoneUtil.TriggerZoneEvent("Loop");
        if (__instance.currentRun != null) __instance.currentRun.unvisitedWorldNames.Clear();
        if (CustomZoneUtil.currentCampaign != "Default")
        {
            __instance.currentRun.visitedWorldNames.Clear();
            ++__instance.currentRun.loopNum;
            CustomZoneUtil.GenerateCampaignWorlds();
            __instance.currentRun.Loop();
            if (__instance.ctrl.currentPlayer.beingObj.tags.Contains(Tag.Shopkeeper))
                __instance.currentRun.yamiObtained = true;
            __instance.ResetWorld(__instance.currentWorld.nameString);
            __instance.StartZone(__instance.currentRun.zoneNum, __instance.currentZoneDot, true);
            if (__instance.currentRun.loopNum > SaveDataCtrl.Get<int>("MostLoops"))
                SaveDataCtrl.Set<int>("MostLoops", __instance.currentRun.loopNum);
            if (__instance.currentRun.loopNum > SaveDataCtrl.Get<int>(__instance.ctrl.currentHeroObj.nameString + "MostLoops"))
                SaveDataCtrl.Set<int>(__instance.ctrl.currentHeroObj.nameString + "MostLoops", __instance.currentRun.loopNum);
            return false;
        }
        return true;
    }

}

public static class CustomZoneUtil
{
    //Default zone types
    public static ZoneType[] defaults = { ZoneType.Battle, ZoneType.Boss, ZoneType.Campsite, ZoneType.Danger, ZoneType.DarkShop, ZoneType.Distress, ZoneType.Genocide, ZoneType.Idle, ZoneType.Miniboss, ZoneType.Normal, ZoneType.Pacifist, ZoneType.PvP, ZoneType.Random, ZoneType.Shop, ZoneType.Treasure, ZoneType.World };

    //Lua scripts handling zone events.
    public static List<object> ZoneEventScripts = new List<object>();
    public static List<Script> ZoneEventBaseScripts = new List<Script>();

    //Custom campaign data.
    public static Dictionary<string, List<string>> customCampaignWorlds = new Dictionary<string, List<string>>();
    public static Dictionary<string, bool> customCampaignStatic = new Dictionary<string, bool>();
    public static Dictionary<string, string> customCampaignCharacters = new Dictionary<string, string>();

    public static string currentCampaign = "Default";


    static public void Setup()
    {

    }

    //Generates the initial list of custom worlds.
    public static void GenerateCampaignWorlds()
    {
        var runCtrl = S.I.runCtrl;
        runCtrl.xmlReader.XMLtoGetWorlds(runCtrl.xmlReader.GetDataFile("Zones.xml"));
        if (customCampaignStatic.ContainsKey(currentCampaign) && customCampaignStatic[currentCampaign])
        {
            GenerateStaticCampaignWorlds();
        }
        else
        {
            GenerateRandomCampaignWorlds();
        }
        runCtrl.progressBar.Set();
        runCtrl.currentRun.worldName = runCtrl.currentRun.unvisitedWorldNames[0];
        runCtrl.currentRun.visitedWorldNames.Add(runCtrl.currentRun.unvisitedWorldNames[0]);
        runCtrl.currentRun.unvisitedWorldNames.RemoveAt(0);
        runCtrl.currentWorld = runCtrl.worlds[runCtrl.currentRun.worldName];
    }

    //Generates a random sequence of custom worlds.
    public static void GenerateRandomCampaignWorlds()
    {
        var runCtrl = S.I.runCtrl;
        List<string> stringList = new List<string>((IEnumerable<string>)customCampaignWorlds[currentCampaign]);
        foreach (string key in new List<string>((IEnumerable<string>)stringList))
        {
            if (Utils.SharesTags(runCtrl.ctrl.currentHeroObj.tags, runCtrl.worlds[key].tags))
                stringList.Remove(runCtrl.worlds[key].nameString);
        }
        for (int count = stringList.Count; count > 0; --count)
        {
            int index = runCtrl.NextPsuedoRand(0, stringList.Count);
            runCtrl.currentRun.unvisitedWorldNames.Add(stringList[index]);
            stringList.Remove(stringList[index]);
        }

    }

    //Generates a static sqeuence of custom worlds.
    public static void GenerateStaticCampaignWorlds()
    {
        var runCtrl = S.I.runCtrl;
        List<string> stringList = new List<string>((IEnumerable<string>)customCampaignWorlds[currentCampaign]);
        foreach (var world in new List<string>(stringList))
        {
            runCtrl.currentRun.unvisitedWorldNames.Add(world);
            stringList.Remove(world);
        }
    }

    //Lua function to add a world to a custom campaign
    public static void AddWorldToCustomCampaign(string campaign, string world, bool staticMode = false)
    {
        if (!customCampaignWorlds.ContainsKey(campaign))
        {
            customCampaignWorlds.Add(campaign, new List<string>());
        }
        customCampaignWorlds[campaign].Add(world);
        if (customCampaignStatic.ContainsKey(campaign))
        {
            customCampaignStatic[campaign] = staticMode;
        }
        else
        {
            customCampaignStatic.Add(campaign, staticMode);
        }
    }

    //Lua function to default a character to a custom campaign.
    public static void AddCharacterToCustomCampaign(string campaign, string beingID)
    {
        if (customCampaignCharacters.ContainsKey(beingID))
        {
            customCampaignCharacters[beingID] = campaign;
        }
        else
        {
            customCampaignCharacters.Add(beingID, campaign);
        }
    }

    //Lua function to add manual generation to a world.
    public static void AddWorldToManualGeneration(string world)
    {
        if (!CustomWorldGenerator.manualGeneration.ContainsKey(world))
        {
            CustomWorldGenerator.manualGeneration.Add(world, true);
        } 
        else
        {
            CustomWorldGenerator.manualGeneration[world] = true;
        }
    }

    //Adds a new zonetype.
    public static void AddZoneType(string name, string sprite)
    {
        Debug.Log("Attempting to add zonetype with name: " + name);
        if (LuaPowerData.customEnums[typeof(ZoneType)].Contains(name))
        {
            return;
        }
        LuaPowerData.customEnums[typeof(ZoneType)].Add(name);
        S.I.runCtrl.worldBar.zoneSprites.Add(name, LuaPowerSprites.GetSprite(sprite));
        Debug.Log("ZoneType added with name: " + GetZoneType(name).ToString());
    }

    //Gets the zonetype enum from a string.
    public static ZoneType GetZoneType(string name)
    {
        return (ZoneType)LuaPowerData.customEnums[typeof(ZoneType)].IndexOf(name);
    }

    //Triggers an event twice, once with zonetype information as a prefix.
    public static void TriggerTypeZoneEvent(ZoneType type, string eventName)
    {
        TriggerZoneEvent(eventName);
        TriggerZoneEvent(type.ToString() + ":" + eventName);
    }


    //Lua event for zones.
    public static void TriggerZoneEvent(string eventName)
    {
        for (int i = 0; i < ZoneEventScripts.Count; i++)
        {
            ZoneEventBaseScripts[i].Globals["world"] = S.I.runCtrl.currentWorld;
            ZoneEventBaseScripts[i].Globals["eventName"] = eventName;
            ZoneEventBaseScripts[i].Globals["ctrl"] = S.I.batCtrl;
            ZoneEventBaseScripts[i].Globals["spawnCtrl"] = S.I.spCtrl;
            S.I.mainCtrl.StartCoroutine(MoreLuaPower_FunctionHelper.EffectRoutine(ZoneEventBaseScripts[i].CreateCoroutine(ZoneEventScripts[i])));
            ZoneEventBaseScripts[i].Globals.Remove("eventName");
            ZoneEventBaseScripts[i].Globals.Remove("ctrl");
            ZoneEventBaseScripts[i].Globals.Remove("spawnCtrl");
            ZoneEventBaseScripts[i].Globals.Remove("world");
        }
        CustomWorldGenerator.MakeZoneSectionVisible(eventName);
    }

}
