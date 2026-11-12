namespace NShared;

[JsonSerializable(Javascript = true)]
public enum EGuildMemberRank
{
	GuildMaster = 1,
	GuildAdmin,
	GuildMember,
	GuildNewMember,
	GuildGhostMember
}
