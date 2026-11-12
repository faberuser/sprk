namespace NShared;

[JsonSerializable(Javascript = true)]
public class EventCalendarData
{
	public int Id { get; set; }

	public EventBonusType BonusType { get; set; }

	public int Regular { get; set; }

	public string Color { get; set; }

	public int BeginYear { get; set; }

	public int BeginMonth { get; set; }

	public int BeginDay { get; set; }

	public int EndYear { get; set; }

	public int EndMonth { get; set; }

	public int EndDay { get; set; }

	public string DayOfWeek { get; set; }

	public int Show { get; set; }

	public EventCalendarLocalInfo LocalInfo { get; set; }
}
