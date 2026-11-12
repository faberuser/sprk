using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class GuildSkillData : IDataKeySelector<int>
{
	[Key(0)]
	public int GuildSkillIndex { get; set; }

	[Key(1)]
	public GuildSkillType GuildSkillType { get; set; }

	[Key(2)]
	public string SkillDescParam { get; set; }

	[Key(3)]
	public string SkillDesc { get; set; }

	[Key(4)]
	public EventBonusType BonusType { get; set; }

	[Key(5)]
	public string Code { get; set; }

	[Key(6)]
	public string ToolTipName { get; set; }

	[Key(7)]
	public string ToolTipDesc { get; set; }

	[Key(8)]
	public BattleType[] BattleTypes { get; set; }

	int IDataKeySelector<int>.Key => GuildSkillIndex;

	public override string ToString()
	{
		return "GuildSkillIndex = " + GuildSkillIndex;
	}
}
