using HarmonyLib;
using MoonSharp.Interpreter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Xml;
using UnityEngine;

namespace CustomBosses
{
    public class CustomBoss : Boss
    {

        public bool serif_mode;

        public bool lock_facing_direction = false;

        public override void Start()
        {

            var music = CustomBossIndex.boss_music.ContainsKey(beingObj.beingID) ? CustomBossIndex.boss_music[beingObj.beingID] : "";
            var AllAudioClips = Traverse.Create(S.I.itemMan).Field("allAudioClips").GetValue<Dictionary<string, AudioClip>>();
            if (!AllAudioClips.ContainsKey(music))
            {
                Debug.Log(music + " does not exist");
            }
            else
            {
                S.I.muCtrl.Stop();
                S.I.muCtrl.Play(AllAudioClips[music], true);
            }

            healthStages = new List<int>();
            Debug.Log("Custom Boss Start");
            if (CustomBossIndex.final_bosses.Contains(beingObj.beingID)) endGameOnExecute = true;
            base.Start();
            if (CustomBossIndex.serif_mode_bosses.Contains(beingObj.beingID))
            {
                battleGrid.SetSerif();
                serif_mode = true;
            }

            if (CustomBossIndex.face_right.Contains(beingObj.beingID))
            {
                transform.right = Vector3.zero - Vector3.left;
            }
        }

        protected override void Update()
        {
            base.Update();
            if (serif_mode)
            {
                foreach (Player currentPlayer in ctrl.currentPlayers)
                {
                    if ((bool)(UnityEngine.Object)currentPlayer)
                    {
                        if (currentPlayer.mov.currentTile.x > mov.currentTile.x)
                        {
                            if (!lock_facing_direction)
                                transform.right = Vector3.zero - Vector3.left;
                            currentPlayer.transform.right = Vector3.zero - Vector3.right;
                            transform.right = Vector3.zero - Vector3.left;
                            foreach (Component currentPet in currentPlayer.currentPets)
                                currentPet.transform.right = Vector3.zero - Vector3.right;
                        }
                        else if (currentPlayer.mov.currentTile.x < mov.currentTile.x)
                        {
                            if (!lock_facing_direction)
                                transform.right = Vector3.zero - Vector3.right;
                            currentPlayer.transform.right = Vector3.zero - Vector3.left;
                            transform.right = Vector3.zero - Vector3.right;
                            foreach (Component currentPet in currentPlayer.currentPets)
                                currentPet.transform.right = Vector3.zero - Vector3.left;
                        }
                    }
                }
            }
        }

        public override void ExecutePlayer()
        {
            //ChangeState(beingObj.beingID + "_Execute");
            ClearProjectiles();
            ctrl.DestroyEnemiesAndStructures(this);
            if (downed || dontExecutePlayer)
                return;
            dontInterruptAnim = false;
            dontInterruptChannelAnim = false;
            dontHitAnim = false;
            ApplyStun(false, true, true);
            mov.lerpTimeMods.Clear();
            battleGrid.FixAllTiles();
            anim.SetTrigger("toIdle");
            if (!(CustomBossIndex.mercy.ContainsKey(beingObj.beingID) && !CustomBossIndex.mercy[beingObj.beingID]) && UnityEngine.Random.Range(1, 101) > beingObj.lethality + runCtrl.currentRun.hostagesKilled + runCtrl.currentRun.worldTierNum + runCtrl.currentRun.bossExecutions * 2)
            {
                StartCoroutine(_Mercy());
            }
            else
            {
                //this.attemptedToExecute = true;
                StartCoroutine(ExecutePlayerC());
            }
        }

        public IEnumerator _DialogueC(string line)
        {
            talkBubble.Show();
            yield return new WaitForSeconds(talkBubble.AnimateText(line));
            talkBubble.Hide();
        }

        protected override void LastWord()
        {
            talkBubble.StopAllCoroutines();
            talkBubble.Show();
            if (CustomBossIndex.genocide_lines.ContainsKey(beingObj.beingID))
            {
                talkBubble.SetText(CustomBossIndex.genocide_lines[beingObj.beingID][UnityEngine.Random.Range(0, CustomBossIndex.genocide_lines[beingObj.beingID].Count())]);
            }
            DownEffects();
        }

        public List<CustomBossAction> GetValidAtIndex(CustomBossStatePattern pattern, int index)
        {
            List<CustomBossAction> re = new List<CustomBossAction>();

            if (pattern.length > 0)
            {
                re.AddRange(from action in pattern.actions where action.index == index select action);
                if (re.Count == 0)
                {
                    re.AddRange(from action in pattern.actions where action.index == 0 select action);
                }
            }
            else
            {
                re.AddRange(pattern.actions.AsEnumerable());
            }

            return re;
        }

        public override IEnumerator Executed()
        {
            var doesnt_count = CustomBossIndex.kill_not_counted.Contains(beingObj.beingID);
            runCtrl.progressBar.SetBossFate();
            if (!doesnt_count) ctrl.IncrementStat("TotalExecutions");
            if (!doesnt_count) runCtrl.currentRun.RemoveAssist(beingObj);
            if (endGameOnExecute)
                ctrl.AddObstacle((Being)this);
            LastWord();
            ctrl.runCtrl.worldBar.Close();
            ctrl.idCtrl.HideOnwardButton();
            runCtrl.worldBar.available = false;
            foreach (Player currentPlayer in ctrl.currentPlayers)
            {
                Player thePlayer = currentPlayer;
                thePlayer.RemoveStatus(Status.Poison);
                thePlayer = (Player)null;
            }
            yield return (object)new WaitForEndOfFrame();
            deCtrl.TriggerAllArtifacts(FTrigger.OnBossKill);
            talkBubble.Fade();
            if (!doesnt_count) ++runCtrl.currentRun.bossExecutions;
            DeathEffects(false);
            S.I.StartCoroutine(_DeathFinal().CancelWith(gameObject));
        }

        public static void AddBeing(GameObject gameObject, BeingObject being)
        {
            if (being.type == (BeingType)Enum.Parse(typeof(BeingType), "Boss"))
            {
                Debug.Log("Created CustomBoss component");
                gameObject.AddComponent<CustomBoss>();
            }
        }

    }

    [HarmonyPatch]
    static class CustomBoss_MiscPatches
    {

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Boss), nameof(Boss._StartDialogue))]
        static bool _StartDialogue(Boss __instance, string key, ref IEnumerator __result)
        {
            if (__instance is CustomBoss)
            {
                switch (key)
                {

                    case "Intro":
                        if (CustomBossIndex.intro_lines.ContainsKey(__instance.beingObj.beingID))
                        {
                            __result = ((CustomBoss)__instance)._DialogueC(CustomBossIndex.intro_lines[__instance.beingObj.beingID][UnityEngine.Random.Range(0, CustomBossIndex.intro_lines[__instance.beingObj.beingID].Count())]);
                            return false;
                        }
                        else
                        {
                            return false;
                        }
                    case "Execution":
                        if (CustomBossIndex.execution_lines.ContainsKey(__instance.beingObj.beingID))
                        {
                            __result = ((CustomBoss)__instance)._DialogueC(CustomBossIndex.execution_lines[__instance.beingObj.beingID][UnityEngine.Random.Range(0, CustomBossIndex.execution_lines[__instance.beingObj.beingID].Count())]);
                            return false;
                        }
                        else
                        {
                            return false;
                        }
                    case "Spare":
                        if (CustomBossIndex.spare_lines.ContainsKey(__instance.beingObj.beingID))
                        {
                            __result = ((CustomBoss)__instance)._DialogueC(CustomBossIndex.spare_lines[__instance.beingObj.beingID][UnityEngine.Random.Range(0, CustomBossIndex.spare_lines[__instance.beingObj.beingID].Count())]);
                            return false;
                        }
                        else
                        {
                            return false;
                        }
                    case "Downed":
                        if (CustomBossIndex.defeated_lines.ContainsKey(__instance.beingObj.beingID))
                        {
                            __result = ((CustomBoss)__instance)._DialogueC(CustomBossIndex.defeated_lines[__instance.beingObj.beingID][UnityEngine.Random.Range(0, CustomBossIndex.defeated_lines[__instance.beingObj.beingID].Count())]);
                            return false;
                        }
                        else
                        {
                            return false;
                        }
                    case "Flawless":
                        if (CustomBossIndex.perfect_lines.ContainsKey(__instance.beingObj.beingID))
                        {
                            __result = ((CustomBoss)__instance)._DialogueC(CustomBossIndex.perfect_lines[__instance.beingObj.beingID][UnityEngine.Random.Range(0, CustomBossIndex.perfect_lines[__instance.beingObj.beingID].Count())]);
                            return false;
                        }
                        else
                        {
                            return false;
                        }
                        break;
                    case "Mercy":
                        if (CustomBossIndex.mercy_lines.ContainsKey(__instance.beingObj.beingID))
                        {
                            __result = ((CustomBoss)__instance)._DialogueC(CustomBossIndex.mercy_lines[__instance.beingObj.beingID][UnityEngine.Random.Range(0, CustomBossIndex.mercy_lines[__instance.beingObj.beingID].Count())]);
                            return false;
                        }
                        else
                        {
                            return false;
                        }
                }
                __result = ((CustomBoss)__instance)._DialogueC("I AM ERROR");
                return false;
            }
            else
            {
                return true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ItemObject), nameof(ItemObject.Trigger))]
        public static bool Trigger(ItemObject __instance, FTrigger fTrigger, bool doublecast, ref Being hitBeing, int forwardedHitDamage)
        {
            if (hitBeing == null) hitBeing = __instance.being;
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UnlockCtrl), nameof(UnlockCtrl.ShowNextUnlock))]
        public static bool ShowNextUnlock(UnlockCtrl __instance, int i)
        {
            if (i >= __instance.hiddenUnlocks.Count)
                i = 0;
            if (__instance.hiddenUnlocks.Count > 0)
            {
                if (__instance.hiddenUnlocks[i].itemObj == null)
                {
                    CharacterCard component = __instance.hiddenUnlocks[i].GetComponent<CharacterCard>();
                    if (component.charAnim.runtimeAnimatorController == null)
                    {
                        component.charAnim.runtimeAnimatorController = S.I.batCtrl.baseCharacterAnim;
                        var overrider = component.charAnim.gameObject.AddComponent<AnimationOverrider>();
                        var animator = component.charAnim.gameObject.AddComponent<SpriteAnimator>();
                        animator.spriteRend = component.charRend;
                        overrider.enabled = true;
                        overrider.Set(animator, component.charAnim, component.beingObj.animName, S.I.itemMan);
                    }
                }
            }
            return true;
        }
    }

    public class CustomBossAction
    {
        public string type = "None";

        public string value = "None";

        public float duration = 0.0f;

        public int index = 0;

        public string target = "";

        public float chance = 1.0f;
    }

    public class CustomBossStatePattern
    {
        public bool loop = true;

        public int length = 0;

        public string state_on_complete;

        public float duration = 0.0f;

        public List<CustomBossAction> actions = new List<CustomBossAction>();

        public string tier = "0+";
    }

    static class CustomBossIndex
    {


        public static Dictionary<string, string> boss_music = new Dictionary<string, string>();

        public static Dictionary<string, List<string>> intro_lines = new Dictionary<string, List<string>>();

        public static Dictionary<string, List<string>> execution_lines = new Dictionary<string, List<string>>();

        public static Dictionary<string, List<string>> spare_lines = new Dictionary<string, List<string>>();

        public static Dictionary<string, List<string>> defeated_lines = new Dictionary<string, List<string>>();

        public static Dictionary<string, List<string>> perfect_lines = new Dictionary<string, List<string>>();

        public static Dictionary<string, List<string>> genocide_lines = new Dictionary<string, List<string>>();

        public static Dictionary<string, List<string>> mercy_lines = new Dictionary<string, List<string>>();

        public static Dictionary<string, bool> mercy = new Dictionary<string, bool>();

        public static List<string> final_bosses = new List<string>();

        public static List<string> serif_mode_bosses = new List<string>();

        public static List<string> kill_not_counted = new List<string>();

        public static List<string> face_right = new List<string>();

        static public void Setup()
        {
            boss_music.Clear();
            intro_lines.Clear();
            execution_lines.Clear();
            spare_lines.Clear();
            defeated_lines.Clear();
            perfect_lines.Clear();
            genocide_lines.Clear();
            mercy_lines.Clear();
            mercy.Clear();
            final_bosses.Clear();
            serif_mode_bosses.Clear();
        }

        
    }

    [HarmonyPatch]
    public static class MiscPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(RunCtrl), nameof(RunCtrl.GoToNextZone))]
        public static bool Next(RunCtrl __instance)
        {
            var battleGrid = S.I.tiCtrl.mainBattleGrid;
            for (int index1 = 0; index1 < battleGrid.gridLength; ++index1)
            {
                for (int index2 = 0; index2 < battleGrid.gridHeight; ++index2)
                    battleGrid.grid[index1, index2].SetAlign(index1 < battleGrid.gridLength / 2 ? 1 : -1);
            }
            foreach (var player in S.I.batCtrl.currentPlayers)
            {
                player.transform.right = Vector3.zero - Vector3.left;
                if (player.mov.currentTile.x > 3)
                {
                    player.mov.MoveTo(player.mov.currentTile.x - 4, player.mov.currentTile.y, false, true, false, false);
                }
            }
            return true;
        }
    }

    public static class DataHandler
    {

        public static void Setup()
        {
            if (!LuaPowerData.customEnums[typeof(BeingType)].Contains("Boss")) LuaPowerData.customEnums[typeof(BeingType)].Add("Boss");
        }

        public static void BossHandler(FileInfo info)
        {
            SpawnCtrl spCtrl = S.I.spCtrl;
            spCtrl.CreateBeingObjectPrototypes(info.Name, (BeingType)Enum.Parse(typeof(BeingType), "Boss"), true, info.DirectoryName);
        }

        [HarmonyPatch(typeof(BeingObject))]
        [HarmonyPatch("ReadXmlPrototype")]
        static class CustomBoss_ReadXmlPrototype
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                bool after_virtual = false;
                bool done = false;
                var to_call = AccessTools.Method(typeof(CustomBoss_Read), nameof(CustomBoss_Read.Switch));
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
                    Debug.LogError("Custom Boss XML Reader Transpiler FAILURE!!! This is likely the result of an update.");
                }
            }

        }


        static class CustomBoss_Read
        {
            private static string last_id = "";

            public static void Switch(XmlReader reader, BeingObject beingObj)
            {
                switch (reader.Name)
                {
                    case "FaceRight":
                        CustomBossIndex.face_right.Add(beingObj.beingID);
                        break;
                    case "Unlocks":
                        var heroID = beingObj.nameString.Replace("Boss", "");
                        if (S.I.spCtrl.heroDictionary.ContainsKey(heroID))
                        {
                            S.I.spCtrl.heroDictionary[heroID].tags.Remove(Tag.Unlock);
                        }
                        break;
                    case "DebugUnlocks":
                        var heroID2 = beingObj.nameString.Replace("Boss", "");
                        SaveDataCtrl.Set<bool>(heroID2, false);
                        break;
                    case "Music":
                        if (!CustomBossIndex.boss_music.ContainsKey(beingObj.beingID))
                        {
                            var content = reader.ReadElementContentAsString();
                            if (!string.IsNullOrEmpty(content)) CustomBossIndex.boss_music.Add(beingObj.beingID, content);
                        }
                        break;
                    case "SerifMode":
                        if (reader.ReadElementContentAsBoolean())
                        {
                            Debug.Log("Serif mode boss added with ID" + beingObj.beingID);
                            CustomBossIndex.serif_mode_bosses.Add(beingObj.beingID);
                        }
                        break;
                    case "FinalBoss":
                        if (reader.ReadElementContentAsBoolean()) CustomBossIndex.final_bosses.Add(beingObj.beingID);
                        break;
                    case "IntroLine":
                        if (!CustomBossIndex.intro_lines.ContainsKey(beingObj.beingID))
                        {
                            CustomBossIndex.intro_lines.Add(beingObj.beingID, new List<string>());
                        }
                        CustomBossIndex.intro_lines[beingObj.beingID].Add(reader.ReadElementContentAsString());
                        break;
                    case "ExecutionLine":
                        if (!CustomBossIndex.execution_lines.ContainsKey(beingObj.beingID))
                        {
                            CustomBossIndex.execution_lines.Add(beingObj.beingID, new List<string>());
                        }
                        CustomBossIndex.execution_lines[beingObj.beingID].Add(reader.ReadElementContentAsString());
                        break;
                    case "SpareLine":
                        if (!CustomBossIndex.spare_lines.ContainsKey(beingObj.beingID))
                        {
                            CustomBossIndex.spare_lines.Add(beingObj.beingID, new List<string>());
                        }
                        CustomBossIndex.spare_lines[beingObj.beingID].Add(reader.ReadElementContentAsString());
                        break;
                    case "DownedLine":
                        if (!CustomBossIndex.defeated_lines.ContainsKey(beingObj.beingID))
                        {
                            CustomBossIndex.defeated_lines.Add(beingObj.beingID, new List<string>());
                        }
                        CustomBossIndex.defeated_lines[beingObj.beingID].Add(reader.ReadElementContentAsString());
                        break;
                    case "FlawlessLine":
                        if (!CustomBossIndex.perfect_lines.ContainsKey(beingObj.beingID))
                        {
                            CustomBossIndex.perfect_lines.Add(beingObj.beingID, new List<string>());
                        }
                        CustomBossIndex.perfect_lines[beingObj.beingID].Add(reader.ReadElementContentAsString());
                        break;
                    case "KilledLine":
                        if (!CustomBossIndex.genocide_lines.ContainsKey(beingObj.beingID))
                        {
                            CustomBossIndex.genocide_lines.Add(beingObj.beingID, new List<string>());
                        }
                        CustomBossIndex.genocide_lines[beingObj.beingID].Add(reader.ReadElementContentAsString());
                        break;
                    case "MercyLine":
                        if (!CustomBossIndex.mercy_lines.ContainsKey(beingObj.beingID))
                        {
                            CustomBossIndex.mercy_lines.Add(beingObj.beingID, new List<string>());
                        }
                        CustomBossIndex.mercy_lines[beingObj.beingID].Add(reader.ReadElementContentAsString());
                        break;
                    case "Mercy":
                        if (!CustomBossIndex.mercy.ContainsKey(beingObj.beingID)) CustomBossIndex.mercy.Add(beingObj.beingID, reader.ReadElementContentAsBoolean());
                        break;
                    case "DontCountKill":
                        if (reader.ReadElementContentAsBoolean())
                        {
                            CustomBossIndex.kill_not_counted.Add(beingObj.beingID);
                        }
                        break;
                }


            }
        }

        [HarmonyPatch(typeof(SpawnCtrl))]
        [HarmonyPatch(nameof(SpawnCtrl.CreateBeing))]
        static class CustomBoss_CreateBeing
        {

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var code_start = -1;

                MethodInfo addBeing = AccessTools.Method(type: typeof(CustomBoss), name: nameof(CustomBoss.AddBeing));

                MethodInfo debugcall = AccessTools.Method(type: typeof(Debug), name: nameof(Debug.LogError), parameters: new Type[] { typeof(object) });

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Call && codes[i].OperandIs(debugcall))
                    {
                        code_start = i;
                        break;
                    }
                }
                if (code_start == -1)
                {
                    Debug.LogError("CreateBeingTranspiler Failed");
                    return codes.AsEnumerable();
                }

                codes.RemoveAt(code_start);
                var to_add = new List<CodeInstruction>();
                to_add.Add(new CodeInstruction(OpCodes.Pop));
                to_add.Add(new CodeInstruction(OpCodes.Ldloc_2));
                to_add.Add(new CodeInstruction(OpCodes.Ldarg_1));
                to_add.Add(new CodeInstruction(OpCodes.Call, addBeing));
                codes.InsertRange(code_start, to_add.AsEnumerable());

                return codes.AsEnumerable();
            }
        }

        /*[HarmonyPatch(typeof(SpawnCtrl))]
        [HarmonyPatch(nameof(SpawnCtrl.CreateBeingObjectPrototypes))]
        static class CustomBoss_CreateBeingObjectPrototypes
        {

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                bool found_first_create_object = false;
                bool finished = false;
                var constructor = typeof(BeingObject).GetConstructor(new Type[] { });
                List<CodeInstruction> move = new List<CodeInstruction>();
                LocalBuilder builder = null;
                foreach (var instruction in instructions)
                {
                    yield return instruction;
                    if (!finished && found_first_create_object && builder == null)
                    {
                        builder = (LocalBuilder) instruction.operand;
                        yield return new CodeInstruction(OpCodes.Ldloc_S, builder);
                        yield return new CodeInstruction(OpCodes.Ldarg_2);
                        yield return new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(BeingObject), nameof(BeingObject.type)));
                        finished = true;
                    }
                    if (!finished && instruction.opcode == OpCodes.Newobj && instruction.operand == (object)constructor)
                    {
                        found_first_create_object = true;
                    }
                }
                if (!finished)
                {
                    Debug.LogError("CreateBeingObjectPrototypes Transpiler failed!!! This is likely the result of an update.");
                }
            }
        }*/

    }
}