using System;
using System.Collections.Generic;
using NShared.NOperationValueReaderRuntime;
using NVespa;

namespace NShared;

public class OperationValueReaderFactory
{
	private static Dictionary<string, IOperationValueReader> _readerDictionary;

	static OperationValueReaderFactory()
	{
		_readerDictionary = new Dictionary<string, IOperationValueReader>(StringComparer.OrdinalIgnoreCase);
		InitReader();
		InitStatReader();
	}

	private static void InitReader()
	{
		AddReader(new AllyCount());
		AddReader(new EnemyCount());
		AddReader(new TimeMs());
		AddReader(new RemainTimeMs());
		AddReader(new NShared.NOperationValueReaderRuntime.BossCcCurGauge());
		AddReader(new NShared.NOperationValueReaderRuntime.BossCcMaxGauge());
		AddReader(new SkillLevel());
	}

	private static void InitStatReader()
	{
		Array values = Enum.GetValues(typeof(StatType));
		foreach (CreatureReaderBase.CreatureType value in Enum.GetValues(typeof(CreatureReaderBase.CreatureType)))
		{
			foreach (StatType item in values)
			{
				if (item != StatType.None)
				{
					AddReader(value.ToString() + "." + item, new StatReader(value, item));
				}
			}
			AddReader(value.ToString() + ".DistanceMm", new DistanceReader(value));
			AddReader(value.ToString() + ".StateCount", new StateCount(value));
			AddReader(value.ToString() + ".LastDamage", new LastDamage(value));
			AddReader(value.ToString() + ".LastAttack", new LastAttack(value));
			AddReader(value.ToString() + ".SummoneeCount", new SummoneeCount(value));
		}
	}

	private static void AddReader(string key, IOperationValueReader reader)
	{
		_readerDictionary.Add("$" + key, reader);
	}

	private static void AddReader(IOperationValueReader reader)
	{
		AddReader(reader.GetType().Name, reader);
	}

	private static IOperationValueReader GetReader(string key)
	{
		_readerDictionary.TryGetValue(key, out var value);
		return value;
	}

	public static long Read(string key, string[] arguments, OperationInfo operatinInfo)
	{
		IOperationValueReader reader = GetReader(key);
		if (reader == null)
		{
			VespaDebug.LogError("Unknown operation value reader key : " + key);
			return 0L;
		}
		return reader.Read(arguments, operatinInfo);
	}

	public static IOperationValueReader GetOperationValueReader(string key)
	{
		return GetReader(key);
	}

	public static StatType GetReadingStatType(string key)
	{
		if (!(GetReader(key) is IStatReader statReader))
		{
			return StatType.None;
		}
		return statReader.GetStatType();
	}
}
