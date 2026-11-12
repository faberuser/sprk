namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class RequestDuel
{
	public long GroupId { get; set; }

	public string Nick { get; set; }

	public int TeamLevel { get; set; }

	public int AvatarIndex { get; set; }
}
