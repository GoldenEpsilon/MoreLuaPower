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



public class CustomWorldGenerator
{
    public static CustomWorldGenerator CURRENT = null;

    public static bool AutoGeneration = false;

    public static List<object> WorldInitScripts = new List<object>();
    public static List<Script> WorldInitBaseScripts = new List<Script>();

    public static List<object> WorldPostScripts = new List<object>();
    public static List<Script> WorldPostBaseScripts = new List<Script>();
    public static Dictionary<string, bool> manualGeneration = new Dictionary<string, bool>();

    public static List<ZoneDot> refreshWorldDots = new List<ZoneDot>();

    public static Dictionary<string, List<ZoneDot>> hiddenSections = new Dictionary<string, List<ZoneDot>>();
    public static Dictionary<ZoneDot, string> invisDots = new Dictionary<ZoneDot, string>();
    public static Dictionary<ZoneDot, Dictionary<ZoneDot, RectTransform>> invisLines = new Dictionary<ZoneDot, Dictionary<ZoneDot, RectTransform>>();

    //=======================================================================================================================================================================================


    public static void MakeZoneSectionVisible(string sectionKey)
    {
        if (hiddenSections.ContainsKey(sectionKey))
        {
            var dots = hiddenSections[sectionKey];
            foreach (ZoneDot dot in dots)
            {
                if (dot.image != null) dot.image.enabled = true;
                if (dot.fgImage != null) dot.fgImage.enabled = true;
                if (dot.bgImage != null) dot.bgImage.enabled = true;
                foreach (ZoneDot otherDot in dot.previousDots)
                {
                    otherDot.nextDots.Add(dot);
                    if (invisLines.ContainsKey(dot))
                    {
                        if (invisLines[dot].ContainsKey(otherDot))
                        {
                            var line = invisLines[dot][otherDot];
                            otherDot.nextLines.Add(line);
                            line.GetComponent<Image>().enabled = true;
                        }
                    }
                }
                invisDots.Remove(dot);
                invisLines.Remove(dot);
            }
            hiddenSections.Remove(sectionKey);
        }
    }


    //================================================================================================================================================================================================================================================================================================

    public List<RectTransform> columnTransforms = new List<RectTransform>();

    public WorldBar bar = null;
    public Dictionary<PostProcessZoneGenerator, float> probabilities = new Dictionary<PostProcessZoneGenerator, float>();
    public Dictionary<int, Dictionary<string, int>> column_uses = new Dictionary<int, Dictionary<string, int>>();
    public Dictionary<string, int> total_uses = new Dictionary<string, int>();
    public int numSteps = 7;

    private bool post = false;

    List<string> stringList = null;

    public List<ManualZoneGenerator> manualZoneGenerators = new List<ManualZoneGenerator>();

    public List<PostProcessZoneGenerator> postProcess = new List<PostProcessZoneGenerator>();

    public Dictionary<ZoneDot, ZoneGenerator> source = new Dictionary<ZoneDot, ZoneGenerator>();

    //================================================================================================================================================================================================================================================================================================

    public CustomWorldGenerator(WorldBar theBar)
    {
        invisLines.Clear();
        hiddenSections.Clear();
        invisDots.Clear();
        stringList = new List<string>((IEnumerable<string>)theBar.runCtrl.currentRun.unvisitedWorldNames);
        bar = theBar;
        refreshWorldDots.Clear();
        World world = bar.runCtrl.currentWorld;
    }

    //================================================================================================================================================================================================================================================================================================

    protected void GetManualGenerators(World world)
    {
        DynValue worldVal = UserData.Create(world);
        DynValue generatorVal = UserData.Create(this);
        for (int i = 0; i < WorldInitScripts.Count; i++)
        {
            WorldInitBaseScripts[i].Globals["world"] = worldVal;
            WorldInitBaseScripts[i].Globals["generator"] = generatorVal;
            S.I.mainCtrl.StartCoroutine(MoreLuaPower_FunctionHelper.EffectRoutine(WorldInitBaseScripts[i].CreateCoroutine(WorldInitScripts[i])));
            WorldInitBaseScripts[i].Globals.Remove("world");
            WorldInitBaseScripts[i].Globals.Remove("generator");
        }
        Debug.Log("Got manual world generators!");
    }

    protected void GetPostProcessGenerators(World world)
    {
        DynValue worldVal = UserData.Create(world);
        DynValue generatorVal = UserData.Create(this);
        for (int i = 0; i < WorldPostScripts.Count; i++)
        {
            WorldPostBaseScripts[i].Globals["world"] = worldVal;
            WorldPostBaseScripts[i].Globals["generator"] = generatorVal;
            S.I.mainCtrl.StartCoroutine(MoreLuaPower_FunctionHelper.EffectRoutine(WorldPostBaseScripts[i].CreateCoroutine(WorldPostScripts[i])));
            WorldPostBaseScripts[i].Globals.Remove("world");
            WorldPostBaseScripts[i].Globals.Remove("generator");
        }
        Debug.Log("Got postprocess world generators!");
    }

    //================================================================================================================================================================================================================================================================================================

    protected bool AttemptGen(ZoneDot dot, ZoneGenerator zoneGen)
    {
        if (!(column_uses[dot.stepNum].ContainsKey(zoneGen.activationKey) && zoneGen.maxColumnActivations <= column_uses[dot.stepNum][zoneGen.activationKey]) && !(total_uses.ContainsKey(zoneGen.activationKey) && zoneGen.maxWorldActivations <= total_uses[zoneGen.activationKey]))
        {
            if (zoneGen.CanActivate(dot))
            {
                if (zoneGen is PostProcessZoneGenerator)
                {
                    var gen = zoneGen as PostProcessZoneGenerator;
                    float prob = probabilities.ContainsKey(gen) ? probabilities[gen] : gen.probability;
                    if (prob >= bar.runCtrl.NextWorldRand(0, 100))
                    {
                        if (!probabilities.ContainsKey(gen))
                        {
                            probabilities.Add(gen, gen.probability);
                        }
                        probabilities[gen] /= gen.probabilityReductionFactor;
                        return true;
                    }
                    else if (gen.probabilityReductionOnFail)
                    {
                        if (!probabilities.ContainsKey(gen))
                        {
                            probabilities.Add(gen, gen.probability);
                        }
                        probabilities[gen] /= gen.probabilityReductionFactor;
                    }
                }
                else
                {
                    return true;
                }

            }
        }
        return false;
    }

    protected bool AttemptGen(int stepNum, ZoneGenerator zoneGen)
    {
        if (!(column_uses[stepNum].ContainsKey(zoneGen.activationKey) && zoneGen.maxColumnActivations <= column_uses[stepNum][zoneGen.activationKey]) && !(total_uses.ContainsKey(zoneGen.activationKey) && zoneGen.maxWorldActivations <= total_uses[zoneGen.activationKey]))
        {
            if (zoneGen is PostProcessZoneGenerator)
            {
                var gen = zoneGen as PostProcessZoneGenerator;
                float prob = probabilities.ContainsKey(gen) ? probabilities[gen] : gen.probability;
                if (prob >= bar.runCtrl.NextWorldRand(0, 100))
                {
                    if (!probabilities.ContainsKey(gen))
                    {
                        probabilities.Add(gen, gen.probability);
                    }
                    probabilities[gen] /= gen.probabilityReductionFactor;
                    return true;
                }
                else if (gen.probabilityReductionOnFail)
                {
                    if (!probabilities.ContainsKey(gen))
                    {
                        probabilities.Add(gen, gen.probability);
                    }
                    probabilities[gen] /= gen.probabilityReductionFactor;
                }
            }
            else
            {
                return true;
            }
        }
        return false;
    }

    protected void EditDotWithGen(ZoneDot dot, ZoneGenerator zoneGen)
    {

        if (zoneGen.zoneType == ZoneType.World && !(zoneGen is ManualZoneGenerator && ((ManualZoneGenerator)zoneGen).automaticWorldSelection))
        {
            try
            {
                dot.world = bar.runCtrl.worlds[zoneGen.worldName];
                dot.worldName = zoneGen.worldName;
                dot.imageName = dot.world.iconName;
                if (zoneGen.refreshCurrentworld) refreshWorldDots.Add(dot);
                else refreshWorldDots.Remove(dot);
            }
            catch
            {
                Debug.LogError("No world exists with name: " + zoneGen.worldName);
            }

        }
        else if (dot.type == ZoneType.World)
        {
            refreshWorldDots.Remove(dot);
        }
        if (zoneGen.dark) dot.SetDark();
        dot.SetType(zoneGen.zoneType);
        if (source.ContainsKey(dot)) source[dot] = zoneGen;
        else source.Add(dot, zoneGen);

        if (!column_uses.ContainsKey(dot.stepNum)) column_uses.Add(dot.stepNum, new Dictionary<string, int>());

        if (!column_uses[dot.stepNum].ContainsKey(zoneGen.activationKey))
        {
            column_uses[dot.stepNum].Add(zoneGen.activationKey, 1);
        }
        column_uses[dot.stepNum][zoneGen.activationKey]++;
        if (!total_uses.ContainsKey(zoneGen.activationKey))
        {
            total_uses.Add(zoneGen.activationKey, 1);
        }
        total_uses[zoneGen.activationKey]++;

        if (invisDots.ContainsKey(dot))
        {
            var key = invisDots[dot];
            if (hiddenSections.ContainsKey(key))
            {
                hiddenSections[key].Remove(dot);
                if (dot.image != null) dot.image.enabled = true;
                if (dot.fgImage != null) dot.fgImage.enabled = true;
                if (dot.bgImage != null) dot.bgImage.enabled = true;
            }
        }
    }

    //================================================================================================================================================================================================================================================================================================

    public bool ApplyPostProcess(ZoneDot dot)
    {
        bool edited = false;
        foreach (var gen in postProcess)
        {
            if (AttemptGen(dot, gen))
            {
                gen.effect(dot);
                edited = true;
            }
        }
        foreach (ZoneDot otherDot in bar.currentZoneDots)
        {
            if (Connect(dot, otherDot)) edited = true;
        }
        return edited;
    }

    public ZoneDot CreateNextDot(ZoneDot dot, ZoneGenerator gen, bool add = true)
    {
        var newDot = CreateDot(dot.stepNum + 1, gen);
        if (add) AddAfter(newDot, dot);
        return newDot;
    }

    public ZoneDot CreatePrevDot(ZoneDot dot, ZoneGenerator gen, bool add = true)
    {
        var newDot = CreateDot(dot.stepNum + 1, gen);
        if (add) AddBefore(newDot, dot);
        return newDot;
    }

    public bool AddAfter(ZoneDot dot, ZoneDot root)
    {
        int b = 2;
        if (!dot.previousDots.Contains(root)) dot.previousDots.Add(root);
        else b--;
        if (!root.nextDots.Contains(dot)) root.nextDots.Add(dot);
        else b--;
        return b > 0;
    }

    public bool AddBefore(ZoneDot dot, ZoneDot root)
    {
        int b = 2;
        if (!root.previousDots.Contains(dot)) root.previousDots.Add(dot);
        else b--;
        if (!dot.nextDots.Contains(root)) dot.nextDots.Add(root);
        else b--;
        return b > 0;
    }

    public ZoneDot CreateDot(int index1, ZoneGenerator gen)
    {
        RectTransform rectTransform = columnTransforms[index1];
        ZoneDot zoneDot = UnityEngine.Object.Instantiate<ZoneDot>(bar.zoneDotPrefab, bar.transform.position, bar.transform.rotation, rectTransform.transform);
        zoneDot.stepNum = index1;
        zoneDot.worldBar = bar;
        zoneDot.idCtrl = bar.idCtrl;
        zoneDot.btnCtrl = bar.btnCtrl;
        zoneDot.transform.name = "ZoneDot - Step: " + (object)index1;
        zoneDot.verticalSpacing = bar.defaultVerticalSpacing + gen.verticalSpacing;
        var step = bar.currentZoneSteps[index1];
        if (!gen.addFirst) step.Add(zoneDot);
        else step.Insert(0, zoneDot);
        bar.currentZoneDots.Add(zoneDot);
        if (gen.relativeTransforms)
        {
            if (step.Count == 1)
            {
                zoneDot.transform.position = rectTransform.position;
            }
            else
            {
                if (gen.addFirst)
                {
                    zoneDot.transform.position = step[1].transform.position + new Vector3(0.0f, zoneDot.verticalSpacing * bar.rect.localScale.y, 0.0f);
                }
                else
                {
                    zoneDot.transform.position = step[step.Count - 2].transform.position - new Vector3(0.0f, zoneDot.verticalSpacing * bar.rect.localScale.y, 0.0f);
                }
            }
        }
        else
        {
            for (int i = 0; i < step.Count; i++)
            {
                var dot = step[i];
                dot.transform.localPosition = new Vector3(0.0f, ((float)(step.Count - 1) / 2f - (float)(i)) * zoneDot.verticalSpacing * rectTransform.localScale.y, 0.0f);
            }
        }


        if (gen.zoneType == ZoneType.World && gen is ManualZoneGenerator)
        {
            ManualZoneGenerator manual = (ManualZoneGenerator)gen;
            //Default world stuff
            if (manual.automaticWorldSelection)
            {
                if (stringList.Count > 0)
                {
                    int index3 = bar.runCtrl.NextWorldRand(0, stringList.Count);
                    zoneDot.worldName = stringList[index3];
                    stringList.Remove(stringList[index3]);
                }
                else
                {
                    if (bar.runCtrl.currentRun.bossExecutions >= 7)
                    {
                        zoneDot.worldName = "Genocide";
                        zoneDot.imageName = "WorldWasteland";
                    }
                    else if (bar.runCtrl.currentRun.bossExecutions >= 1)
                    {
                        zoneDot.worldName = "Normal";
                        zoneDot.imageName = "WorldWasteland";
                    }
                    else
                    {
                        zoneDot.worldName = "Pacifist";
                        zoneDot.imageName = "WorldWasteland";
                    }

                }
                zoneDot.world = bar.runCtrl.worlds[zoneDot.worldName];
                zoneDot.imageName = zoneDot.world.iconName;
            }

        }
        EditDotWithGen(zoneDot, gen);

        return zoneDot;
    }

    //================================================================================================================================================================================================================================================================================================

    public void Generate()
    {
        CURRENT = this;
        try
        {
            foreach (Component component in bar.zoneDotContainer)
            {
                UnityEngine.Object.Destroy((UnityEngine.Object)component.gameObject);
            }
            bar.currentZoneDots.Clear();
            bar.currentZoneSteps.Clear();
            if (bar.btnCtrl.hideUICounter < 1)
                bar.detailPanel.gameObject.SetActive(true);
            bar.runCtrl.currentRun.lastWorldGenOrigin = bar.runCtrl.currentRun.currentWorldGen;

            World world = bar.runCtrl.currentWorld;

            var manual = manualGeneration.ContainsKey(world.nameString) && manualGeneration[world.nameString];

            if (!manual)
            {
			    CustomWorldGenerator.AutoGeneration = true;
                return;
                /*
                bar.GenerateWorldBar(-666);
                foreach (var step in bar.currentZoneSteps)
                {
                    if (step.Count > 0)
                    {
                        columnTransforms.Add((RectTransform)step[0].transform.parent);
                    }
                    else
                    {
                        Debug.LogError("Failed to get transforms of dot columns in non manual world generation!");
                        return;
                    }
                }
                */
            }
            else
            {
                GetManualGenerators(world);
                RunManualGeneration();
            }

            world.numZones = bar.currentZoneSteps.Count - 1;

            RunPostProcessGenerators(manual);
        }
        catch (Exception e)
        {
            Debug.LogError("Exception created when running world generation: " + e.Message + "\n" + e.StackTrace);
        }
    }

    public void RunManualGeneration()
    {
        List<ZoneDot> previousDots = new List<ZoneDot>();

        column_uses.Clear();
        for (int i = 0; i < bar.currentZoneSteps.Count; i++) column_uses.Add(i, new Dictionary<string, int>());

        World world = bar.runCtrl.currentWorld;

        foreach (var gen in manualZoneGenerators)
        {
            if (total_uses.ContainsKey(gen.activationKey) && total_uses[gen.activationKey] >= gen.maxWorldActivations) continue;
            List<int> validColumns = gen.columns.Where((column) => (column_uses.ContainsKey(column) && !(column_uses[column].ContainsKey(gen.activationKey) && column_uses[column][gen.activationKey] < gen.maxColumnActivations))).ToList();
            while (validColumns.Count > 0)
            {
                var column = validColumns[0];
                var step = bar.currentZoneSteps[column];
                validColumns.RemoveAt(0);
                var basicWorldsUsed = false;
                while (AttemptGen(column, gen))
                {
                    ZoneDot dot = CreateDot(column, gen);
                    if (dot.type == ZoneType.World)
                    {
                        if (gen.automaticWorldSelection)
                        {
                            if (stringList.Count > 0)
                            {
                                basicWorldsUsed = true;
                            }
                            else if (basicWorldsUsed)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        foreach (ZoneDot dot in bar.currentZoneDots)
        {
            foreach (ZoneDot otherDot in bar.currentZoneDots)
            {
                Connect(dot, otherDot);
            }
        }

        Debug.Log("Manual generation complete.");
    }

    public bool Connect(ZoneDot dot, ZoneDot otherDot)
    {
        if (dot != otherDot)
        {
            var source1 = source.ContainsKey(dot) ? source[dot] : null;
            var source2 = source.ContainsKey(otherDot) ? source[otherDot] : null;
            bool b1 = false;
            if (source1 != null)
            {
                b1 = true;
                if (source1.connectConds.Count == 0) source1.AddDefaultConnectRules();
                foreach (var cond in source1.connectConds)
                {
                    if (!cond(dot, otherDot, source2 == null))
                    {
                        b1 = false;
                        break;
                    }
                }
            }
            bool b2 = false;
            if (source2 != null)
            {
                b2 = true;
                if (source2.connectConds.Count == 0) source2.AddDefaultConnectRules();
                foreach (var cond in source2.connectConds)
                {
                    if (!cond(otherDot, dot, source1 == null))
                    {
                        b2 = false;
                        break;
                    }
                }
            }
            if (b1 || b2)
            {
                if (dot.stepNum > otherDot.stepNum)
                {
                    return AddAfter(dot, otherDot);
                }
                else if (dot.stepNum < otherDot.stepNum)
                {
                    return AddBefore(dot, otherDot);
                }
            }
        }
        return false;
    }

    public void RunPostProcessGenerators(bool manual = false)
    {
        post = true;

        World world = bar.runCtrl.currentWorld;

        GetPostProcessGenerators(world);

        column_uses.Clear();
        for (int i = 0; i < bar.currentZoneSteps.Count; i++) column_uses.Add(i, new Dictionary<string, int>());

        //world.numZones = bar.currentZoneSteps.Count - 1;

        for (int j = 0; j < 4; j++)
        {
            bool b = false;

            for (int i = 0; i < bar.currentZoneDots.Count; i++)
            {
                ZoneDot dot = bar.currentZoneDots[i];
                b = ApplyPostProcess(dot) ? true : b;
            }

            if (!b)
            {
                break;
            }
        }

        foreach (var dot in bar.currentZoneDots)
        {
            dot.nextDots = dot.nextDots.OrderBy<ZoneDot, int>((Func<ZoneDot, int>)(t => t.transform.GetSiblingIndex())).ToList<ZoneDot>();
			for (int i = dot.nextLines.Count - 1; i >= 0; i--) {
				GameObject.Destroy(dot.nextLines[i].gameObject, 0);
				dot.nextLines.Remove(dot.nextLines[i]);
			}
			//dot.ClearLines();
			dot.CreateLines();
		}

        foreach (var dot in bar.currentZoneDots)
        {
            foreach (var otherDot in dot.nextDots)
            {
                if (!otherDot.previousDots.Contains(dot)) otherDot.previousDots.Add(dot);
            }
        }

        foreach (var dot in invisDots.Keys)
        {
            if (dot.image != null) dot.image.enabled = false;
            if (dot.fgImage != null) dot.fgImage.enabled = false;
            if (dot.bgImage != null) dot.bgImage.enabled = false;
            foreach (var oDot in dot.previousDots)
            {
                var index = oDot.nextDots.IndexOf(dot);
                if (index != -1)
                {
                    if (oDot.nextLines.Count > index)
                    {
                        var line = oDot.nextLines[oDot.nextDots.IndexOf(dot)];
                        line.GetComponent<Image>().enabled = false;
                        if (!invisLines.ContainsKey(dot)) invisLines.Add(dot, new Dictionary<ZoneDot, RectTransform>());
                        invisLines[dot].Add(oDot, line);
                        Debug.LogError("Added dot and line to invisLines");
                        oDot.nextDots.Remove(dot);
                        oDot.nextLines.Remove(line);
                    }
                    else
                    {
                        Debug.LogError("Dot Connection Line Error");
                    }
                }
                else
                {
                    Debug.LogError("PreviousDot - NextDot Mismatch");
                }

            }
        }

        if (bar.currentZoneSteps.Count > 0)
        {
            if (bar.currentZoneSteps[0].Count > 0)
            {
                bar.selectionMarker.transform.position = bar.currentZoneSteps[0][0].transform.position;
            }
            else
            {
                Debug.LogError("World Generation did not result in a starting ZoneDot");
            }
        }
        else
        {
            Debug.LogError("World Generation did not result in any zone steps (columns)");
        }

    }

    //================================================================================================================================================================================================================================================================================================

    public class ZoneGenerator
    {
        public List<Func<ZoneDot, ZoneDot, bool, bool>> connectConds = new List<Func<ZoneDot, ZoneDot, bool, bool>>();

        public ZoneType zoneType = ZoneType.Battle;
        public string activationKey = "";
        public int maxColumnActivations = 1;
        public int maxWorldActivations = int.MaxValue;

        public List<int> columns = new List<int>();

        public float verticalSpacing;

        //World
        public string worldName = "Genocide";
        public bool refreshCurrentworld = false;

        public bool dark = false;

        public bool relativeTransforms = false;

        public bool addFirst = false;

        public virtual void SetWorldData(string nameString, bool reAdd)
        {
            zoneType = ZoneType.World;
            worldName = nameString;
            refreshCurrentworld = reAdd;
        }

        public virtual void SetWorldDataMirror(bool reAdd)
        {
            zoneType = ZoneType.World;
            worldName = S.I.batCtrl.currentPlayer.beingObj.nameString;
            refreshCurrentworld = reAdd;
        }

        public void AddColumnToRange(int i)
        {
            columns.Add(i);
        }

        public void AddAllColumns()
        {
            for (int i = 0; i < 100; i++)
            {
                columns.Add(i);
            }

        }

        public virtual bool CanActivate(ZoneDot dot)
        {
            return columns.Contains(dot.stepNum);
        }

        public void Add1ColumnConnectRule()
        {
            connectConds.Add((dot, otherDot, otherDefault) =>
            {
                return Math.Abs(dot.stepNum - otherDot.stepNum) == 1;
            });
        }

        public void AddKeysConnectRule(IEnumerable<string> keys)
        {
            connectConds.Add((dot, otherDot, otherDefault) =>
            {
                if (!otherDefault)
                {
                    var source2 = CustomWorldGenerator.CURRENT.source[otherDot];
                    return keys.Contains(source2.activationKey);
                }
                else
                {
                    return true;
                }
            });
        }

        public void AddKeysBlacklistConnectRule(IEnumerable<string> keys)
        {
            connectConds.Add((dot, otherDot, otherDefault) =>
            {
                if (!otherDefault)
                {
                    var source2 = CustomWorldGenerator.CURRENT.source[otherDot];
                    return !keys.Contains(source2.activationKey);
                }
                else
                {
                    return true;
                }
            });
        }

        public void AddNoDefaultConnectRule()
        {
            connectConds.Add((dot, otherDot, otherDefault) =>
            {
                return !otherDefault;
            });
        }

        public void NoIntendedConnectRule()
        {
            connectConds.Add((dot, otherDot, otherDefault) =>
            {
                return false;
            });
        }

        public virtual void AddDefaultConnectRules()
        {
            Add1ColumnConnectRule();
            AddNoDefaultConnectRule();
        }

        public void AddMultiColumnConnectRule(IEnumerable<int> columns)
        {
            connectConds.Add((dot, otherDot, otherDefault) =>
            {
                return columns.Contains(otherDot.stepNum);
            });
        }

        //EXPERIMENTAL
        public void SetLuaConnectRule(Closure function)
        {
            connectConds.Add((dot, otherDot, otherDefault) =>
            {
                try
                {
                    var result = function.Call(this, dot, otherDot, otherDefault);
                    return result.Boolean;
                }
                catch (Exception e)
                {
                    Debug.LogError("ZoneGenerator Lua Connect Condition call exception: " + e.Message);
                    return false;
                }
            });
        }
    }

    public class ManualZoneGenerator : ZoneGenerator
    {
        public bool automaticWorldSelection = true;

        public override void SetWorldData(string nameString, bool reAdd)
        {
            base.SetWorldData(nameString, reAdd);
            automaticWorldSelection = false;
        }

        public override void SetWorldDataMirror(bool reAdd)
        {
            base.SetWorldDataMirror(reAdd);
            automaticWorldSelection = false;
        }
    }

    public class PostProcessZoneGenerator : ZoneGenerator
    {
        public float probability = 100f;
        public float probabilityReductionFactor = 1.0f;
        public bool probabilityReductionOnFail = false;
        public bool updateTransform = true;

        public Predicate<ZoneDot> cond = (dot) => (true);
        public Func<ZoneDot, ZoneDot> effect = (dot) =>
        {
            Debug.LogError(dot.transform.name + " has been effected by a generator with no effect.");
            return null;
        };

        public PostProcessZoneGenerator()
        {
            relativeTransforms = true;
        }

        public override bool CanActivate(ZoneDot dot)
        {
            return base.CanActivate(dot) && cond(dot);
        }

        public void SetReplace(ZoneType type)
        {
            cond = (dot) => (dot.type == type);
            effect = (dot) =>
            {
                CustomWorldGenerator.CURRENT.EditDotWithGen(dot, this);
                return dot;
            };
        }

        public void SetReplaceAfter(ZoneType replaceType, ZoneType afterType)
        {
            cond = (dot) => (dot.type == replaceType && dot.previousDots.Where(otherDot => (otherDot.type == afterType)).Count() > 0);
            effect = (dot) =>
            {
                CustomWorldGenerator.CURRENT.EditDotWithGen(dot, this);
                return dot;
            };
        }

        public void SetReplaceBefore(ZoneType replaceType, ZoneType beforeType)
        {
            cond = (dot) => (dot.type == replaceType && dot.nextDots.Where(otherDot => (otherDot.type == beforeType)).Count() > 0);
            effect = (dot) =>
            {
                CustomWorldGenerator.CURRENT.EditDotWithGen(dot, this);
                return dot;
            };
        }

        public void SetAddAfter(ZoneType afterType, bool addToNext = true)
        {
            cond = (dot) => (dot.type == afterType);
            effect = (dot) =>
            {
                return CustomWorldGenerator.CURRENT.CreateNextDot(dot, this, addToNext);
            };
        }

        public void SetAddBefore(ZoneType beforeType, bool addToPrev = true)
        {
            cond = (dot) => (dot.type == beforeType);
            effect = (dot) =>
            {
                return CustomWorldGenerator.CURRENT.CreatePrevDot(dot, this, addToPrev);
            };
        }

        public void SetInvisible(string activationKey, string sectionKey)
        {
            cond = (dot) =>
            {
                var source = CustomWorldGenerator.CURRENT.source.ContainsKey(dot) ? CustomWorldGenerator.CURRENT.source[dot] : null;
                return source != null && source.activationKey == activationKey && !(invisDots.ContainsKey(dot) && invisDots[dot] == activationKey);
            };
            effect = (dot) =>
            {
                if (!hiddenSections.ContainsKey(sectionKey)) hiddenSections.Add(sectionKey, new List<ZoneDot>());
                hiddenSections[sectionKey].Add(dot);
                if (!invisDots.ContainsKey(dot)) invisDots.Add(dot, sectionKey);
                else invisDots[dot] = sectionKey;
                return dot;
            };
        }

        //EXPERIMENTAL
        public void SetLuaCondition(Closure function)
        {
            cond = (dot) =>
            {
                try
                {
                    var result = function.Call(this, dot);
                    return result.Boolean;
                }
                catch (Exception e)
                {
                    Debug.LogError("PostProcessZoneGenerator Lua Condition call exception: " + e.Message);
                    return false;
                }
            };
        }

        //EXPERIMENTAL
        public void SetLuaEffect(Closure function)
        {
            effect = (dot) =>
            {
                try
                {
                    function.Call(this, dot);
                    return dot;
                }
                catch (Exception e)
                {
                    Debug.LogError("PostProcessZoneGenerator Lua Condition call exception: " + e.Message);
                    return dot;
                }
            };
        }
    }

    //=========================================LUA================================================================================================================================================================================================================

    public int NumOfColumns()
    {
        return bar.currentZoneSteps.Count;
    }


    public ManualZoneGenerator CreateManualGenerator()
    {
        var gen = new ManualZoneGenerator();
        manualZoneGenerators.Add(gen);
        return gen;
    }

    public void CreateColumns(int num)
    {
        if (!post || true)
        {
            for (int i = 0; i < num; i++)
            {
                CreateColumn();
            }
        }
    }

    public void CreateColumn()
    {

        RectTransform rectTransform = new GameObject("ZoneStep").AddComponent<RectTransform>();
        Vector3 vector3 = bar.zoneDotContainer.transform.position - new Vector3((float)((double)bar.width / 2.0 - (double)bar.width / 6 * (double)bar.currentZoneSteps.Count) * bar.zoneDotContainer.lossyScale.x, 0.0f, 0.0f);
        rectTransform.localScale = bar.zoneDotContainer.lossyScale;
        rectTransform.SetParent(bar.zoneDotContainer, true);
        rectTransform.position = vector3;
        rectTransform.sizeDelta = new Vector2(10f, 100f);
        columnTransforms.Add(rectTransform);
        bar.currentZoneSteps.Add(new List<ZoneDot>());

    }

    public void InsertNewColumn(int index)
    {

        RectTransform rectTransform = new GameObject("ZoneStep").AddComponent<RectTransform>();
        Vector3 vector3 = bar.zoneDotContainer.transform.position - new Vector3((float)((double)bar.width / 2.0 - (double)bar.width / 6 * (double)index) * bar.zoneDotContainer.lossyScale.x, 0.0f, 0.0f);
        rectTransform.localScale = bar.zoneDotContainer.lossyScale;
        rectTransform.SetParent(bar.zoneDotContainer, true);
        rectTransform.transform.position = vector3;
        rectTransform.sizeDelta = new Vector2(10f, 100f);
        for (int i = index; i < bar.currentZoneSteps.Count; i++)
        {
            var step = bar.currentZoneSteps[i];
            var transform = columnTransforms[i];
            transform.localScale = bar.zoneDotContainer.lossyScale;
            transform.position = transform.position + new Vector3(((float)bar.width / 6f * (float)1) * bar.zoneDotContainer.lossyScale.x, 0.0f, 0.0f);
            transform.sizeDelta = new Vector2(10f, 100f);
            foreach (ZoneDot zoneDot in step)
            {
                zoneDot.stepNum = zoneDot.stepNum + 1;
                zoneDot.transform.position = transform.position + new Vector3(0.0f, ((float)(step.Count - 1) / 2f - (float)(step.IndexOf(zoneDot) - 1)) * zoneDot.verticalSpacing * bar.rect.localScale.y, 0.0f);
            }
        }
        columnTransforms.Insert(index, rectTransform);
        bar.currentZoneSteps.Insert(index, new List<ZoneDot>());

    }

    public PostProcessZoneGenerator CreatePostProcessGenerator()
    {
        var gen = new PostProcessZoneGenerator();
        postProcess.Add(gen);
        return gen;
    }
}

