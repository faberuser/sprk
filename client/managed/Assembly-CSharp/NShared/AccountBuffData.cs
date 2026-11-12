using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class AccountBuffData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int SkillIndex { get; set; }

	[Key(2)]
	public int SkillLevel { get; set; }

	[Key(3)]
	public int SkillValue { get; set; }

	[Key(4)]
	public BattleType[] BattleType { get; set; }

	[Key(5)]
	public string ToolTipDesc { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
