using System;
using NGame2.NUtil;
using UnityEngine;

namespace NGame2.NScene.NLobbyScenePrerequisites;

internal class QueryMarketInfoWrapper
{
	public static bool ForceTimeout;

	public const float Timeout = 10f;

	private QueryMarketInfo _queryMarketInfo = new QueryMarketInfo();

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
			return Time.realtimeSinceStartup - _queryTime > 10f;
		}
	}

	public void Query()
	{
		try
		{
			QueryInternal();
		}
		catch (Exception i_object)
		{
			Debug.LogError("QueryMarketInfo.Query exception: " + Vespa_Common_Class_Extension_Method.TOSTRING(i_object));
			IsDone = true;
		}
	}

	private void QueryInternal()
	{
		_queryTime = Time.realtimeSinceStartup;
		if (!ForceTimeout)
		{
			Debug.Log("QueryMarketInfo.Query");
			_queryMarketInfo.Query(delegate
			{
				IsDone = true;
			});
		}
	}

	public bool CheckNewAppArrival()
	{
		try
		{
			if (IsTimeout)
			{
				Debug.LogError("QueryMarketInfo Timeout");
				return false;
			}
			if (_queryMarketInfo == null)
			{
				return false;
			}
			if (_queryMarketInfo.Verify())
			{
				return false;
			}
			return true;
		}
		catch (Exception i_object)
		{
			Debug.LogError("QueryServerDown.QuitIfNewAppArrival exception: " + Vespa_Common_Class_Extension_Method.TOSTRING(i_object));
		}
		return false;
	}
}
