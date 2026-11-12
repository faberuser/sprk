namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class SetGuildMemberRank
{
	public long AccountId { get; set; }

	public byte Rank { get; set; }
}
