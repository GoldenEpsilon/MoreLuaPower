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

public struct CustomZone
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
        return true;
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        Debug.Log("Zone Transpiler running!");
        Queue<CodeInstruction> state = new Queue<CodeInstruction>();
        //bool once = true;
        CodeInstruction warp = null;
        foreach (var instruction in instructions)
        {

            if (state.Count == 0 && instruction.opcode == OpCodes.Ldarg_3)
            {
                state.Enqueue(instruction);
                //if (once) add = true;
                //once = false;
            }
            if (state.Count == 1 && instruction.opcode == OpCodes.Ldc_I4_1)
            {
                state.Enqueue(instruction);
                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CustomZoneUtil), nameof(CustomZoneUtil.IrregularSpawn)));
                continue;
            }
            /*if (state.Count == 1 && instruction.opcode == OpCodes.Ldc_I4_2)
            {
                state.Enqueue(instruction);
                instruction.opcode = OpCodes.Ldc_I4_4;
                add = false;
            }*/
            /*if (state.Count == 2 && instruction.opcode == OpCodes.Beq_S)
            {
                state.Clear();
                
                add = false;
                instruction.opcode = OpCodes.Br;
                warp = instruction;
            }*/
            if (state.Count == 2 && instruction.opcode == OpCodes.Beq_S)
            {
                state.Enqueue(instruction);
                //og.operand = instruction.operand;
                instruction.opcode = OpCodes.Brtrue;
            }

            yield return instruction;

            


        }
    }
}

[HarmonyPatch(typeof(XMLReader), nameof(XMLReader.XMLtoGetWorlds))]
public static class WorldXMLPatch
{

    public static void HandleWorld(World world, XmlAttribute attribute)
    {
        switch (attribute.Name)
        {
            case "nextWorlds":
                {
                    CustomZoneUtil.AddWorldToCustomGeneration(world.nameString);
                    string str = attribute.Value is string ? attribute.Value : "";
                    if (str != "")
                    {
                        str = str.Replace(" ", "");
                        string[] split = str.Split(',');
                        CustomWorldGenerator.next_worlds.Add(world, new List<string>(split));
                    }
                    break;
                }
            case "manualGeneration":
                {
                    CustomZoneUtil.AddWorldToCustomGeneration(world.nameString);
                    bool b = false;
                    bool.TryParse(attribute.Value, out b);
                    CustomWorldGenerator.manualGeneration.Add(world, b);
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
}

[HarmonyPatch]
public static class MiscPatches
{

    public static ZoneType[] defaults = { ZoneType.Battle, ZoneType.Boss, ZoneType.Campsite, ZoneType.Danger, ZoneType.DarkShop, ZoneType.Distress, ZoneType.Genocide, ZoneType.Idle, ZoneType.Miniboss, ZoneType.Normal, ZoneType.Pacifist, ZoneType.PvP, ZoneType.Random, ZoneType.Shop, ZoneType.Treasure, ZoneType.World };

    public static Dictionary<ZoneType, CustomZone> custom_zones = new Dictionary<ZoneType, CustomZone>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SpawnCtrl), nameof(SpawnCtrl.SpawnZoneC))]
    public static bool SpawnCustomZone(SpawnCtrl __instance, ZoneType zoneType)
    {
        if (!defaults.Contains(zoneType))
        {
            Debug.Log("Spawning Custom Zone!");
            if (custom_zones.ContainsKey(zoneType))
            {
                CustomZone zone = custom_zones[zoneType];
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
                    else Debug.Log("Did not spawn boss. None to spawn.");
                }
            }
            return true;
        }
        else
        {
            return true;
        }
    }

    [HarmonyPatch]
    [HarmonyPatch(typeof(RunCtrl), nameof(RunCtrl.GoToNextZone))]
    static class BossRush_RunCtrlPatches
    {
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
}


[HarmonyPatch(typeof(WorldBar), nameof(WorldBar.GenerateWorldBar))]
public static class WorldBarPatches
{

    public static bool Prefix(WorldBar __instance, int numSteps)
    {
        World world = __instance.runCtrl.currentWorld;
        if (!CustomZoneUtil.worldRegistry.Contains(world.nameString))
        {
            return true;
        }

        var gen = new CustomWorldGenerator(__instance);
        gen.Generate();
        return false;
    }
    

    
}

public static class CustomZoneUtil
{

    public static List<string> worldRegistry = new List<string>();

    static public void Setup()
    {

    }

    public static void AddWorldToCustomGeneration(string world)
    {
        if (!worldRegistry.Contains(world))
        {
            worldRegistry.Add(world);
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
                if (b) Debug.Log(((bool)u.GetMethod("Equals", new System.Type[] { t }).Invoke(c2.operand, new object[] { c1.operand }) || (bool)t.GetMethod("Equals", new System.Type[] { u }).Invoke(c1.operand, new object[] { c2.operand })));
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
        CustomZone zone = new CustomZone();
        S.I.runCtrl.worldBar.zoneSprites.Add(name, LuaPowerSprites.GetSprite(sprite));
        zone.can_continue = can_continue;
        zone.music = music;
        zone.spawn_environment = spawn_environment;
        zone.spawn_enemies = spawn_enemies;
        zone.spawn_boss = spawn_boss;
        Debug.Log("ZoneType added with name: " + GetZoneType(name).ToString());
        MiscPatches.custom_zones.Add(GetZoneType(name), zone);
    }

    public static ZoneType GetZoneType(string name)
    {
        return (ZoneType)LuaPowerData.customEnums[typeof(ZoneType)].IndexOf(name);
    }

    public static void HandleEnemyAttribute(XmlAttribute attribute, string beingID)
    {
        Debug.Log(beingID);
    }

    public static bool IrregularSpawn(ZoneType type)
    {
        return type != ZoneType.Battle && type != ZoneType.Danger;
    }

}
