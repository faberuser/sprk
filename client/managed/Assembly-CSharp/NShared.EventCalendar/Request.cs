namespace NShared.EventCalendar;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "event/event_calendar";

	public int EventCalendarVersion { get; set; }

	public string Language { get; set; }
}
