using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class SoulWeaponUpgradeData : IDataKeySelector<int>
{
	[Key(0)]
	public int Grade { get; set; }

	[Key(1)]
	public int UpgradeRate { get; set; }

	[Key(2)]
	public int ReqReinforceLevel { get; set; }

	int IDataKeySelector<int>.Key => Grade;
}
