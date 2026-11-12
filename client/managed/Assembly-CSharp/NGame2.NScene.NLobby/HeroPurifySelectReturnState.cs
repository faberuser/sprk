using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class HeroPurifySelectReturnState : LobbyScene2.IReturnState
{
	private CachedHeroInfo _heroInfo;

	public string BgmCode => string.Empty;

	public HeroPurifySelectReturnState(CachedHeroInfo heroInfo)
	{
		_heroInfo = heroInfo;
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
		if (_heroInfo != null)
		{
			HeroAwakeType heroAwakeType = (_heroInfo.IsMaxStar() ? HeroAwakeType.Transcend : HeroAwakeType.Awaken);
			SceneAutoBehaviour<HeroTrialDungeonManagement>.instance.OpenPurifyHeroSelectWindow(heroAwakeType, _heroInfo);
		}
		else
		{
			SceneAutoBehaviour<HeroTrialDungeonManagement>.instance.OpenPurifyHeroSelectWindow();
		}
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}
}
