using System;
using System.Collections;
using System.Collections.Generic;

using HarmonyLib;
using UnityEngine;

static class LuaPowerMisc
{
	public static FTrigger GetFTrigger(string name) {
		return (FTrigger)Enum.Parse(typeof(FTrigger), name);
	}
	public static Brand GetBrand(string name) {
		return (Brand)Enum.Parse(typeof(Brand), name);
	}
	public static Status GetStatus(string name) {
		return (Status)Enum.Parse(typeof(Status), name);
	}
	public static Enhancement GetUpgrade(string name) {
		return (Enhancement)Enum.Parse(typeof(Enhancement), name);
	}
}

[HarmonyPatch(typeof(Debug), nameof(Debug.Log))]
[HarmonyPatch(new Type[] { typeof(object) })]

static class MoreLuaPower_InstallSpeed
{
	[HarmonyPrefix]
	private static bool UnnecessaryLogs(ref object message) 
	{
		if (message.ToString().StartsWith("Creating SprClip Mod") || message.ToString().Contains("Mods installed in")) 
		{
			return false;
		}
		return true;
	}

	// ---------------- // STORE LOGS // ---------------- //

	public static List<object> afterLogs = new List<object>();
	public static List<object> afterWarnings = new List<object>();
	public static List<object> afterErrors = new List<object>();

	public static bool afterRelease = true;

	[HarmonyPrefix]
	[HarmonyPriority(Priority.Last)]
	
	internal static bool StoreLog (object message) 
	{
		if (afterRelease)
		{
			afterLogs.Add(message);
			return false;
		}
		return true;
	}

	[HarmonyPatch(typeof(Debug), nameof(Debug.LogWarning))]
	[HarmonyPatch(new Type[] { typeof(object) })]
	[HarmonyPrefix]
	[HarmonyPriority(Priority.Last)]
	
	internal static bool StoreWarning (object message)
	{
		if (afterRelease)
		{
			afterWarnings.Add(message);
			return false;
		}
		return true;
	}

	[HarmonyPatch(typeof(Debug), nameof(Debug.LogError))]
	[HarmonyPatch(new Type[] { typeof(object) })]
	[HarmonyPrefix]
	[HarmonyPriority(Priority.Last)]
	
	internal static bool StoreError (object message)
	{
		if (afterRelease)
		{
			if (message.ToString().Contains("Failed to load sprite") || ( message.ToString().Contains("Texture at") && message.ToString().Contains("UNABLE to be loaded") )
			   || message.ToString().Contains("Invalid Check:"))
			{
				afterErrors.Add(message);
				return false;
			}
		}
		return true;
	}

	// ---------------- // RELEASE LOGS // ---------------- //

	[HarmonyPatch(typeof(ItemManager), nameof(ItemManager.LoadItemData))]
        [HarmonyPatch(new Type[] {})]
        [HarmonyPostfix]
	[HarmonyPriority(Priority.Normal)]
	
	internal static void ReleaseLogs()
        {
            	if (!S.I.modCtrl.processing && afterRelease)
            	{
                	S.I.mainCtrl.StartCoroutine(_ReleaseLogs());
            	}
        }
        internal static IEnumerator _ReleaseLogs()
        {
        	afterRelease = false;

        	yield return new WaitForEndOfFrame();

        	foreach (object item in afterLogs)
        	{
        		string message = item.ToString();

			// Compacts the install text a bit
                	if (message.Contains("Installing") && message.Contains("Steam\\steamapps\\"))
                	{
                    		message = "Installing " + message.Substring(message.IndexOf("steamapps\\") + "steamapps\\".Length);
                	}
                	if (message.Contains("Installing") && message.Contains("StreamingAssets\\Mods"))
                	{
                    	message = "Installing " + message.Substring(message.IndexOf("StreamingAssets\\") + "StreamingAssets\\".Length);
                	}

                	Debug.Log(message);

                	if (item.ToString().Contains("MoreLuaPower Version"))
                	{	
                    		Traverse.Create(S.I).Field("consoleView").Field("console").GetValue<ConsoleCtrl>().appendLogLine(message);
                	}

                	yield return new WaitForEndOfFrame();
            	}
		afterLogs.Clear();

            	foreach (object item in afterWarnings)
            	{
                	Debug.LogWarning(item);
                	yield return new WaitForEndOfFrame();
            	}
		afterWarnings.Clear();
		
            	foreach (object item in afterErrors)
            	{
                	Debug.LogError(item);
                	yield return new WaitForEndOfFrame();
            	}
		afterErrors.Clear();
		
            	yield break;
        }
}

[HarmonyPatch(typeof(UnlockCtrl), nameof(UnlockCtrl.ShowNextUnlock))]
static class MoreLuaPower_BaseFixes
{
    [HarmonyPostfix]
    internal static void FixUnlockCardSprites()
    {
        ChoiceCard shownCard = S.I.unCtrl.shownUnlocks[S.I.unCtrl.shownUnlocks.Count - 1];
        CharacterCard unlockCard = shownCard.GetComponent<CharacterCard>();

        if (!S.I.itemMan.animations.ContainsKey(unlockCard.beingObj.animName)
            && S.I.itemMan.spriteAnimClips.ContainsKey(unlockCard.beingObj.animName + "_idle")
            && unlockCard.gameObject.GetComponent<SpriteAnimator>() == null
            && unlockCard.charRend != null)
        {
            SpriteAnimator animator = unlockCard.gameObject.AddComponent<SpriteAnimator>();
            animator.spriteRend = unlockCard.charRend;
            animator.AssignClip(S.I.itemMan.GetClip(unlockCard.beingObj.animName + "_idle"));
            unlockCard.charAnim.enabled = false;
        }
    }
}
