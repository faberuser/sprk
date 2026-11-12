using System.Collections.Generic;
using NGame2.NOrdeal;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NAccount.NCampaign;

public class DungeonCompleteCheckerContainer : SingletonAuto<DungeonCompleteCheckerContainer>
{
	private Dictionary<BattleType, IDungeonCompleteChecker> _chckerDictionary = new Dictionary<BattleType, IDungeonCompleteChecker>();

	public DungeonCompleteCheckerContainer()
	{
		_chckerDictionary.Add(BattleType.GuildRaid, new GuildRaidDungeonCompleteChecker());
	}

	public bool Check(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty)
	{
		if (IsOrdealArenaChapter(chapterIndex))
		{
			return CheckOrdealArenaOthers(chapterIndex, dungeonIndex);
		}
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			return false;
		}
		IDungeonCompleteChecker value = null;
		_chckerDictionary.TryGetValue(data.BattleType, out value);
		return value?.IsCompleted(chapterIndex, dungeonIndex, difficulty) ?? CheckDefault(chapterIndex, dungeonIndex, difficulty);
	}

	private bool CheckDefault(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty)
	{
		ChapterDungeonInfo chapterDungeonInfo = (AccountManager.IsLogined() ? GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.Get(chapterIndex, dungeonIndex) : null);
		if (chapterDungeonInfo == null)
		{
			return false;
		}
		if (string.IsNullOrEmpty(chapterDungeonInfo.CompletedTime))
		{
			return false;
		}
		if (chapterDungeonInfo.MaxStar / 10 < (int)difficulty)
		{
			return false;
		}
		return true;
	}

	private bool IsOrdealArenaChapter(int chapterIndex)
	{
		return GameRoot.Instance.OrdealArenaNodeDataContainer.IsOrdealArenaChapter(chapterIndex);
	}

	private bool CheckOrdealArenaOthers(int chapterIndex, int dungeonIndex)
	{
		return GlobalAutoBehaviour<OrdealArenaManager>.instance.IsCompleteNode(chapterIndex, dungeonIndex);
	}
}
