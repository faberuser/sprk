using System;
using NShared;

namespace NGame2.NBattleContext;

public static class PunishmentHelper
{
	public static float GetFlaskExp(int chapter, int dungeon, int clearWave, int endWave, int endKillCount)
	{
		int punishmentGroupIndex = GameRoot.Instance.KarmaDungeonDataContainer.GetDataByChapterDungeon(chapter, dungeon).PunishmentGroupIndex;
		PunishmentGroupData data = GameRoot.Instance.PunishmentGroupDataContainer.GetData(punishmentGroupIndex, EPunishmentDungeonType.Karma);
		_ = GameRoot.Instance.KarmaGaugeExponentialDataContainer;
		float num = data.InitialShardPoint;
		float num2 = 0f;
		if (clearWave < 1)
		{
			CampaignWaveData data2 = GameRoot.Instance.CampaignWaveDataContainer.GetData(chapter, dungeon, DungeonDifficultyType.Easy, scenario: false, 1);
			int num3 = 0;
			if (data2 != null)
			{
				if (__Extension_Method_ICollection__.IsNullOrEmpty(data2.GetCreatureList()))
				{
					return 0f;
				}
				num3 = data2.GetCreatureList().Count;
			}
			return num / (float)num3 * (float)endKillCount;
		}
		num2 = num;
		float num4 = num;
		float num5 = 0f;
		if (clearWave > 1)
		{
			for (int i = 2; i <= clearWave; i++)
			{
				num5 = GameRoot.Instance.KarmaGaugeExponentialDataContainer.GetExponentValue(punishmentGroupIndex, i);
				num4 *= num5;
				num2 += num4;
			}
		}
		CampaignWaveData data3 = GameRoot.Instance.CampaignWaveDataContainer.GetData(chapter, dungeon, DungeonDifficultyType.Easy, scenario: false, 1);
		int num6 = 0;
		num5 = GameRoot.Instance.KarmaGaugeExponentialDataContainer.GetExponentValue(punishmentGroupIndex, endWave);
		num4 *= num5;
		if (data3 != null)
		{
			if (__Extension_Method_ICollection__.IsNullOrEmpty(data3.GetCreatureList()))
			{
				return 0f;
			}
			num6 = data3.GetCreatureList().Count;
		}
		return num2 + num4 / (float)num6 * (float)endKillCount;
	}

	public static void GetFullFlask(int flaskIndex, int flaskCount, float collectedExp, out int selectedFullFlaksCount, out int defaultFullFlaskCount)
	{
		selectedFullFlaksCount = 0;
		defaultFullFlaskCount = 0;
		float num = collectedExp;
		ShardFlaskItemData data = GameRoot.Instance.ShardFlaskItemDataContainer.GetData(flaskIndex);
		if (data == null)
		{
			return;
		}
		int flaskGauge = data.FlaskGauge;
		if (flaskGauge <= 0)
		{
			return;
		}
		if (!data.IsDefault)
		{
			selectedFullFlaksCount = (int)(collectedExp / (float)flaskGauge);
			selectedFullFlaksCount = Math.Min(selectedFullFlaksCount, flaskCount);
			if (selectedFullFlaksCount <= 0)
			{
				defaultFullFlaskCount = 0;
				return;
			}
			num -= (float)(flaskGauge * selectedFullFlaksCount);
		}
		int defaultFlaskIndex = GameRoot.Instance.ShardFlaskItemDataContainer.GetDefaultFlaskIndex(data.GroupIndex);
		ShardFlaskItemData data2 = GameRoot.Instance.ShardFlaskItemDataContainer.GetData(defaultFlaskIndex);
		if (data2 != null)
		{
			int flaskGauge2 = data2.FlaskGauge;
			defaultFullFlaskCount = (int)(num / (float)flaskGauge2);
		}
	}
}
