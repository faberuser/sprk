using System.Collections;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NTutorial;
using NGame2.NUI.NWorldMap;
using NShared;
using NShared.NEvent;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class ZoomMapNode : ITutorialRuntime, IEventRuntime
{
	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		if (!SceneAutoBehaviour<WorldMapManagement>.isValidInstance)
		{
			yield break;
		}
		while (SceneAutoBehaviour<WorldMapManagement>.instance.IsMapLoading)
		{
			yield return null;
		}
		MapLayout instance = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
		int argInt = TutorialUtility.GetArgInt(tutorialData, 0);
		if (argInt > 0)
		{
			float argFloat = TutorialUtility.GetArgFloat(tutorialData, 1);
			bool executeEnd = false;
			instance.MapViewer.FocusNodeMode(argFloat, argInt, delegate
			{
				executeEnd = true;
			});
			while (!executeEnd)
			{
				yield return null;
			}
		}
		else
		{
			bool executeEnd2 = false;
			instance.MapViewer.ReleaseFocusNode(delegate
			{
				executeEnd2 = true;
			});
			while (!executeEnd2)
			{
				yield return null;
			}
		}
	}
}
