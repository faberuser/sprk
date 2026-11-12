using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class SoulWeaponReinforceData : IDataKeySelector<int>
{
	[Key(0)]
	public int ReinforceLevel { get; set; }

	[Key(1)]
	public int StatRate { get; set; }

	[Key(2)]
	public int RequiredExp { get; set; }

	[Key(3)]
	public int SuccessRateMark { get; set; }

	[Key(4)]
	public int ReqGold { get; set; }

	[Key(5)]
	public string[] UnAvailableItemCode { get; set; }

	int IDataKeySelector<int>.Key => ReinforceLevel;
}
