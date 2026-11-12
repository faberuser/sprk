using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class RemoveMaterialEquipItem : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
			for (int i = 0; i < tutorialData.Argument.Length; i++)
			{
				int argInt = TutorialUtility.GetArgInt(tutorialData, i);
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(argInt, null);
			}
		}
	}
}
