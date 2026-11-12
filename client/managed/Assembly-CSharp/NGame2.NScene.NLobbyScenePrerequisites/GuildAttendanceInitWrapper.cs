using System;
using NGame2.NGuild;
using NGame2.NUI.NManager;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NScene.NLobbyScenePrerequisites;

internal class GuildAttendanceInitWrapper : NGame2.NScene.NLobbyScenePrerequisites.IRequestWrapper
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
		catch (Exception)
		{
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
		if (!GlobalAutoBehaviour<GuildManager>.isValidInstance)
		{
			IsDone = true;
			return;
		}
		if (!GlobalAutoBehaviour<GuildManager>.instance.CheckIsGuild)
		{
			IsDone = true;
			return;
		}
		if (GlobalAutoBehaviour<GuildManager>.instance.GuildAttendanceInfo != null && GlobalAutoBehaviour<GuildManager>.instance.GuildMemberAttendanceInfo != null)
		{
			IsDone = true;
			return;
		}
		if (!SceneAutoBehaviour<GuildManagement2>.isValidInstance)
		{
			IsDone = true;
			return;
		}
		SceneAutoBehaviour<GuildManagement2>.instance.RequestGetGuildAttendance(delegate
		{
			if (SceneAutoBehaviour<WorldMapManagement>.isValidInstance)
			{
				SceneAutoBehaviour<WorldMapManagement>.instance.UpdateGuildAttendanceNewIndicator();
			}
			IsDone = true;
		});
	}
}
