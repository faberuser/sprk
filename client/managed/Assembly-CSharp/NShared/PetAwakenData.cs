using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class PetAwakenData : IDataKeySelector<PetAwakenDataKey>
{
	[Key(0)]
	public int PetIndex { get; set; }

	[Key(1)]
	public int Star { get; set; }

	[Key(2)]
	public string ItemCode1 { get; set; }

	[Key(3)]
	public int ItemCount1 { get; set; }

	[Key(4)]
	public int ReqGold { get; set; }

	PetAwakenDataKey IDataKeySelector<PetAwakenDataKey>.Key => new PetAwakenDataKey(PetIndex, Star);
}
