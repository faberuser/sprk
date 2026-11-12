using System;
using System.Collections.Generic;
using NShared.NCampaign;
using NVespa;

namespace NShared;

public class BattleTest
{
	public static BattleInstance Run<TBattleInstance>(int chapterIndex, int dungeonIndex, int difficulty, int runTimeMs, IBattleEventServiceContext context = null, Func<bool> stopFunc = null) where TBattleInstance : BattleInstance, new()
	{
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			VespaDebug.LogError("Dungeon not found : chapterIndex=" + chapterIndex + " dungeonIndex=" + dungeonIndex);
			return null;
		}
		new DefeatAllEnemy();
		bool flag = false;
		BattleInstanceBuilder battleInstanceBuilder = new BattleInstanceBuilder();
		battleInstanceBuilder.SetCampaignDungeon(chapterIndex, dungeonIndex, difficulty);
		battleInstanceBuilder.SetBattleEventContext(context);
		battleInstanceBuilder.EnableSnapshot(flag);
		battleInstanceBuilder.AddPlayer(0L, TeamType.Left);
		battleInstanceBuilder.AddMonsterForTest(0L, 1, 2, 10, 0, 1);
		battleInstanceBuilder.AddMonsterForTest(0L, 2, 2, 10, 0, 1);
		battleInstanceBuilder.AddMonsterForTest(0L, 3, 2, 10, 0, 1);
		battleInstanceBuilder.AddMonsterForTest(0L, 4, 2, 10, 0, 1);
		CampaignUpdater campaignUpdater = battleInstanceBuilder.BuildUpdater<TBattleInstance>(data.ChapterIndex, data.DungeonIndex, DungeonDifficultyType.Easy, isScenario: false);
		BattleInstance battleInstance = campaignUpdater.BattleInstance;
		if (context != null)
		{
			context.BattleInstance = battleInstance;
		}
		campaignUpdater.StartBattle();
		try
		{
			int num = 1000000;
			while (battleInstance.GlobalTimer.GetCurrentTimeMs() < runTimeMs && (stopFunc == null || !stopFunc()))
			{
				campaignUpdater.Update();
				battleInstance.UpdateTick(flag);
				SnapshotObjectPoolContainer.FlushPendingRelease();
				if (battleInstance.IsBattleEnded() || num-- <= 0)
				{
					break;
				}
			}
		}
		catch (Exception ex)
		{
			VespaDebug.LogError(ex.ToString());
		}
		return battleInstance;
	}

	public static void TestSnapshotPoolIntegrity()
	{
		DateTime utcNow = DateTime.UtcNow;
		bool poolEnabled = SnapshotObjectPoolOption.PoolEnabled;
		SnapshotObjectPoolOption.PoolEnabled = false;
		SnapshotObjectPoolContainer.FlushPendingRelease();
		SnapshotObjectPoolContainer.Reset();
		List<ResultCreatureInfo> creatureInfoList = BattleInstanceIntegrityChecker.GetCreatureInfoList(Run<BattleInstance>(1, 20, 0, 20000));
		SnapshotObjectPoolOption.PoolEnabled = true;
		SnapshotObjectPoolContainer.FlushPendingRelease();
		SnapshotObjectPoolContainer.Reset();
		List<ResultCreatureInfo> creatureInfoList2 = BattleInstanceIntegrityChecker.GetCreatureInfoList(Run<BattleInstance>(1, 20, 0, 20000));
		SnapshotObjectPoolOption.PoolEnabled = false;
		SnapshotObjectPoolContainer.FlushPendingRelease();
		SnapshotObjectPoolContainer.Reset();
		_ = (DateTime.UtcNow - utcNow).TotalSeconds;
		if (BattleInstanceIntegrityChecker.CheckCreatureResult(creatureInfoList, creatureInfoList2) != null)
		{
			VespaDebug.LogError("SnapshotPool integrity check failed!");
		}
		SnapshotObjectPoolOption.PoolEnabled = poolEnabled;
	}
}
