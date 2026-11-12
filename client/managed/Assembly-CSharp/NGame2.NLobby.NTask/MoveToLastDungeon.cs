using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NTask;
using NVespa.NTask.NRuntime;

namespace NGame2.NLobby.NTask;

public class MoveToLastDungeon : MoveToNode
{
	public override ITask Create(string[] values)
	{
		bool valueBool = ArgParser.GetValueBool(values, 0);
		int chapterIndex = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.LatelyBattleChapterIndex;
		int num = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.LatelyBattleDungeonIndex;
		if (!IsDungeonNode(chapterIndex, num))
		{
			ChapterDungeonInfo[] values2 = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.GetValues();
			if (__Extension_Method_ICollection__.COUNT(values2) > 0)
			{
				for (int num2 = __Extension_Method_ICollection__.COUNT(values2) - 1; num2 >= 0; num2--)
				{
					ChapterDungeonInfo chapterDungeonInfo = values2[num2];
					CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterDungeonInfo.ChapterIndex, chapterDungeonInfo.DungeonIndex);
					if (data != null && (data.BattleType == BattleType.Campaign || data.BattleType == BattleType.Boss || data.BattleType == BattleType.Hideout))
					{
						chapterIndex = chapterDungeonInfo.ChapterIndex;
						num = chapterDungeonInfo.DungeonIndex;
						break;
					}
				}
			}
		}
		if (valueBool && IsDungeonNode(chapterIndex, num + 1))
		{
			num++;
		}
		if (!IsDungeonNode(chapterIndex, num))
		{
			CenterMessage.Open(LocalizationManager.GetString(WorldMap.WORLDMAP_CANNOT_VISIT_NODE));
			return null;
		}
		return new DelegateTask(Run(chapterIndex, num, runNodeAction: false));
	}

	public static bool IsDungeonNode(int chapterIndex, int dungeonIndex)
	{
		if (chapterIndex == 0 || dungeonIndex == 0)
		{
			return false;
		}
		return GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, dungeonIndex)?.Function.IsBattleFunction() ?? false;
	}
}
