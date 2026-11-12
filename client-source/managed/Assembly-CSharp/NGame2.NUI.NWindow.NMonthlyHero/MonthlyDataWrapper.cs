using System;
using NShared;

namespace NGame2.NUI.NWindow.NMonthlyHero;

public struct MonthlyDataWrapper
{
	public static MonthlyDataWrapper Null = new MonthlyDataWrapper(null, DateTime.MinValue, null);

	public MonthlyHeroData Data;

	public DateTime OpenDate;

	public Func<DateTime> CurrentTimeFunc;

	public MonthlyDataWrapper(MonthlyHeroData data, DateTime openDate, Func<DateTime> timeFunc)
	{
		Data = data;
		OpenDate = openDate;
		CurrentTimeFunc = timeFunc;
	}

	public bool IsOpen()
	{
		return Vespa_System_Action_Extension_Method.fire(CurrentTimeFunc) > OpenDate;
	}

	public bool IsNull()
	{
		return Equals(Null);
	}

	public bool Equals(MonthlyDataWrapper toCompare)
	{
		if (Data == toCompare.Data)
		{
			return OpenDate == toCompare.OpenDate;
		}
		return false;
	}
}
