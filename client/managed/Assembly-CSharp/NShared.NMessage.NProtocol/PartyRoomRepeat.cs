namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class PartyRoomRepeat
{
	public enum ResultType
	{
		CANCEL_MASTER,
		CANCEL_STAMINA,
		CANCEL_IVENTORY,
		CANCEL_LEAVE_PARTY,
		NONE
	}

	public bool IsRepeat { get; set; }

	public ResultType Result { get; set; }
}
