using HarmonyLib;
using System;
using UnityEngine;

static class LuaPowerMisc
{
	public static FTrigger GetFTrigger(string name) {
		return (FTrigger)Enum.Parse(typeof(FTrigger), name);
	}
}