using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using E7.Introloop;
using HarmonyLib;
using UnityEngine;

//examples:
//LuaPowerCutscenes.PlayCutsceneURL("https://www.videvo.net/videvo_files/converted/2014_12/preview/ULTRASOUND_3.mov74497.webm", true, true, true, 1);
//LuaPowerCutscenes.PlayCutscene("file:///D:/Username/Videos/Captures/Funny.mp4", true, true, true, 1);

public class LuaPowerCutscenes
{
    public static void Setup() {
        LuaPowerData.videos["goodEndingFalse"] = "goodEndingFalse";
        LuaPowerData.videos["goodEndingTrue"] = "goodEndingTrue";
        LuaPowerData.videos["badEnding"] = "badEnding";
    }
    public static void LoadCutscene(string path, string PATH, string name) {
        LuaPowerData.videos[name] = Path.Combine(PATH, path);
    }
    public static void PlayCutsceneURL(string URL, bool pausemusic = true, bool clearprojectiles = true, bool stopenemies = true, int endlag = 1) {
        S.I.batCtrl.StartCoroutine(new LuaPowerCutscenes().Cutscene(URL, pausemusic, clearprojectiles, stopenemies, endlag));
    }
    public static void PlayCutscene(string clip, bool pausemusic = true, bool clearprojectiles = true, bool stopenemies = true, int endlag = 1) {
        S.I.batCtrl.StartCoroutine(new LuaPowerCutscenes().Cutscene(LuaPowerData.videos[clip], pausemusic, clearprojectiles, stopenemies, endlag));
    }

    public IEnumerator Cutscene(string URL, bool pausemusic, bool clearprojectiles, bool stopenemies, int endlag) {
        BC batCtrl = S.I.batCtrl;
        CGCtrl cgCtrl = S.I.cgCtrl;

        if (pausemusic) {
            if (IntroloopPlayer.Instance.IsPausable()) {
                IntroloopPlayer.Instance.Pause();
            }
        }

        batCtrl.AddControlBlocks(Block.GameEnd);


        if (clearprojectiles) {
            batCtrl.ti.mainBattleGrid.ClearProjectiles(true);
        }

        if (stopenemies) {
            for (int index = batCtrl.ti.mainBattleGrid.currentEnemies.Count - 1; index >= 0; --index) {
                batCtrl.ti.mainBattleGrid.currentEnemies[index].ApplyStun(false, true, true);
                batCtrl.ti.mainBattleGrid.currentEnemies[index].mov.SetState(State.Attacking);
            }
            for (int index = batCtrl.ti.mainBattleGrid.currentStructures.Count - 1; index >= 0; --index) {
                batCtrl.ti.mainBattleGrid.currentStructures[index].ApplyStun(false, true, true);
                batCtrl.ti.mainBattleGrid.currentStructures[index].mov.SetState(State.Attacking);
            }
        }





        cgCtrl.handGood.gameObject.SetActive(false);
        cgCtrl.handGrab.gameObject.SetActive(false);
        cgCtrl.terraArm.gameObject.SetActive(false);
        cgCtrl.unCtrl.itemGrids.blocksRaycasts = false;
        cgCtrl.charEvil.gameObject.SetActive(false);
        cgCtrl.cast.gameObject.SetActive(false);
        cgCtrl.stand.gameObject.SetActive(false);
        cgCtrl.mirror.gameObject.SetActive(false);

        cgCtrl.ctrl.GameState = GState.CG;
        cgCtrl.vidPlayer.playbackSpeed = Time.timeScale;

        Traverse.Create(cgCtrl).Field("skip").SetValue(false);


        cgCtrl.Open();
        cgCtrl.anim.SetBool("OnScreen", true);
        if ((UnityEngine.Object)cgCtrl.mainVideoScreen != (UnityEngine.Object)null) {
            cgCtrl.mainVideoScreen.sortingLayerName = cgCtrl.canvas.sortingLayerName;
            cgCtrl.mainVideoScreen.sortingOrder = cgCtrl.canvas.sortingOrder;
        }

        if (URL == "goodEndingFalse")
            cgCtrl.vidPlayer.clip = cgCtrl.goodEndingFalse;
        else if (URL == "goodEndingTrue")
            cgCtrl.vidPlayer.clip = cgCtrl.goodEndingTrue;
        else if (URL == "badEnding")
            cgCtrl.vidPlayer.clip = cgCtrl.badEnding;
        else
            cgCtrl.vidPlayer.url = URL;


        cgCtrl.videoScreen.enabled = false;
        cgCtrl.vidPlayer.Prepare();

        Traverse.Create(cgCtrl).Method("SetVideoScreenScale");


        float prepDuration = 0.0f;
        //while (!cgCtrl.vidPlayer.isPrepared && (double)prepDuration < 5.0)
        while (!cgCtrl.vidPlayer.isPrepared) {
            prepDuration += Time.deltaTime;
            yield return (object)null;
        }
        cgCtrl.ctrl.camCtrl.TransitionOutHigh("InstantBlack");
        cgCtrl.videoScreen.enabled = true;
        cgCtrl.vidPlayer.Play();

        Traverse.Create(cgCtrl).Field("playing").SetValue(true);



        while (!(bool)Traverse.Create(cgCtrl).Field("endReached").GetValue())
            yield return (object)null;



        cgCtrl.anim.SetBool("OnScreen", false);
        cgCtrl.ctrl.camCtrl.TransitionInHigh("LeftWipe");

        Traverse.Create(cgCtrl).Field("endReached").SetValue(false);



        if (S.I.ANIMATIONS)
            yield return (object)new WaitForSeconds(1.5f);
        cgCtrl.vidPlayer.Stop();
        cgCtrl.Close();


        Traverse.Create(cgCtrl).Field("playing").SetValue(false);
        Traverse.Create(cgCtrl).Field("skip").SetValue(false);

        cgCtrl.Close();
        batCtrl.GameState = GState.Battle;
        batCtrl.camCtrl.TransitionOutHigh("LeftWipe");


        if (pausemusic) {

            if (!IntroloopPlayer.Instance.IsPlaying()) {
                IntroloopPlayer.Instance.Resume();
            }
        }

        float Pausing = 0.0f;
        while ((double)Pausing < (double)endlag) {
            Pausing += Time.deltaTime;
            yield return (object)null;
        }


        S.I.batCtrl.RemoveControlBlocks(Block.GameEnd);

        if (stopenemies) {
            for (int index = batCtrl.ti.mainBattleGrid.currentEnemies.Count - 1; index >= 0; --index) {
                batCtrl.ti.mainBattleGrid.currentEnemies[index].StartCoroutine(batCtrl.ti.mainBattleGrid.currentEnemies[index].StartLoopC());
                batCtrl.ti.mainBattleGrid.currentEnemies[index].mov.SetState(State.Idle);
            }
            for (int index = batCtrl.ti.mainBattleGrid.currentStructures.Count - 1; index >= 0; --index) {
                batCtrl.ti.mainBattleGrid.currentStructures[index].StartCoroutine(batCtrl.ti.mainBattleGrid.currentStructures[index].StartLoopC());
                batCtrl.ti.mainBattleGrid.currentStructures[index].mov.SetState(State.Idle);
            }
        }


    }
}