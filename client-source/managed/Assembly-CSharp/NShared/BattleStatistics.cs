using System;
using System.Collections.Generic;

namespace NShared;

public class BattleStatistics : SnapshotObject
{
	private readonly SnapshotObjectMember<SnapshotDictionary<int, long>> _valuesHolder;

	public SnapshotDictionary<int, long> Values => _valuesHolder.Get();

	public BattleStatistics()
	{
		_valuesHolder = new SnapshotObjectMember<SnapshotDictionary<int, long>>(this);
	}

	public void Init()
	{
		_valuesHolder.Alloc();
	}

	public void Add(int key, long value)
	{
		if (Values.TryGetValue(key, out var value2))
		{
			Values[key] = value2 + value;
		}
		else
		{
			Values[key] = value;
		}
	}

	public long Get(int key)
	{
		long value = 0L;
		Values.TryGetValue(key, out value);
		return value;
	}

	public long Sum()
	{
		long sum = 0L;
		Values.ForEachIrregular(delegate(KeyValuePair<int, long> pair)
		{
			sum += pair.Value;
		});
		return sum;
	}

	public long Max()
	{
		long max = long.MinValue;
		Values.ForEachIrregular(delegate(KeyValuePair<int, long> pair)
		{
			max = Math.Max(max, pair.Value);
		});
		return max;
	}

	public void ForEachIrregular(Action<KeyValuePair<int, long>> action)
	{
		Values.ForEachIrregular(action);
	}
}
