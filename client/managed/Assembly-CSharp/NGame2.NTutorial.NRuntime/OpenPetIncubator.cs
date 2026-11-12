using System.Collections;
using NGame2.NUI.NManager.NPet;
using NGame2.NUI.NWindow.NPet;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class OpenPetIncubator : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		PetHouseWindow instance = SceneAutoBehaviour<PetManagement>.instance.GetInstance<PetHouseWindow>(openedOnly: true);
		if (instance == null)
		{
			VespaDebug.LogError("PetHouseWindow is Not Opened");
		}
		else
		{
			instance.FocusPetIncubator();
		}
	}
}
