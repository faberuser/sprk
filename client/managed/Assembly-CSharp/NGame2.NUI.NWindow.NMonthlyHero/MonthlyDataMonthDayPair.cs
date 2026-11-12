namespace NGame2.NUI.NWindow.NMonthlyHero;

public struct MonthlyDataMonthDayPair
{
	public static MonthlyDataMonthDayPair Null = new MonthlyDataMonthDayPair(-1, -1);

	public int Month;

	public int Day;

	public MonthlyDataMonthDayPair(int month, int day)
	{
		Month = month;
		Day = day;
	}

	public bool IsNull()
	{
		return Equals(Null);
	}

	public bool Equals(MonthlyDataMonthDayPair toCompare)
	{
		if (Month == toCompare.Month)
		{
			return Day == toCompare.Day;
		}
		return false;
	}
}
