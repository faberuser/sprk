using System.Collections.Generic;
using NGame2.NUtil.NEquip;
using NShared;
using NVespa;

namespace NGame2.NUI.NHelper.NEquip;

public class EquipScore
{
	private EquipItemInfo _equipItemInfo;

	private Dictionary<EquipAutoKey, long> _equipScoreCache = new Dictionary<EquipAutoKey, long>();

	private long _defaultEquipScore;

	public EquipScore(EquipItemInfo equipItemInfo)
	{
		_equipItemInfo = equipItemInfo;
	}

	public void ClearCache()
	{
		_equipScoreCache.Clear();
		_defaultEquipScore = 0L;
	}

	public long GetEquipItemScore(CreatureData targetCreature)
	{
		if (_equipItemInfo == null)
		{
			return 0L;
		}
		if (targetCreature == null)
		{
			VespaDebug.LogError("creature is null");
			return 0L;
		}
		EquipAutoKey key = new EquipAutoKey(targetCreature.TagType, _equipItemInfo.EquipItemData.PartType.GetEquipSlotIndex());
		EquipAutoData data = GameRoot.Instance.EquipAutoDataContainer.GetData(key);
		if (data == null)
		{
			if (_defaultEquipScore == 0L)
			{
				StatData equipStat = StatCalculator.GetEquipStat(_equipItemInfo, null, null, applyExtraStat: false);
				_defaultEquipScore = GetDefaultEquipItemScore(equipStat);
			}
			return _defaultEquipScore;
		}
		if (_equipScoreCache.TryGetValue(key, out var value))
		{
			return value;
		}
		_equipScoreCache[key] = GetEquipItemScore(_equipItemInfo, data);
		return _equipScoreCache[key];
	}

	private long GetEquipItemScore(EquipItemInfo equipInfo, EquipAutoData autoData)
	{
		StatData statData = EquipStatCalculator.Calculate(equipInfo);
		long num = 0L;
		num += autoData.GetMaxStat1(statData) * autoData.MainStatFactor1;
		num += autoData.GetMaxStat2(statData) * autoData.MainStatFactor2;
		num += autoData.GetMaxStat3(statData) * autoData.MainStatFactor3;
		if (SingletonAuto<ConditionChecker>.instance.Check(equipInfo, autoData.ExtraStatApplyCondition))
		{
			num += EquipStatCalculator.GetMaxOfAddedStat(autoData.ExtraStatType);
			num += EquipStatCalculator.GetMaxOfMutedStat(autoData.ExtraStatType);
		}
		return num;
	}

	private long GetDefaultEquipItemScore(StatData statData)
	{
		if (statData == null)
		{
			return -1L;
		}
		long maxStat = statData.GetMaxStat(StatType.PhysicalAttack, StatType.MagicalAttack);
		long maxStat2 = statData.GetMaxStat(StatType.PhysicalDefense, StatType.MagicalDefense);
		long stat = statData.GetStat(StatType.MaxHp);
		return maxStat * 1000 + maxStat2 * 1000 + stat * 50;
	}
}
