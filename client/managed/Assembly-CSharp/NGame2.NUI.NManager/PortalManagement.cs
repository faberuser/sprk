using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager.Mission;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NEntryMenu;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NTask.NRuntime;
using UnityEngine;

namespace NGame2.NUI.NManager;

public class PortalManagement : BaseManagement<PortalManagement>, PortalSelect3.Listener, PortalSelect3.IDataProvider, PortalMoveToPopup.Listener
{
	protected override void Awake()
	{
		base.Awake();
		AddInstance<PortalRenewal>("Resources/Prefab2/Window/Portal/PortalRenewal");
		AddInstance<PortalSelect3>("Resources/Prefab2/Window/Portal/PortalSelect3");
	}

	public void OpenPortalRenewalWindow(PortalRenewalCategory portalRenewalCategory = PortalRenewalCategory.Chapter)
	{
		PortalRenewal portalRenewal = GetInstance<PortalRenewal>();
		if (!portalRenewal.IsOpen())
		{
			portalRenewal.Open(portalRenewalCategory);
		}
	}

	public void OpenPortalSelectWindow()
	{
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("OpenNodeEventChapterIndex");
		int valueInt2 = GameRoot.Instance.ConstantDataContainer.GetValueInt("OpenNodeEventDungeonIndex");
		if (Helper.IsAllNodeFunctionOpened(containsDisableMsg: false, out var _) != Helper.FunctionStateData.EFunctionState.Enabled)
		{
			CenterMessage.Open(LocalizationManager.GetString(Portal.CANNOT_OPEN_PORTAL_MIN_NODE, valueInt, valueInt2));
		}
		else
		{
			base.TaskFactory.Open(delegate(PortalSelect3 window)
			{
				window.Init(this, this);
			}).Commit();
		}
	}

	public void MoveDirectDungeon(int chapterIndex, int dungeonIndex)
	{
		PortalChildListPopup.OpenDungeonListPopup(chapterIndex, dungeonIndex, delegate(int x, int y)
		{
			OpenPortalMoveToPopup(x, y);
		}, delegate
		{
			CenterMessage.Open(LocalizationManager.GetString(WorldMap.WORLDMAP_CANNOT_VISIT_NODE));
		});
	}

	private void OpenPortalMoveToPopup(int chapterIndex, int dungeonIndex)
	{
		PortalData data = GameRoot.Instance.PortalDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			CampaignNodeData data2 = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, dungeonIndex);
			if (data2 != null)
			{
				PortalMoveToPopup.Open(this, data2);
			}
		}
		else
		{
			PortalMoveToPopup.Open(this, data);
		}
	}

	private IEnumerator CoOpenContentsByAdventureNotice(int chapterIndex, int dungeonIndex)
	{
		yield return new WaitForSeconds(0.3f);
		OpenContentsByAdventureNotice(chapterIndex, dungeonIndex);
	}

	private void OpenContentsByAdventureNotice(int chapterIndex, int dungeonIndex)
	{
		PortalData data = GameRoot.Instance.PortalDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			return;
		}
		AdventureNoticeData adventureNoticeData = GameRoot.Instance.AdventureNoticeDataContainer.GetData(data.AdventureNoticeCode);
		if (adventureNoticeData == null)
		{
			return;
		}
		NGame2.NAccount.UserCampaignManager.ChapterIndex = adventureNoticeData.NodeChapterIndex;
		NGame2.NAccount.UserCampaignManager.DungeonIndex = adventureNoticeData.NodeDungeonIndex;
		new SequenceTask().ADDTASK(base.TaskFactory.Run(delegate
		{
			string valueString = ArgParser.GetValueString(adventureNoticeData.Action, 0);
			if (!String_Extension_Method.IsNullOrEmpty(valueString))
			{
				NGame2.NUI.NWindow.NEntryMenu.IAction action = SingletonAuto<ActionContainer>.instance.GetAction(valueString);
				if (action != null)
				{
					List<object> list = new List<object>();
					string[] action2 = adventureNoticeData.Action;
					foreach (string item in action2)
					{
						list.Add(item);
					}
					action.Run(null, null, list);
				}
			}
		})).WrapperTouchControl().DisableTouch()
			.Commit();
	}

	void PortalSelect3.Listener.MoveMap(int ChapterIndex, int DungeonIndex)
	{
		OpenPortalMoveToPopup(ChapterIndex, DungeonIndex);
	}

	void PortalSelect3.Listener.OpenContentsShortCut(int chapterIndex, int dungeonIndex)
	{
	}

	void PortalSelect3.Listener.OpenPopup(int chapterIndex, int dungeonIndex)
	{
		MoveDirectDungeon(chapterIndex, dungeonIndex);
	}

	void PortalSelect3.Listener.OnClickQuestButton()
	{
		SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.OpenMissionMain();
	}

	void PortalSelect3.Listener.OnClickMissionButton(MissionComponentData data)
	{
		SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.OpenMissionMain();
		SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.OpenMissionPopup(data);
	}

	void PortalSelect3.Listener.ClickMoveLastBattlePos()
	{
		int latelyBattleChapterIndex = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.LatelyBattleChapterIndex;
		int latelyBattleDungeonIndex = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.LatelyBattleDungeonIndex;
		CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(latelyBattleChapterIndex, latelyBattleDungeonIndex);
		if (data != null)
		{
			PortalMoveToPopup.Open(this, data);
		}
	}

	bool PortalSelect3.IDataProvider.CanGoToLatelyBattleNode()
	{
		int latelyBattleChapterIndex = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.LatelyBattleChapterIndex;
		int latelyBattleDungeonIndex = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.LatelyBattleDungeonIndex;
		if (latelyBattleChapterIndex == 0 || latelyBattleDungeonIndex == 0)
		{
			return false;
		}
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		if (latelyBattleChapterIndex == chapterIndex && latelyBattleDungeonIndex == dungeonIndex)
		{
			return false;
		}
		return true;
	}

	void PortalMoveToPopup.Listener.MoveNode(int chapterIndex, int dungeonIndex)
	{
		SceneAutoBehaviour<LobbyManagement>.instance.MoveNode(chapterIndex, dungeonIndex);
		PortalSelect3 portalSelect = GetInstance<PortalSelect3>();
		if (portalSelect != null && portalSelect.IsOpen())
		{
			portalSelect.Close();
		}
		PortalRenewal portalRenewal = GetInstance<PortalRenewal>();
		if (portalRenewal != null && portalRenewal.IsOpen())
		{
			portalRenewal.Close();
		}
	}
}
