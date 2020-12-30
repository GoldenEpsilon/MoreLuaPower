using System;
using UnityEngine;
using HarmonyLib;

public enum LogLevel 
{
	Nothing,
	Critical,
	Major,
	Normal,
	Minor,
	Info,
	All
}

public static class MPLog
{
	public static LogLevel fileLogLevel = LogLevel.Minor;
	public static LogLevel consoleLogLevel = LogLevel.Normal;
	public static void Log(string message, LogLevel logLevel = LogLevel.Normal) {
		if ((int)logLevel <= (int)fileLogLevel) {
			Debug.Log("LOG - (" + logLevel.ToString() + "): " + message);
		}
		if ((int)logLevel <= (int)consoleLogLevel) {
			Traverse.Create(S.I).Field("consoleView").Field("console").GetValue<ConsoleCtrl>().appendLogLine("LOG - (" + logLevel.ToString() + "): " + message);
		}
	}
	public static void ChangeFileLogLevel(LogLevel logLevel = LogLevel.Minor) {
		fileLogLevel = logLevel;
	}
	public static void ChangeConsoleLogLevel(LogLevel logLevel = LogLevel.Minor) {
		consoleLogLevel = logLevel;
	}
}