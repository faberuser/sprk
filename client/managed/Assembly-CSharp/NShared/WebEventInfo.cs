namespace NShared;

[JsonSerializable(Javascript = true)]
public class WebEventInfo
{
	public string ImageUrl { get; set; }

	public string LinkUrl { get; set; }

	public int FirstEventId { get; set; }

	public string QueryString { get; set; }

	public WebContentInfo[] WebContents { get; set; }
}
