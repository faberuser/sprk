using System;
using NGame2.NAccount;
using NGame2.NEventTrigger;
using NShared.NEvent;
using NVespa;
using UnityEngine;

namespace NGame2.NScene.NLobbyScenePrerequisites;

internal class RaiseChapterMoveEventWrapper
{
	public static bool ForceTimeout;

	public const float Timeout = 20f;

	private float _raiseTime;

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
			return Time.realtimeSinceStartup - _raiseTime > 20f;
		}
	}

	public void Raise()
	{
		try
		{
			RaiseInternal();
		}
		catch (Exception i_object)
		{
			Debug.LogError("RaiseChapterMoveEvent.Raise exception: " + Vespa_Common_Class_Extension_Method.TOSTRING(i_object));
			IsDone = true;
		}
	}

	private void RaiseInternal()
	{
		_raiseTime = Time.realtimeSinceStartup;
		if (!ForceTimeout)
		{
			int chapterIndex = UserCampaignManager.ChapterIndex;
			SingletonAuto<EventTriggerManager>.instance.RaiseEvent(new ChapterMove
			{
				ChapterIndex = chapterIndex
			}, null, delegate
			{
				IsDone = true;
			});
		}
	}
}
