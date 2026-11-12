namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class InviteRematchRes
{
	public enum ResultType
	{
		Accepted,
		Rejected,
		CannotAccept
	}

	public ResultType Result { get; set; }

	public MatchInfo MatchInfo { get; set; }
}
