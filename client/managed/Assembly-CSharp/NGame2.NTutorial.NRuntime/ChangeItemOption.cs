using System.Collections;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class ChangeItemOption : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(tutorialData.Argument))
		{
			int argInt = TutorialUtility.GetArgInt(tutorialData, 0);
			int argInt2 = TutorialUtility.GetArgInt(tutorialData, 1);
			int argInt3 = TutorialUtility.GetArgInt(tutorialData, 2);
			int argInt4 = TutorialUtility.GetArgInt(tutorialData, 3);
			SceneAutoBehaviour<BlacksmithManagement>.instance.GetInstance<EquipOptionRenewScene>().Open(null, argInt, argInt2, argInt3, argInt4);
		}
	}
}
