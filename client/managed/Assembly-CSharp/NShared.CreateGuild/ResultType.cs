namespace NShared.CreateGuild;

public enum ResultType
{
	Fail,
	Success,
	AlreadyJoined,
	NotEnoughGold,
	NotEnoughTeamLevel,
	EmptyName,
	InvalidName,
	AlreadyExistName,
	TooShortName,
	TooLongName,
	CensoredWord,
	SensitiveWord,
	InvalidGuildIntroduce,
	TooLongGuildIntroduce,
	NotElapsedGuildRequestCoolTime
}
