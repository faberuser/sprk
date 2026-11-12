using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class LeftMenuButtonData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public LeftMenuButtonType LeftMenuButtonType { get; set; }

	[Key(2)]
	public string SubType { get; set; }

	[Key(3)]
	public int ShowIndex { get; set; }

	[Key(4)]
	public string TaskName { get; set; }

	[Key(5)]
	public string[] TaskValue { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "Index = " + Index;
	}
}
