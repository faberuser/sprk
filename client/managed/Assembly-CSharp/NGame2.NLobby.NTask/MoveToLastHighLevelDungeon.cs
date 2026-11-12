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

public class MoveToLastHighLevelDungeon : MoveToNode
{
	public override ITask Create(string[] values)
	{
		int latelyHighLevelChapterIndex = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.LatelyHighLevelChapterIndex;
		int latelyHighLevelDungeonIndex = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.LatelyHighLevelDungeonIndex;
		if (!MoveToLastDungeon.IsDungeonNode(latelyHighLevelChapterIndex, latelyHighLevelDungeonIndex))
		{
			CampaignDungeonKey firstHighLevelDungeon = GetFirstHighLevelDungeon(1);
			if (firstHighLevelDungeon != null)
			{
				return new DelegateTask(Run(firstHighLevelDungeon.Item1, firstHighLevelDungeon.Item2, runNodeAction: false));
			}
		}
		if (!MoveToLastDungeon.IsDungeonNode(latelyHighLevelChapterIndex, latelyHighLevelDungeonIndex))
		{
			CenterMessage.Open(LocalizationManager.GetString(WorldMap.WORLDMAP_CANNOT_VISIT_NODE));
			return null;
		}
		return new DelegateTask(Run(latelyHighLevelChapterIndex, latelyHighLevelDungeonIndex, runNodeAction: false));
	}

	private CampaignDungeonKey GetFirstHighLevelDungeon(int chapterIndex)
	{
		List<CampaignNodeData> allDataOfFunctionType = GameRoot.Instance.CampaignNodeDataContainer.GetAllDataOfFunctionType(NodeFunctionType.MoveChapter, chapterIndex);
		if (__Extension_Method_ICollection__.COUNT(allDataOfFunctionType) == 0)
		{
			return null;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allDataOfFunctionType); i++)
		{
			CampaignNodeData campaignNodeData = allDataOfFunctionType[i];
			if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanVisit(campaignNodeData.ChapterIndex, campaignNodeData.DungeonIndex))
			{
				int valueInt = campaignNodeData.GetValueInt(0);
				CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(valueInt, 2);
				if (data != null && data.ReqStaminaType == StaminaType.HideoutKey)
				{
					return new CampaignDungeonKey(valueInt, 1);
				}
			}
		}
		return null;
	}
}
