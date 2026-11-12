using System.Collections;
using System.Linq;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class CloseAllWindow : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		string[] arguments = tutorialData.Argument;
		for (int i = 0; i < 3; i++)
		{
			GlobalAutoBehaviour<GlobalManagement>.instance.ForEach(delegate(IManagementWrapper managementWrapper)
			{
				if (!managementWrapper.IsPersistent())
				{
					string name = managementWrapper.GetType().Name;
					if (arguments == null || arguments.Contains(name))
					{
						managementWrapper.CloseAll(immediately: false);
					}
				}
			});
		}
	}
}
