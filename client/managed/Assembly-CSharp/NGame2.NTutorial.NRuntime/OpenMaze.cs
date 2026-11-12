using System.Collections;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class OpenMaze : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime, Shop.Listener
{
	bool Shop.Listener.OnCloseRequest(Shop closeThis)
	{
		return true;
	}

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
		LocalAccountInt localAccountInt = new LocalAccountInt(0, "LastMazeTowerOffset");
		int num = 0;
		localAccountInt.Value = num;
		SceneAutoBehaviour<TowerManagement>.instance.OpenMazeTowerList();
	}
}
