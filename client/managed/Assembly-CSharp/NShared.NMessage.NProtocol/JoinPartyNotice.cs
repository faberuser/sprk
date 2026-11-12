using NShared.NParty;

namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class JoinPartyNotice
{
	public PartyInfo PartyInfo { get; set; }
}
