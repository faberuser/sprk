namespace NShared;

[JsonSerializable(Javascript = true)]
public class LobbyNoticeLocalInfo
{
	public string Language { get; set; }

	public string ImageUrl { get; set; }

	public string LinkUrl { get; set; }

	public string Message { get; set; }

	public bool IsWebImage()
	{
		if (!ImageUrl.Contains("https://"))
		{
			return ImageUrl.Contains("http://");
		}
		return true;
	}
}
