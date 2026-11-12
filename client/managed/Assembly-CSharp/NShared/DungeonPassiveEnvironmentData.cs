using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class DungeonPassiveEnvironmentData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public TeamType TeamType { get; set; }

	[Key(2)]
	public int GroupIndex { get; set; }

	[Key(3)]
	public string[] FilterValues { get; set; }

	[Key(4)]
	public string[] StateValues { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
