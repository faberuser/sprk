using System;
using System.Collections;
using NGame2.NAccount;
using NGame2.NGuild;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWorldMap;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NLobby.NActivity;

public class MoveToNode : IActivity
{
	private int _chapterIndex;

	private int _targetNode;

	private bool _executeNodeFunction;

	private bool _warp;

	private bool _enterCanVisitLastNode;

	private string _extraParameter1;

	private string _extraParameter2;

	private string _extraParameter3;

	public static IActivity Create(string[] values)
	{
		int valueInt = ArgParser.GetValueInt(values, 0);
		int valueInt2 = ArgParser.GetValueInt(values, 1);
		bool valueBool = ArgParser.GetValueBool(values, 2);
		bool valueBool2 = ArgParser.GetValueBool(values, 3);
		bool valueBool3 = ArgParser.GetValueBool(values, 4);
		string valueString = ArgParser.GetValueString(values, 5);
		string valueString2 = ArgParser.GetValueString(values, 6);
		string valueString3 = ArgParser.GetValueString(values, 7);
		return new MoveToNode(valueInt, valueInt2, valueBool, valueBool2, valueBool3, valueString, valueString2, valueString3);
	}

	public MoveToNode(int chapterIndex, int targetNode, bool executeNodeFunction, bool warp, bool enterCanVisitLastNode = false, string extraParameter1 = "", string extraParameter2 = "", string extraParameter3 = "")
	{
		_chapterIndex = chapterIndex;
		_targetNode = targetNode;
		_executeNodeFunction = executeNodeFunction;
		_warp = warp;
		_enterCanVisitLastNode = enterCanVisitLastNode;
		_extraParameter1 = extraParameter1;
		_extraParameter2 = extraParameter2;
		_extraParameter3 = extraParameter3;
	}

	IEnumerator IActivity.coStart()
	{
		if (_chapterIndex == 999 && _targetNode == 7 && GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info != null)
		{
			_chapterIndex = 995;
			_targetNode = 1;
		}
		int canVisitLastNode = _targetNode;
		bool targetNodeCanVisit = true;
		while (canVisitLastNode > 1 && !GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanVisit(_chapterIndex, canVisitLastNode))
		{
			canVisitLastNode--;
			targetNodeCanVisit = false;
		}
		if (!GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanVisit(_chapterIndex, canVisitLastNode))
		{
			CenterMessage.Open(LocalizationManager.GetString(WorldMap.WORLDMAP_CANNOT_VISIT_NODE));
			Debug.LogError("You cannot go there...");
			yield break;
		}
		bool moveStarted = false;
		SceneAutoBehaviour<WorldMapManagement>.instance.MovePlayerToDungeon(_chapterIndex, canVisitLastNode, DisableTouch: false, _warp, delegate
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
		if (!_executeNodeFunction)
		{
			yield break;
		}
		if (NGame2.NAccount.UserCampaignManager.DungeonIndex == _targetNode)
		{
			Action after = null;
			if (_extraParameter1 != "")
			{
				after = GetExtraAction(_extraParameter1, _extraParameter2, _extraParameter3, _chapterIndex, _targetNode);
			}
			SceneAutoBehaviour<LobbyManagement>.instance.ExecuteCurrentNode(after);
		}
		else if (!targetNodeCanVisit && NGame2.NAccount.UserCampaignManager.DungeonIndex == canVisitLastNode)
		{
			if (_enterCanVisitLastNode)
			{
				SceneAutoBehaviour<LobbyManagement>.instance.ExecuteCurrentNode(null);
			}
			else
			{
				CenterMessage.Open(LocalizationManager.GetString(WorldMap.WORLDMAP_CANNOT_VISIT_NODE));
			}
		}
	}

	private Action GetExtraAction(string extraParameter1, string extraParameter2, string extraParameter3, int chapterIndex, int targetNode)
	{
		Action result = null;
		NodeFunctionType type = Helper.GetFunctionState(chapterIndex, targetNode, containsDisableText: true).Type;
		if (type == NodeFunctionType.EntryMenu || type == NodeFunctionType.Temple || type == NodeFunctionType.NPC)
		{
			NGame2.NUI.NWindow.EntryMenu entryMenu = SceneAutoBehaviour<EntryMenuManagement>.instance.GetInstance<NGame2.NUI.NWindow.EntryMenu>();
			if (!int.TryParse(extraParameter2, out var index))
			{
				return null;
			}
			switch (extraParameter1)
			{
			case "EntryMenu":
			{
				if (!String_Extension_Method.IsNullOrEmpty(extraParameter3) && int.TryParse(extraParameter3, out var result2))
				{
					SceneAutoBehaviour<EntryMenuManagement>.instance.ForceShopGroupIndex = result2;
				}
				result = delegate
				{
					entryMenu.OpenEntryMenu(index);
				};
				break;
			}
			case "BottomMenu":
				result = delegate
				{
					entryMenu.OpenBottomMenu(index);
				};
				break;
			case "OpenShop":
				if (!String_Extension_Method.IsNullOrEmpty(extraParameter3))
				{
					if (int.TryParse(extraParameter3, out var groupIndex))
					{
						result = delegate
						{
							entryMenu.OpenEntryMenuByShopIndex(extraParameter1, index, groupIndex);
						};
						break;
					}
				}
				result = delegate
				{
					entryMenu.OpenEntryMenuByShopIndex(extraParameter1, index);
				};
				break;
			}
		}
		return result;
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
