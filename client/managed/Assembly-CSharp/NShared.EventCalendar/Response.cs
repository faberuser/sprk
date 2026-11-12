namespace NShared.EventCalendar;

[Response]
public class Response : GenericResponse<ResultType>
{
	public int EventCalendarVersion { get; set; }

	public int NewEvent { get; set; }

	public EventCalendarData[] EventCalendarDatas { get; set; }
}
