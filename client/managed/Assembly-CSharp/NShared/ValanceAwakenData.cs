using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ValanceAwakenData : IDataKeySelector<ValanceAwakenDataKey>
{
	ValanceAwakenDataKey IDataKeySelector<ValanceAwakenDataKey>.Key => new ValanceAwakenDataKey(Tier, DetailIndex, CreatureTagType, Star);

	[Key(0)]
	public byte Tier { get; set; }

	[Key(1)]
	public int DetailIndex { get; set; }

	[Key(2)]
	public CreatureTagType CreatureTagType { get; set; }

	[Key(3)]
	public int Star { get; set; }

	[Key(4)]
	public int ReqGold { get; set; }

	[Key(5)]
	public string ItemCode1 { get; set; }

	[Key(6)]
	public int ItemCount1 { get; set; }
}
