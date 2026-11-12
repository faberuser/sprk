using System;
using System.Diagnostics;
using NShared;
using NVespa;

namespace NGame2.NTutorial;

public static class TutorialUtility
{
	[Conditional("UNITY_EDITOR")]
	public static void Log(object obj)
	{
	}

	public static string GetArg(TutorialData tutorialData, int index)
	{
		if (tutorialData == null || tutorialData.Argument == null || tutorialData.Argument.Length == 0)
		{
			return string.Empty;
		}
		if (index >= tutorialData.Argument.Length)
		{
			return string.Empty;
		}
		tutorialData.Argument[index] = tutorialData.Argument[index].Trim();
		return tutorialData.Argument[index];
	}

	public static int GetArgInt(TutorialData tutorialData, int index)
	{
		string arg = GetArg(tutorialData, index);
		int result = 0;
		int.TryParse(arg, out result);
		return result;
	}

	public static T GetArgEnum<T>(TutorialData tutorialData, int index) where T : struct
	{
		string arg = GetArg(tutorialData, index);
		try
		{
			return EnumTools.Parse(arg, default(T));
		}
		catch (Exception)
		{
		}
		return default(T);
	}

	public static float GetArgFloat(TutorialData tutorialData, int index, float defaultValue = 0f)
	{
		if (!float.TryParse(GetArg(tutorialData, index), out var result))
		{
			return defaultValue;
		}
		return result;
	}

	public static bool GetArgBool(TutorialData tutorialData, int index, bool defaultValue = false)
	{
		string arg = GetArg(tutorialData, index);
		bool result = defaultValue;
		if (!bool.TryParse(arg, out result))
		{
			result = defaultValue;
		}
		return result;
	}
}
