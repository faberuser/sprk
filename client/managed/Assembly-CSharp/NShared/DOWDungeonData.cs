using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class DOWDungeonData : IDataKeySelector<int>
{
	[Key(0)]
	public int DungeonIndex { get; set; }

	[Key(1)]
	public int[] OpenDOWs { get; set; }

	[Key(2)]
	public CreatureTagType ClassType { get; set; }

	int IDataKeySelector<int>.Key => DungeonIndex;

	public override string ToString()
	{
		return "DOWDungeonData Index=" + DungeonIndex;
	}
}
