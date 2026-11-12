namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class GuildSkillLevelUp
{
	public int GuildSkillIndex { get; set; }

	public byte Level { get; set; }
}
