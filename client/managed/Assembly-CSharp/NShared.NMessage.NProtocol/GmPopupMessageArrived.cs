namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class GmPopupMessageArrived
{
	public string Title { get; set; }

	public string Content { get; set; }
}
