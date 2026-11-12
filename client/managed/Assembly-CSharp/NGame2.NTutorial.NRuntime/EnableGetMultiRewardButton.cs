using System.Collections;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class EnableGetMultiRewardButton : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		MultiplayReward instance = SceneAutoBehaviour<EndCampaign>.instance.GetInstance<MultiplayReward>();
		if (!(instance == null))
		{
			instance.EnableGetMultiRewardButton();
		}
	}
}
