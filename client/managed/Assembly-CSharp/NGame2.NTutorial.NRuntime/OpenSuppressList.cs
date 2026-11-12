using System.Collections;
using System.Collections.Generic;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class OpenSuppressList : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
{
	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		Vespa_Common_Class_Extension_Method.Interface((ITutorialRuntimeImmediate)this).RunImmediate(window, tutorialData);
		yield break;
	}

	void ITutorialRuntimeImmediate.RunImmediate(TutorialWindow window, TutorialData tutorialData)
	{
		int argInt = TutorialUtility.GetArgInt(tutorialData, 0);
		int argInt2 = TutorialUtility.GetArgInt(tutorialData, 1);
		if (GameRoot.Instance.GuildSuppressChapterDataContainer.GetData(argInt) == null)
		{
			VespaDebug.LogError("OpenSuppressBoss : SuppressChapterData not found. raidIndex=" + argInt);
			return;
		}
		if (!SceneAutoBehaviour<GuildSuppressManagement>.isValidInstance)
		{
			VespaDebug.LogError("OpenSuppressBoss : GuildSuppressManagement is not found");
			return;
		}
		bool completeOpen = false;
		List<int> list = new List<int>();
		if (tutorialData.Argument.Length > 2)
		{
			for (int i = 0; i < tutorialData.Argument.Length; i++)
			{
				if (i != 0 && i != 1)
				{
					int argInt3 = TutorialUtility.GetArgInt(tutorialData, i);
					list.Add(argInt3);
				}
			}
		}
		SceneAutoBehaviour<GuildSuppressManagement>.instance.OpenSuppressList(argInt, argInt2, list, delegate
		{
			completeOpen = true;
		});
	}
}
