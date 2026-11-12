using System;
using NGame2.NUI.NManager;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NScene.NLobbyScenePrerequisites;

internal class TreasureHouseInitWrapper : NGame2.NScene.NLobbyScenePrerequisites.IRequestWrapper
{
	public static bool ForceTimeout;

	public const float Timeout = 20f;

	private float _queryTime;

	public bool IsDoneOrTimeout
	{
		get
		{
			if (!IsDone)
			{
				return IsTimeout;
			}
			return true;
		}
	}

	public bool IsDone { get; private set; }

	public bool IsTimeout
	{
		get
		{
			if (IsDone)
			{
				return false;
			}
			return Time.realtimeSinceStartup - _queryTime > 20f;
		}
	}

	public void Init()
	{
		try
		{
			OpenInternal();
		}
		catch (Exception i_object)
		{
			Debug.LogError("TreasureHouseInitWrapper.instance.Open exception: " + Vespa_Common_Class_Extension_Method.TOSTRING(i_object));
			IsDone = true;
		}
	}

	private void OpenInternal()
	{
		_queryTime = Time.realtimeSinceStartup;
		if (ForceTimeout)
		{
			IsDone = true;
			return;
		}
		Action action = delegate
		{
			IsDone = true;
		};
		SceneAutoBehaviour<TreasureHouseManagement>.instance.RequestTreasureHouseInfo(action, action);
	}
}
