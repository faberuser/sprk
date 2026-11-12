using NShared.NParty;

namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class JoinPartyReq
{
	public enum ResultType
	{
		Fail,
		Success,
		JoinPartyReq_NotOpenedHardMode,
		JoinPartyReq_NotOpenedChallengeRaidDiff,
		JoinPartyReq_NotOpenedTechnoMagic,
		JoinPartyReq_NotOpenedFieldRaid
	}

	public MemberInfo Invitee { get; set; }

	public bool Accepted { get; set; }

	public long RoomNo { get; set; }

	public ResultType Result { get; set; }

	public long SendKey { get; set; }
}
