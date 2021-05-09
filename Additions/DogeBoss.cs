using HarmonyLib;
using MEC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;

public class DogeBoss : Boss
{

    public override void Activate()
    {
        //Debug.Log("DogeBoss: Activate");
        enabled = true;
        base.Activate();
    }

    public override void Start()
    {
        var music = DogeBossData.boss_music.ContainsKey(beingObj.beingID) ? DogeBossData.boss_music[beingObj.beingID] : null;
        var AllAudioClips = Traverse.Create(S.I.itemMan).Field("allAudioClips").GetValue<Dictionary<string, AudioClip>>();
        if (music == null || !AllAudioClips.ContainsKey(music.musicID))
        {
            Debug.Log("Boss music for " + beingObj.beingID + " does not exist");
        }
        else
        {
            S.I.muCtrl.Stop();
            float introBoundary = music.introBoundary;
            float endBoundary = music.endBoundary;
            LuaPowerSound.PlayCustomMusicIntroLoop(music.musicID, introBoundary, endBoundary);
        }
        base.Start();
    }

    public IEnumerator _DialogueC(string line)
    {
        //Debug.Log("DogeBoss: _DialogueC");
        talkBubble.Show();
        yield return new WaitForSeconds(talkBubble.AnimateText(line));
        talkBubble.Hide();
    }

    public override IEnumerator DownC(bool destroyStructures = true, bool showZoneButtons = true)
    {
        S.I.muCtrl.PauseIntroLoop();
        S.I.muCtrl.Stop();
        yield return base.DownC(destroyStructures, showZoneButtons);
    }

    protected override void LastWord()
    {
        //Debug.Log("DogeBoss: LastWord");
        talkBubble.StopAllCoroutines();
        if (DogeBossData.killed_lines.ContainsKey(beingObj.beingID))
        {
            //Debug.Log("DogeBoss: Playing last word");
            talkBubble.Show(true);
            S.I.PlayOnce(talkBubble.vocSynth);
            talkBubble.SetText(DogeBossData.GetRandomBossLine(DogeBossData.killed_lines, beingObj.beingID, S.I.batCtrl.currentHeroObj.nameString));
        }
        DownEffects();
    }

    public override IEnumerator Executed()
    {
        DogeBoss boss = this;
        //Debug.Log("DogeBoss: Executed");
        var doesnt_count = DogeBossData.kill_not_counted.Contains(beingObj.beingID);
        if (!doesnt_count) boss.ctrl.IncrementStat("TotalExecutions");
        if (!doesnt_count) boss.runCtrl.currentRun.RemoveAssist(beingObj);
        SetCustomBossFate(false);
        boss.runCtrl.progressBar.SetBossFate();
        if (boss.endGameOnExecute)
            boss.ctrl.AddObstacle((Being)this);
        boss.LastWord();
        boss.ctrl.runCtrl.worldBar.Close();
        boss.ctrl.idCtrl.HideOnwardButton();
        boss.runCtrl.worldBar.available = false;
        foreach (Player currentPlayer in boss.ctrl.currentPlayers)
            currentPlayer.RemoveStatus(Status.Poison);
        yield return (object)new WaitForEndOfFrame();
        boss.deCtrl.TriggerAllArtifacts(FTrigger.OnBossKill);
        //talkBubble.Fade();
        //talkBubble.anim.SetTrigger("fade");
        if (!doesnt_count) ++boss.runCtrl.currentRun.bossExecutions;
        boss.DeathEffects(false);
        S.I.StartCoroutine(boss._DeathFinalNoMEC());
    }

    protected override IEnumerator SpareC(ZoneDot nextZoneDot)
    {
        SetCustomBossFate(true);
        return base.SpareC(nextZoneDot);
    }

    // The killed / spared icon on the map is dependent on whether or not you have "Boss[WorldName]" as a key in the current run Assists list
    // This spoofs that key on spare so the world bar is updated correctly. Fortunately it doesn't cause any problems with spawning assists.
    private void SetCustomBossFate(bool spared)
    {
        if (spared && runCtrl.currentZoneDot.type == ZoneType.Boss)
        {
            var worldNameKey = "Boss" + runCtrl.currentRun.visitedWorldNames[this.runCtrl.currentRun.visitedWorldNames.Count - 1];
            if(!runCtrl.currentRun.HasAssist(worldNameKey))
            {
                runCtrl.currentRun.assists.Add(worldNameKey, false);
            }
        }
    }

    //_DeathFinal without MEC Wait coroutines, which for whatever reason (possibly they use some kind of internal timer MPL cant access)
    // complete instantly, instead of the Unity Waits that actually pause for the given amount of time
    public IEnumerator _DeathFinalNoMEC()
    {
        //Debug.Log("DogeBoss : _DeathFinal");
        Being being = this;
        being.inDeathSequence = true;
        being.sp.explosionGen.CreateExplosionString(1, being.transform.position, being.transform.rotation, being);
        S.I.PlayOnce(being.explosionSound, being.IsReference());
        if ((bool)(UnityEngine.Object)being.talkBubble)
            being.talkBubble.Fade();
        yield return float.NegativeInfinity;
        being.Remove();
        System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        stopWatch.Start();
        yield return new WaitForSeconds(0.2f);
        being.inDeathSequence = false;
        being.spriteRend.enabled = false;
        being.shadow.SetActive(false);
        yield return new WaitForSeconds(0.6f);
        if ((bool)(UnityEngine.Object)being.talkBubble)
        {
            UnityEngine.Object.Destroy((UnityEngine.Object)being.talkBubble.gameObject);
        }
        stopWatch.Stop();
    }

    public override IEnumerator Loop()
    {
        if(DogeBossData.intro_lines.ContainsKey(bossID))
        {
            yield return StartCoroutine(_StartDialogue("Intro"));
        }
        yield return base.Loop();
    }


    //Override of ExecutePlayerC allowing for custom executions
    //Moves the boss a configurable number of tiles in front of the player
    //Casts a pre-execution spell if the boss has one, allowing for visual effects, anim changes, etc.
    //Casts spell listed in the XML as its execution
    //If the execution fails, the boss will continue its normal loop, hopefully killing the player
    public override IEnumerator ExecutePlayerC()
    {
        var executionExists = DogeBossData.execution_spells.ContainsKey(bossID);
        if (executionExists)
        {
            DogeBoss dogeBoss = this;
            CustomExecution execution = DogeBossData.execution_spells[beingObj.beingID][UnityEngine.Random.Range(0, DogeBossData.execution_spells[beingObj.beingID].Count())];
            yield return (object)new WaitForSeconds(0.2f);
            dogeBoss.ResetAnimTriggers();
            dogeBoss.transform.right = Vector3.left;
            yield return (object)new WaitForSeconds(0.4f);
            dogeBoss.mov.MoveToTile(dogeBoss.ctrl.currentPlayer.TileLocal(execution.tile), true, false);
            yield return (object)new WaitWhile(new Func<bool>(() => dogeBoss.mov.state == State.Moving));
            yield return (object)new WaitForSeconds(0.2f);
            if(execution.preExecutionSpellID != null)
            {
                var preExecutionSpell = deCtrl.CreateSpellBase(execution.preExecutionSpellID, this, false);
                preExecutionSpell.StartCast(false, 0, false);
            }
            yield return (object)new WaitForSeconds(0.2f);
            yield return (object)dogeBoss.StartCoroutine(dogeBoss._StartDialogue("Execution"));
            if ((bool)(UnityEngine.Object)dogeBoss.ctrl.currentPlayer && dogeBoss.ctrl.currentPlayer.downed)
            {
                yield return (object)new WaitForSeconds(0.4f);
                var executionSpell = deCtrl.CreateSpellBase(execution.spellID, this, false);
                if(execution.preExecutionSpellID == null) anim.SetTrigger("spellCast");
                executionSpell.StartCast(false, 0, false);
                yield return (object)new WaitForSeconds(2f);
                if ((bool)(UnityEngine.Object)dogeBoss.ctrl.currentPlayer && dogeBoss.ctrl.currentPlayer.downed)
                {
                    dogeBoss.mov.MoveToTile(dogeBoss.ctrl.currentPlayer.TileLocal(execution.tile), true, false);
                    yield return (object)new WaitWhile(new Func<bool>(() => dogeBoss.mov.state == State.Moving));
                    yield return (object)new WaitForSeconds(0.4f);
                }
            }
            if (dogeBoss.ctrl.PlayersActive())
                dogeBoss.StartCoroutine(dogeBoss.StartLoopC());
        }
        else
        {
            Debug.Log("No execution spell found");
            yield return base.ExecutePlayerC();
        }
    }
}

[HarmonyPatch]
static class DogeBoss_Patches
{
    private static Dictionary<string, GameObject> customBosses = new Dictionary<string, GameObject>();

    //Reads the "Boss" tag affixed to beings as the indicator to add them to the boss list
    [HarmonyPostfix]
    [HarmonyPatch(typeof(BeingObject), nameof(BeingObject.ReadXmlPrototype))]
    static void ReadXmlPrototype(ref BeingObject __instance)
    {
        if(__instance.tags.Contains(Tag.Boss) && !customBosses.ContainsKey(__instance.beingID))
        {
            //Debug.Log("Found boss tag, adding custom boss to allBosses"); 
            GameObject customBoss = new GameObject();
            customBoss.AddComponent<DogeBoss>();
            Boss bossComponent = customBoss.GetComponent<Boss>();
            bossComponent.bossID = __instance.beingID;
            bossComponent.enabled = false;
            customBosses.Add(__instance.beingID, customBoss);
            Debug.Log("Added " + bossComponent.bossID + " to customBosses");
            //foreach (var bossObj in customBosses.Values.ToList())
            //{
            //    foreach (var component in bossObj.GetComponents(typeof(Component)))
            //    {
            //        Debug.Log(component.GetType());
            //    }
            //}
        }
    }

    //Adds custom bosses to bossDictionary to have the game recognize them as bosses for things like Player.DownC()
    [HarmonyPrefix]
    [HarmonyPatch(typeof(RunCtrl), nameof(RunCtrl.StartCampaign))]
    static void StartCampaign(ref RunCtrl __instance, ref SpawnCtrl ___spCtrl)
    {
        Debug.Log("Adding custom bosses to boss dictionary");
        foreach (GameObject customBoss in customBosses.Values.ToList())
        {
            if(!___spCtrl.bossDictionary.ContainsKey(customBoss.GetComponent<Boss>().bossID))
            {
                ___spCtrl.bossDictionary.Add(customBoss.GetComponent<Boss>().bossID, customBoss);
            }
        }
    }

    [HarmonyPatch]
    static class DogeBoss_MiscPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Boss), nameof(Boss._StartDialogue))]
        static bool _StartDialogue(Boss __instance, string key, ref IEnumerator __result)
        {
            if (__instance is DogeBoss)
            {
                //Debug.Log("DogeBoss: _StartDialogue patch");
                //Debug.Log("Playing custom " + key + " line");
                switch (key)
                {

                    case "Intro":
                        if (DogeBossData.intro_lines.ContainsKey(__instance.beingObj.beingID))
                        {
                            __result = ((DogeBoss)__instance)._DialogueC(DogeBossData.GetRandomBossLine(DogeBossData.intro_lines, __instance.beingObj.beingID, S.I.batCtrl.currentHeroObj.nameString));
                        }
                        break;
                    case "Execution":
                        if (DogeBossData.execution_lines.ContainsKey(__instance.beingObj.beingID))
                        {
                            __result = ((DogeBoss)__instance)._DialogueC(DogeBossData.GetRandomBossLine(DogeBossData.execution_lines, __instance.beingObj.beingID, S.I.batCtrl.currentHeroObj.nameString));
                        }
                        break;
                    case "Spare":
                        if (DogeBossData.spare_lines.ContainsKey(__instance.beingObj.beingID))
                        {
                            __result = ((DogeBoss)__instance)._DialogueC(DogeBossData.GetRandomBossLine(DogeBossData.spare_lines, __instance.beingObj.beingID, S.I.batCtrl.currentHeroObj.nameString));
                        }
                        break;
                    case "Downed":
                        if (DogeBossData.defeated_lines.ContainsKey(__instance.beingObj.beingID))
                        {
                            __result = ((DogeBoss)__instance)._DialogueC(DogeBossData.GetRandomBossLine(DogeBossData.defeated_lines, __instance.beingObj.beingID, S.I.batCtrl.currentHeroObj.nameString));
                        }
                        break;
                    case "Flawless":
                        if (DogeBossData.perfect_lines.ContainsKey(__instance.beingObj.beingID))
                        {
                            __result = ((DogeBoss)__instance)._DialogueC(DogeBossData.GetRandomBossLine(DogeBossData.perfect_lines, __instance.beingObj.beingID, S.I.batCtrl.currentHeroObj.nameString));
                        }
                        break;
                    case "Mercy":
                        if (DogeBossData.mercy_lines.ContainsKey(__instance.beingObj.beingID))
                        {
                            __result = ((DogeBoss)__instance)._DialogueC(DogeBossData.GetRandomBossLine(DogeBossData.mercy_lines, __instance.beingObj.beingID, S.I.batCtrl.currentHeroObj.nameString));
                        }
                        break;
                    default:
                        __result = ((DogeBoss)__instance)._DialogueC("I AM ERROR");
                        break;
                }
                if(__result == null)
                {
                    Debug.Log("Found no " + key + " line");
                    __result = ((DogeBoss)__instance)._DialogueC(__instance.bossID + "/" + key);
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        //inserts a function call before the switch statement in the function in order to nab the xml without destroying it.
        [HarmonyPatch(typeof(BeingObject))]
        [HarmonyPatch("ReadXmlPrototype")]
        static class CustomBoss_ReadXmlPrototype
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                bool after_virtual = false;
                bool done = false;
                var to_call = AccessTools.Method(typeof(DogeBoss_Read), nameof(DogeBoss_Read.Switch));
                var reader_code = OpCodes.Ldloc_0;
                var success = false;
                CodeInstruction prev = null;
                foreach (CodeInstruction instruction in instructions)
                {

                    yield return instruction;
                    if (!done && after_virtual && (instruction.opcode == reader_code))
                    {
                        Debug.Log("Custom Boss XML Reader Transpiler Success!");
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, to_call);
                        yield return new CodeInstruction(reader_code);
                        success = true;
                        done = true;
                    }
                    if (!done && instruction.opcode == OpCodes.Callvirt && (MethodInfo)instruction.operand == AccessTools.PropertyGetter(typeof(XmlReader), "IsEmptyElement"))
                    {
                        after_virtual = true;
                        reader_code = prev.opcode;
                    }
                    prev = instruction;
                }

                if (!success)
                {
                    Debug.LogError("Custom Boss XML Reader Transpiler FAILURE!!! This is likely the result of an update. Please inform the makers of MoreLuaPower");
                }
            }

        }


        static class DogeBoss_Read
        {

            public static void Switch(XmlReader reader, BeingObject beingObj)
            {
                switch (reader.Name)
                {
                    case "Music":
                        if (!DogeBossData.boss_music.ContainsKey(beingObj.beingID))
                        {
                            DogeBossData.boss_music.Add(beingObj.beingID, new CustomBossMusic(reader));
                        } else
                        {
                            Debug.LogError("Boss " + beingObj.beingID + " can only have one boss music track");
                        }
                        break;
                    case "IntroLine":
                        AddBossLine(ref DogeBossData.intro_lines, beingObj.beingID, reader);
                        break;
                    case "ExecutionLine":
                        AddBossLine(ref DogeBossData.execution_lines, beingObj.beingID, reader);
                        break;
                    case "SparedLine":
                        AddBossLine(ref DogeBossData.spare_lines, beingObj.beingID, reader);
                        break;
                    case "DownedLine":
                        AddBossLine(ref DogeBossData.defeated_lines, beingObj.beingID, reader);
                        break;
                    case "FlawlessLine":
                        AddBossLine(ref DogeBossData.perfect_lines, beingObj.beingID, reader);
                        break;
                    case "KilledLine":
                        AddBossLine(ref DogeBossData.killed_lines, beingObj.beingID, reader);
                        break;
                    case "MercyLine":
                        AddBossLine(ref DogeBossData.mercy_lines, beingObj.beingID, reader);
                        break;
                    case "ExecutionSpell":
                        if (!DogeBossData.execution_spells.ContainsKey(beingObj.beingID))
                        {
                            DogeBossData.execution_spells.Add(beingObj.beingID, new List<CustomExecution>());
                        }
                        var newExecution = new CustomExecution(reader);
                        DogeBossData.execution_spells[beingObj.beingID].Add(newExecution);
                        break;
                    //case "Mercy":
                    //    if (!CustomBossIndex.mercy.ContainsKey(beingObj.beingID)) CustomBossIndex.mercy.Add(beingObj.beingID, reader.ReadElementContentAsBoolean());
                    //    break;
                    case "DontCountKill":
                        if (reader.ReadElementContentAsBoolean())
                        {
                            DogeBossData.kill_not_counted.Add(beingObj.beingID);
                        }
                        break;
                }
            }

            public static void AddBossLine(ref Dictionary<string, Dictionary<string, List<string>>> lines, string beingID, XmlReader reader)
            {
                if (!lines.ContainsKey(beingID))
                {
                    lines.Add(beingID, new Dictionary<string, List<string>>());
                }

                var bossLines = lines[beingID];
                var characterAttrb = reader.GetAttribute("character");
                if (characterAttrb == null)
                {
                    characterAttrb = "";
                }
                if (!bossLines.ContainsKey(characterAttrb))
                {
                    bossLines.Add(characterAttrb, new List<string>());
                }

                bossLines[characterAttrb].Add(reader.ReadElementContentAsString());
            }
        }
    }
}

public class CustomExecution
{
    public string spellID;
    public string preExecutionSpellID;
    public int tile = 4;

    public CustomExecution (XmlReader reader)
    {
        var tileAttrb = reader.GetAttribute("tile");
        var preExecutionAttrb = reader.GetAttribute("preExecution");
        if(tileAttrb != null)
        {
            if(!int.TryParse(tileAttrb, out tile))
            {
                Debug.Log("Boss execution \"tile\" attribute is not an integer");
            }
        }

        if (preExecutionAttrb != null)
        {
            preExecutionSpellID = preExecutionAttrb;
        }
        spellID = reader.ReadElementContentAsString();
    }
}

public class CustomBossMusic
{
    public string musicID;
    public float introBoundary = 0;
    public float endBoundary = 300;

    public CustomBossMusic (XmlReader reader)
    {
        var introBoundaryAttrb = reader.GetAttribute("introBoundary");
        var endBoundaryAttrb = reader.GetAttribute("endBoundary");
        if (introBoundaryAttrb != null)
        {
            if (!float.TryParse(introBoundaryAttrb, out introBoundary))
            {
                Debug.Log("Boss music \"introBoundary\" attribute is not a float");
            }
        }

        if (endBoundaryAttrb != null)
        {
            if (!float.TryParse(endBoundaryAttrb, out endBoundary))
            {
                Debug.Log("Boss music \"endBoundary\" attribute is not a float");
            }
        }

        musicID = reader.ReadElementContentAsString();
    }
}

static class DogeBossData
{
    public static Dictionary<string, CustomBossMusic> boss_music = new Dictionary<string, CustomBossMusic>();

    public static Dictionary<string, Dictionary<string,List<string>>> intro_lines = new Dictionary<string, Dictionary<string, List<string>>>();

    public static Dictionary<string, Dictionary<string, List<string>>> execution_lines = new Dictionary<string, Dictionary<string, List<string>>>();

    public static Dictionary<string, Dictionary<string, List<string>>> spare_lines = new Dictionary<string, Dictionary<string, List<string>>>();

    public static Dictionary<string, Dictionary<string, List<string>>> defeated_lines = new Dictionary<string, Dictionary<string, List<string>>>();

    public static Dictionary<string, Dictionary<string, List<string>>> perfect_lines = new Dictionary<string, Dictionary<string, List<string>>>();

    public static Dictionary<string, Dictionary<string, List<string>>> killed_lines = new Dictionary<string, Dictionary<string, List<string>>>();

    public static Dictionary<string, Dictionary<string, List<string>>> mercy_lines = new Dictionary<string, Dictionary<string, List<string>>>();

    public static Dictionary<string, List<CustomExecution>> execution_spells = new Dictionary<string, List<CustomExecution>>();

    //public static Dictionary<string, bool> mercy = new Dictionary<string, bool>();

    public static List<string> kill_not_counted = new List<string>();

    //public static Dictionary<string, bool> worlds_spared = new Dictionary<string, bool>();

    static public void Setup()
    {
        boss_music.Clear();
        intro_lines.Clear();
        execution_lines.Clear();
        spare_lines.Clear();
        defeated_lines.Clear();
        perfect_lines.Clear();
        killed_lines.Clear();
        mercy_lines.Clear();
        kill_not_counted.Clear();
        //worlds_spared.Clear();
        //mercy.Clear();
    }

    // Helper method that returns a boss' tier, or if the being isn't a boss, what the boss tier would be currently
    static public int GetBossTier(Being being)
    {
        if (being is Boss b)
        {
            return b.tier;
        }

        int tier = being.ctrl.baseBossTier;
        for (int index = 0; index < S.I.runCtrl.currentRun.worldTierNum; index += 2)
            ++tier;
        if (S.I.EDITION == Edition.Dev && S.I.BOSS_TEST_MODE)
            tier = 0;

        tier = Mathf.Clamp(tier, 0, 6);
        return tier;
    }

    public static string GetRandomBossLine(Dictionary<string, Dictionary<string, List<string>>> lines, string beingID, string characterID)
    {
        if (!lines.ContainsKey(beingID))
        {
            Debug.Log("Missing line for custom boss " + beingID);
            return "";
        }

        var bossLines = lines[beingID];
        if (!bossLines.ContainsKey(characterID))
        {
            if(!bossLines.ContainsKey(""))
            {
                Debug.Log("Missing line for custom boss " + beingID);
                return "";
            }

            return bossLines[""][UnityEngine.Random.Range(0, bossLines[""].Count())];
        }

        return bossLines[characterID][UnityEngine.Random.Range(0, bossLines[characterID].Count())];
    }
}