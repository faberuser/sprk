using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class SkillLevelFactorData : IDataKeySelector<int>
{
	int IDataKeySelector<int>.Key => Level;

	[Key(0)]
	public int Level { get; set; }

	[Key(1)]
	public int Factor1 { get; set; }

	[Key(2)]
	public int Factor2 { get; set; }

	[Key(3)]
	public int Factor3 { get; set; }

	[Key(4)]
	public int Factor4 { get; set; }

	[Key(5)]
	public int Factor5 { get; set; }

	[Key(6)]
	public int Factor6 { get; set; }

	[Key(7)]
	public int Factor7 { get; set; }

	[Key(8)]
	public int Factor8 { get; set; }

	[Key(9)]
	public int Factor9 { get; set; }

	[Key(10)]
	public int Factor10 { get; set; }

	public override string ToString()
	{
		return "Level=" + Level;
	}

	public int GetFactor(int type)
	{
		return type switch
		{
			1 => Factor1, 
			2 => Factor2, 
			3 => Factor3, 
			4 => Factor4, 
			5 => Factor5, 
			6 => Factor6, 
			7 => Factor7, 
			8 => Factor8, 
			9 => Factor9, 
			10 => Factor10, 
			_ => 0, 
		};
	}
}
