using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NTask;
using NVespa.NTask.NRuntime;

namespace NGame2.NLobby.NTask;

public class MoveToLastConquestDungeon : MoveToNode
{
	public override ITask Create(string[] values)
	{
		int latelyConquestChapterIndex = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.LatelyConquestChapterIndex;
		int latelyConquestDungeonIndex = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.LatelyConquestDungeonIndex;
		if (!MoveToLastDungeon.IsDungeonNode(latelyConquestChapterIndex, latelyConquestDungeonIndex))
		{
			CampaignDungeonKey firstConquestDungeon = GetFirstConquestDungeon(2);
			if (firstConquestDungeon != null)
			{
				return new DelegateTask(Run(firstConquestDungeon.Item1, firstConquestDungeon.Item2, runNodeAction: false));
			}
		}
		if (!MoveToLastDungeon.IsDungeonNode(latelyConquestChapterIndex, latelyConquestDungeonIndex))
		{
			CenterMessage.Open(LocalizationManager.GetString(WorldMap.WORLDMAP_CANNOT_VISIT_NODE));
			return null;
		}
		return new DelegateTask(Run(latelyConquestChapterIndex, latelyConquestDungeonIndex, runNodeAction: false));
	}

	private CampaignDungeonKey GetFirstConquestDungeon(int chapterIndex)
	{
		IList<CampaignDungeonData> allDataOfBattleType = GameRoot.Instance.CampaignDungeonDataContainer.GetAllDataOfBattleType(BattleType.Conquest, chapterIndex);
		if (__Extension_Method_ICollection__.COUNT(allDataOfBattleType) == 0)
		{
			return null;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allDataOfBattleType); i++)
		{
			CampaignDungeonData campaignDungeonData = allDataOfBattleType[i];
			if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanVisit(campaignDungeonData.ChapterIndex, campaignDungeonData.DungeonIndex))
			{
				return new CampaignDungeonKey(campaignDungeonData.ChapterIndex, campaignDungeonData.DungeonIndex);
			}
		}
		return null;
	}
}
