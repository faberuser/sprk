using System.Collections.Generic;

namespace NShared.NCampaign.Karma;

public class KarmaUpdater : CampaignUpdater
{
	public int PunishmentGroupIndex;

	public KarmaDungeonData CurrentLevelKarmaData;

	public int CurrentKarmaLevel;

	public KarmaUpdater(BattleInstance battleInstance, int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty, bool scenario, List<IBattleLogRuntime> initialBattleLogList, int randomSeedByMonth, int[] projectileGroupIndicesFromServer = null)
		: base(battleInstance, chapterIndex, dungeonIndex, difficulty, scenario, initialBattleLogList, randomSeedByMonth, projectileGroupIndicesFromServer)
	{
		KarmaDungeonData dataByChapterDungeon = GameRoot.Instance.KarmaDungeonDataContainer.GetDataByChapterDungeon(chapterIndex, dungeonIndex);
		if (dataByChapterDungeon != null)
		{
			CurrentLevelKarmaData = dataByChapterDungeon;
			PunishmentGroupIndex = dataByChapterDungeon.PunishmentGroupIndex;
		}
	}

	public void SetNextLevel()
	{
		base.WaveIndex = 1;
		if (CurrentLevelKarmaData != null)
		{
			int nextLevel = CurrentLevelKarmaData.NextLevel;
			KarmaDungeonData data = GameRoot.Instance.KarmaDungeonDataContainer.GetData(PunishmentGroupIndex, nextLevel);
			if (data != null)
			{
				CurrentLevelKarmaData = data;
				CurrentKarmaLevel = data.StageLevel;
				base.ChapterIndex = data.ChapterIndex;
				base.DungeonIndex = data.DungeonIndex;
				base.MaxStageLevel++;
			}
		}
	}
}
