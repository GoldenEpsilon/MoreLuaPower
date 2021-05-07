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

public static class UtilityFunctions
{
    //Allows beings with base-game sprites to be overridden by attaching an AnimationOverrider if they don't have one
    public static void OverrideAnimator(Being being, string animName)
    {
        //If the anim name is found in the base game sprites, set the animator and disable any overrider by setting its controller name to empty
        if (S.I.itemMan.animations.ContainsKey(animName))
        {
            being.SetAnimatorController(S.I.itemMan.GetAnim(animName));
            if(being.animOverrider != null)
            {
                being.animOverrider.controllerName = "";
            }
            return;
        }

        //Otherwise, if the being already has an overrider, set that
        if (being.animOverrider != null)
        {
            being.animOverrider.controllerName = animName;
        } else { // Otherwise, add an overrider and set the animation
            if (being.sprAnim == null)
            {
                Debug.Log("Adding sprite animator to " + being.beingObj.beingID);
                being.sprAnim = being.gameObject.AddComponent<SpriteAnimator>();
                being.sprAnim.enabled = false;
            }
            Debug.Log("Adding animation overrider to " + being.beingObj.beingID);
            being.animOverrider = being.gameObject.AddComponent<AnimationOverrider>();
            being.animOverrider.Set(being.sprAnim, being.anim, animName, S.I.itemMan);
            being.anim.runtimeAnimatorController = string.IsNullOrEmpty(being.beingObj.animBase) ? S.I.batCtrl.baseCharacterAnim : S.I.batCtrl.baseMonsterAnim;
        }
    }
}
