using System.Collections;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NTutorial;
using NGame2.NUI.NWorldMap;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NTutorial.NRuntime;

public class FocusWidgetMove : ITutorialRuntime, IEventRuntime
{
	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		if (window == null)
		{
			yield return null;
		}
		if (!SceneAutoBehaviour<WorldMapManagement>.isValidInstance)
		{
			yield return null;
		}
		string arg = TutorialUtility.GetArg(tutorialData, 0);
		int argInt = TutorialUtility.GetArgInt(tutorialData, 1);
		int offsetX = TutorialUtility.GetArgInt(tutorialData, 2);
		int offsetY = TutorialUtility.GetArgInt(tutorialData, 3);
		Transform widgetTransform = Vespa_Unity_Transform_Extension_Method.FindFlexible(GameUIRoot.instance.transform, arg, argInt);
		if (widgetTransform == null)
		{
			VespaDebug.LogError(Define.LogPrefix + " Failed to find widget : " + arg);
			yield return null;
		}
		MapLayout instance = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
		int num = 1;
		bool executeEnd = false;
		instance.MapViewer.FocusPositionMode(num, new Vector2(widgetTransform.localPosition.x + (float)offsetX, widgetTransform.localPosition.y + (float)offsetY), 1f, delegate
		{
			executeEnd = true;
		});
		while (!executeEnd)
		{
			yield return null;
		}
	}
}
