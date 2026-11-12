using System;
using System.Collections;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;
using UnityEngine;

namespace NGame2.NTutorial.NRuntime;

public class WaitWidgetTouch : ITutorialRuntime, IEventRuntime, ICustomTutorialSkippable
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
		bool argBool = TutorialUtility.GetArgBool(tutorialData, 6);
		bool argBool2 = TutorialUtility.GetArgBool(tutorialData, 7);
		Transform transform = Vespa_Unity_Transform_Extension_Method.FindFlexible(GameUIRoot.instance.transform, arg, argInt);
		if (transform == null)
		{
			VespaDebug.LogError(Define.LogPrefix + " Failed to find widget : " + arg);
			return false;
		}
		BoxCollider boxCollider = transform.GetComponent<BoxCollider>();
		if (boxCollider == null)
		{
			boxCollider = transform.GetComponentInChildren<BoxCollider>();
			if (boxCollider == null)
			{
				VespaDebug.LogError(Define.LogPrefix + " boxCollider component is null : " + arg);
			}
		}
		int minX;
		int minY;
		int maxX;
		int maxY;
		GameObject gameObject;
		if (boxCollider != null)
		{
			window.CalcPositionByBoxCollider(boxCollider, argInt2, argInt3, argInt4, argInt5, out minX, out minY, out maxX, out maxY);
			gameObject = boxCollider.gameObject;
		}
		else
		{
			UIWidget component = transform.GetComponent<UIWidget>();
			if (!(component != null))
			{
				VespaDebug.LogError(Define.LogPrefix + " Widget component is null : " + arg);
				return false;
			}
			window.CalcPositionByWidget(component, argInt2, argInt3, argInt4, argInt5, out minX, out minY, out maxX, out maxY);
			gameObject = component.gameObject;
		}
		window.SetTouchCallback(onTouch, oneshot: true, minX, minY, maxX, maxY, argBool, argBool2, gameObject);
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
