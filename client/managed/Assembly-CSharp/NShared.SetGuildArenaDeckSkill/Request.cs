namespace NShared.SetGuildArenaDeckSkill;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild_arena/set_guild_arena_deck_skill";

	public int SeasonIndex { get; set; }

	public int[] SkillSlotIndices1 { get; set; }

	public int[] SkillSlotIndices2 { get; set; }

	public int[] SkillSlotIndices3 { get; set; }

	public int[] SkillSlotIndices4 { get; set; }

	public int[] SkillSlotIndices5 { get; set; }
}
