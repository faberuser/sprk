using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class HeroTrialDungeonReturnState : LobbyScene2.IReturnState
{
	private int _selectedHeroIndex;

	private int _selectedHeroStar;

	private int _selectedHeroTranscend;

	public string BgmCode => string.Empty;

	public HeroTrialDungeonReturnState(int heroIndex, int heroStar, int heroTranscend)
	{
		_selectedHeroIndex = heroIndex;
		_selectedHeroStar = heroStar;
		_selectedHeroTranscend = heroTranscend;
	}

	void LobbyScene2.IReturnState.Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		List<CampaignNodeData> allDataOfFunctionType = GameRoot.Instance.CampaignNodeDataContainer.GetAllDataOfFunctionType(NodeFunctionType.Temple);
		if (__Extension_Method_ICollection__.COUNT(allDataOfFunctionType) == 0)
		{
			return;
		}
		int templeChapterIndex = allDataOfFunctionType[0].ChapterIndex;
		int templeDungeonIndex = allDataOfFunctionType[0].DungeonIndex;
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		if (templeChapterIndex == chapterIndex && templeDungeonIndex == dungeonIndex)
		{
			SceneAutoBehaviour<EntryMenuManagement>.instance.OpenEntryMenu(chapterIndex, dungeonIndex, skipAni: true, playNPCSound: false, isIgnoreTrigger: true, delegate
			{
				Execute(info, onEnd);
			});
			return;
		}
		SceneAutoBehaviour<LobbyManagement>.instance.MoveMap(templeChapterIndex, templeDungeonIndex, delegate
		{
			SceneAutoBehaviour<EntryMenuManagement>.instance.OpenEntryMenu(templeChapterIndex, templeDungeonIndex, skipAni: true, playNPCSound: false, isIgnoreTrigger: true, delegate
			{
				Execute(info, onEnd);
			});
		});
	}

	public void Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		SceneAutoBehaviour<HeroTrialDungeonManagement>.instance.OpenTrialChallengeHeroSelect(HeroAwakeType.Awaken, delegate
		{
			HandleOpenFlag(info.OpenFlag);
		});
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}

	private void HandleOpenFlag(LobbyScene2.ReturnStateOpenFlag openFlag)
	{
		if ((openFlag & LobbyScene2.ReturnStateOpenFlag.OpenInventory) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<InventoryManagement>.instance.OpenInventory();
		}
		else if ((openFlag & LobbyScene2.ReturnStateOpenFlag.OpenHeroDeck) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<MyHeroManagement>.instance.OpenMyDeck();
		}
		else if ((openFlag & LobbyScene2.ReturnStateOpenFlag.OpenPartySetting) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<HeroTrialDungeonManagement>.instance.OpenSelectedHeroBattlePartySetting(_selectedHeroIndex, _selectedHeroStar, _selectedHeroTranscend);
		}
		else if ((openFlag & LobbyScene2.ReturnStateOpenFlag.OpenNewbieShop) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Beginner.ToString());
		}
		else if ((openFlag & LobbyScene2.ReturnStateOpenFlag.OpenEventShop) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Event.ToString());
		}
	}
}
