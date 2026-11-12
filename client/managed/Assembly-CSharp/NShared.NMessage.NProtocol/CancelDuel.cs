namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class CancelDuel
{
	public long GroupId { get; set; }

	public bool IsReject { get; set; }
}
