using System;
using System.Collections;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;
using UnityEngine;

namespace NGame2.NTutorial.NRuntime;

public class WaitWidgetLongTouch : ITutorialRuntime, IEventRuntime, ICustomTutorialSkippable
{
	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	private bool InitTouchCallback(TutorialWindow window, TutorialData tutorialData, Action onTouch)
	{
		string arg = TutorialUtility.GetArg(tutorialData, 0);
		int argInt = TutorialUtility.GetArgInt(tutorialData, 1);
		int argInt2 = TutorialUtility.GetArgInt(tutorialData, 2);
		int argInt3 = TutorialUtility.GetArgInt(tutorialData, 3);
		int argInt4 = TutorialUtility.GetArgInt(tutorialData, 4);
		int argInt5 = TutorialUtility.GetArgInt(tutorialData, 5);
		Transform transform = Vespa_Unity_Transform_Extension_Method.FindFlexible(GameUIRoot.instance.transform, arg, argInt);
		if (transform == null)
		{
			VespaDebug.LogError(Define.LogPrefix + " Failed to find widget : " + arg);
			return false;
		}
		UIWidget component = transform.GetComponent<UIWidget>();
		int minX;
		int minY;
		int maxX;
		int maxY;
		GameObject gameObject;
		if (component != null)
		{
			window.CalcPositionByWidget(component, argInt2, argInt3, argInt4, argInt5, out minX, out minY, out maxX, out maxY);
			gameObject = component.gameObject;
		}
		else
		{
			BoxCollider component2 = transform.GetComponent<BoxCollider>();
			if (component2 == null)
			{
				VespaDebug.LogError(Define.LogPrefix + " Widget component is null : " + arg);
				return false;
			}
			window.CalcPositionByBoxCollider(component2, argInt2, argInt3, argInt4, argInt5, out minX, out minY, out maxX, out maxY);
			gameObject = component2.gameObject;
		}
		window.SetLongTouchCallback(onTouch, oneshot: true, minX, minY, maxX, maxY, gameObject);
		return true;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		bool touched = false;
		if (InitTouchCallback(window, tutorialData, delegate
		{
			touched = true;
		}))
		{
			while (!touched)
			{
				yield return null;
			}
		}
	}

	IEnumerator ICustomTutorialSkippable.Skip(TutorialWindow window, TutorialData tutorialData)
	{
		if (InitTouchCallback(window, tutorialData, null))
		{
			window.ForceRunTouchAction();
		}
		yield break;
	}
}
