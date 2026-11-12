using System.Collections;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class OpenSuppressBoss : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		if (GameRoot.Instance.GuildSuppressChapterDataContainer.GetData(argInt) == null)
		{
			VespaDebug.LogError("OpenSuppressBoss : SuppressChapterData not found. raidIndex=" + argInt);
			return;
		}
		bool completeOpen = false;
		SceneAutoBehaviour<GuildSuppressManagement>.instance.OpenGuildSuppressRanking(argInt, delegate
		{
			completeOpen = true;
		});
	}
}
