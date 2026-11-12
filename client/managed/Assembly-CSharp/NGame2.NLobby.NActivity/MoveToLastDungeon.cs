using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class MoveToLastDungeon
{
	private MoveToNode _moveTo;

	public static IActivity Create(string[] values)
	{
		bool valueBool = ArgParser.GetValueBool(values, 0);
		int num = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.LatelyBattleChapterIndex;
		int num2 = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.LatelyBattleDungeonIndex;
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(num);
		if (data == null || !data.MainField || !IsDungeonNode(num, num2))
		{
			ChapterDungeonInfo[] values2 = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.GetValues();
			if (__Extension_Method_ICollection__.COUNT(values2) > 0)
			{
				for (int num3 = __Extension_Method_ICollection__.COUNT(values2) - 1; num3 >= 0; num3--)
				{
					ChapterDungeonInfo chapterDungeonInfo = values2[num3];
					CampaignChapterData data2 = GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterDungeonInfo.ChapterIndex);
					if (data2 != null && data2.MainField)
					{
						CampaignDungeonData data3 = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterDungeonInfo.ChapterIndex, chapterDungeonInfo.DungeonIndex);
						if (data3 != null && (data3.BattleType == BattleType.Campaign || data3.BattleType == BattleType.Boss || data3.BattleType == BattleType.Hideout))
						{
							num = chapterDungeonInfo.ChapterIndex;
							num2 = chapterDungeonInfo.DungeonIndex;
							break;
						}
					}
				}
			}
		}
		if (valueBool && IsDungeonNode(num, num2 + 1))
		{
			num2++;
		}
		if (!IsDungeonNode(num, num2))
		{
			CenterMessage.Open(LocalizationManager.GetString(WorldMap.WORLDMAP_CANNOT_VISIT_NODE));
			return null;
		}
		return new MoveToNode(num, num2, executeNodeFunction: false, warp: false);
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
