using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class OrdealArenaBuffData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string Name { get; set; }

	[Key(2)]
	public int Grade { get; set; }

	[Key(3)]
	public OperationType OptionType { get; set; }

	[Key(4)]
	public int OptionValue { get; set; }

	[Key(5)]
	public int Rate { get; set; }

	[Key(6)]
	public string IconPath { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
