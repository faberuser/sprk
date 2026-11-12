using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NShared.BeginCampaign;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class CampaignReturnState : LobbyScene2.IReturnState
{
	private EnterDungeonData _enterDungeon;

	public string BgmCode => string.Empty;

	public CampaignReturnState(EnterDungeonData enterDungeon)
	{
		_enterDungeon = enterDungeon;
	}

	public void Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		ExecuteInternal(info);
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}

	private void ExecuteInternal(LobbyScene2.ReturnStateInfo info)
	{
		if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenHeroDeck) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<MyHeroManagement>.instance.OpenMyDeck();
		}
		else if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenPartySetting) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<LobbyCampaignDungeon>.instance.OpenDungeonInfo(_enterDungeon.Chapter, _enterDungeon.OriginalDungeon, _enterDungeon.LinkedDungeon);
			if (GameOptionData.EditorCanAutoPlay)
			{
				return;
			}
			if (GlobalAutoBehaviour<AccountManager>.instance.IsMaxEquipItemCount())
			{
				string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
				string message = LocalizationManager.GetString(ResultType.EquipItemFull);
				MessagePopup.OpenEnabled(title, message, delegate
				{
					SceneAutoBehaviour<InventoryManagement>.instance.OpenInventory();
				}, null, "MOVE_BAG", bYesEnabled: true);
			}
			else
			{
				SceneAutoBehaviour<LobbyCampaignDungeon>.instance.OpenPartySetting(_enterDungeon);
			}
		}
		else if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenNextDungeon) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			if (_enterDungeon.OriginalDungeonData == null)
			{
				return;
			}
			int nextDungeonIndex = _enterDungeon.OriginalDungeonData.NextDungeonIndex;
			if (GameRoot.Instance.CampaignDungeonDataContainer.GetData(_enterDungeon.Chapter, nextDungeonIndex) != null)
			{
				int linkedDungeonIndex = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.GetLinkedDungeonIndex(_enterDungeon.Chapter, nextDungeonIndex);
				SceneAutoBehaviour<WorldMapManagement>.instance.MovePlayerToDungeon(_enterDungeon.Chapter, nextDungeonIndex, DisableTouch: false, warp: true);
				if (Helper.GetFunctionState(_enterDungeon.Chapter, linkedDungeonIndex, containsDisableText: false).Type.IsBattleFunction())
				{
					SceneAutoBehaviour<LobbyCampaignDungeon>.instance.OpenDungeonInfo(_enterDungeon.Chapter, nextDungeonIndex, linkedDungeonIndex);
				}
			}
		}
		else if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenInventory) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<InventoryManagement>.instance.OpenInventory();
		}
		else if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenCashShop) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
		}
		else if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenNewbieShop) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Beginner.ToString());
		}
		else if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenEventShop) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Event.ToString());
		}
		else if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenGameGuide) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<LobbyManagement>.instance.OpenGameGuideWindow();
		}
		else if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenNextChapterConquestDungeon) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			if (_enterDungeon.OriginalDungeonData == null)
			{
				return;
			}
			int chapter = _enterDungeon.Chapter;
			CampaignDungeonData[] conquestDungeonData = GameRoot.Instance.CampaignDungeonDataContainer.GetConquestDungeonData(chapter + 1);
			if (__Extension_Method_ICollection__.COUNT(conquestDungeonData) == 0)
			{
				AutoNextBattle.StopWithClickButton();
				return;
			}
			CampaignDungeonData campaignDungeonData = conquestDungeonData[0];
			if (campaignDungeonData == null)
			{
				AutoNextBattle.StopWithClickButton();
				return;
			}
			int linkedDungeonIndex2 = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.GetLinkedDungeonIndex(campaignDungeonData.ChapterIndex, campaignDungeonData.DungeonIndex);
			SceneAutoBehaviour<WorldMapManagement>.instance.MovePlayerToDungeon(campaignDungeonData.ChapterIndex, campaignDungeonData.DungeonIndex, DisableTouch: false, warp: true);
			if (Helper.GetFunctionState(campaignDungeonData.ChapterIndex, campaignDungeonData.DungeonIndex, containsDisableText: false).Type.IsBattleFunction())
			{
				SceneAutoBehaviour<LobbyCampaignDungeon>.instance.OpenDungeonInfo(campaignDungeonData.ChapterIndex, campaignDungeonData.DungeonIndex, linkedDungeonIndex2);
			}
		}
		else
		{
			if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenNextChapterHideOut) == 0 || _enterDungeon.OriginalDungeonData == null)
			{
				return;
			}
			int chapter2 = _enterDungeon.Chapter;
			CampaignDungeonData campaignDungeonData2 = null;
			IList<CampaignDungeonData> allDataOfBattleType = GameRoot.Instance.CampaignDungeonDataContainer.GetAllDataOfBattleType(BattleType.Hideout, chapter2);
			IList<CampaignDungeonData> allDataOfBattleType2 = GameRoot.Instance.CampaignDungeonDataContainer.GetAllDataOfBattleType(BattleType.Hideout, chapter2 + 1);
			if (allDataOfBattleType2.Count <= 0)
			{
				AutoNextBattle.StopWithClickButton();
				return;
			}
			int num = -1;
			for (int num2 = 0; num2 < allDataOfBattleType.Count; num2++)
			{
				if (allDataOfBattleType[num2].ChapterIndex == _enterDungeon.Chapter && allDataOfBattleType[num2].DungeonIndex == _enterDungeon.LinkedDungeonData.DungeonIndex)
				{
					num = num2;
					break;
				}
			}
			if (allDataOfBattleType2.Count >= num)
			{
				campaignDungeonData2 = allDataOfBattleType2[num];
			}
			if (num == -1 || campaignDungeonData2 == null)
			{
				AutoNextBattle.StopWithClickButton();
				return;
			}
			int linkedDungeonIndex3 = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.GetLinkedDungeonIndex(campaignDungeonData2.ChapterIndex, campaignDungeonData2.DungeonIndex);
			SceneAutoBehaviour<WorldMapManagement>.instance.MovePlayerToDungeon(campaignDungeonData2.ChapterIndex, campaignDungeonData2.DungeonIndex, DisableTouch: false, warp: true);
			if (Helper.GetFunctionState(campaignDungeonData2.ChapterIndex, campaignDungeonData2.DungeonIndex, containsDisableText: false).Type.IsBattleFunction())
			{
				SceneAutoBehaviour<LobbyCampaignDungeon>.instance.OpenDungeonInfo(campaignDungeonData2.ChapterIndex, campaignDungeonData2.DungeonIndex, linkedDungeonIndex3);
			}
		}
	}
}
