namespace NShared;

[JsonSerializable]
public class GuildSkillInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => SkillIndex;

	public int SkillIndex { get; set; }

	public byte SkillLevel { get; set; }

	public byte ReqCount { get; set; }

	public string UpdatedTime { get; set; }
}
