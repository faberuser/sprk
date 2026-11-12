using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class SoulWeaponSkillData : IDataKeySelector<string>
{
	private const int AddSkillCount = 5;

	[Key(0)]
	public string ItemCode { get; set; }

	[Key(1)]
	public int[] ActiveConditionSkillIndices { get; set; }

	[Key(2)]
	public int BasicSkillIndex { get; set; }

	[Key(3)]
	public int MaxUseCount { get; set; }

	[Key(4)]
	public int AddSkillIndex1 { get; set; }

	[Key(5)]
	public int AddSkillIndex2 { get; set; }

	[Key(6)]
	public int AddSkillIndex3 { get; set; }

	[Key(7)]
	public int AddSkillIndex4 { get; set; }

	[Key(8)]
	public int AddSkillIndex5 { get; set; }

	[Key(9)]
	public string CheckState { get; set; }

	[Key(10)]
	public string StackTypeName { get; set; }

	[Key(11)]
	public string[] UpgradeDescription { get; private set; }

	string IDataKeySelector<string>.Key => ItemCode;

	public int GetAddSkillIndex(int grade)
	{
		return grade switch
		{
			1 => AddSkillIndex1, 
			2 => AddSkillIndex2, 
			3 => AddSkillIndex3, 
			4 => AddSkillIndex4, 
			5 => AddSkillIndex5, 
			_ => 0, 
		};
	}
}
