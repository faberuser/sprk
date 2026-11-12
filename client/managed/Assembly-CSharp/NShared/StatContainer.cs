using System;
using System.Collections.Generic;

namespace NShared;

public class StatContainer
{
	private static readonly Random _random = new Random(DateTime.UtcNow.Millisecond);

	private long _obf;

	private Dictionary<StatType, long> _statValueDic;

	public StatContainer()
	{
		_obf = (long)_random.Next() * (long)_random.Next();
	}

	public long GetStat(StatType statType)
	{
		if (_statValueDic == null)
		{
			return 0L;
		}
		if (!_statValueDic.TryGetValue(statType, out var value))
		{
			return 0L;
		}
		return value ^ _obf;
	}

	public void SetStat(StatType statType, long value)
	{
		if (value == 0L)
		{
			if (_statValueDic != null && _statValueDic.ContainsKey(statType))
			{
				_statValueDic.Remove(statType);
			}
			return;
		}
		if (_statValueDic == null)
		{
			_statValueDic = new Dictionary<StatType, long>(StatTypeComparer.Default);
		}
		_statValueDic[statType] = value ^ _obf;
	}

	public void AddStat(StatType statType, long value)
	{
		if (value != 0L)
		{
			long value2 = GetStat(statType) + value;
			SetStat(statType, value2);
		}
	}

	public void MultStat(StatType statType, int value)
	{
		if (value != 0)
		{
			long value2 = GetStat(statType) * Math.Max(1000 + value, 0) / 1000;
			SetStat(statType, value2);
		}
	}

	public bool EqualStat(StatContainer other)
	{
		StatType diffStat;
		long diffLValue;
		long diffRValue;
		return EqualStat(other, out diffStat, out diffLValue, out diffRValue);
	}

	public bool EqualStat(StatContainer other, out string diffLog, out long diffLValue, out long diffRValue)
	{
		if (!EqualStat(other, out StatType diffStat, out diffLValue, out diffRValue))
		{
			diffLog = diffStat.ToString() + "_" + diffLValue + "_" + diffRValue;
			return false;
		}
		diffLog = null;
		return true;
	}

	public bool EqualStat(StatContainer other, out StatType diffStat, out long diffLValue, out long diffRValue)
	{
		for (int i = 0; i < StatData.StatTypes.Length; i++)
		{
			StatType statType = StatData.StatTypes[i];
			long stat = GetStat(statType);
			long num = other?.GetStat(statType) ?? 0;
			if (stat != num)
			{
				diffStat = statType;
				diffLValue = stat;
				diffRValue = num;
				return false;
			}
		}
		diffStat = StatType.None;
		diffLValue = 0L;
		diffRValue = 0L;
		return true;
	}

	public long GetCheckSum()
	{
		long num = 0L;
		if (_statValueDic != null)
		{
			foreach (KeyValuePair<StatType, long> item in _statValueDic)
			{
				long num2 = (long)item.Key;
				long num3 = item.Value ^ _obf;
				num ^= num2;
				num ^= num3;
			}
		}
		return num;
	}
}
