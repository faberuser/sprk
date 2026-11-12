namespace NShared;

[JsonSerializable(Javascript = true)]
public class WebContentInfo
{
	public int Index { get; set; }

	public string Title { get; set; }

	public string BeginTime { get; set; }

	public string EndTime { get; set; }

	public string ImageUrl { get; set; }

	public byte DisplayEvent { get; set; }

	public string DisplayOnTime { get; set; }

	public string DisplayOffTime { get; set; }

	public bool HasAReward { get; set; }

	public bool IsComplete { get; set; }
}
