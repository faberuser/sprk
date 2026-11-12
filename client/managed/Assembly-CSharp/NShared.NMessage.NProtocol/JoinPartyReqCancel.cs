namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class JoinPartyReqCancel
{
	public enum ResultType
	{
		TooLate,
		ServerFailed,
		InvalidSendKey
	}

	public ResultType Result { get; set; }
}
