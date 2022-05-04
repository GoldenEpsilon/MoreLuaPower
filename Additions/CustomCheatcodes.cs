using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

[HarmonyPatch(typeof(Player))]
[HarmonyPatch("Start")]
class UseCustomCheats
{
    static void Postfix(Player __instance) {
        if (!(__instance.rewiredPlayer == null || __instance.animOverrider == null)) {

            if (TauntReviverList.charlist.Contains(__instance.beingObj.beingID) || TauntReviverList.charlist.Contains(__instance.animOverrider.controllerName)) {
                __instance.gameObject.AddComponent<TauntReviver>();

                __instance.GetComponent<TauntReviver>().thePlayer = __instance;
            }

            foreach (CustomCheatcode Cheatcode in CustomCheatcodesList.CheatsList) {
                if (Cheatcode.CharName == __instance.beingObj.beingID) {
                    if (__instance.rewiredPlayer.GetButton(Cheatcode.ButtonName)) {
                        SpellObject LuaUser = S.I.deCtrl.CreateSpellBase("Kunai", __instance, false);

                        EffectActions.CallFunctionWithItem(Cheatcode.FunctionName, LuaUser);
                    }
                }
            }
        }
    }
}


public class CustomCheatcode
{
    public string CharName;
    public string ButtonName;
    public string FunctionName;

    public CustomCheatcode(string Char, string Button, string function) {
        CharName = Char;
        ButtonName = Button;
        FunctionName = function;
    }
}

public static class CustomCheatcodesList
{
    public static List<CustomCheatcode> CheatsList = new List<CustomCheatcode>();

    public static void SetCheatcode(string Charname, string Button, string function) {
        CustomCheatcodesList.CheatsList.Add(new CustomCheatcode(Charname, Button, function));
    }
}

public static class TauntReviverList
{
    public static List<string> charlist = new List<string>();

    public static void AddCharacter(string Charname) {
        charlist.Add(Charname);
    }
}

public class TauntReviver : MonoBehaviour
{
    public Player thePlayer;

    public virtual void Update() {
        if (thePlayer.rewiredPlayer.GetButtonDown("RemoveSpell") & thePlayer.rewiredPlayer.GetButtonDown("Shuffle")) {
            thePlayer.anim.SetTrigger("taunt");
        }
    }
}