using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class EventStepDataContainer : JitDataContainerDefault<int, EventStepData>
{
	public const string TABLENAME = "EventStepTable";

	public override TablePathDefine PathDefine => new TablePathDefine("EventStepTable");

	public EventStepData[] GetDatas(CurrencyType currencyType, EventStepType eventStepType)
	{
		List<EventStepData> list = new List<EventStepData>();
		foreach (EventStepData allDatum in GetAllData())
		{
			if (allDatum.CurrencyType == currencyType && allDatum.Type == eventStepType)
			{
				list.Add(allDatum);
			}
		}
		return list.ToArray();
	}

	public EventStepData GetData(CurrencyType currencyType, EventStepType eventStepType, int targetStep)
	{
		foreach (EventStepData allDatum in GetAllData())
		{
			if (allDatum.CurrencyType == currencyType && allDatum.Type == eventStepType && allDatum.Step == targetStep)
			{
				return allDatum;
			}
		}
		return null;
	}

	public int GetMissionTypeCount(CurrencyType currencyType, EventStepType eventStepType)
	{
		int num = 0;
		foreach (EventStepData allDatum in GetAllData())
		{
			if (allDatum.CurrencyType == currencyType && allDatum.Type == eventStepType)
			{
				num++;
			}
		}
		return num;
	}

	public int GetMissionMaxStep(CurrencyType currencyType, EventStepType eventStepType)
	{
		int num = 0;
		foreach (EventStepData allDatum in GetAllData())
		{
			if (allDatum.CurrencyType == currencyType && allDatum.Type == eventStepType)
			{
				num++;
			}
		}
		return num;
	}

	public EventStepData GetEventStepData(CurrencyType currencyType, EventStepType eventStepType, int step)
	{
		int missionMaxStep = GetMissionMaxStep(currencyType, eventStepType);
		if (step >= missionMaxStep)
		{
			return GetData(currencyType, eventStepType, missionMaxStep);
		}
		return GetData(currencyType, eventStepType, step);
	}
}
