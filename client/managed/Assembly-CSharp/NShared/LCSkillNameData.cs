using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class LCSkillNameData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int SkillIndex1 { get; set; }

	[Key(2)]
	public string SkillNameKey1 { get; set; }

	[Key(3)]
	public int SkillIndex2 { get; set; }

	[Key(4)]
	public string SkillNameKey2 { get; set; }

	[Key(5)]
	public int SkillIndex3 { get; set; }

	[Key(6)]
	public string SkillNameKey3 { get; set; }

	[Key(7)]
	public int SkillIndex4 { get; set; }

	[Key(8)]
	public string SkillNameKey4 { get; set; }

	[Key(9)]
	public int SkillIndex5 { get; set; }

	[Key(10)]
	public string SkillNameKey5 { get; set; }

	[Key(11)]
	public int SkillIndex6 { get; set; }

	[Key(12)]
	public string SkillNameKey6 { get; set; }

	[Key(13)]
	public int SkillIndex7 { get; set; }

	[Key(14)]
	public string SkillNameKey7 { get; set; }

	[Key(15)]
	public int SkillIndex8 { get; set; }

	[Key(16)]
	public string SkillNameKey8 { get; set; }

	[Key(17)]
	public int SkillIndex9 { get; set; }

	[Key(18)]
	public string SkillNameKey9 { get; set; }

	[Key(19)]
	public int SkillIndex10 { get; set; }

	[Key(20)]
	public string SkillNameKey10 { get; set; }

	[Key(21)]
	public int SkillIndex11 { get; set; }

	[Key(22)]
	public string SkillNameKey11 { get; set; }

	[Key(23)]
	public int SkillIndex12 { get; set; }

	[Key(24)]
	public string SkillNameKey12 { get; set; }

	[Key(25)]
	public int SkillIndex13 { get; set; }

	[Key(26)]
	public string SkillNameKey13 { get; set; }

	[Key(27)]
	public int SkillIndex14 { get; set; }

	[Key(28)]
	public string SkillNameKey14 { get; set; }

	[Key(29)]
	public int SkillIndex15 { get; set; }

	[Key(30)]
	public string SkillNameKey15 { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
