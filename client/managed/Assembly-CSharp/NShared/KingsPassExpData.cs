using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class KingsPassExpData : IDataKeySelector<KingsPassExpKey>
{
	[Key(0)]
	public int ExpIndex { get; set; }

	[Key(1)]
	public StaminaType StaminaType { get; set; }

	[Key(2)]
	public int Multiple { get; set; }

	KingsPassExpKey IDataKeySelector<KingsPassExpKey>.Key => new KingsPassExpKey(ExpIndex, StaminaType);

	public override string ToString()
	{
		return "Index=" + ExpIndex;
	}
}
