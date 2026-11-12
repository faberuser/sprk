using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ShardFlaskItemData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int GroupIndex { get; set; }

	[Key(2)]
	public int ShowIndex { get; set; }

	[Key(3)]
	public int FlaskGauge { get; set; }

	[Key(4)]
	public int ShardItemIndex { get; set; }

	[Key(5)]
	public bool IsDefault { get; set; }

	[Key(6)]
	public string BattleIconPath { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
