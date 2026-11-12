namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class KickedGuildMember
{
	public long AccountId { get; set; }

	public string Nick { get; set; }

	public string GuildWithdrawTime { get; set; }
}
