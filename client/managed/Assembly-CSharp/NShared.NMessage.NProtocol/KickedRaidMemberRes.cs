namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class KickedRaidMemberRes
{
	public long[] KickedAccountIds { get; set; }

	public long ReplaceMasterAccountId { get; set; }
}
