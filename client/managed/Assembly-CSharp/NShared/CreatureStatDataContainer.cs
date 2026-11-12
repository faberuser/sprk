using NShared.NJit;

namespace NShared;

public class CreatureStatDataContainer : IJitDataContainer
{
	private CreatureStarGradeStatDataContainer _starGradeStatDataContainer;

	private CreatureLevelStatDataContainer2 _levelStatDataContainer;

	public bool IsLoaded { get; private set; }

	public string TableDataPath => "";

	public string LoadError { get; private set; }

	void IJitDataContainer.Load()
	{
		_starGradeStatDataContainer = TableRoot.Instance.GetDataContainer<CreatureStarGradeStatDataContainer>();
		_levelStatDataContainer = TableRoot.Instance.GetDataContainer<CreatureLevelStatDataContainer2>();
		IsLoaded = true;
	}

	public int GetLevelStatFactor(int creatureIndex, int star, int transcended)
	{
		return _starGradeStatDataContainer.GetData(new CreatureStarGradeStatKey(creatureIndex, star, transcended))?.LevelStatFactor ?? 1000;
	}

	public StatData GetStatData(int creatureIndex, int star, int level, int transcended, StatType[] filteredTypes = null, StatType[] excludeLevesStatFactorTypes = null)
	{
		CreatureLevelStatData2 data = _levelStatDataContainer.GetData(creatureIndex);
		if (data == null)
		{
			return new StatData();
		}
		CreatureStarGradeStatData data2 = _starGradeStatDataContainer.GetData(new CreatureStarGradeStatKey(creatureIndex, star, transcended));
		if (data2 == null)
		{
			return new StatData();
		}
		StatData statData = new StatData();
		StatType[] array = ((filteredTypes != null && filteredTypes.Length != 0) ? filteredTypes : StatData.StatTypes);
		foreach (StatType statType in array)
		{
			int num = data2.LevelStatFactor;
			int factor = 0;
			if (GameRoot.Instance.CreatureStarStatDataContainer.GetStatFactor(star, transcended, statType, out factor))
			{
				num = factor;
			}
			long num2 = data.GetStat(statType, level);
			if (excludeLevesStatFactorTypes == null || excludeLevesStatFactorTypes.Length == 0 || !excludeLevesStatFactorTypes.Contains(statType))
			{
				num2 = num2 * num / 1000;
			}
			long stat = data2.GetStat(statType);
			long value = num2 + stat;
			statData.SetStat(statType, value);
		}
		return statData;
	}
}
