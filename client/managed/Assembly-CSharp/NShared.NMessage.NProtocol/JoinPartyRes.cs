using NShared.NParty;

namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class JoinPartyRes
{
	public enum ResultType
	{
		Fail,
		Success,
		InvalidParty,
		PartyFull,
		SlotFull,
		Banned,
		AlreadyJoined,
		DungeonLocked
	}

	public ResultType Result { get; set; }

	public PartyInfo PartyInfo { get; set; }

	public int MemberIndex { get; set; }

	public long SendKey { get; set; }
}
