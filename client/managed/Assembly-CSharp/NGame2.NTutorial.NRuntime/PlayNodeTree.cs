using System.Collections;
using NGame2.NBattleField;
using NGame2.NNodeTree;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;

namespace NGame2.NTutorial.NRuntime;

public class PlayNodeTree : ITutorialRuntime, IEventRuntime
{
	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		string arg = TutorialUtility.GetArg(tutorialData, 0);
		string nodeTreeCode = TutorialUtility.GetArg(tutorialData, 1);
		PackLoad packLoad = SingletonAuto<PackContainer>.instance.GetPack(arg);
		while (!packLoad.ISDONE())
		{
			yield return null;
		}
		TreeData treeData = null;
		if (packLoad.ISVALID())
		{
			treeData = packLoad.Value.GetNodeTree(nodeTreeCode);
		}
		if (treeData != null)
		{
			SingletonAuto<BattleFacade>.instance.NodeTreeStart(treeData);
		}
	}
}
