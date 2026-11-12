namespace NShared.LevelUpGuildSkill;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild/level_up_guild_skill";

	public int SkillIndex { get; set; }

	public byte SkillLevel { get; set; }
}
