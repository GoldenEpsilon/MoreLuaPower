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

public struct CustomZoneTypeData
{
    public bool spawn_enemies;
    public bool spawn_boss;
    public bool spawn_environment;
    public string music;
    public bool can_continue;
}

[HarmonyPatch(typeof(XMLReader), nameof(XMLReader.XMLtoZoneData))]
public static class ZoneXMLPatch
{

    public static bool Prefix(XMLReader __instance, World worldn, ref int stageNum, ZoneType zoneType)
    {
        S.I.spCtrl.enemyToSpawn.Clear();
        S.I.spCtrl.bossesToSpawn.Clear();
        S.I.spCtrl.enemySpawnX.Clear();
        S.I.spCtrl.enemySpawnY.Clear();
        S.I.spCtrl.enemyChance.Clear();
        return CustomZoneUtil.defaults.Contains(zoneType);
    }

    /*public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        Debug.Log("Zone Transpiler running!");
        Queue<CodeInstruction> state = new Queue<CodeInstruction>();
        foreach (var instruction in instructions)
        {

            if (state.Count == 0 && instruction.opcode == OpCodes.Ldarg_3)
            {
                state.Enqueue(instruction);
            }
            if (state.Count == 1 && instruction.opcode == OpCodes.Ldc_I4_1)
            {
                state.Enqueue(instruction);
                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CustomZoneUtil), nameof(CustomZoneUtil.IrregularSpawn)));
                continue;
            }
            if (state.Count == 2 && instruction.opcode == OpCodes.Beq_S)
            {
                state.Enqueue(instruction);
                instruction.opcode = OpCodes.Brtrue;
            }

            yield return instruction;

            


        }
    }*/
}

/*[HarmonyPatch(typeof(XMLReader), nameof(XMLReader.XMLtoGetWorlds))]
public static class WorldXMLPatch
{

    public static void HandleWorld(World world, XmlAttribute attribute)
    {
        switch (attribute.Name)
        {
            case "manualGeneration":
                {
                    bool b = false;
                    bool.TryParse(attribute.Value, out b);
                    CustomWorldGenerator.manualGeneration.Add(world.nameString, b);
                    break;
                }
        }
    }

    public static bool Prefix(XMLReader __instance, string data)
    {
        return true;
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        Debug.Log("World Transpiler running!");
        int sequence1 = 0;
        int sequence2 = 0;
        int sequence3 = 0;
        bool done = false;
        var desired_sequence1 = new CodeInstruction[] {
            new CodeInstruction(OpCodes.Ldc_I4_1, null) ,
            new CodeInstruction(OpCodes.Add, null) ,
            null,
            new CodeInstruction(OpCodes.Newobj, null),
            new CodeInstruction(OpCodes.Stloc_S, null)
        };

        var desired_sequence2 = new CodeInstruction[] {
            new CodeInstruction(OpCodes.Ldloc_S, null) ,
            null,
            new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(World),nameof(World.id)))
        };

        var desired_sequence3 = new CodeInstruction[] {
            new CodeInstruction(OpCodes.Br, null),
            new CodeInstruction(OpCodes.Ldloc_S, null) ,
            new CodeInstruction(OpCodes.Callvirt, (object) AccessTools.PropertyGetter(typeof(IEnumerator), "Current")) ,
            new CodeInstruction(OpCodes.Castclass, (object) typeof(XmlAttribute)) ,
            new CodeInstruction(OpCodes.Stloc_S, null)
        };

        LocalBuilder world_builder = null;
        LocalBuilder attribute_builder = null;
        foreach (var instruction in instructions)
        {

            if (sequence2 == desired_sequence2.Length)
            {
                if (world_builder != null && attribute_builder != null)
                {
                    var additions = new CodeInstruction[]
                {
                        new CodeInstruction(OpCodes.Ldloc_S, world_builder),
                        new CodeInstruction(OpCodes.Ldloc_S, attribute_builder),
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(WorldXMLPatch), nameof(WorldXMLPatch.HandleWorld)))
                };

                    foreach (var addtion in additions)
                    {
                        yield return addtion;
                    }
                    done = true;
                    Debug.Log("WORLD TRANSPILER SUCCES!");
                }


                sequence2 = 0;

            }


            yield return instruction;

            if (!done)
            {
                var current_comp1 = desired_sequence1[sequence1];
                if (current_comp1 == null || (instruction.opcode == current_comp1.opcode && CustomZoneUtil.CompareOperand(instruction, current_comp1)))
                {
                    sequence1++;
                }
                else
                {
                    if (sequence1 != 0) Debug.Log("Sequence 1 Reset at " + sequence1);
                    sequence1 = 0;
                }
                var current_comp2 = desired_sequence2[sequence2];
                if (current_comp2 == null || (world_builder != null && attribute_builder != null && instruction.opcode == current_comp2.opcode && CustomZoneUtil.CompareOperand(instruction, current_comp2)))
                {
                    sequence2++;
                }
                else
                {
                    if (sequence2 != 0) Debug.Log("Sequence 2 Reset at " + sequence2);
                    sequence2 = 0;
                }
                var current_comp3 = desired_sequence3[sequence3];
                if (current_comp3 == null || (world_builder != null && instruction.opcode == current_comp3.opcode && CustomZoneUtil.CompareOperand(instruction, current_comp3)))
                {

                    sequence3++;
                }
                else
                {
                    if (sequence3 != 0) Debug.Log("Sequence 3 Reset at " + sequence3);
                    sequence3 = 0;

                }
            }

            if (sequence1 == desired_sequence1.Length)
            {
                world_builder = (LocalBuilder)instruction.operand;
                desired_sequence2[0].operand = world_builder;
                if (world_builder != null) Debug.Log("Found world builder!");
                else Debug.LogError("Null world builder!");
                sequence1 = 0;
            }

            if (sequence3 == desired_sequence3.Length)
            {
                attribute_builder = (LocalBuilder)instruction.operand;
                if (attribute_builder != null) Debug.Log("Found attribute builder!");
                else Debug.LogError("Null attribute builder!");
                sequence3 = 0;
            }
        }
    }

    public static void Postfix(XMLReader __instance, string data)
    {
        var worlds = S.I.runCtrl.worlds;
    }
}*/

[HarmonyPatch]
public static class CustomZoneMiscPatches
{

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SpawnCtrl), nameof(SpawnCtrl.SpawnZoneC))]
    public static bool SpawnCustomZone(SpawnCtrl __instance, ZoneType zoneType)
    {
        if (!CustomZoneUtil.defaults.Contains(zoneType))
        {
            if (custom_zones.ContainsKey(zoneType))
            {
                CustomZoneTypeData zone = custom_zones[zoneType];
                __instance.ti.mainBattleGrid.currentEnemies.Clear();
                if (zone.can_continue)
                {
                    __instance.idCtrl.MakeWorldBarAvailable();
                }
                if (zone.music != "")
                {
                    LuaPowerSound.PlayCustomMusic(zone.music);
                }
                else
                {
                    S.I.muCtrl.PlayBattle();
                }
                if (zone.spawn_enemies)
                {
                    __instance.SpawnBattleZone();
                }
                if (zone.spawn_environment)
                {
                    __instance.SpawnEnvironment();
                }
                if (zone.spawn_boss)
                {
                    if (__instance.bossesToSpawn.Count > 0) __instance.SpawnBoss();
                }
            }
        }
        CustomZoneUtil.TriggerTypeZoneEvent(zoneType, "Init");
        return true;
    }

    public static Dictionary<ZoneType, CustomZoneTypeData> custom_zones = new Dictionary<ZoneType, CustomZoneTypeData>();

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

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Boss), nameof(Boss.Spare))]
    public static bool SpareBossZoneEvent(Boss __instance)
    {
        CustomZoneUtil.TriggerTypeZoneEvent(__instance.runCtrl.currentZoneDot.type, "Spare");
        return true;
    }

    public static bool updated = false;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ZoneDot), "Update")]
    public static bool ZoneDotUpdate(ZoneDot __instance)
    {
        if (updated || __instance == null || __instance.worldBar == null || __instance.worldBar.zoneDotContainer == null) return false;
        var bar = __instance.worldBar;
        var edit = new Vector3(0.0f, bar.runCtrl.currentZoneDot.transform.position.y / 2, 0.0f);
        bar.zoneDotContainer.localPosition = bar.zoneDotContainer.localPosition - edit;
        updated = true;
        return false;
        
    }


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

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Follower), "Update")]
    static bool FollowerUpdatePrefix(Follower __instance)
    {
        if (S.I == null) return true;
        if (S.I.runCtrl == null) return true;
        var bar = S.I.runCtrl.worldBar;
        if (bar != null && bar.runCtrl.currentZoneDot != null && (__instance == bar.locationMarker || __instance == bar.selectionMarker))
        {
            var edit = new Vector3(0.0f, bar.runCtrl.currentZoneDot.transform.localPosition.y, 0.0f);
            __instance.transform.localPosition = __instance.transform.localPosition + edit;
        }
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Follower), "Update")]
    static void FollowerUpdatePostfix(Follower __instance)
    {
        if (S.I == null) return;
        if (S.I.runCtrl == null) return;
        var bar = S.I.runCtrl.worldBar;
        if (bar != null && bar.runCtrl.currentZoneDot != null && (__instance == bar.locationMarker || __instance == bar.selectionMarker))
        {
            var edit = new Vector3(0.0f, bar.runCtrl.currentZoneDot.transform.localPosition.y, 0.0f);
            __instance.transform.localPosition = __instance.transform.localPosition - edit;
        }
       
    }

}


[HarmonyPatch]
public static class WorldBarPatches
{

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

    [HarmonyPrefix]
    [HarmonyPatch(typeof(WorldBar), nameof(WorldBar.Open))]
    public static bool OpenPrefix(WorldBar __instance)
    {
        return __instance.runCtrl.currentZoneDot != null;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(RunCtrl), nameof(RunCtrl.CreateNewRun))]
    public static bool CreateRunPrefix(RunCtrl __instance, int zoneNum, int worldTierNum, bool campaign, string seed = "")
    {
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

    [HarmonyPrefix]
    [HarmonyPatch(typeof(RunCtrl), nameof(RunCtrl.LoopRun))]
    public static bool LoopRunPrefix(RunCtrl __instance)
    {
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

    /*[HarmonyPrefix]
    [HarmonyPatch(typeof(RunCtrl), nameof(RunCtrl.ChangeWorld))]
    public static bool SetProgressPrefix(RunCtrl __instance, string worldName)
    {
        var progress = __instance.progressBar;
        if ((worldName == "Genocide" || worldName == "Pacfifist" || worldName == "Normal") && __instance.currentRun != null)
        {
            var worldsCopy = new List<Image>(progress.worldDots);
            progress.worldDots.Clear();
            for (int i = __instance.currentRun.visitedWorldNames.Count; i < worldsCopy.Count; i++)
            {
                var worldImage = worldsCopy[i];
                worldsCopy[i] = null;
                UnityEngine.Object.Destroy(worldImage);
            }
            progress.worldDots.AddRange(worldsCopy.Where(image => image != null));
        }
        //else if (!S.I.runCtrl.worlds[worldName].tags.Contains(Tag.Pool))
        //{

        //}
        return true;
    }*/

}

public static class CustomZoneUtil
{

    public static ZoneType[] defaults = { ZoneType.Battle, ZoneType.Boss, ZoneType.Campsite, ZoneType.Danger, ZoneType.DarkShop, ZoneType.Distress, ZoneType.Genocide, ZoneType.Idle, ZoneType.Miniboss, ZoneType.Normal, ZoneType.Pacifist, ZoneType.PvP, ZoneType.Random, ZoneType.Shop, ZoneType.Treasure, ZoneType.World };

    public static List<object> ZoneEventScripts = new List<object>();
    public static List<Script> ZoneEventBaseScripts = new List<Script>();

    public static Dictionary<string, List<string>> customCampaignWorlds = new Dictionary<string, List<string>>();
    public static Dictionary<string, bool> customCampaignStatic = new Dictionary<string, bool>();
    public static Dictionary<string, string> customCampaignCharacters = new Dictionary<string, string>();

    public static string currentCampaign = "Default";


    static public void Setup()
    {

    }

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


    public static bool CompareOperand(CodeInstruction c1, CodeInstruction c2)
    {
        if (c2.operand == null)
        {
            return true;
        }
        if (c1.opcode == OpCodes.Br || c1.opcode == OpCodes.Brfalse || c1.opcode == OpCodes.Brfalse_S || c1.opcode == OpCodes.Brtrue || c1.opcode == OpCodes.Brtrue_S || c1.opcode == OpCodes.Br_S)
        {
            return true;
        }
        if ((c1.operand == null) == (c2.operand == null))
        {
            if (c1.operand != null)
            {
                var t = c1.operand.GetType();
                var u = c2.operand.GetType();
                var b = (t.IsAssignableFrom(u) || u.IsAssignableFrom(t));
                return b && ((bool)u.GetMethod("Equals", new System.Type[] { t }).Invoke(c2.operand, new object[] { c1.operand }) || (bool)t.GetMethod("Equals", new System.Type[] { u }).Invoke(c1.operand, new object[] { c2.operand }));
            }
            else
            {
                return true;
            }
        }
        else
        {
            return false;
        }
    }

    public static void AddZone(string name, string sprite, string music, bool can_continue = false, bool spawn_environment = false, bool spawn_enemies = true, bool spawn_boss = true)
    {
        Debug.Log("Attempting to add zonetype with name: " + name);
        if (LuaPowerData.customEnums[typeof(ZoneType)].Contains(name))
        {
            Debug.LogError("Custom Zone Type '" + name + "' already exists.");
            return;
        }
        LuaPowerData.customEnums[typeof(ZoneType)].Add(name);
        CustomZoneTypeData zone = new CustomZoneTypeData();
        S.I.runCtrl.worldBar.zoneSprites.Add(name, LuaPowerSprites.GetSprite(sprite));
        zone.can_continue = can_continue;
        zone.music = music;
        zone.spawn_environment = spawn_environment;
        zone.spawn_enemies = spawn_enemies;
        zone.spawn_boss = spawn_boss;
        Debug.Log("ZoneType added with name: " + GetZoneType(name).ToString());
        CustomZoneMiscPatches.custom_zones.Add(GetZoneType(name), zone);
    }

    public static ZoneType GetZoneType(string name)
    {
        return (ZoneType)LuaPowerData.customEnums[typeof(ZoneType)].IndexOf(name);
    }

    public static void HandleEnemyAttribute(XmlAttribute attribute, string beingID)
    {
    }

    public static bool IrregularSpawn(ZoneType type)
    {
        return type != ZoneType.Battle && type != ZoneType.Danger;
    }

    public static void TriggerTypeZoneEvent(ZoneType type, string eventName)
    {
        TriggerZoneEvent(eventName);
        TriggerZoneEvent(type.ToString() + ":" + eventName);
    }

    public static void TriggerZoneEvent(string eventName)
    {
        for (int i = 0; i < ZoneEventScripts.Count; i++)
        {
            ZoneEventBaseScripts[i].Globals["eventName"] = eventName;
            ZoneEventBaseScripts[i].Globals["ctrl"] = S.I.batCtrl;
            ZoneEventBaseScripts[i].Globals["spawnCtrl"] = S.I.spCtrl;
            S.I.mainCtrl.StartCoroutine(MoreLuaPower_FunctionHelper.EffectRoutine(ZoneEventBaseScripts[i].CreateCoroutine(ZoneEventScripts[i])));
            ZoneEventBaseScripts[i].Globals.Remove("eventName");
            ZoneEventBaseScripts[i].Globals.Remove("ctrl");
            ZoneEventBaseScripts[i].Globals.Remove("spawnCtrl");
        }
        CustomWorldGenerator.MakeZoneSectionVisible(eventName);
    }

}
