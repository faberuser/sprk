namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class BecomeGuildGhostMembers
{
	public long[] GhostMemberIds { get; set; }
}
