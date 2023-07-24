using HarmonyLib;
using System;
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