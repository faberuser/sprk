using NGame2.NAccount;
using NGame2.NBattleContext;
using NShared;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUtil.NDataHelper;

public class DungeonHelper
{
	public static string GetDungeonName(int chapterIndex, int dungeonIndex)
	{
		RaidData raidDataByDungeon = GameRoot.Instance.RaidDataContainer.GetRaidDataByDungeon(chapterIndex, dungeonIndex);
		if (raidDataByDungeon != null)
		{
			return GetDungeonName(raidDataByDungeon);
		}
		return GetDungeonName(GetEnterDungeonData(chapterIndex, dungeonIndex));
	}

	public static EnterDungeonData GetEnterDungeonData(int chapterIndex, int dungeonIndex)
	{
		DungeonDifficultyType difficulty = (DungeonDifficultyType)Helper.GetDifficulty(chapterIndex, dungeonIndex);
		bool isScenario = false;
		return GetEnterDungeonData(chapterIndex, dungeonIndex, difficulty, isScenario);
	}

	public static EnterDungeonData GetEnterDungeonData(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty, bool isScenario)
	{
		int linkedDungeonIndex = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.GetLinkedDungeonIndex(chapterIndex, dungeonIndex);
		return new EnterDungeonData(chapterIndex, dungeonIndex, linkedDungeonIndex, difficulty, isScenario);
	}

	public static string GetDungeonName(EnterDungeonData enterDungeonData)
	{
		if (enterDungeonData == null)
		{
			return "";
		}
		CampaignNodeData linkedNodeData = enterDungeonData.LinkedNodeData;
		if (linkedNodeData == null)
		{
			CampaignDungeonData linkedDungeonData = enterDungeonData.LinkedDungeonData;
			if (linkedDungeonData == null)
			{
				return null;
			}
			EventDungeonData data = GameRoot.Instance.EventDungeonDataContainer.GetData(linkedDungeonData.ChapterIndex, linkedDungeonData.DungeonIndex);
			if (data != null)
			{
				return LocalizationManager.GetStringDirect(data.DungeonName);
			}
		}
		else if (!String_Extension_Method.IsNullOrEmpty(linkedNodeData.PortalNameCode))
		{
			return LocalizationManager.GetStringDirect(linkedNodeData.PortalNameCode);
		}
		return linkedNodeData?.Name;
	}

	public static string GetDungeonName(RaidData raidData)
	{
		if (raidData == null)
		{
			return null;
		}
		return LocalizationManager.GetStringDirect(raidData.Name);
	}
}
