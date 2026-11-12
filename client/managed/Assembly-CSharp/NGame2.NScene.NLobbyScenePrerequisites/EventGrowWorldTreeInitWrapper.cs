using System;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NScene.NLobbyScenePrerequisites;

internal class EventGrowWorldTreeInitWrapper : NGame2.NScene.NLobbyScenePrerequisites.IRequestWrapper
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
			Debug.LogError("EventGrowWorldTreeInitWrapper.instance.Open exception: " + Vespa_Common_Class_Extension_Method.TOSTRING(i_object));
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
		SceneAutoBehaviour<EventGrowWorldTreeManagement>.instance.RequestEventStepInfo(delegate(EventStepInfo eventStepInfo)
		{
			if (GlobalAutoBehaviour<AccountManager>.instance == null)
			{
				IsDone = true;
			}
			else if (GlobalAutoBehaviour<AccountManager>.instance.EventGrowWorldTreeManager == null)
			{
				IsDone = true;
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.EventGrowWorldTreeManager.PlayerEventStepInfo = eventStepInfo;
				LobbyManagement instance = SceneAutoBehaviour<LobbyManagement>.instance;
				if (instance != null)
				{
					LobbyRightGridWindow instance2 = instance.GetInstance<LobbyRightGridWindow>();
					if (instance2 != null)
					{
						StaminaConsumePointInfoComponent componentInChildren = instance2.GetComponentInChildren<StaminaConsumePointInfoComponent>();
						if (componentInChildren != null)
						{
							componentInChildren.UpdateWorldGrowTreeEventInfo();
						}
					}
				}
				if (SceneAutoBehaviour<WorldMapManagement>.isValidInstance)
				{
					SceneAutoBehaviour<WorldMapManagement>.instance.UpdateEventWorldTreeIndicator();
				}
				IsDone = true;
			}
		}, delegate(string errorMessage)
		{
			VespaDebug.LogError(errorMessage);
			IsDone = true;
		});
	}
}
