using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class OrdealArenaEventData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public EOrdealEventType EventType { get; set; }

	[Key(2)]
	public int Condition { get; set; }

	[Key(3)]
	public int Rate { get; set; }

	[Key(4)]
	public string TexturePath { get; set; }

	[Key(5)]
	public string[] DescKeys { get; set; }

	[Key(6)]
	public int Volume { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
