using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class GuildSkillLevelData : GuildSkillDataBase, IDataKeySelector<GuildSkillLevelKey2>
{
	GuildSkillLevelKey2 IDataKeySelector<GuildSkillLevelKey2>.Key => new GuildSkillLevelKey2(base.GuildSkillIndex, base.GuildSkillLevel);

	[Key(3)]
	public int ActivityPoint { get; set; }

	[Key(4)]
	public int TotalActivityPoint { get; set; }

	[Key(5)]
	public int[] Values { get; set; }

	[Key(6)]
	public int SuppressPoint { get; set; }

	[Key(7)]
	public int TotalSuppressPoint { get; set; }

	[Key(8)]
	public int TrainingCenterLevel { get; set; }

	public override string ToString()
	{
		return "GuildSkillIndex = " + base.GuildSkillIndex + " Level=" + base.GuildSkillLevel;
	}
}
