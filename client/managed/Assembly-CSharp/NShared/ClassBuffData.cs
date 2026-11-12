using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class ClassBuffData : ClassBuffDataBase, IDataKeySelector<int>
{
	int IDataKeySelector<int>.Key => Index;

	[Key(3)]
	public int Index { get; set; }

	[Key(4)]
	public CreatureTagType TagType { get; set; }

	[Key(5)]
	public int ClassBuffLevel { get; set; }

	[Key(6)]
	public long NeedPoint { get; set; }

	[Key(7)]
	public int NeedClassBuffIndex { get; set; }

	[Key(8)]
	public int NeedClassBuffLevel { get; set; }

	[Key(9)]
	public int NeedHeroCount { get; set; }

	[Key(10)]
	public string ClassBuffIconPath { get; set; }

	[Key(11)]
	public string BuffOptionTitleName { get; set; }

	[Key(12)]
	public string BuffOptionTitleDesc { get; set; }

	[Key(13)]
	public byte StatUsePercentage { get; set; }

	[Key(14)]
	public byte Grade { get; set; }
}
