namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class AcceptJoinGuild
{
	public GuildMemberInfo MemberInfo { get; set; }
}
