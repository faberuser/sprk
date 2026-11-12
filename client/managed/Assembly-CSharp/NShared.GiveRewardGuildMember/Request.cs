namespace NShared.GiveRewardGuildMember;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild/give_reward_guild_member";

	public int ChapterIndex { get; set; }

	public int Index { get; set; }
}
