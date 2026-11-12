using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class SoulWeaponEtherData : IDataKeySelector<string>
{
	[Key(0)]
	public string ItemCode { get; set; }

	[Key(1)]
	public int ExpPoints { get; set; }

	[Key(2)]
	public int ReqGold { get; set; }

	string IDataKeySelector<string>.Key => ItemCode;
}
