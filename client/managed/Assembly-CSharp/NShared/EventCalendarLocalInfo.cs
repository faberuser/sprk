namespace NShared;

[JsonSerializable]
public class EventCalendarLocalInfo
{
	public string Language { get; set; }

	public string Title { get; set; }

	public string SimpleDescription { get; set; }

	public string Description { get; set; }

	public string Nametag { get; set; }

	public string NoticeUrl { get; set; }
}
