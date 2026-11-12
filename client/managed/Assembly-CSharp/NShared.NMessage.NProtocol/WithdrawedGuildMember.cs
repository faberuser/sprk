namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class WithdrawedGuildMember
{
	public long AccountId { get; set; }

	public string Nick { get; set; }
}
