namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class GuildSkillInit
{
	public int ActivityPoint { get; set; }

	public int SuppressPoint { get; set; }
}
