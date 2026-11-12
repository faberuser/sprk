using System;
using System.Collections.Generic;
using System.Reflection;
using NGame2.NCutScene;
using NGame2.NScene;
using NGame2.NTutorial;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUI;

public class UIEventAttribute : Attribute
{
	public bool AllowDuringPatchDownloading;

	public static bool IsEnableEvent(List<EventDelegate> list)
	{
		if (list == null || list.Count == 0)
		{
			return true;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (!CheckIsEnableEvent(list[0]))
			{
				ShowMessage();
				return false;
			}
		}
		return true;
	}

	private static bool CheckIsEnableEvent(EventDelegate del)
	{
		if (!NeedCheck())
		{
			return true;
		}
		if (del == null || del.target == null)
		{
			return true;
		}
		bool result = false;
		Type type = del.target.GetType();
		object[] customAttributes = type.GetCustomAttributes(typeof(UIEventAttribute), inherit: true);
		if (customAttributes != null && customAttributes.Length != 0 && customAttributes[0] is UIEventAttribute uIEventAttribute)
		{
			result = uIEventAttribute.AllowDuringPatchDownloading;
		}
		MemberInfo[] member = type.GetMember(del.methodName);
		if (member != null && member.Length != 0)
		{
			customAttributes = member[0].GetCustomAttributes(typeof(UIEventAttribute), inherit: true);
			if (customAttributes != null && customAttributes.Length != 0 && customAttributes[0] is UIEventAttribute uIEventAttribute2)
			{
				result = uIEventAttribute2.AllowDuringPatchDownloading;
			}
		}
		return result;
	}

	public static void ShowMessage()
	{
	}

	public static bool NeedCheck()
	{
		if (GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType != LoadingManager.ESceneType.Lobby)
		{
			return false;
		}
		if (SingletonUpdater<TutorialManager>.isValidInstance && SingletonUpdater<TutorialManager>.instance.IsProgressing)
		{
			return false;
		}
		if (SceneAutoBehaviour<CutSceneManager>.isValidInstance)
		{
			_ = SceneAutoBehaviour<CutSceneManager>.instance.IsPlaying;
			return false;
		}
		return false;
	}
}
