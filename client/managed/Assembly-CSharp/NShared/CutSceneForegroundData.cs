using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CutSceneForegroundData : IDataKeySelector<int>
{
	[Key(0)]
	public string DecorateWidget { get; set; }

	[Key(1)]
	public string AssetPath { get; set; }

	int IDataKeySelector<int>.Key => DecorateWidget.GetHashCode();
}
