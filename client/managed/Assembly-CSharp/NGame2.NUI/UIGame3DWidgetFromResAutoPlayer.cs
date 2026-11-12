using System.Collections.Generic;
using NVespa;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI;

[RequireComponent(typeof(UIGame3DWidgetFromRes))]
public class UIGame3DWidgetFromResAutoPlayer : MonoBehaviour
{
	public float Delay;

	public List<EventDelegate> OnStart = new List<EventDelegate>();

	public List<EventDelegate> OnComplete = new List<EventDelegate>();

	private void OnEnable()
	{
		SingletonUpdater<Executer>.instance.DelayRun(Delay, this, delegate
		{
			EventDelegate.Execute(OnStart);
			GetComponent<UIGame3DWidgetFromRes>().Play(OnPlayed);
		});
	}

	private void OnPlayed()
	{
		EventDelegate.Execute(OnComplete);
	}
}
