namespace NShared.RequestJoinGuild;

public enum ResultType
{
	Fail,
	Success,
	NoGuild,
	MaxMember,
	MaxJoinRequest,
	GuildMaxJoinRequest,
	AlreadyJoinRequested,
	InvalidGuildLevel,
	AlreadyJoined,
	LowTeamLevel,
	NotElapsedGuildRequestCoolTime
}
