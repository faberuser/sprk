using System.Collections;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;

namespace NGame2.NTutorial.NRuntime;

public class EquipRuneItem : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		int argInt3 = TutorialUtility.GetArgInt(tutorialData, 2);
		int argInt4 = TutorialUtility.GetArgInt(tutorialData, 3);
		new EquipItemInfo
		{
			ItemIndex = argInt,
			Star = ((argInt2 > 0) ? argInt2 : 0),
			Level = argInt3,
			SlotIndex = 9999,
			RuneSlotCount = argInt4,
			RuneItemIndex1 = TutorialUtility.GetArgInt(tutorialData, 4),
			RuneItemIndex2 = TutorialUtility.GetArgInt(tutorialData, 5),
			RuneItemIndex3 = TutorialUtility.GetArgInt(tutorialData, 6)
		};
	}
}
