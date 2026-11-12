using System.Collections;
using NGame2.NUI.NWindow.NTutorial;
using NGameAsset;
using NShared;
using NShared.NEvent;
using NVespa;

namespace NGame2.NTutorial.NRuntime;

public class PlaySound : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		SoundStock stock = new SoundStock
		{
			Code = TutorialUtility.GetArg(tutorialData, 0),
			LocalTimer = MainTimer.DefaultLocal
		};
		SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 10f);
	}
}
