namespace NShared;

[JsonSerializable]
public class ChattingActionData
{
	public long SenderId;

	public string SenderName;

	public long ReceiverId;

	public string ReceiverName;

	public string Name { get; set; }

	public string Data { get; set; }
}
