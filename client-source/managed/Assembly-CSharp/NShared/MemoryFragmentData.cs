using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class MemoryFragmentData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string OpenNpcCode { get; set; }

	[Key(2)]
	public string PortalPath { get; set; }

	[Key(3)]
	public string PortraitPath { get; set; }

	[Key(4)]
	public string DungeonName { get; set; }

	[Key(5)]
	public string DungeonDesc { get; set; }

	[Key(6)]
	public string SelectDungeonName { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
