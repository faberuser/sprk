using System;
using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWorldMap;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class MoveToNpc : IActivity
{
	private string _npcCode;

	private bool _executeNodeFunction;

	private bool _warp;

	private Action _onComplete;

	public static IActivity Create(string[] values)
	{
		string valueString = ArgParser.GetValueString(values, 0);
		bool valueBool = ArgParser.GetValueBool(values, 1);
		bool valueBool2 = ArgParser.GetValueBool(values, 2);
		return new MoveToNpc(valueString, valueBool, valueBool2, null);
	}

	public MoveToNpc(string npcCode, bool executeNodeFunction, bool warp, Action onComplete)
	{
		_npcCode = npcCode;
		_executeNodeFunction = executeNodeFunction;
		_warp = warp;
		_onComplete = onComplete;
	}

	IEnumerator IActivity.coStart()
	{
		CampaignNodeData entryMenuNode = GameRoot.Instance.CampaignNodeDataContainer.GetEntryMenuNode(_npcCode);
		if (entryMenuNode == null)
		{
			yield break;
		}
		int chapterIndex = entryMenuNode.ChapterIndex;
		int dungeonIndex = entryMenuNode.DungeonIndex;
		bool targetNodeCanVisit = true;
		while (dungeonIndex > 1 && !GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanVisit(chapterIndex, dungeonIndex))
		{
			dungeonIndex--;
			targetNodeCanVisit = false;
		}
		if (!GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanVisit(chapterIndex, dungeonIndex))
		{
			CenterMessage.Open(LocalizationManager.GetString(WorldMap.WORLDMAP_CANNOT_VISIT_NODE));
			yield break;
		}
		bool moveStarted = false;
		SceneAutoBehaviour<WorldMapManagement>.instance.MovePlayerToDungeon(chapterIndex, dungeonIndex, DisableTouch: false, _warp, delegate
		{
			moveStarted = true;
		});
		while (!moveStarted)
		{
			yield return null;
		}
		MapLayout mayLayout = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
		while (SceneAutoBehaviour<WorldMapManagement>.instance.IsMapLoading || mayLayout.IsPlayerMoving())
		{
			yield return null;
		}
		if (_executeNodeFunction)
		{
			if (NGame2.NAccount.UserCampaignManager.DungeonIndex == dungeonIndex)
			{
				SceneAutoBehaviour<LobbyManagement>.instance.ExecuteCurrentNode(_onComplete);
			}
			else if (!targetNodeCanVisit && NGame2.NAccount.UserCampaignManager.DungeonIndex == dungeonIndex)
			{
				CenterMessage.Open(LocalizationManager.GetString(WorldMap.WORLDMAP_CANNOT_VISIT_NODE));
			}
		}
	}

	void IActivity.OnRequestStop()
	{
		if (SceneAutoBehaviour<LobbyManagement>.instance != null)
		{
			SceneAutoBehaviour<LobbyManagement>.instance.CheckUpdatePlayGuide = true;
			SceneAutoBehaviour<LobbyManagement>.instance.CheckPlayGuideStartCourtineStatus = true;
		}
	}
}
