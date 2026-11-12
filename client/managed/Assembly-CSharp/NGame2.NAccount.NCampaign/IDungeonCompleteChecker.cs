using NShared;

namespace NGame2.NAccount.NCampaign;

public interface IDungeonCompleteChecker
{
	bool IsCompleted(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty);
}
