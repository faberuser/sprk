using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EclipseStartData : IDataKeySelector<int>
{
	[Key(0)]
	public int RecordWave { get; set; }

	[Key(1)]
	public int StartWave { get; set; }

	int IDataKeySelector<int>.Key => RecordWave;
}
