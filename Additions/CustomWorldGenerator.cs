using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml;
using System.Linq;
using UnityEngine;

using MoonSharp.Interpreter;
using System;

public class CustomWorldGenerator
{

    public static List<object> AddScripts = new List<object>();
    public static List<Script> AddBaseScripts = new List<Script>();

    public static Dictionary<World, List<string>> next_worlds = new Dictionary<World, List<string>>();
    public static Dictionary<World, List<string>> branch_worlds = new Dictionary<World, List<string>>();
    public static Dictionary<World, int> num_steps = new Dictionary<World, int>();
    public static Dictionary<World, bool> manualGeneration = new Dictionary<World, bool>();

    public static List<ZoneDot> refreshWorldDots = new List<ZoneDot>();

    public static bool IsCustomWorld(World world)
    {
        return next_worlds.ContainsKey(world) || branch_worlds.ContainsKey(world) || num_steps.ContainsKey(world) || manualGeneration.ContainsKey(world);
    }

    //================================================================================================================================================

    public List<CustomColumnGenerator> columns = new List<CustomColumnGenerator>();
    public List<RectTransform> columnTransforms = new List<RectTransform>();

    public WorldBar bar = null;
    public CustomWorldGenerator generator = null;
    public Dictionary<CustomZoneGenerator, float> probabilities = new Dictionary<CustomZoneGenerator, float>();
    public Dictionary<int, Dictionary<string, int>> column_uses = new Dictionary<int, Dictionary<string, int>>();
    public Dictionary<string, int> total_uses = new Dictionary<string, int>();
    public int numSteps = 7;

    public Dictionary<ZoneDot, CustomZoneGenerator> source = new Dictionary<ZoneDot, CustomZoneGenerator>();

    public CustomWorldGenerator(WorldBar theBar)
    {
        bar = theBar;
        refreshWorldDots.Clear();
        World world = bar.runCtrl.currentWorld;
        if (num_steps.ContainsKey(world))
        {
            numSteps = num_steps[world];
        }
        GetColumnGenerators(world);
        //WorldGen2(__instance, numSteps);
    }

    protected bool AttemptGen(int stepNum, CustomZoneGenerator zoneGen)
    {
        if(!(column_uses[stepNum].ContainsKey(zoneGen.activationKey) && zoneGen.maxCollumnActivations <= column_uses[stepNum][zoneGen.activationKey]) && !(total_uses.ContainsKey(zoneGen.activationKey) && zoneGen.maxWorldActivations <= total_uses[zoneGen.activationKey]))
        {
            float prob = probabilities.ContainsKey(zoneGen) ? probabilities[zoneGen] : zoneGen.probability;
            if (prob >= bar.runCtrl.NextWorldRand(0, 100))
            {
                if (!probabilities.ContainsKey(zoneGen))
                {
                    probabilities.Add(zoneGen, zoneGen.probability);
                }
                probabilities[zoneGen] /= zoneGen.probabilityReductionFactor;
                return true;
            }
            else if (zoneGen.probabilityReductionOnFail)
            {
                if (!probabilities.ContainsKey(zoneGen))
                {
                    probabilities.Add(zoneGen, zoneGen.probability);
                }
                probabilities[zoneGen] /= zoneGen.probabilityReductionFactor;
            }
        }
        return false;
    }

    protected void EditDotsWithGen(List<ZoneDot> dots, CustomZoneGenerator zoneGen, Predicate<ZoneDot> cond)
    {
        for (int index = 0; index < dots.Count; index++)
        {
            ZoneDot dot = dots[index];

            if (!cond(dot)) continue;

            if (AttemptGen(dot.stepNum, zoneGen)) EditDotWithGen(dot, zoneGen);
        }
    }

    protected void EditDotWithGen(ZoneDot dot, CustomZoneGenerator zoneGen)
    {
        
        if ((zoneGen.genType & (int)ZoneGeneratorFlags.REPLACE) != 0)
        {
            if (zoneGen.zoneType == ZoneType.World)
            {
                dot.worldName = zoneGen.worldName;
                dot.world = bar.runCtrl.worlds[dot.worldName];
                dot.imageName = dot.world.iconName;
                if (zoneGen.refreshCurrentworld) refreshWorldDots.Add(dot);
                else refreshWorldDots.Remove(dot);
            }
            else if (dot.type == ZoneType.World)
            {
                refreshWorldDots.Remove(dot);
            }
        }
        if (zoneGen.dark) dot.SetDark();
        dot.SetType(zoneGen.zoneType);
        
        if (!column_uses[dot.stepNum].ContainsKey(zoneGen.activationKey))
        {
            column_uses[dot.stepNum].Add(zoneGen.activationKey, 0);
        }
        column_uses[dot.stepNum][zoneGen.activationKey]++;
        if (!total_uses.ContainsKey(zoneGen.activationKey))
        {
            total_uses.Add(zoneGen.activationKey, 0);
        }
        total_uses[zoneGen.activationKey]++;
    }

    protected void GetColumnGenerators(World world)
    {
        if (!manualGeneration.ContainsKey(world) || !manualGeneration[world]) columns.AddRange(CreateDefaultColumnGenerators());
        DynValue worldVal = UserData.Create(world);
        DynValue generatorVal = UserData.Create(this);
        for (int i = 0; i < AddScripts.Count; i++)
        {
            AddBaseScripts[i].Globals["world"] = worldVal;
            AddBaseScripts[i].Globals["generator"] = generatorVal;
            S.I.mainCtrl.StartCoroutine(MoreLuaPower_FunctionHelper.EffectRoutine(AddBaseScripts[i].CreateCoroutine(AddScripts[i])));
            AddBaseScripts[i].Globals.Remove("world");
            AddBaseScripts[i].Globals.Remove("generator");
        }
        Debug.Log("Got generators!");
    }

    protected static List<CustomColumnGenerator> CreateDefaultColumnGenerators()
    {
        List<CustomColumnGenerator> DefaultGenerators = new List<CustomColumnGenerator>();

        CustomZoneGenerator miniBoss = new CustomZoneGenerator();
        miniBoss.maxWorldActivations = 1;
        miniBoss.genType = (int)ZoneGeneratorFlags.REPLACE;
        miniBoss.replaceType = ZoneType.Battle;
        miniBoss.zoneType = ZoneType.Miniboss;
        miniBoss.probability = 50.0f;
        miniBoss.probabilityReductionFactor = 0.5f;
        miniBoss.probabilityReductionOnFail = true;
        miniBoss.limitLow = 0;
        miniBoss.limitHigh = 0;
        miniBoss.activationKey = "Miniboss";


        CustomZoneGenerator beforeBattleGen1 = new CustomZoneGenerator();
        beforeBattleGen1.activationKey = "Before Battle 1";
        beforeBattleGen1.beforeType = ZoneType.Battle;
        beforeBattleGen1.replaceType = ZoneType.Battle;
        beforeBattleGen1.zoneType = ZoneType.Danger;
        beforeBattleGen1.genType = (int)ZoneGeneratorFlags.REPLACE | (int)ZoneGeneratorFlags.BEFORE;
        beforeBattleGen1.probability = 50.0f / 1.0f;
        beforeBattleGen1.probabilityReductionFactor = 2.0f;

        CustomZoneGenerator beforeBattleGen2 = new CustomZoneGenerator();
        beforeBattleGen2.activationKey = "Before Battle 2";
        beforeBattleGen2.beforeType = ZoneType.Battle;
        beforeBattleGen2.replaceType = ZoneType.Battle;
        beforeBattleGen2.zoneType = ZoneType.Distress;
        beforeBattleGen2.genType = (int)ZoneGeneratorFlags.REPLACE | (int)ZoneGeneratorFlags.BEFORE;
        beforeBattleGen2.probability = 50.0f / 1.0f;
        beforeBattleGen2.probabilityReductionFactor = 2.0f;

        CustomZoneGenerator beforeBattleGen3 = new CustomZoneGenerator();
        beforeBattleGen3.activationKey = "Before Battle 3";
        beforeBattleGen3.beforeType = ZoneType.Battle;
        beforeBattleGen3.replaceType = ZoneType.Battle;
        beforeBattleGen3.zoneType = ZoneType.Treasure;
        beforeBattleGen3.genType = (int)ZoneGeneratorFlags.REPLACE | (int)ZoneGeneratorFlags.BEFORE;
        beforeBattleGen3.probability = 100.0f / 1.0f;
        beforeBattleGen3.probabilityReductionFactor = 2.0f;

        for (int i = 0; i < 8; i++)
        {
            CustomColumnGenerator collumGen = new CustomColumnGenerator();
            DefaultGenerators.Add(collumGen);
            switch (i)
            {
                case 0:
                    {
                        collumGen.numZonesDefault = 1;
                    }
                    break;
                case 1:
                    {
                        CustomZoneGenerator afterBattleGen1 = new CustomZoneGenerator();
                        afterBattleGen1.activationKey = "After Battle 1";
                        afterBattleGen1.afterType = ZoneType.Battle;
                        afterBattleGen1.replaceType = ZoneType.Battle;
                        afterBattleGen1.zoneType = ZoneType.Danger;
                        afterBattleGen1.genType = (int)ZoneGeneratorFlags.REPLACE | (int)ZoneGeneratorFlags.AFTER;
                        afterBattleGen1.probability = 100.0f / 2.0f;
                        afterBattleGen1.probabilityReductionFactor = 1.0f / 2.0f;
                        afterBattleGen1.probabilityReductionOnFail = true;
                        afterBattleGen1.maxWorldActivations = 1;

                        CustomZoneGenerator afterBattleGen2 = new CustomZoneGenerator();
                        afterBattleGen2.activationKey = "After Battle 2";
                        afterBattleGen2.afterType = ZoneType.Battle;
                        afterBattleGen2.replaceType = ZoneType.Battle;
                        afterBattleGen2.zoneType = ZoneType.Distress;
                        afterBattleGen2.genType = (int)ZoneGeneratorFlags.REPLACE | (int)ZoneGeneratorFlags.AFTER;
                        afterBattleGen2.probability = 100.0f / 2.0f;
                        afterBattleGen2.probabilityReductionFactor = 1.0f / 2.0f;
                        afterBattleGen2.probabilityReductionOnFail = true;
                        afterBattleGen2.maxWorldActivations = 1;
                        //afterBattleGen2.dark = true;

                        collumGen.numZonesDefault = 3;
                        //collumGen.connectionType = ZoneGeneratorConncetions.SPLIT;
                        collumGen.zoneGenerators.Add(afterBattleGen1);
                        collumGen.zoneGenerators.Add(afterBattleGen2);
                    }
                    break;
                case 2:
                    {
                        collumGen.numZonesDefault = 3;
                        collumGen.numZonesRemovePotential = 1;
                        //collumGen.connectionType = ZoneGeneratorConncetions.MAINTAIN_SPLIT;
                        collumGen.zoneGenerators.Add(miniBoss);
                        collumGen.zoneGenerators.Add(beforeBattleGen1);
                        collumGen.zoneGenerators.Add(beforeBattleGen2);
                        collumGen.zoneGenerators.Add(beforeBattleGen3);
                    }
                    break;
                case 3:
                    {
                        collumGen.numZonesDefault = 3;
                        collumGen.numZonesRemovePotential = 1;
                        //collumGen.connectionType = ZoneGeneratorConncetions.SPLIT;
                        collumGen.zoneGenerators.Add(miniBoss);
                        collumGen.zoneGenerators.Add(beforeBattleGen1);
                        collumGen.zoneGenerators.Add(beforeBattleGen2);
                        collumGen.zoneGenerators.Add(beforeBattleGen3);
                        break;
                    }
                case 4:
                    {
                        CustomZoneGenerator shopGen = new CustomZoneGenerator();
                        shopGen.genType = (int)ZoneGeneratorFlags.REPLACE;
                        shopGen.zoneType = ZoneType.Shop;
                        shopGen.replaceType = ZoneType.Battle;
                        shopGen.probability = 100.0f / 3.0f;
                        shopGen.probabilityReductionFactor = 2.0f / 3.0f;
                        shopGen.probabilityReductionOnFail = true;
                        shopGen.maxWorldActivations = 1;
                        shopGen.activationKey = "Shop";

                        CustomZoneGenerator treasureGen = new CustomZoneGenerator();
                        treasureGen.zoneType = ZoneType.Treasure;
                        treasureGen.replaceType = ZoneType.Battle;
                        treasureGen.limitLow = 0;
                        treasureGen.limitHigh = 2;
                        treasureGen.genType = (int)ZoneGeneratorFlags.REPLACE;
                        treasureGen.probability = 200.0f / 3.0f;
                        treasureGen.activationKey = "Treasure";

                        CustomZoneGenerator campGen = new CustomZoneGenerator();
                        campGen.zoneType = ZoneType.Campsite;
                        campGen.replaceType = ZoneType.Treasure;
                        campGen.genType = (int)ZoneGeneratorFlags.REPLACE;
                        campGen.activationKey = "Camp";
                        campGen.maxWorldActivations = 1;
                        collumGen.numZonesDefault = 3;
                        //collumGen.connectionType = ZoneGeneratorConncetions.SPLIT;
                        collumGen.zoneGenerators.Add(shopGen);
                        collumGen.zoneGenerators.Add(treasureGen);
                        collumGen.zoneGenerators.Add(campGen);
                        collumGen.zoneGenerators.Add(beforeBattleGen3);
                        collumGen.zoneGenerators.Add(beforeBattleGen1);
                        collumGen.zoneGenerators.Add(beforeBattleGen2);
                        break;
                    }

                case 5:
                    {

                        CustomZoneGenerator shopGen = new CustomZoneGenerator();
                        shopGen.genType = (int)ZoneGeneratorFlags.REPLACE;
                        shopGen.zoneType = ZoneType.Shop;
                        shopGen.replaceType = ZoneType.Battle;
                        shopGen.probability = 50.0f;
                        shopGen.probabilityReductionFactor = 1.0f / 2.0f;
                        shopGen.probabilityReductionOnFail = true;
                        shopGen.maxWorldActivations = 1;
                        shopGen.activationKey = "Shop";

                        collumGen.numZonesDefault = 3;
                        collumGen.numZonesRemovePotential = 1;
                        collumGen.zoneGenerators.Add(shopGen);

                        break;
                    }
                case 6:
                    {
                        collumGen.numZonesDefault = 1;
                        collumGen.defaultZoneType = ZoneType.Boss;
                        break;
                    }
                case 7:
                    {
                        collumGen.numZonesDefault = 3;
                        collumGen.defaultZoneType = ZoneType.World;
                        collumGen.additionalVerticalSpacing = 7.0f;
                    }
                    break;
            }
        }
        return DefaultGenerators;
    }

    public bool CanConnect(ZoneDot d1, ZoneDot d2)
    {
        if (d1 == d2 || d1.stepNum >= d2.stepNum) return false;

        return false;

        var gen = columns[d1.stepNum];
        if (!gen.possibleConnectionColumns.Contains(d2.stepNum - d1.stepNum)) return false; 
        var list1 = bar.currentZoneSteps[d1.stepNum];
        var list2 = bar.currentZoneSteps[d2.stepNum];
        var gen1 = columns[d1.stepNum];
        var gen2 = columns[d2.stepNum];
        var i1 = list1.IndexOf(d1);
        var i2 = list2.IndexOf(d2);
        float pos1 = 2.0f * ((((float)i1 + 0.5f) / list1.Count()) - 0.5f);
        float pos2 = 2.0f * ((((float)i2 + 0.5f) / list2.Count()) - 0.5f);
        return pos1 == 0 || pos1 * pos2 >= 0;
    }

    public bool TraverseAndAdd(ZoneDot dot, List<ZoneDot> covered)
    {
        if (dot == null) return false;
        covered.Add(dot);
        if (dot.type != ZoneType.World && dot.stepNum != bar.currentZoneSteps.Count - 1)
        {
            foreach (var otherDot in dot.nextDots)
            {
                if (!covered.Contains(otherDot)) TraverseAndAdd(otherDot, covered);
            }
            while (TraverseAndAdd(FindAddNext(dot), covered)) ;
            ZoneDot result = CreateNextDot(dot);
            while (result)
            {
                TraverseAndAdd(result, covered);
                result = CreateNextDot(dot);
            }

        }
        if (dot.stepNum != 0 && dot.previousDots.Count == 0)
        {
            foreach (var otherDot in dot.previousDots)
            {
                if (!covered.Contains(otherDot)) TraverseAndAdd(otherDot, covered);
            }
            while (TraverseAndAdd(FindAddPrev(dot), covered)) ;
            ZoneDot result = CreatePrevDot(dot);
            while (result)
            {
                TraverseAndAdd(result, covered);
                result = CreatePrevDot(dot);
            }
        }
        return true;
    }

    public ZoneDot CreateNextDot(ZoneDot dot)
    {
        var columnGen = columns[dot.stepNum + 1];
        foreach (var gen in columnGen.zoneGenerators)
        {
            if ((gen.genType & (int)ZoneGeneratorFlags.AFTER) != 0 && (gen.genType & (int)ZoneGeneratorFlags.ADD) != 0)
            {
                if (gen.afterType != dot.type) continue;
                if (AttemptGen(dot.stepNum+1, gen))
                {
                    var newDot = CreateDot(dot.stepNum + 1);
                    newDot.type = columnGen.defaultZoneType;
                    EditDotWithGen(newDot, gen);
                    AddAfter(newDot, dot);
                    return newDot;
                }
            }
        }
        return null;
    }

    public ZoneDot CreatePrevDot(ZoneDot dot)
    {
        var columnGen = columns[dot.stepNum - 1];
        foreach (var gen in columnGen.zoneGenerators)
        {
            if ((gen.genType & (int)ZoneGeneratorFlags.BEFORE) != 0 && (gen.genType & (int)ZoneGeneratorFlags.ADD) != 0)
            {
                if (gen.beforeType != dot.type) continue;
                if (AttemptGen(dot.stepNum - 1, gen))
                {
                    var newDot = CreateDot(dot.stepNum - 1);
                    newDot.type = columnGen.defaultZoneType;
                    EditDotWithGen(newDot, gen);
                    AddBefore(newDot, dot);
                    return newDot;
                }
            }
        }
        return null;
    }


    public ZoneDot FindAddNext(ZoneDot dot)
    {
        foreach (var otherDot in bar.currentZoneDots)
        {
            if (!dot.nextDots.Contains(otherDot) && CanConnect(dot, otherDot))
            {
                AddAfter(otherDot, dot);
                return otherDot;
            }
        }
        return null;
    }

    public ZoneDot FindAddPrev(ZoneDot dot)
    {
        foreach (var otherDot in bar.currentZoneDots)
        {
            if (!dot.nextDots.Contains(otherDot) && CanConnect(otherDot, dot) )
            {
                AddBefore(otherDot, dot);
                return otherDot;
            }
        }
        return null;
    }

    public void AddAfter(ZoneDot dot, ZoneDot root)
    {
        dot.previousDots.Add(root);
        root.AddNextDot(dot);
    }

    public void AddBefore(ZoneDot dot, ZoneDot root)
    {
        root.previousDots.Add(dot);
        dot.AddNextDot(root);
    }

    public ZoneDot CreateDot(int index1)
    {
        ZoneDot zoneDot = UnityEngine.Object.Instantiate<ZoneDot>(bar.zoneDotPrefab, bar.transform.position, bar.transform.rotation, columnTransforms[index1].transform);
        zoneDot.stepNum = index1;
        zoneDot.worldBar = bar;
        zoneDot.idCtrl = bar.idCtrl;
        zoneDot.btnCtrl = bar.btnCtrl;
        zoneDot.transform.name = "ZoneDot - Step: " + (object)index1;
        zoneDot.verticalSpacing = bar.defaultVerticalSpacing;
        bar.currentZoneDots.Add(zoneDot);
        return zoneDot;
    }

    public void Generate()
    {
        foreach (Component component in bar.zoneDotContainer)
        {
            UnityEngine.Object.Destroy((UnityEngine.Object)component.gameObject);
        }
        bar.currentZoneDots.Clear();
        bar.currentZoneSteps.Clear();
        if (bar.btnCtrl.hideUICounter < 1)
            bar.detailPanel.gameObject.SetActive(true);
        List<ZoneDot> previousDots = new List<ZoneDot>();
        List<string> stringList = new List<string>((IEnumerable<string>)bar.runCtrl.currentRun.unvisitedWorldNames);
        int num1 = 100;
        if (bar.runCtrl.currentRun != null && bar.runCtrl.currentRun.hellPasses.Contains(2))
            bar.dangerChance = 200f;
        else
            bar.dangerChance = 100f;
        bar.runCtrl.currentRun.lastWorldGenOrigin = bar.runCtrl.currentRun.currentWorldGen;
        World world = bar.runCtrl.currentWorld;

        Dictionary<CustomZoneGenerator, float> probabilities = new Dictionary<CustomZoneGenerator, float>();

        Dictionary<string, int> total_uses = new Dictionary<string, int>();

        int previousSplitState = 0;
        int index1 = 0;

        foreach (CustomColumnGenerator gen in columns)
        {
            RectTransform rectTransform = new GameObject("ZoneStep").AddComponent<RectTransform>();
            List<ZoneDot> stepDots = new List<ZoneDot>();
            bar.currentZoneSteps.Add(stepDots);
            Vector3 vector3 = bar.zoneDotContainer.transform.position - new Vector3((float)((double)bar.width / 2.0 - (double)bar.width / 6 * (double)index1) * bar.zoneDotContainer.lossyScale.x, 0.0f, 0.0f);
            rectTransform.localScale = bar.zoneDotContainer.lossyScale;
            rectTransform.SetParent(bar.zoneDotContainer, true);
            rectTransform.transform.position = vector3;
            rectTransform.sizeDelta = new Vector2(10f, (float)num1);
            columnTransforms.Add(rectTransform);
            column_uses.Add(index1, new Dictionary<string, int>());
            index1++;
        }

        for (index1 = 0; index1 < columns.Count; index1++)
        {
            CustomColumnGenerator gen = columns[index1];
            List<ZoneDot> stepDots = bar.currentZoneSteps[index1];
            
            World current = bar.runCtrl.currentWorld;
            List<string> next_world_list = next_worlds.ContainsKey(current) ? next_worlds[current] : new List<string>();
            List<ZoneDot> nextPreviousDots = new List<ZoneDot>();
            int index2 = 1;

            Dictionary<string, int> column_uses = new Dictionary<string, int>();

            int genNumZones = gen.numZonesDefault + bar.runCtrl.NextWorldRand(0, gen.numZonesAdditionPotential + 1) - bar.runCtrl.NextWorldRand(0, gen.numZonesRemovePotential + 1);
            int numZones = (gen.defaultZoneType != ZoneType.World ? genNumZones : Math.Min(genNumZones, Math.Max(1, stringList.Count)));
            for (index2 = 1; index2 <= numZones; index2++)
            {
                ZoneDot dot = CreateDot(index1);
                dot.type = gen.defaultZoneType;
                dot.verticalSpacing += gen.additionalVerticalSpacing;
                stepDots.Add(dot);
                nextPreviousDots.Add(dot);
                if (dot.type == ZoneType.World)
                {
                    //Default world stuff
                    bool genocideRun = bar.runCtrl.savedBossKills >= 7;

                    if (stringList.Count > 0)
                    {
                        if (next_world_list.Count == 0)
                        {
                            int index3 = bar.runCtrl.NextWorldRand(0, stringList.Count);
                            dot.worldName = stringList[index3];
                            dot.world = bar.runCtrl.worlds[dot.worldName];
                            dot.imageName = dot.world.iconName;
                            stringList.Remove(stringList[index3]);
                        }
                        else
                        {
                            try
                            {
                                int index3 = bar.runCtrl.NextWorldRand(0, next_world_list.Count);
                                dot.worldName = next_world_list[index3];
                                dot.world = bar.runCtrl.worlds[dot.worldName];
                                dot.imageName = dot.world.iconName;
                                next_world_list.RemoveAt(index3);
                            }
                            catch (Exception e)
                            {

                                Debug.LogError("Next world error: " + e.Message);
                            }

                        }
                    }
                    else
                    {
                        if (bar.runCtrl.savedBossKills >= 7)
                        {
                            dot.worldName = "Genocide";
                            dot.imageName = "WorldWasteland";
                        }
                        else if (bar.runCtrl.savedBossKills >= 1)
                        {
                            dot.worldName = "Normal";
                            dot.imageName = "WorldWasteland";
                        }
                        else
                        {
                            dot.worldName = "Pacifist";
                            dot.imageName = "WorldWasteland";
                        }
                        dot.world = bar.runCtrl.worlds[dot.worldName];
                    }
                    

                }

                dot.SetType(gen.defaultZoneType);
            }

            //int splitState = gen.connectionType == ZoneGeneratorConncetions.MAINTAIN_SPLIT ? previousSplitState : 0;

            /*for (int dotIndex = 0; dotIndex < stepDots.Count; dotIndex++)
            {
                ZoneDot dot = stepDots[dotIndex];
                dot.transform.position = vector3 + new Vector3(0.0f, ((float)(gen.numZonesDefault - 1) / 2f - (float)(dotIndex - 1)) * dot.verticalSpacing * bar.rect.localScale.y, 0.0f);
                bar.currentZoneDots.Add(dot);
                if (index1 != 0)
                {
                    List<ZoneDot> step = bar.currentZoneSteps[index1 - 1];
                    CustomColumnGenerator previousGen = generators[index1 - 1];

                    for (int otherDotIndex = 0; otherDotIndex < step.Count; otherDotIndex++)
                    {
                        ZoneDot previousDot = step[otherDotIndex];

                        if (previousDot.type != ZoneType.World)
                        {
                            bool add = true;

                            int relative1 = dotIndex - stepDots.Count / 2;
                            if (relative1 == 0)
                            {
                                if (stepDots.Count % 2 == 1)
                                {
                                    if (splitState == 0)
                                    {
                                        if (bar.runCtrl.NextWorldRand(0, 2) == 0) splitState = -1;
                                        else splitState = 1;
                                    }
                                }
                                else
                                {
                                    relative1++;
                                }
                            }

                            int relative2 = otherDotIndex - step.Count / 2;
                            if (relative2 == 0)
                            {
                                if (step.Count % 2 == 1)
                                {
                                    if (previousSplitState == 0)
                                    {
                                        if (bar.runCtrl.NextWorldRand(0, 2) == 0) previousSplitState = -1;
                                        else previousSplitState = 1;
                                    }

                                }
                                else
                                {
                                    relative2++;
                                }
                            }

                            if (previousGen.connectionType == ZoneGeneratorConncetions.MAINTAIN_SPLIT || previousGen.connectionType == ZoneGeneratorConncetions.SPLIT)
                            {
                                if ((relative1 == 0 ? Math.Sign(splitState == 0 ? 1 : splitState) : Math.Sign(relative1)) != (relative2 == 0 ? Math.Sign(previousSplitState == 0 ? 1 : previousSplitState) : Math.Sign(relative2)))
                                {
                                    add = false;
                                }
                            }

                            if (add)
                            {
                                previousDot.AddNextDot(dot);
                                dot.previousDots.Add(previousDot);
                            }
                        }
                    }
                }
            }*/

            //previousSplitState = splitState;

            /*if (index1 != 0)
            {
                for (int prevIndex = 0; prevIndex < previousDots.Count - 1; prevIndex++)
                {
                    ZoneDot prevDot = previousDots[prevIndex];
                    ZoneDot nextDot = previousDots[prevIndex + 1];
                    if (bar.runCtrl.NextWorldRand(0, 2) == 0)
                    {
                        var res = (from dot in prevDot.nextDots where (from other_dot in nextDot.nextDots where stepDots.IndexOf(other_dot) - stepDots.IndexOf(dot) == -1 select other_dot).Count() > 0 select dot).ToList();
                        if (res.Count() > 0)
                        {
                            var forwardDot = res[bar.runCtrl.NextWorldRand(0, res.Count())];
                            forwardDot.previousDots.Remove(prevDot);
                            prevDot.nextDots.Remove(forwardDot);
                        }

                    }
                    else
                    {
                        var res = (from dot in nextDot.nextDots where (from other_dot in prevDot.nextDots where stepDots.IndexOf(other_dot) - stepDots.IndexOf(dot) == 1 select other_dot).Count() > 0 select dot).ToList();
                        if (res.Count() > 0)
                        {
                            var forwardDot = res[bar.runCtrl.NextWorldRand(0, res.Count())];
                            forwardDot.previousDots.Remove(nextDot);
                            nextDot.nextDots.Remove(forwardDot);
                        }
                    }

                }
            }*/
            
            if (index1 != 0)
            {
                /*List<ZoneDot> step = bar.currentZoneSteps[index1 - 1];
                CustomColumnGenerator previousGen = columns[index1 - 1];
                foreach (CustomZoneGenerator zoneGen in previousGen.zoneGenerators)
                {
                    if ((zoneGen.genType & (int)ZoneGeneratorFlags.REPLACE) != 0 && (zoneGen.genType & (int)ZoneGeneratorFlags.BEFORE) != 0)
                    {
                        EditDotsWithGen(step, zoneGen, dot => (dot.type == zoneGen.replaceType && dot.nextDots.Find(otherDot => otherDot.type == zoneGen.beforeType)));
                    }

                }*/
            }

            previousDots.Clear();
            previousDots = new List<ZoneDot>((IEnumerable<ZoneDot>)nextPreviousDots);
        }

        Debug.Log("World generation almost complete.");

        TraverseAndAdd(bar.currentZoneSteps[0][0], new List<ZoneDot>());

        for (int i = 0; i < bar.currentZoneSteps.Count; i++)
        {
            List<ZoneDot> step = bar.currentZoneSteps[i];
            CustomColumnGenerator gen = columns[i];
            foreach (CustomZoneGenerator zoneGen in gen.zoneGenerators)
            {
                Debug.Log(zoneGen.replaceType);
                if ((zoneGen.genType & (int)ZoneGeneratorFlags.REPLACE) != 0 && (zoneGen.genType & (int)ZoneGeneratorFlags.AFTER) != 0)
                {
                    EditDotsWithGen(step, zoneGen, dot => (dot.type == zoneGen.replaceType && dot.previousDots.Find(otherDot => otherDot.type == zoneGen.afterType)));
                }
                else if (zoneGen.genType == (int)ZoneGeneratorFlags.REPLACE)
                {
                    EditDotsWithGen(step, zoneGen, dot => (dot.type == zoneGen.replaceType));
                }
            }
            foreach (CustomZoneGenerator zoneGen in gen.zoneGenerators)
            {
                if ((zoneGen.genType & (int)ZoneGeneratorFlags.REPLACE) != 0 && (zoneGen.genType & (int)ZoneGeneratorFlags.BEFORE) != 0)
                {
                    EditDotsWithGen(step, zoneGen, dot => (dot.type == zoneGen.replaceType && dot.nextDots.Find(otherDot => otherDot.type == zoneGen.beforeType)));
                }

            }

        }


        for (int i = 0; i < columnTransforms.Count; i++)
        {
            RectTransform rectTransform = columnTransforms[i];
            for (int j = 0; j < bar.currentZoneSteps[i].Count; j++)
            {
                var dot = bar.currentZoneSteps[i][j];
                dot.transform.position = rectTransform.position + new Vector3(0.0f, ((float)(bar.currentZoneSteps[i].Count - 1) / 2f - (float)(j - 1)) * bar.currentZoneSteps[i][j].verticalSpacing * bar.rect.localScale.y, 0.0f);
            }
        }

        foreach (ZoneDot dot in bar.currentZoneDots)
        {
            dot.nextDots = dot.nextDots.OrderBy<ZoneDot, int>((Func<ZoneDot, int>)(t => t.transform.GetSiblingIndex())).ToList<ZoneDot>();
            dot.CreateLines();
        }

        bar.selectionMarker.transform.position = bar.currentZoneDots[0].transform.position;

        Debug.Log("World generation complete.");
    }

    //==============================LUA=============================================

    public CustomColumnGenerator GetColumn(int index)
    {
        return columns[index];
    }

    public int NumOfColumns()
    {
        return columns.Count;
    }

    public CustomColumnGenerator CreateColumn()
    {
        CustomColumnGenerator gen = new CustomColumnGenerator();
        columns.Add(gen);
        return gen;
    }

    public CustomColumnGenerator InsertNewColumn(int index)
    {
        CustomColumnGenerator gen = new CustomColumnGenerator();
        columns.Insert(index, gen);
        return gen;
    }
}

public class CustomColumnGenerator
{
    public List<CustomZoneGenerator> zoneGenerators = new List<CustomZoneGenerator>();
    public ZoneType defaultZoneType = ZoneType.Battle;
    public int numZonesDefault = 1;
    public int numZonesRemovePotential = 0;
    public int numZonesAdditionPotential = 0;
    public int groups = 1;
    public List<int> possibleConnectionColumns = new List<int>(new int[] { 1 });
    //public ZoneGeneratorConncetions connectionType = ZoneGeneratorConncetions.ALL;
    public float additionalVerticalSpacing = 0.0f;

    public CustomZoneGenerator CreateNewZoneGen()
    {
        var gen = new CustomZoneGenerator();
        zoneGenerators.Add(gen);
        return gen;
    }

    public CustomZoneGenerator CreateNewTopZoneGen()
    {
        var gen = new CustomZoneGenerator();
        zoneGenerators.Insert(0, gen);
        return gen;
    }

    public void AddZoneGen(CustomZoneGenerator gen)
    {
        zoneGenerators.Add(gen);
    }

    public void AddExtraColumnConnections(int relative)
    {
        if (!possibleConnectionColumns.Contains(relative))
        {
            possibleConnectionColumns.Add(relative);
        }
    }

    public void RemoveColumnConnections(int relative)
    {
        if (possibleConnectionColumns.Contains(relative))
        {
            possibleConnectionColumns.Remove(relative);
        }
    }
}

public enum ZoneGeneratorFlags
{
    NONE = 0,
    REPLACE = (1 << 0),
    ADD = (1 << 1),
    AFTER = (1 << 2),
    BEFORE = (1 << 3)
}

/*public enum ZoneGeneratorConncetions
{
    ALL,
    SPLIT,
    MAINTAIN_SPLIT,
}*/

public class CustomZoneGenerator
{
    public int genType = (int)ZoneGeneratorFlags.NONE;
    public ZoneType zoneType = ZoneType.Battle;
    public string activationKey = "";
    public int maxCollumnActivations = 666;
    public int maxWorldActivations = 666;
    public float probability = 100f;
    public float probabilityReductionFactor = 1.0f;
    public bool probabilityReductionOnFail = false;
    public bool dark = false;

    //Replace
    public ZoneType replaceType = ZoneType.Battle;

    //Add

    //AfterEvery
    public ZoneType afterType = ZoneType.Battle;

    //BeforeEvery
    public ZoneType beforeType = ZoneType.Battle;

    //Limit (TODO)
    public int limitLow = 0;
    public int limitHigh = 666;

    //World
    public string worldName = "Genocide";
    public bool refreshCurrentworld = false;

    public void SetAdd()
    {
        genType |= (int)ZoneGeneratorFlags.ADD;
    }

    public void SetReplace(ZoneType type)
    {
        genType |= (int)ZoneGeneratorFlags.REPLACE;
        replaceType = type;
    }

    public void SetGoAfter(ZoneType type)
    {
        genType |= (int)ZoneGeneratorFlags.AFTER;
        afterType = type;
    }

    public void SetGoBefore(ZoneType type)
    {
        genType |= (int)ZoneGeneratorFlags.BEFORE;
        beforeType = type;
    }

    public void SetWorldData(string nameString, bool reAdd)
    {
        zoneType = ZoneType.World;
        worldName = nameString;
        refreshCurrentworld = reAdd;
    }
}

