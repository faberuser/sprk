using NShared.NParty;

namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class ChangePartyMemberStatus
{
	public MemberStatus Status { get; set; }
}
