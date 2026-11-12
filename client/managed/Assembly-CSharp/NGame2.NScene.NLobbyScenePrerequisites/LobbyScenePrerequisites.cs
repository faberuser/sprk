using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI3.NAttendance;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NScene.NLobbyScenePrerequisites;

public class LobbyScenePrerequisites
{
	private class RequestWrapperHandler
	{
		private List<NGame2.NScene.NLobbyScenePrerequisites.IRequestWrapper> _list = new List<NGame2.NScene.NLobbyScenePrerequisites.IRequestWrapper>();

		public void RequestWrapper(params NGame2.NScene.NLobbyScenePrerequisites.IRequestWrapper[] wrappers)
		{
			_list.Clear();
			if (wrappers == null)
			{
				return;
			}
			_list.AddRange(wrappers);
			foreach (NGame2.NScene.NLobbyScenePrerequisites.IRequestWrapper item in _list)
			{
				item.Init();
			}
		}

		public bool IsAllDoneOrTimeOut()
		{
			int num;
			for (num = 0; num < __Extension_Method_ICollection__.COUNT(_list); num++)
			{
				if (!_list[num].IsDoneOrTimeout)
				{
					return false;
				}
				_list.RemoveAt(num);
				num--;
			}
			return true;
		}
	}

	private static RequestWrapperHandler _requestWrapperHandler_before_RequestEnterLobby = new RequestWrapperHandler();

	private static RequestWrapperHandler _requestWrapperHandler_after_RequestEnterLobby = new RequestWrapperHandler();

	public static void Request(Action onComplete)
	{
		VespaCoroutine.StartScene(coLoop(onComplete));
	}

	private static IEnumerator coLoop(Action onComplete)
	{
		OpenLobbyWindows();
		NGame2.NScene.NLobbyScenePrerequisites.QueryServerDownWrapper queryServerDown = new NGame2.NScene.NLobbyScenePrerequisites.QueryServerDownWrapper();
		queryServerDown.Query();
		NGame2.NScene.NLobbyScenePrerequisites.QueryMarketInfoWrapper queryMarketInfo = new NGame2.NScene.NLobbyScenePrerequisites.QueryMarketInfoWrapper();
		queryMarketInfo.Query();
		NGame2.NScene.NLobbyScenePrerequisites.WorldMapOpenWrapper worldMapOpen = new NGame2.NScene.NLobbyScenePrerequisites.WorldMapOpenWrapper();
		worldMapOpen.Open();
		Debug.Log("request wait begin");
		while (!queryServerDown.IsDoneOrTimeout)
		{
			yield return null;
		}
		Debug.Log("completeQueryServerDown");
		while (!queryMarketInfo.IsDoneOrTimeout)
		{
			yield return null;
		}
		Debug.Log("completeQueryMarketInfo");
		while (!worldMapOpen.IsDoneOrTimeout)
		{
			yield return null;
		}
		Debug.Log("completeWorldMapOpen");
		if (!queryServerDown.QuitIfServerDown() && !queryMarketInfo.CheckNewAppArrival())
		{
			Debug.Log("raise chapter move event");
			NGame2.NScene.NLobbyScenePrerequisites.RaiseChapterMoveEventWrapper raiseChapterMoveEvent = new NGame2.NScene.NLobbyScenePrerequisites.RaiseChapterMoveEventWrapper();
			raiseChapterMoveEvent.Raise();
			while (!raiseChapterMoveEvent.IsDoneOrTimeout)
			{
				yield return null;
			}
			Debug.Log("Chapter move event ended.");
			_requestWrapperHandler_before_RequestEnterLobby.RequestWrapper(new BonusAttendance20221001_Facade.InitWrapper_when_EnterLobby());
			while (!_requestWrapperHandler_before_RequestEnterLobby.IsAllDoneOrTimeOut())
			{
				yield return null;
			}
			RequestEnterLobbyWrapper requestEnterLobby = new RequestEnterLobbyWrapper();
			requestEnterLobby.Request();
			while (!requestEnterLobby.IsDoneOrTimeout)
			{
				yield return null;
			}
			requestEnterLobby.HandleResponse();
			_requestWrapperHandler_after_RequestEnterLobby.RequestWrapper(new NGame2.NScene.NLobbyScenePrerequisites.GuildAttendanceInitWrapper(), new NGame2.NScene.NLobbyScenePrerequisites.WorldBossInitWrapper(), new NGame2.NScene.NLobbyScenePrerequisites.TreasureHouseInitWrapper());
			while (!_requestWrapperHandler_after_RequestEnterLobby.IsAllDoneOrTimeOut())
			{
				yield return null;
			}
			Vespa_System_Action_Extension_Method.fire(onComplete);
		}
	}

	private static void OpenLobbyWindows()
	{
		try
		{
			Debug.Log("OpenLobbyWindows");
			SceneAutoBehaviour<LobbyManagement>.instance.OpenLobbyWindows();
			SceneAutoBehaviour<LobbyCampaignDungeon>.CreateInstance();
			SceneAutoBehaviour<RaidManagement>.CreateInstance();
			SceneAutoBehaviour<WorldBossManagement>.CreateInstance();
			SceneAutoBehaviour<MyHeroManagement>.instance.Init();
			SceneAutoBehaviour<ChattingManagement>.CreateInstance();
			SceneAutoBehaviour<MultiPartyManagement>.CreateInstance();
			SceneAutoBehaviour<SubStoryManagement>.CreateInstance();
			SceneAutoBehaviour<OrdealArenaManagement>.CreateInstance();
		}
		catch (Exception i_object)
		{
			Debug.LogError("exception: " + Vespa_Common_Class_Extension_Method.TOSTRING(i_object));
		}
	}
}
