using System;
using System.Collections.Generic;

namespace NShared;

public class StatFieldManipulator<T>
{
	private class FieldInfo
	{
		public StatType StatType;

		public Func<T, long> Getter;

		public Action<T, long> Setter;
	}

	private readonly Dictionary<StatType, FieldInfo> _fieldInfos = new Dictionary<StatType, FieldInfo>(StatTypeComparer.Default);

	public void InitField(StatType statType, Func<T, long> getter, Action<T, long> setter)
	{
		_fieldInfos[statType] = new FieldInfo
		{
			StatType = statType,
			Getter = getter,
			Setter = setter
		};
	}

	public void InitField(StatType statType, Func<T, int> getter, Action<T, int> setter)
	{
		_fieldInfos[statType] = new FieldInfo
		{
			StatType = statType,
			Getter = (T d) => getter(d),
			Setter = delegate(T d, long v)
			{
				setter(d, (int)v);
			}
		};
	}

	public void InitField(StatType statType, Func<T, short> getter, Action<T, short> setter)
	{
		_fieldInfos[statType] = new FieldInfo
		{
			StatType = statType,
			Getter = (T d) => getter(d),
			Setter = delegate(T d, long v)
			{
				setter(d, (short)v);
			}
		};
	}

	private FieldInfo GetFieldInfo(StatType statType)
	{
		FieldInfo value = null;
		_fieldInfos.TryGetValue(statType, out value);
		return value;
	}

	public long GetStat(T data, StatType statType)
	{
		return GetFieldInfo(statType)?.Getter(data) ?? 0;
	}

	public void SetStat(T data, StatType statType, long value)
	{
		GetFieldInfo(statType)?.Setter(data, value);
	}

	public void AddStat(T data, StatType statType, long value)
	{
		long value2 = GetStat(data, statType) + value;
		SetStat(data, statType, value2);
	}
}
